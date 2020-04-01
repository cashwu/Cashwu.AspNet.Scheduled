using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NCrontab;

namespace Cashwu.AspNet.Scheduled
{
    internal class SchedulerHostedService
    {
        private const string LogPrefix = "Cashwu.Scheduler";
        
        private readonly List<ScheduledTaskWrapper> _scheduledTasks = new List<ScheduledTaskWrapper>();

        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskExceptionEvent;
        
        public event EventHandler<string> LogEvent;

        internal SchedulerHostedService(string prefixAssemblyName)
        {
            var referenceTime = DateTime.UtcNow;
            var scheduledTasks = ScheduledTasks(prefixAssemblyName);

            foreach (var scheduledTask in scheduledTasks)
            {
                var scheduledTaskWrapper = new ScheduledTaskWrapper
                {
                    BaseTime = referenceTime
                };

                if (string.IsNullOrWhiteSpace(scheduledTask.Schedule))
                {
                    scheduledTaskWrapper.Task = scheduledTask;
                    scheduledTaskWrapper.NextRuntTime = referenceTime;
                }
                else
                {
                    scheduledTaskWrapper.Schedule = CrontabSchedule.Parse(scheduledTask.Schedule);
                    scheduledTaskWrapper.Task = scheduledTask;

                    if (scheduledTask.IsLazy)
                    {
                        scheduledTaskWrapper.NextRuntTime = referenceTime.AddSeconds(10);
                    }
                    else
                    {
                        scheduledTaskWrapper.NextRuntTime = referenceTime;
                    }
                }

                scheduledTaskWrapper.Order = scheduledTask.Order == 0 ? int.MaxValue : scheduledTask.Order;

                _scheduledTasks.Add(scheduledTaskWrapper);
            }
        }

        internal async Task StartAsync(CancellationToken stoppingToken)
        {
            LogEvent?.Invoke(this, $"--- {LogPrefix} {nameof(StartAsync)} ---");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ExecuteOnceAsync(CancellationToken stoppingToken)
        {
            var referenceTime = DateTime.UtcNow;

            var tasksThatShouldRun = _scheduledTasks.Where(a => a.ShouldRun(referenceTime)).OrderBy(a => a.Order);

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                LogEvent?.Invoke(this, $"--- {LogPrefix} Run {taskThatShouldRun.Task} ---");
            
                taskThatShouldRun.Increment();

                await Task.Run(async () =>
                {
                    try
                    {
                        if (taskThatShouldRun.Task.Delay != 0)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(taskThatShouldRun.Task.Delay), stoppingToken);
                        }
                        
                        await taskThatShouldRun.Task.ExecuteAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        LogEvent?.Invoke(this, ex.ToString());
                        
                        var args = new UnobservedTaskExceptionEventArgs(ex as AggregateException ?? new AggregateException(ex));

                        UnobservedTaskExceptionEvent?.Invoke(this, args);

                        if (!args.Observed)
                        {
                            throw;
                        }
                    }
                }, stoppingToken);
            }
        }

        private IEnumerable<IScheduledTask> ScheduledTasks(string prefixAssemblyName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic);

            var types = assemblies.Where(a => a.FullName.StartsWith(prefixAssemblyName))
                                  .SelectMany(a => a.DefinedTypes.Where(t => t.GetInterfaces().Contains(typeof(IScheduledTask))));

            return types.Select(Activator.CreateInstance).Cast<IScheduledTask>();
        }
    }
}