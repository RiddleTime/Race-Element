using ACCManager.Data.ACC.Core.Game;
using RaceElement.Core.Jobs;
using RaceElement.Data.ACC.Core.Config;
using RaceElement.Data.ACC.HotKey;
using RaceElement.Util.Settings;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RaceElement.Data.ACC.Core.Game.Jobs
{
    public class ReplaySaver : AbstractJob
    {
        public static readonly JobKey JobKey = new JobKey("replay-saver", "acc-jobs");
        public static readonly TriggerKey TriggerKey = new TriggerKey("replaySaverTrigger");

        private static DateTime LastReplaySave = DateTime.MinValue;

        public async Task Execute(IJobExecutionContext context)
        {
            if (AccProcess.IsRunning)
            {
                var replayJson = new ReplaySettings().Get();
                var accSettings = new AccSettings().Get();

                if (accSettings.AutoRecordReplay && replayJson.AutoSaveEnabled == 1)
                {
                    if (ACCSharedMemory.Instance.ReadGraphicsPageFile().Status == ACCSharedMemory.AcStatus.AC_LIVE)
                    {
                        if (DateTime.UtcNow.Subtract(LastReplaySave) > new TimeSpan(0, 0, replayJson.MaxTimeReplaySeconds))
                        {
                            Debug.WriteLine("Auto save is enabled");
                            LastReplaySave = AccHotkeys.SaveReplay();

                            Debug.WriteLine(LastReplaySave);
                        }
                    }
                }
                else
                {
                    await context.Scheduler.UnscheduleJob(TriggerKey);
                    Debug.WriteLine("Auto save is not enabled, unscheduled replay saver");
                }
            }
        }

        public static void Schedule()
        {
            //IJobDetail job = JobBuilder.Create<ReplaySaver>().WithIdentity(JobKey).Build();
            //if (!AccScheduler.Scheduler.CheckExists(JobKey).Result)
            //    AccScheduler.Scheduler.ScheduleJob(job, TriggerBuilder.Create()
            //                .WithIdentity(TriggerKey)
            //                .WithSimpleSchedule(x => x.WithInterval(new TimeSpan(0, 0, 15)).RepeatForever()).ForJob(JobKey).Build());
        }

        public override JobStatus Initiate()
        {
            throw new NotImplementedException();
        }

        public override JobStatus Execute()
        {
            throw new NotImplementedException();
        }

        public override void Complete()
        {
            throw new NotImplementedException();
        }
    }
}
