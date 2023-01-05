using ACCManager.Data.ACC.Core.Game;
using Quartz;
using RaceElement.Data.ACC.Core.Game.Jobs;
using System;
using System.Threading.Tasks;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.Data.ACC.Tracker
{
    public class PageStaticTracker : IJob
    {
        public static readonly JobKey JobKey = new JobKey("PageStaticTracker", "acc-jobs");
        public static readonly TriggerKey TriggerKey = new TriggerKey("PageStaticTracker");

        public static event EventHandler<SPageFileStatic> Tracker;

        public async Task Execute(IJobExecutionContext context)
        {
            Tracker?.Invoke(null, ACCSharedMemory.Instance.ReadStaticPageFile());
        }

        public static void Schedule()
        {
            IJobDetail job = JobBuilder.Create<PageStaticTracker>().WithIdentity(JobKey).Build();
            AccScheduler.Scheduler.ScheduleJob(job, TriggerBuilder.Create()
                        .WithIdentity(TriggerKey)
                        .WithSimpleSchedule(x => x.WithInterval(new TimeSpan(0, 0, 0, 0, 500)).RepeatForever()).ForJob(JobKey).Build());
        }
    }
}
