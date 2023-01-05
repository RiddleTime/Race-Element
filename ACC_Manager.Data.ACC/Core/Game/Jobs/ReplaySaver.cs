using ACCManager.Data.ACC.Core.Game;
using Quartz;
using RaceElement.Data.ACC.Core.Config;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RaceElement.Data.ACC.Core.Game.Jobs
{
    public class ReplaySaver : IJob
    {
        public static readonly JobKey JobKey = new JobKey("replay-saver", "acc-jobs");
        public static readonly TriggerKey TriggerKey = new TriggerKey("replaySaverTrigger");

        public async Task Execute(IJobExecutionContext context)
        {
            var replayJson = new ReplaySettings().Get();

            if (replayJson.AutoSaveEnabled == 1)
            {
                Debug.WriteLine("Auto save is enabled");
            }
            else
            {
                await context.Scheduler.UnscheduleJob(TriggerKey);
                Debug.WriteLine("Auto save is not enabled, unscheduled replay saver");
            }
        }

        public static void Schedule()
        {
            IJobDetail job = JobBuilder.Create<ReplaySaver>().WithIdentity(JobKey).Build();
            AccScheduler.Scheduler.ScheduleJob(job, TriggerBuilder.Create()
                        .WithIdentity(TriggerKey)
                        .WithSimpleSchedule(x => x.WithInterval(new TimeSpan(0, 0, 5)).RepeatForever()).ForJob(JobKey).Build());
        }
    }
}
