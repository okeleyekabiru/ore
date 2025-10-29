# Social Media Publishing Worker

## Overview

The Ore platform now includes a comprehensive background worker system for scheduling and publishing social media posts across multiple platforms. This system provides reliable, retryable publishing with OAuth token management and comprehensive audit logging.

## Architecture

### Core Components

1. **Social Media Publishers** (`ISocialMediaPublisher`)
   - Platform-specific implementations for Meta, LinkedIn, X (Twitter)
   - Handle API communication and error handling
   - Support retry logic for transient failures

2. **OAuth Token Management** (`IOAuthTokenService`)
   - Secure storage and retrieval of access tokens
   - Automatic token refresh capabilities
   - Platform-specific OAuth workflows

3. **Background Worker** (`ContentDistributionJob`)
   - Quartz.NET-based job scheduling
   - Exponential backoff retry logic
   - Comprehensive audit logging

4. **Database Entities**
   - `SocialMediaAccount`: OAuth token storage
   - `ContentDistribution`: Publishing scheduling and status
   - `AuditLog`: Publishing event tracking

## Features

### ✅ Multi-Platform Support
- **Meta (Facebook)**: Graph API integration
- **LinkedIn**: Professional network publishing
- **X (Twitter)**: Tweet publishing with character limits

### ✅ Robust Retry Logic
- Exponential backoff strategy
- Configurable retry limits
- Platform-specific error handling
- Permanent vs. temporary failure detection

### ✅ OAuth Token Management
- Secure token storage with encryption
- Automatic refresh handling
- Token expiration detection
- Multi-team token isolation

### ✅ Audit & Monitoring
- Comprehensive logging of all publish attempts
- Success/failure tracking with metadata
- PostgreSQL-based audit trail
- Structured logging with Serilog

### ✅ Scheduling System
- Quartz.NET-based job orchestration
- Precise scheduling with timezone support
- Background processing capabilities
- Graceful shutdown handling

## Database Schema

### SocialMediaAccounts Table
```sql
CREATE TABLE "SocialMediaAccounts" (
    "Id" uuid NOT NULL,
    "TeamId" uuid NOT NULL,
    "Platform" integer NOT NULL,
    "AccountName" character varying(255) NOT NULL,
    "AccessToken" character varying(2000) NOT NULL,
    "RefreshToken" character varying(2000),
    "ExpiresAt" timestamp with time zone,
    "IsActive" boolean NOT NULL,
    "LastUsedAt" timestamp with time zone,
    "CreatedOnUtc" timestamp with time zone NOT NULL,
    "ModifiedOnUtc" timestamp with time zone,
    CONSTRAINT "PK_SocialMediaAccounts" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_SocialMediaAccounts_Team_Platform" 
ON "SocialMediaAccounts" ("TeamId", "Platform");
```

## Usage

### 1. Setting Up OAuth Tokens

```csharp
// Store OAuth tokens for a team
await oauthTokenService.StoreTokensAsync(
    teamId: Guid.Parse("team-id"),
    platform: PlatformType.Meta,
    accountName: "company-page",
    accessToken: "access-token",
    refreshToken: "refresh-token",
    expiresAt: DateTime.UtcNow.AddHours(1));
```

### 2. Scheduling Content Publishing

```csharp
// Schedule content for publishing
var scheduleCommand = new ScheduleContentCommand(
    ContentId: contentId,
    Platform: PlatformType.LinkedIn,
    PublishOnUtc: DateTime.UtcNow.AddHours(2),
    RetryInterval: TimeSpan.FromMinutes(5),
    MaxRetryCount: 3);

await mediator.Send(scheduleCommand);
```

### 3. Publishing Process Flow

1. **Job Execution**: Quartz.NET triggers `ContentDistributionJob`
2. **Token Retrieval**: Get valid OAuth token for team/platform
3. **Content Publishing**: Call platform-specific publisher
4. **Result Processing**: Update database and create audit logs
5. **Retry Logic**: Schedule retry on failure (if applicable)

## Configuration

