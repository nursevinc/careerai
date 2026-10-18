using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareerAI.Application.DTOs.Analysis;
using CareerAI.Application.DTOs.Auth;
using CareerAI.Application.DTOs.JobApplications;
using CareerAI.Application.DTOs.Resumes;
using Xunit;

namespace CareerAI.Tests.Integration;

public sealed record TestUser(HttpClient Client, string Email, string Password, AuthResponse Auth)
{
    public Guid Id => Auth.User.Id;
}

// ProblemDetails yanıtının test için gereken kısmı
public sealed record ProblemResponse(int? Status, string? Title, string? Detail, string? Code);

public static class ApiTestHelpers
{
    public const string DefaultPassword = "Test1234";

    // Fake AI'ın tanıdığı becerileri içeren örnek CV ve ilan. Beklenen skor: (4*3) / (5*3 + 1) = %75
    public const string SampleCvText = "Nur Test. Skills: C#, ASP.NET Core, SQL Server, React, Git.";
    public const string SampleJobDescription = "Required: C#, ASP.NET Core, SQL Server, React, Docker. Preferred: Azure.";

    // Her çağrıda rastgele e-postalı yeni bir kullanıcı: testler birbirinden bağımsız
    public static async Task<TestUser> RegisterUserAsync(this CareerApiFactory factory, string password = DefaultPassword)
    {
        var client = factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@test.com";

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Test User", email, password));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.UseToken(auth.AccessToken);
        return new TestUser(client, email, password, auth);
    }

    public static void UseToken(this HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public static MultipartFormDataContent CreateUpload(byte[] bytes, string fileName)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

        var content = new MultipartFormDataContent();
        content.Add(file, "file", fileName);
        return content;
    }

    public static async Task<HttpResponseMessage> UploadAsync(this HttpClient client, byte[] bytes, string fileName)
    {
        using var content = CreateUpload(bytes, fileName);
        return await client.PostAsync("/api/resumes/upload", content);
    }

    public static async Task<ResumeDto> UploadResumeAsync(
        this HttpClient client, string cvText = SampleCvText, string fileName = "cv.pdf")
    {
        var response = await client.UploadAsync(TestPdf.Create(cvText), fileName);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var resume = await response.Content.ReadFromJsonAsync<ResumeDto>();
        Assert.NotNull(resume);
        return resume;
    }

    public static async Task<JobApplicationDto> CreateJobAsync(
        this HttpClient client, string description = SampleJobDescription)
    {
        var response = await client.PostAsJsonAsync(
            "/api/job-applications",
            new CreateJobApplicationRequest("Junior .NET Developer", "ABC Teknoloji", description));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var job = await response.Content.ReadFromJsonAsync<JobApplicationDto>();
        Assert.NotNull(job);
        return job;
    }

    public static Task<HttpResponseMessage> AnalyzeAsync(this HttpClient client, Guid jobId, Guid resumeId) =>
        client.PostAsJsonAsync($"/api/job-applications/{jobId}/analyze", new AnalyzeRequest(resumeId));

    public static async Task<ProblemResponse> ReadProblemAsync(this HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.NotNull(problem);
        return problem;
    }

    public static async Task AssertStatusAsync(this Task<HttpResponseMessage> request, HttpStatusCode expected)
    {
        var response = await request;
        Assert.Equal(expected, response.StatusCode);
    }
}