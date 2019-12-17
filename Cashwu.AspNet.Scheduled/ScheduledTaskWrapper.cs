using System;
using NCrontab;

namespace Cashwu.AspNet.Scheduled
{
    internal class ScheduledTaskWrapper
    {
        private bool _firstRunEnd;

        public CrontabSchedule Schedule { get; set; }

        public IScheduledTask Task { get; set; }

        public DateTime NextRuntTime { get; set; }

        public DateTime LastRunTime { get; set; }

        public bool ShouldApplicationStartedRun => Task.IsLazy == false && _firstRunEnd == false;

        public void Increment()
        {
            LastRunTime = NextRuntTime;
            NextRuntTime = Schedule.GetNextOccurrence(NextRuntTime);

            if (ShouldApplicationStartedRun)
            {
                _firstRunEnd = true;
            }
        }

        public bool ShouldRun(DateTime currentTime)
        {
            return (NextRuntTime < currentTime && LastRunTime != NextRuntTime) || ShouldApplicationStartedRun;
        }
    }
}