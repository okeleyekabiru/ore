using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ore.Api.Contracts.BrandSurveys;
using Ore.Api.Tests.Infrastructure;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Ore.Infrastructure.Persistence;
using Xunit;

namespace Ore.Api.Tests.Integration.BrandSurveys;

public sealed class SurveyResponsesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scopeFactory;

    public SurveyResponsesControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task CreateSurveyResponse_PersistsSubmissionAndUpdatesBrandVoice()
    {
        await ResetDatabaseAsync();
        var teamId = await CreateTeamAsync("Delta Team");
        var surveyId = await CreateSurveyAsync(teamId, "Voice Survey");

        var surveyDetailsResponse = await _client.GetFromJsonAsync<ApiResponse<BrandSurveyDetailsResponse>>($"/api/brand-surveys/{surveyId}");
        Assert.NotNull(surveyDetailsResponse);
        Assert.True(surveyDetailsResponse!.Success);
        Assert.NotNull(surveyDetailsResponse.Data);
        var question = surveyDetailsResponse.Data!.Questions.First();

        var request = new CreateSurveyResponseRequest(
            surveyId,
            null,
            new[]
            {
                new SurveyAnswerRequest(question.Id, "Confident yet warm", null),
            },
            new BrandVoiceProfileRequest(
                "Confident",
                "Warm",
                "Growth teams",
                "Increase share of voice",
                "LegacyCorp",
                new[] { "Helpful", "Bold" }));

        var response = await _client.PostAsJsonAsync("/api/survey-response", request);
        response.EnsureSuccessStatusCode();

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var submissions = await dbContext.BrandSurveySubmissions
            .Include(s => s.Answers)
            .Where(s => s.UserId == TestConstants.DefaultUserId)
            .ToListAsync();

        Assert.Single(submissions);
        var submission = submissions[0];
        Assert.Equal(surveyId, submission.SurveyId);
        Assert.Single(submission.Answers);
        Assert.Equal("Confident yet warm", submission.Answers.First().Value);

        var team = await dbContext.Teams.AsNoTracking().FirstAsync(t => t.Id == teamId);
        Assert.NotNull(team.BrandVoice);
        Assert.Equal("Confident", team.BrandVoice!.Voice);
        Assert.Contains("Helpful", team.BrandVoice!.Keywords);
    }

    [Fact]
    public async Task GetResponsesByUser_ReturnsDetailedAnswers()
    {
        await ResetDatabaseAsync();
        var teamId = await CreateTeamAsync("Echo Team");
        var surveyId = await CreateSurveyAsync(teamId, "Discovery Survey");

        var details = await _client.GetFromJsonAsync<ApiResponse<BrandSurveyDetailsResponse>>($"/api/brand-surveys/{surveyId}");
        Assert.NotNull(details);
        Assert.True(details!.Success);
        var question = details.Data!.Questions.First();

        await _client.PostAsJsonAsync(
            "/api/survey-response",
            new CreateSurveyResponseRequest(
                surveyId,
                null,
                new[] { new SurveyAnswerRequest(question.Id, "Playful", null) },
                null));

        var result = await _client.GetFromJsonAsync<ApiResponse<SurveyResponseDetailsResponse[]>>($"/api/survey-response/{TestConstants.DefaultUserId}");
        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.NotNull(result.Data);
        var submission = Assert.Single(result.Data!);
        Assert.Equal(surveyId, submission.SurveyId);
        Assert.Equal("Discovery Survey", submission.SurveyTitle);
        var answer = Assert.Single(submission.Answers);
        Assert.Equal(question.Id, answer.QuestionId);
        Assert.Equal(SurveyQuestionType.Text, answer.Type);
        Assert.Equal("Playful", answer.Value);
    }

    private async Task<Guid> CreateTeamAsync(string name)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var team = new Team(name);
        context.Teams.Add(team);
        await context.SaveChangesAsync();
        return team.Id;
    }

    private async Task<Guid> CreateSurveyAsync(Guid teamId, string title)
    {
        var request = new CreateBrandSurveyRequest(
            teamId,
            title,
            $"{title} description",
            "Onboarding",
            new[]
            {
                new CreateSurveyQuestionRequest("Tell us about your brand", SurveyQuestionType.Text, 0, Array.Empty<string>())
            });

        var response = await _client.PostAsJsonAsync("/api/brand-surveys", request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        return payload.Data;
    }

    private async Task ResetDatabaseAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
}
