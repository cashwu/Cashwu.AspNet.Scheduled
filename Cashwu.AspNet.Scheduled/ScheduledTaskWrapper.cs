using System;
using NCrontab;

namespace Cashwu.AspNet.Scheduled
{
    internal class ScheduledTaskWrapper
    {
        private bool _firstRun = true;
        
        public CrontabSchedule Schedule { get; set;  }

        public IScheduledTask Task { get; set; }

        public DateTime NextRuntTime { get; set; }
        
        public DateTime BaseTime { get; set; }

        public int Order { get; set; }

        public void Increment()
        {
            if (Schedule == null)
            {
                NextRuntTime = DateTime.MaxValue;
            }
            else
            {
                NextRuntTime = Schedule.GetNextOccurrence(_firstRun ? BaseTime : NextRuntTime);
            }

            if (_firstRun)
            {
                _firstRun = false;
            }
        }

        public bool ShouldRun(DateTime currentTime)
        {
            return currentTime > NextRuntTime;
        }
    }
}