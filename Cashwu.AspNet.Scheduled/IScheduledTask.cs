using System.Threading;
using System.Threading.Tasks;

namespace Cashwu.AspNet.Scheduled
{
    public interface IScheduledTask
    {
        /// <summary>
        /// Cron job 
        /// </summary>
        string Schedule { get; }

        /// <summary>
        /// Set Lazy = false, will application start run
        /// </summary>
        bool IsLazy { get; }

        /// <summary>
        /// Job content
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}