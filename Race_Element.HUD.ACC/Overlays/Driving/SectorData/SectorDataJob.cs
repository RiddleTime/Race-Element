﻿using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using System;

namespace RaceElement.HUD.ACC.Overlays.Driving.SectorData
{
    internal class SectorDataModel
    {
        /// <summary>
        /// 0 indexed
        /// </summary>
        public required int SectorIndex { get; set; }

        public required float SpeedMin { get; set; }
        public required float SpeedMax { get; set; }
    }

    internal class SectorDataJob : AbstractLoopJob
    {
        private int _lastSectorIndex = -1;

        private SectorDataModel _currentData;

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
                    SpeedMin = physics.SpeedKmh,
                    SpeedMax = physics.SpeedKmh
                };
            }
            else
            {
                if (_currentData.SpeedMax < physics.SpeedKmh) _currentData.SpeedMax = physics.SpeedKmh;
                if (_currentData.SpeedMin > physics.SpeedKmh) _currentData.SpeedMin = physics.SpeedKmh;
            }

            _lastSectorIndex = graphics.CurrentSectorIndex;
        }

    }
}