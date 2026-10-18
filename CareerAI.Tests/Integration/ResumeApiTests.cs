using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Http.Json;
using System.Text;
using CareerAI.Application.Common;
using CareerAI.Application.DTOs.Resumes;
using Xunit;
using static CareerAI.Tests.Integration.ApiTestHelpers;

namespace CareerAI.Tests.Integration;

[Collection(ApiCollection.Name)]
public class ResumeApiTests(CareerApiFactory factory)
{
    [Fact]
    public async Task Upload_ValidPdf_ReturnsCreated_AndExtractsText()
    {
        var user = await factory.RegisterUserAsync();

        var response = await user.Client.UploadAsync(TestPdf.Create(SampleCvText), "my-cv.pdf");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ResumeDto>();
        Assert.NotNull(created);
        Assert.Equal("my-cv.pdf", created.FileName);
        Assert.True(created.ExtractedTextLength > 0);

        var detail = await user.Client.GetFromJsonAsync<ResumeDetailDto>($"/api/resumes/{created.Id}");
        Assert.NotNull(detail);
        Assert.Contains("React", detail.ExtractedText);
    }

    [Fact]
    public async Task Upload_TextFileDisguisedAsPdf_IsRejected_ByMagicBytes()
    {
        var user = await factory.RegisterUserAsync();

        var response = await user.Client.UploadAsync(Encoding.ASCII.GetBytes("just some text"), "fake.pdf");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("file_not_pdf", (await response.ReadProblemAsync()).Code);
    }

    [Fact]
    public async Task Upload_PdfBytesWithWrongExtension_IsRejected()
    {
        var user = await factory.RegisterUserAsync();

        var response = await user.Client.UploadAsync(TestPdf.Create(SampleCvText), "notes.txt");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("file_not_pdf", (await response.ReadProblemAsync()).Code);
    }

    [Fact]
    public async Task Upload_PdfHeaderButGarbageContent_IsRejected_AsUnreadable()
    {
        var user = await factory.RegisterUserAsync();

        var response = await user.Client.UploadAsync(Encoding.ASCII.GetBytes("%PDF-1.4 corrupted content"), "broken.pdf");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("pdf_unreadable", (await response.ReadProblemAsync()).Code);
    }

    [Fact]
    public async Task Upload_FileLargerThanLimit_IsRejected()
    {
        var user = await factory.RegisterUserAsync();
        var tooLarge = new byte[ResumeLimits.MaxFileSizeBytes + 1];

        var response = await user.Client.UploadAsync(tooLarge, "huge.pdf");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("file_too_large", (await response.ReadProblemAsync()).Code);
    }

    [Fact]
    public async Task Upload_EmptyFile_IsRejected()
    {
        var user = await factory.RegisterUserAsync();

        var response = await user.Client.UploadAsync([], "empty.pdf");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("file_required", (await response.ReadProblemAsync()).Code);
    }

    [Fact]
    public async Task Upload_PathTraversalFileName_IsSanitized_AndNeverUsedOnDisk()
    {
        var user = await factory.RegisterUserAsync();

        var response = await user.Client.UploadAsync(TestPdf.Create(SampleCvText), "../../evil.pdf");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ResumeDto>();
        Assert.NotNull(created);

        // Ekranda gösterilen ad temizlenmiş
        Assert.Equal("evil.pdf", created.FileName);

        // Diskteki dosya adı kullanıcının adı değil, sistemin ürettiği GUID
        var storedNames = Directory.GetFiles(factory.UploadsPath).Select(Path.GetFileName).ToList();
        Assert.DoesNotContain("evil.pdf", storedNames);
        Assert.All(storedNames, name => Assert.Matches("^[0-9a-f]{32}\\.pdf$", name!));
    }

    [Fact]
    public async Task Download_ReturnsTheOriginalPdf_AsAttachment()
    {
        var user = await factory.RegisterUserAsync();
        var bytes = TestPdf.Create(SampleCvText);
        var uploaded = await (await user.Client.UploadAsync(bytes, "cv.pdf")).Content.ReadFromJsonAsync<ResumeDto>();
        Assert.NotNull(uploaded);

        var response = await user.Client.GetAsync($"/api/resumes/{uploaded.Id}/download");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.Equal(bytes, await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task OtherUser_CannotSeeDownloadAnalyzeOrDeleteSomeoneElsesResume()
    {
        var owner = await factory.RegisterUserAsync();
        var intruder = await factory.RegisterUserAsync();
        var resume = await owner.Client.UploadResumeAsync();

        var aiCallsBefore = factory.SkillExtractor.CvCalls;

        // Başkasının CV'si listede görünmez
        var intruderList = await intruder.Client.GetFromJsonAsync<List<ResumeDto>>("/api/resumes");
        Assert.NotNull(intruderList);
        Assert.Empty(intruderList);

        // Başka kullanıcının CV'sine her erişim 404 (403 değil: varlığı bile sızdırılmaz)
        await intruder.Client.GetAsync($"/api/resumes/{resume.Id}").AssertStatusAsync(HttpStatusCode.NotFound);
        await intruder.Client.GetAsync($"/api/resumes/{resume.Id}/download").AssertStatusAsync(HttpStatusCode.NotFound);
        await intruder.Client.PostAsync($"/api/resumes/{resume.Id}/extract-skills", null).AssertStatusAsync(HttpStatusCode.NotFound);
        await intruder.Client.DeleteAsync($"/api/resumes/{resume.Id}").AssertStatusAsync(HttpStatusCode.NotFound);

        // Yetkisiz istek OpenAI'a hiç ulaşmadı: başkasının verisi asla prompt'a girmez
        Assert.Equal(aiCallsBefore, factory.SkillExtractor.CvCalls);

        // Sahibi için hiçbir şey değişmedi
        await owner.Client.GetAsync($"/api/resumes/{resume.Id}").AssertStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_OwnResume_RemovesIt()
    {
        var user = await factory.RegisterUserAsync();
        var resume = await user.Client.UploadResumeAsync();

        await user.Client.DeleteAsync($"/api/resumes/{resume.Id}").AssertStatusAsync(HttpStatusCode.NoContent);
        await user.Client.GetAsync($"/api/resumes/{resume.Id}").AssertStatusAsync(HttpStatusCode.NotFound);
    }
}