using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Cashwu.AspNet.Scheduled
{
    public static class ApplicationScheduled
    {
        public static void Start(string prefixAssemblyName, EventHandler<UnobservedTaskExceptionEventArgs> unobservedTaskExceptionHandler = null, EventHandler<string> logHandler = null)
        {
            Task.Run(async () =>
            {
                try
                {
                    var schedulerHostedService = new SchedulerHostedService(prefixAssemblyName);

                    if (unobservedTaskExceptionHandler != null)
                    {
                        schedulerHostedService.UnobservedTaskExceptionEvent += unobservedTaskExceptionHandler;
                    }

                    if (logHandler != null)
                    {
                        schedulerHostedService.LogEvent += logHandler;
                    }

                    await schedulerHostedService.StartAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                    throw;
                }
            });
        }
    }
}