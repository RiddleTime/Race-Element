using RaceElement.Data.ACC.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.ACC.Core.Game.Tasks
{
    internal class ReplaySaver : LoopTask
    {
        private ReplaySettings _replaySettings;

        public override bool Init()
        {
            _replaySettings = new ReplaySettings();
            var replayJson = _replaySettings.Get(false);
            int replaySaveIntervals = replayJson.AutoSaveMinTimeSeconds;


            return true;
        }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class LoopTask
    {
        public int IntervalMilliseconds { get; set; } = 5000;

        /// <summary>
        /// Called before this task initially starts running.
        /// 
        /// Use this to instantiate your LoopTask properties.
        /// </summary>
        /// <returns>true if you want to start running, false to exit.</returns>
        public abstract bool Init();

        /// <summary>
        /// Called after the set interval expires.
        /// </summary>
        public abstract void Run();

    }

    public static class LoopTaskManager
    {
        private static List<LoopTask> _loopTasks = new List<LoopTask>();

        private static void AddTask(LoopTask task)
        {
            _loopTasks.Add(task);
        }
    }
}
