using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NCrontab;

namespace Cashwu.AspNet.Scheduled
{
    internal class SchedulerHostedService
    {
        private readonly List<ScheduledTaskWrapper> _scheduledTasks = new List<ScheduledTaskWrapper>();

        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        internal SchedulerHostedService(string prefixAssemblyName)
        {
            var referenceTime = DateTime.UtcNow;

            foreach (var scheduledTask in ScheduledTasks(prefixAssemblyName))
            {
                var scheduledTaskWrapper = new ScheduledTaskWrapper
                {
                    Schedule = CrontabSchedule.Parse(scheduledTask.Schedule),
                    Task = scheduledTask
                };

                scheduledTaskWrapper.NextRuntTime = scheduledTaskWrapper.Schedule.GetNextOccurrence(referenceTime);

                _scheduledTasks.Add(scheduledTaskWrapper);
            }
        }

        internal async Task StartAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ExecuteOnceAsync(CancellationToken stoppingToken)
        {
            var taskFactory = new TaskFactory(TaskScheduler.Current);
            var referenceTime = DateTime.UtcNow;

            var tasksThatShouldRun = _scheduledTasks.Where(a => a.ShouldRun(referenceTime)).ToList();

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();

                await taskFactory.StartNew(async () =>
                {
                    try
                    {
                        await taskThatShouldRun.Task.ExecuteAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        var args = new UnobservedTaskExceptionEventArgs(ex as AggregateException ?? new AggregateException(ex));

                        UnobservedTaskException?.Invoke(this, args);

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