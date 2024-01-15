using ACCManager.Data.ACC.Core.Game;
using RaceElement.Core.Jobs;
using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.ACC.Core.Config;
using RaceElement.Data.ACC.HotKey;
using RaceElement.Util.Settings;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RaceElement.Data.ACC.Core.Game.Jobs;

public class ReplaySaver : AbstractLoopJob
{
	private static DateTime LastReplaySave = DateTime.MinValue;

	public ReplaySaver()
	{
		IntervalMillis = 15000;
	}

	public override void RunAction()
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
				Debug.WriteLine("Auto save is not enabled, unscheduled replay saver");
			}
		}
	}
}
