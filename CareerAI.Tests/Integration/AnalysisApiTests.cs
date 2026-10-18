using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Http.Json;
using CareerAI.Application.DTOs.Analysis;
using CareerAI.Application.DTOs.Dashboard;
using CareerAI.Application.DTOs.JobApplications;
using Xunit;
using static CareerAI.Tests.Integration.ApiTestHelpers;

namespace CareerAI.Tests.Integration;

[Collection(ApiCollection.Name)]
public class AnalysisApiTests(CareerApiFactory factory)
{
    [Fact]
    public async Task Analyze_ComputesDeterministicScore_AndPersistsResult()
    {
        var user = await factory.RegisterUserAsync();
        var resume = await user.Client.UploadResumeAsync();
        var job = await user.Client.CreateJobAsync();

        var response = await user.Client.AnalyzeAsync(job.Id, resume.Id);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AnalysisResultDto>();
        Assert.NotNull(result);

        // Zorunlu 5 (4'ü eşleşti), tercih 1 (eksik): (4*3) / (5*3 + 1*1) = 12/16 = %75. Skoru AI değil C# hesaplar.
        Assert.Equal(75, result.MatchScore);
        Assert.Equal(4, result.MatchedSkills.Count);
        Assert.Contains(result.MissingSkills, s => s.Name == "Docker" && s.IsRequired);
        Assert.Contains(result.MissingSkills, s => s.Name == "Azure" && !s.IsRequired);
        Assert.Equal("Test summary.", result.Summary);
        Assert.Equal(2, result.Recommendations.Count);

        // Kalıcı: son analiz aynı sonucu döndürür
        var latest = await user.Client.GetFromJsonAsync<AnalysisResultDto>($"/api/job-applications/{job.Id}/analysis");
        Assert.NotNull(latest);
        Assert.Equal(result.Id, latest.Id);
        Assert.Equal(75, latest.MatchScore);

        // Başvuru listesinde skor görünür
        var jobs = await user.Client.GetFromJsonAsync<List<JobApplicationDto>>("/api/job-applications");
        Assert.NotNull(jobs);
        Assert.Equal(75, jobs.Single(j => j.Id == job.Id).MatchScore);

        // Dashboard
        var dashboard = await user.Client.GetFromJsonAsync<DashboardDto>("/api/dashboard");
        Assert.NotNull(dashboard);
        Assert.Equal(1, dashboard.Stats.TotalAnalyses);
        Assert.Equal(75, dashboard.Stats.HighestScore);
        Assert.Equal(75, dashboard.Stats.AverageScore);
        Assert.Contains(dashboard.TopMissingSkills, s => s.Name == "Docker");
    }

    [Fact]
    public async Task Analyze_Twice_ReusesCachedResumeSkills_AndDoesNotInflateDashboard()
    {
        var user = await factory.RegisterUserAsync();
        var resume = await user.Client.UploadResumeAsync();
        var job = await user.Client.CreateJobAsync();

        var cvCallsBefore = factory.SkillExtractor.CvCalls;
        var jobCallsBefore = factory.SkillExtractor.JobCalls;

        var first = await (await user.Client.AnalyzeAsync(job.Id, resume.Id)).Content.ReadFromJsonAsync<AnalysisResultDto>();
        var second = await (await user.Client.AnalyzeAsync(job.Id, resume.Id)).Content.ReadFromJsonAsync<AnalysisResultDto>();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotEqual(first.Id, second.Id);

        // CV becerileri ilk analizde çıkarıldı, ikincide önbellekten geldi (AI'a tekrar gitmedi). İlan her seferinde taze.
        Assert.Equal(cvCallsBefore + 1, factory.SkillExtractor.CvCalls);
        Assert.Equal(jobCallsBefore + 2, factory.SkillExtractor.JobCalls);

        // Güncel analiz en yenisi
        var latest = await user.Client.GetFromJsonAsync<AnalysisResultDto>($"/api/job-applications/{job.Id}/analysis");
        Assert.NotNull(latest);
        Assert.Equal(second.Id, latest.Id);

        // Yeniden analiz istatistikleri şişirmez: aynı ilan tek sayılır
        var dashboard = await user.Client.GetFromJsonAsync<DashboardDto>("/api/dashboard");
        Assert.NotNull(dashboard);
        Assert.Equal(1, dashboard.Stats.TotalAnalyses);
        Assert.Single(dashboard.RecentAnalyses);
    }

