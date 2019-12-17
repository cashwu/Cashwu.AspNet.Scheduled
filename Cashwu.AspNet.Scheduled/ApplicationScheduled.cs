using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cashwu.AspNet.Scheduled
{
    public static class ApplicationScheduled
    {
        public static void Start(string prefixAssemblyName, EventHandler<UnobservedTaskExceptionEventArgs> unobservedTaskExceptionHandler = null)
        {
            Task.Run(async () =>
            {
                var schedulerHostedService = new SchedulerHostedService(prefixAssemblyName);

                if (unobservedTaskExceptionHandler != null)
                {
                    schedulerHostedService.UnobservedTaskException += unobservedTaskExceptionHandler;
                }

                await schedulerHostedService.StartAsync(CancellationToken.None);
            });
        }
    }
}