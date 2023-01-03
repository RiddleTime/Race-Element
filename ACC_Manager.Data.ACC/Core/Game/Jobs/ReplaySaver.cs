using Quartz;
using RaceElement.Data.ACC.Core.Config;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RaceElement.Data.ACC.Core.Game.Jobs
{
    public class ReplaySaver : IJob
    {
        public static readonly JobKey Key = new JobKey("replay-saver", "acc-jobs");

        private ReplaySettings _replaySettings;

        public ReplaySaver()
        {
            _replaySettings = new ReplaySettings();
            var replayJson = _replaySettings.Get(false);
            int replaySaveIntervals = replayJson.AutoSaveMinTimeSeconds;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            Debug.WriteLine("-------------------            Replay Saver is executing");
        }
    }
}
