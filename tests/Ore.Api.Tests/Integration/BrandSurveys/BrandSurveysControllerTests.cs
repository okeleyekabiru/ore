using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ore.Api.Contracts.BrandSurveys;
using Ore.Api.Tests.Infrastructure;
using Ore.Infrastructure.Persistence;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Xunit;

namespace Ore.Api.Tests.Integration.BrandSurveys;

public sealed class BrandSurveysControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scopeFactory;

    public BrandSurveysControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task UpdateSurvey_PersistsChanges()
    {
        await ResetDatabaseAsync();
        var teamId = await CreateTeamAsync("Alpha Team");
        var surveyId = await CreateSurveyAsync(teamId, "Initial Survey");

        var updateRequest = new UpdateBrandSurveyRequest(
            "Updated Survey",
            "Refined description",
            "Onboarding",
            new[]
            {
                new UpdateSurveyQuestionRequest("Primary choice", SurveyQuestionType.SingleChoice, 0, new[] { "Option A", "Option B" }),
                new UpdateSurveyQuestionRequest("Elaborate answer", SurveyQuestionType.TextArea, 1, Array.Empty<string>())
            });

        var updateResponse = await _client.PutAsJsonAsync($"/api/brand-surveys/{surveyId}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        var surveyResponse = await _client.GetFromJsonAsync<ApiResponse<BrandSurveyDetailsResponse>>($"/api/brand-surveys/{surveyId}");
        Assert.NotNull(surveyResponse);
        var survey = surveyResponse!.Data;
        Assert.NotNull(survey);
        Assert.Equal("Updated Survey", survey!.Title);
        Assert.Equal("Refined description", survey.Description);
        Assert.Equal(2, survey.Questions.Count());
        var ordered = survey.Questions.OrderBy(q => q.Order).ToArray();
        Assert.Equal(SurveyQuestionType.SingleChoice, ordered[0].Type);
        Assert.Equal(2, ordered[0].Options.Count());
        Assert.Equal(SurveyQuestionType.TextArea, ordered[1].Type);
    }

    [Fact]
    public async Task UpdateSurvey_WithInvalidPayload_ReturnsValidationErrors()
    {
        await ResetDatabaseAsync();
        var teamId = await CreateTeamAsync("Beta Team");
        var surveyId = await CreateSurveyAsync(teamId, "Validation Survey");

    var updateRequest = new UpdateBrandSurveyRequest(string.Empty, string.Empty, string.Empty, Array.Empty<UpdateSurveyQuestionRequest>());

        var response = await _client.PutAsJsonAsync($"/api/brand-surveys/{surveyId}", updateRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object?>>();
        Assert.NotNull(payload);
        Assert.False(payload!.Success);
        Assert.NotEmpty(payload.Errors);
    }

    [Fact]
    public async Task ToggleSurveyActivation_ReflectsInListAndDetails()
    {
        await ResetDatabaseAsync();
        var teamId = await CreateTeamAsync("Gamma Team");
        var surveyId = await CreateSurveyAsync(teamId, "Activation Survey");
        var secondarySurveyId = await CreateSurveyAsync(teamId, "Secondary Survey");

        await SendPostAsync($"/api/brand-surveys/{surveyId}/deactivate");
        await SendPostAsync($"/api/brand-surveys/{secondarySurveyId}/deactivate");

        var deactivated = await _client.GetFromJsonAsync<ApiResponse<BrandSurveyDetailsResponse>>($"/api/brand-surveys/{surveyId}");
        Assert.NotNull(deactivated);
        Assert.False(deactivated!.Data!.IsActive);

        await SendPostAsync($"/api/brand-surveys/{surveyId}/activate");

        var reactivated = await _client.GetFromJsonAsync<ApiResponse<BrandSurveyDetailsResponse>>($"/api/brand-surveys/{surveyId}");
        Assert.NotNull(reactivated);
        Assert.True(reactivated!.Data!.IsActive);

        var activeList = await _client.GetFromJsonAsync<ApiResponse<BrandSurveySummaryResponse[]>>($"/api/brand-surveys?teamId={teamId}&includeInactive=false");
        Assert.NotNull(activeList);
        Assert.Single(activeList!.Data!);
        Assert.True(activeList.Data![0].IsActive);

        var fullList = await _client.GetFromJsonAsync<ApiResponse<BrandSurveySummaryResponse[]>>($"/api/brand-surveys?teamId={teamId}&includeInactive=true");
        Assert.NotNull(fullList);
        Assert.Equal(2, fullList!.Data!.Length);
        Assert.Contains(fullList.Data!, s => !s.IsActive);
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
                new CreateSurveyQuestionRequest("Describe your brand", SurveyQuestionType.Text, 0, Array.Empty<string>())
            });

        var response = await _client.PostAsJsonAsync("/api/brand-surveys", request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.NotEqual(Guid.Empty, payload.Data);
        return payload.Data;
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

    private async Task ResetDatabaseAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    private async Task SendPostAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
