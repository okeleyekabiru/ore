using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ore.Api.Contracts.Content;
using Ore.Api.Tests.Infrastructure;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Ore.Domain.ValueObjects;
using Ore.Infrastructure.Persistence;
using Xunit;

namespace Ore.Api.Tests.Integration.Content;

public sealed class ContentControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scopeFactory;

    public ContentControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task Generate_ReturnsSuggestion()
    {
        await ResetDatabaseAsync();
        var brandId = await CreateBrandAsync("Content Lab");

        var request = new GenerateContentRequest(brandId, "AI-driven onboarding", "upbeat", nameof(PlatformType.LinkedIn));
        var response = await _client.PostAsJsonAsync("/api/content/generate", request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<GenerateContentResponse>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.NotNull(payload.Data);
        Assert.False(string.IsNullOrWhiteSpace(payload.Data!.Caption));
        Assert.NotNull(payload.Data.Hashtags);
        Assert.NotEmpty(payload.Data.Hashtags);
    }

    [Fact]
    public async Task Generate_InvalidPlatform_ReturnsFailure()
    {
        await ResetDatabaseAsync();
        var brandId = await CreateBrandAsync("Tone Studio");

        var request = new GenerateContentRequest(brandId, "AI-driven onboarding", "upbeat", "GeoCities");
        var response = await _client.PostAsJsonAsync("/api/content/generate", request);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<GenerateContentResponse>>();
        Assert.NotNull(payload);
        Assert.False(payload!.Success);
    }

    private async Task<Guid> CreateBrandAsync(string name)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var team = new Team(name);
        var voice = BrandVoiceProfile.Create(
            "Confident",
            "Upbeat",
            "Marketing leaders",
            "Drive adoption",
            "Legacy platforms",
            new[] { "AI", "Innovation", "Brand Story" });
        team.SetBrandVoice(voice);

        var domainUsers = context.Set<User>();
        var admin = await domainUsers.FirstOrDefaultAsync(u => u.Id == TestConstants.DefaultUserId);
        if (admin is null)
        {
            admin = new User(TestConstants.DefaultUserId, "admin@test.dev", "Test", "Admin", RoleType.Admin);
            domainUsers.Add(admin);
        }

        team.AddMember(admin);

        var survey = new BrandSurvey(team.Id, "Voice Discovery", "Collects positioning details", "Onboarding");
        survey.AddQuestion("Describe your brand personality", SurveyQuestionType.TextArea, 0);
        survey.AddQuestion("List brand keywords", SurveyQuestionType.MultiChoice, 1, new[] { "Bold", "Playful", "Premium" });

        var submission = new BrandSurveySubmission(survey.Id, admin.Id);
        foreach (var question in survey.Questions)
        {
            var answerValue = question.Type switch
            {
                SurveyQuestionType.MultiChoice => "Bold, Premium",
                _ => "We speak with confidence and clarity."
            };
            submission.AddAnswer(question.Id, answerValue);
        }

    context.Teams.Add(team);
    context.BrandSurveys.Add(survey);
    context.BrandSurveySubmissions.Add(submission);

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
}
