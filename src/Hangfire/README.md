# NDB.Platform.Api.Hangfire

Provider-agnostic Hangfire background job wrapper with a secured dashboard.

---

## Key Types

| Type | Description |
|---|---|
| `NdbHangfireOptions` | Dashboard URL, Basic Auth credentials, worker count, queues |
| `NdbHangfireBasicAuthFilter` | Timing-safe Basic Auth for the Hangfire dashboard |
| `HangfireExtensions` | `AddNdbHangfire()` and `UseNdbHangfireDashboard()` |

---

## Why Provider-Agnostic?

This library does not include a Hangfire storage provider. Install the one that matches your database in the consuming project:

```bash
dotnet add package Hangfire.PostgreSql   # PostgreSQL
dotnet add package Hangfire.SqlServer    # SQL Server
dotnet add package Hangfire.InMemory     # In-memory (dev / testing only)
```

---

## Setup

```csharp
// Program.cs
builder.Services.AddNdbHangfire(
    configure: o =>
    {
        o.WorkerCount       = Environment.ProcessorCount * 5;  // default
        o.Queues            = ["default", "critical"];
        o.BasicAuthPassword = configuration["Hangfire:Password"]!;
    },
    storageCallback: cfg => cfg.UsePostgreSqlStorage(
        connectionString,
        new PostgreSqlStorageOptions { SchemaName = "hangfire" }));

// Pipeline
app.UseNdbHangfireDashboard(o =>
{
    o.DashboardUrl      = "/jobs";
    o.BasicAuthUser     = "admin";
    o.BasicAuthPassword = configuration["Hangfire:Password"]!;
    o.DashboardTitle    = "My App — Background Jobs";
});
```

---

## Startup Validation

`UseNdbHangfireDashboard` calls `NdbHangfireOptions.Validate()` before registering the dashboard. The startup is rejected if:
- `BasicAuthUser` is empty
- `BasicAuthPassword` is empty

This prevents accidentally deploying the dashboard without credentials.

---

## Timing-Safe Authentication

`NdbHangfireBasicAuthFilter` uses `CryptographicOperations.FixedTimeEquals` for credential comparison. This prevents timing attacks — an attacker cannot use response time differences to determine whether a username or password is partially correct.

---

## NdbHangfireOptions Reference

| Property | Default | Description |
|---|---|---|
| `DashboardUrl` | `/jobs` | URL path for the Hangfire dashboard |
| `BasicAuthUser` | `admin` | Dashboard username |
| `BasicAuthPassword` | `""` | Dashboard password — must be set before production |
| `DashboardTitle` | `Background Jobs` | Title shown in the dashboard header |
| `WorkerCount` | `ProcessorCount × 5`, min 20 | Concurrent job workers |
| `Queues` | `["default"]` | Queue names to process |

---

## Enqueueing Jobs

```csharp
// Fire-and-forget:
BackgroundJob.Enqueue<IEmailService>(s => s.SendWelcomeAsync(userId, CancellationToken.None));

// Delayed:
BackgroundJob.Schedule<IEmailService>(
    s => s.SendReminderAsync(userId, CancellationToken.None),
    TimeSpan.FromHours(24));

// Recurring (via Hangfire.RecurringJob):
RecurringJob.AddOrUpdate<IReportService>(
    "daily-report",
    s => s.GenerateDailyAsync(CancellationToken.None),
    Cron.Daily(8));  // 08:00 daily
```
