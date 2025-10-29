using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ore.Api.Contracts.Content;
using Ore.Api.Tests.Infrastructure;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Xunit;

namespace Ore.Api.Tests.Integration.Content;

public sealed class ApprovalWorkflowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scopeFactory;

    public ApprovalWorkflowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task Submit_IndividualUser_AutoApprovesContent()
    {
        await ResetDatabaseAsync();
        var (teamId, userId) = await CreateUserAndTeamAsync("individual@test.com", RoleType.Individual);
        var contentId = await CreateContentAsync(teamId, userId, "Individual Test Content");

        var request = new SubmitContentRequest(contentId);
        var response = await _client.PostAsJsonAsync("/api/content/submit", request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);

        // Verify content was auto-approved
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var content = await dbContext.ContentItems.FindAsync(contentId);
        Assert.NotNull(content);
        Assert.Equal(ContentStatus.Approved, content!.Status);
    }

    [Fact]
    public async Task Submit_TeamUser_RequiresApproval()
    {
        await ResetDatabaseAsync();
        var (teamId, userId) = await CreateUserAndTeamAsync("creator@test.com", RoleType.ContentCreator);
        var contentId = await CreateContentAsync(teamId, userId, "Team Content Needs Approval");

        var request = new SubmitContentRequest(contentId);
        var response = await _client.PostAsJsonAsync("/api/content/submit", request);
        response.EnsureSuccessStatusCode();

        // Verify content is pending approval
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var content = await dbContext.ContentItems.FindAsync(contentId);
        Assert.NotNull(content);
        Assert.Equal(ContentStatus.PendingApproval, content!.Status);
    }

    [Fact]
    public async Task Approve_ValidContent_ApprovesSuccessfully()
    {
        await ResetDatabaseAsync();
        var (teamId, userId) = await CreateUserAndTeamAsync("creator@test.com", RoleType.ContentCreator);
        var (_, managerId) = await CreateUserAndTeamAsync("manager@test.com", RoleType.SocialMediaManager, teamId);
        var contentId = await CreateContentAsync(teamId, userId, "Content To Approve");

        // Submit for approval first
        var submitRequest = new SubmitContentRequest(contentId);
        await _client.PostAsJsonAsync("/api/content/submit", submitRequest);

        // Now approve it
        var approveRequest = new ApproveContentRequest(contentId, "Looks good!");
        var response = await _client.PostAsJsonAsync("/api/content/approve", approveRequest);
        response.EnsureSuccessStatusCode();

        // Verify content was approved
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var content = await dbContext.ContentItems.FindAsync(contentId);
        Assert.NotNull(content);
        Assert.Equal(ContentStatus.Approved, content!.Status);
    }

    [Fact]
    public async Task GetPending_ReturnsOnlyPendingContent()
    {
        await ResetDatabaseAsync();
        var (teamId, userId) = await CreateUserAndTeamAsync("creator@test.com", RoleType.ContentCreator);
        var contentId1 = await CreateContentAsync(teamId, userId, "Pending Content 1");
        var contentId2 = await CreateContentAsync(teamId, userId, "Pending Content 2");
        var contentId3 = await CreateContentAsync(teamId, userId, "Draft Content");

        // Submit two for approval
        await _client.PostAsJsonAsync("/api/content/submit", new SubmitContentRequest(contentId1));
        await _client.PostAsJsonAsync("/api/content/submit", new SubmitContentRequest(contentId2));

        // Get pending content
        var response = await _client.GetAsync($"/api/content/pending?teamId={teamId}");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PendingContentResponse[]>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.NotNull(payload.Data);
        Assert.Equal(2, payload.Data!.Length);
    }

    [Fact]
    public async Task Submit_NonexistentContent_ReturnsFailure()
    {
        await ResetDatabaseAsync();
        var nonexistentId = Guid.NewGuid();

        var request = new SubmitContentRequest(nonexistentId);
        var response = await _client.PostAsJsonAsync("/api/content/submit", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        Assert.NotNull(payload);
        Assert.False(payload!.Success);
    }

    private async Task<(Guid teamId, Guid userId)> CreateUserAndTeamAsync(string email, RoleType role, Guid? existingTeamId = null)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        Team team;
        if (existingTeamId.HasValue)
        {
            team = await dbContext.Teams.FindAsync(existingTeamId.Value) ?? throw new InvalidOperationException("Team not found");
        }
        else
        {
            team = new Team($"Test Team {DateTime.Now.Ticks}");
            dbContext.Teams.Add(team);
        }

        var user = new User(Guid.NewGuid(), email, "Test", "User", role);
        user.AssignTeam(team);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return (team.Id, user.Id);
    }

    private async Task<Guid> CreateContentAsync(Guid teamId, Guid authorId, string title)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var content = new ContentItem(teamId, authorId, title, title, null);
        dbContext.ContentItems.Add(content);
        await dbContext.SaveChangesAsync();

        return content.Id;
    }

    private async Task ResetDatabaseAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Remove data in order to respect foreign key constraints
        dbContext.ContentItems.RemoveRange(dbContext.ContentItems);
        dbContext.ApprovalRecords.RemoveRange(dbContext.ApprovalRecords);
        dbContext.Users.RemoveRange(dbContext.Users);
        dbContext.Teams.RemoveRange(dbContext.Teams);
        dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs);
        await dbContext.SaveChangesAsync();
    }
}