### Worker Configuration (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Quartz": "Information",
      "Ore": "Debug"
    }
  },
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database=ore;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "Quartz": "Information",
        "Ore": "Debug"
      }
    }
  }
}
```

### Publishing Platform Setup

#### Meta (Facebook)
- Requires Facebook App with `pages_manage_posts` permission
- Use Facebook Graph API v18.0+
- Store page access tokens for publishing

#### LinkedIn
- Requires LinkedIn API application
- Need `w_member_social` and `r_liteprofile` scopes
- Use LinkedIn API v2 for UGC posts

#### X (Twitter)
- Requires Twitter API v2 access
- Need OAuth 2.0 with PKCE
- 280 character limit enforcement

## Retry Strategy

### Exponential Backoff Algorithm
```
Delay = BaseDelay × 2^(attempt - 1)
Max Delay = 4 hours
```

### Retry Conditions
- **Retryable**: Rate limits, server errors (5xx), network timeouts
- **Non-Retryable**: Authentication errors, content policy violations, permanent failures

### Example Retry Schedule
- Attempt 1: Immediate
- Attempt 2: 5 minutes
- Attempt 3: 10 minutes  
- Attempt 4: 20 minutes
- Attempt 5: 40 minutes (then give up)

## Monitoring & Observability

### Audit Log Structure
```json
{
  "actor": "system",
  "action": "content_published",
  "entity": "ContentDistribution", 
  "entityId": "distribution-guid",
  "metadata": {
    "DistributionId": "guid",
    "Platform": "LinkedIn",
    "ExternalPostId": "platform-post-id",
    "Title": "Content Title",
    "PublishedAt": "2025-10-28T10:30:00Z"
  }
}
```

### Key Metrics to Monitor
- Publishing success/failure rates by platform
- Token refresh frequency and success
- Average retry attempts per publish
- Job execution duration and frequency

## Security Considerations

### OAuth Token Security
- Tokens stored encrypted in PostgreSQL
- Automatic token rotation and refresh
- Secure token transmission to publishers
- Team-based token isolation

### Error Handling
- Sensitive information filtered from logs
- Secure error message exposure
- Platform-specific error categorization

## Development & Testing

### Running the Worker
```bash
# Start the background worker
dotnet run --project src/Worker

# Run with specific environment
dotnet run --project src/Worker --environment Production
```

### Testing Publishers
```csharp
// Unit test example
var publisher = new MetaPublisher(httpClient, logger);
var request = new SocialMediaPostRequest(
    "Test Title",
    "Test Body", 
    "Test Caption",
    new[] { "#test" },
    null,
    teamId,
    "test-token");

var result = await publisher.PublishAsync(request);
Assert.True(result.IsSuccess);
```

## Troubleshooting

### Common Issues

1. **Token Expiration**
   - Check `SocialMediaAccounts.ExpiresAt`
   - Verify refresh token validity
   - Re-authenticate if needed

2. **Publishing Failures**
   - Check platform API status
   - Verify content compliance
   - Review audit logs for error details

3. **Job Scheduling Issues**
   - Check Quartz.NET configuration
   - Verify database connectivity
   - Monitor worker logs

### Log Analysis
```bash
# Filter publishing logs
grep "content_published\|content_publish_failed" logs/worker.log

# Monitor retry attempts
grep "retry.*distribution" logs/worker.log
```

## Future Enhancements

### Planned Features
- Instagram publishing support
- TikTok integration
- Advanced content scheduling (optimal timing)
- Publishing analytics and insights
- Bulk publishing capabilities
- Content approval workflows

### Integration Opportunities
- Webhook notifications for publish events
- Third-party analytics integration
- Content performance tracking
- A/B testing for publish times

## API Reference

### Publishing Endpoints
- `POST /api/social-media/oauth/connect` - Connect platform account
- `GET /api/social-media/accounts` - List connected accounts  
- `DELETE /api/social-media/accounts/{id}` - Disconnect account
- `GET /api/publishing/status/{distributionId}` - Check publish status

---

This comprehensive social media publishing system provides a robust foundation for automated content distribution across multiple platforms with enterprise-grade reliability and monitoring capabilities.