    [Fact]
    public async Task Analyze_OtherUsersJobOrResume_ReturnsNotFound_AndMakesNoAiCall()
    {
        var owner = await factory.RegisterUserAsync();
        var intruder = await factory.RegisterUserAsync();
        var ownerResume = await owner.Client.UploadResumeAsync();
        var ownerJob = await owner.Client.CreateJobAsync();
        var intruderResume = await intruder.Client.UploadResumeAsync();
        var intruderJob = await intruder.Client.CreateJobAsync();

        var cvCallsBefore = factory.SkillExtractor.CvCalls;
        var jobCallsBefore = factory.SkillExtractor.JobCalls;

        // Başkasının ilanı + kendi CV'si
        await intruder.Client.AnalyzeAsync(ownerJob.Id, intruderResume.Id).AssertStatusAsync(HttpStatusCode.NotFound);
        // Kendi ilanı + başkasının CV'si
        await intruder.Client.AnalyzeAsync(intruderJob.Id, ownerResume.Id).AssertStatusAsync(HttpStatusCode.NotFound);
        // Başkasının ilanının detayı, analizi ve silinmesi
        await intruder.Client.GetAsync($"/api/job-applications/{ownerJob.Id}").AssertStatusAsync(HttpStatusCode.NotFound);
        await intruder.Client.GetAsync($"/api/job-applications/{ownerJob.Id}/analysis").AssertStatusAsync(HttpStatusCode.NotFound);
        await intruder.Client.DeleteAsync($"/api/job-applications/{ownerJob.Id}").AssertStatusAsync(HttpStatusCode.NotFound);

        // Hiçbir AI çağrısı yapılmadı: yetki kontrolü OpenAI'dan ÖNCE çalışıyor
        Assert.Equal(cvCallsBefore, factory.SkillExtractor.CvCalls);
        Assert.Equal(jobCallsBefore, factory.SkillExtractor.JobCalls);

        // Sahibinin ilanı sağlam
        await owner.Client.GetAsync($"/api/job-applications/{ownerJob.Id}").AssertStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Analyze_WhenRecommendationsFail_StillSavesAnalysisWithScore()
    {
        var user = await factory.RegisterUserAsync();
        var resume = await user.Client.UploadResumeAsync();
        var job = await user.Client.CreateJobAsync();

        factory.Recommendations.ShouldFail = true;
        try
        {
            var response = await user.Client.AnalyzeAsync(job.Id, resume.Id);

            // Kademeli bozulma: öneriler olmadan da asıl değer (skor) korunur
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AnalysisResultDto>();
            Assert.NotNull(result);
            Assert.Equal(75, result.MatchScore);
            Assert.Null(result.Summary);
            Assert.Empty(result.Recommendations);
        }
        finally
        {
            factory.Recommendations.ShouldFail = false;
        }
    }

    [Fact]
    public async Task Analyze_SendsOnlyMatchingResultsToRecommendationGenerator()
    {
        var user = await factory.RegisterUserAsync();
        var resume = await user.Client.UploadResumeAsync();
        var job = await user.Client.CreateJobAsync();

        await user.Client.AnalyzeAsync(job.Id, resume.Id).AssertStatusAsync(HttpStatusCode.Created);

        // Öneri modeline giden veri: eşleştirme sonucu. RecommendationRequest'in CV/ilan METNİ alanı yok (veri minimizasyonu).
        var request = factory.Recommendations.LastRequest;
        Assert.NotNull(request);
        Assert.Equal(75, request.MatchScore);
        Assert.Contains("Docker", request.MissingRequiredSkills);
        Assert.Contains("Azure", request.MissingPreferredSkills);
        Assert.Contains("C#", request.CandidateSkills);
    }

    [Fact]
    public async Task Analyze_JobWithoutTechnicalSkills_ReturnsBadRequest()
    {
        var user = await factory.RegisterUserAsync();
        var resume = await user.Client.UploadResumeAsync();
        var job = await user.Client.CreateJobAsync("We are a friendly team that loves coffee and long walks on the beach.");

        var response = await user.Client.AnalyzeAsync(job.Id, resume.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("job_no_skills", (await response.ReadProblemAsync()).Code);
    }

    [Fact]
    public async Task ResumeWithAnalysis_CannotBeDeleted_UntilTheJobIsDeleted()
    {
        var user = await factory.RegisterUserAsync();
        var resume = await user.Client.UploadResumeAsync();
        var job = await user.Client.CreateJobAsync();
        await user.Client.AnalyzeAsync(job.Id, resume.Id).AssertStatusAsync(HttpStatusCode.Created);

        // Analizi olan CV silinemez
        await user.Client.DeleteAsync($"/api/resumes/{resume.Id}").AssertStatusAsync(HttpStatusCode.Conflict);

        // İlan silinince analizleri de gider (cascade), sonra CV silinebilir
        await user.Client.DeleteAsync($"/api/job-applications/{job.Id}").AssertStatusAsync(HttpStatusCode.NoContent);
        await user.Client.DeleteAsync($"/api/resumes/{resume.Id}").AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Dashboard_ForNewUser_ReturnsEmptyButCompleteShape()
    {
        var user = await factory.RegisterUserAsync();

        var dashboard = await user.Client.GetFromJsonAsync<DashboardDto>("/api/dashboard");

        Assert.NotNull(dashboard);
        Assert.Equal(0, dashboard.Stats.TotalApplications);
        Assert.Equal(0, dashboard.Stats.TotalAnalyses);
        Assert.Null(dashboard.Stats.AverageScore);
        Assert.Null(dashboard.Stats.HighestScore);

        // Grafikler boş kullanıcıda da tam: 5 skor aralığı, 6 ay, 0 değerleriyle
        Assert.Equal(5, dashboard.ScoreDistribution.Count);
        Assert.Equal(6, dashboard.ApplicationsOverTime.Count);
        Assert.Empty(dashboard.TopMissingSkills);
        Assert.Empty(dashboard.RecentAnalyses);
    }
}