# Asp.Net scheduled

[![actions](https://github.com/cashwu/Cashwu.AspNet.Scheduled/workflows/.NET%20Core/badge.svg?branch=master)](https://github.com/cashwu/Cashwu.AspNet.Scheduled/actions)

---

[![Nuget](https://img.shields.io/badge/Nuget-Cashwu.Aspnet.Scheduled-blue.svg)](https://www.nuget.org/packages/Cashwu.Aspnet.Scheduled)

---

## Implement your scheduled task from IScheduledTask

- `Schedule` is cron schedule, could referrence [crontab](https://crontab.guru/)
  - minimum 1 minute (* * * * *)
- `IsLazy` is application started run once immediately
- `ExecuteAsync` is your scheduled job

```csharp
public class YourTask : IScheduledTask
{
    public string Schedule { get; }

    public bool IsLazy { get; }
    
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
```

## Global.asax.cs

-  Application_Start method add ApplicationScheduler.Start() in last line 
    - first parameter is scheduler task implement assembly name
    - second parameter is error handler event

```csharp
protected void Application_Start()
{
    ...

    ApplicationScheduled.Start("assembly name", (sender, args) =>
    {
        Console.WriteLine(args.Exception);
    });
}

```
