using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using System;

namespace RaceElement.HUD.ACC.Overlays.Driving.SectorData
{
    internal sealed record SectorDataModel
    {
        /// <summary>
        /// 0 indexed
        /// </summary>
        public required int SectorIndex { get; init; }

        public required float VelocityMin { get; set; }
        public required float VelocityMax { get; set; }
    }

    internal sealed class SectorDataJob : AbstractLoopJob
    {
        private int _lastSectorIndex = -1;

        internal SectorDataModel _currentData;

        public EventHandler<SectorDataModel> OnSectorCompleted;

        public SectorDataJob() => RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

        private void Instance_OnNewSessionStarted(object sender, DbRaceSession e) => _currentData = null;

        public override void AfterCancel() => RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;

        public override void RunAction()
        {
            var physics = ACCSharedMemory.Instance.ReadPhysicsPageFile(true);
            var graphics = ACCSharedMemory.Instance.ReadGraphicsPageFile(true);

            if (graphics.Status != ACCSharedMemory.AcStatus.AC_LIVE)
                return;

            if (_lastSectorIndex != graphics.CurrentSectorIndex || _currentData == null)
            {
                if (_currentData != null)
                    OnSectorCompleted.Invoke(this, _currentData);

                _currentData = new SectorDataModel()
                {
                    SectorIndex = graphics.CurrentSectorIndex,
                    VelocityMin = physics.SpeedKmh,
                    VelocityMax = physics.SpeedKmh
                };
            }
            else
            {
                if (_currentData.VelocityMax < physics.SpeedKmh) _currentData.VelocityMax = physics.SpeedKmh;
                if (_currentData.VelocityMin > physics.SpeedKmh) _currentData.VelocityMin = physics.SpeedKmh;
            }

            _lastSectorIndex = graphics.CurrentSectorIndex;
        }

    }
}
