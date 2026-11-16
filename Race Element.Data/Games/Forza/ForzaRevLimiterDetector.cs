using RaceElement.Data.Games.Forza.ForzaUDP;

namespace RaceElement.Data.Games.Forza;

/// <summary>
/// Forza reports EngineMaxRpm as the end of the tachometer scale, not as the rev limiter,
/// and the limiter moves with engine upgrades and swaps. It is therefore learned from the
/// telemetry stream instead of being looked up per car, and kept in
/// <see cref="ForzaRevLimiterCache"/> so a known engine does not have to be learned again.
/// </summary>
internal sealed class ForzaRevLimiterDetector
{
    /// <summary>Throttle required before a sample is considered, 0 to 1.</summary>
    private const float ThrottleThreshold = 0.98f;

    /// <summary>Fraction of EngineMaxRpm below which samples are ignored.</summary>
    private const float MinRpmFraction = 0.5f;

    /// <summary>Time after a gear change during which rpm is still settling.</summary>
    private const uint GearSettleMs = 250;

    /// <summary>Samples further apart than this are treated as a gap in the stream.</summary>
    private const uint MaxSampleGapMs = 100;

    /// <summary>How long the peak rpm has to stand still before it counts as the limiter.</summary>
    private const uint PlateauHoldMs = 200;

    /// <summary>
    /// Rpm the peak has to gain to count as the engine still pulling. Wide enough to sit
    /// above the rpm oscillation of an engine bouncing off its limiter, which would
    /// otherwise restart the hold over and over.
    /// </summary>
    private const int PeakClimbRpm = 100;

    /// <summary>
    /// Metres per second the car has to gain while the rpm stands still. At the limiter the
    /// car keeps accelerating, at terminal speed in the top gear it does not, and both look
    /// the same to the rpm alone.
    /// </summary>
    private const float MinStallSpeedGain = 0.25f;

    /// <summary>
    /// Fraction of EngineMaxRpm the engine has to climb within one pull. A car under game
    /// control, on a rolling start or on a formation lap, sits at full throttle at near
    /// constant rpm, which otherwise reads exactly like a limiter.
    /// </summary>
    private const float MinPullClimbFraction = 0.05f;

    /// <summary>
    /// Torque the engine has to fall to while the rpm stands still. Forza cuts fuel at the
    /// limiter, so the reported torque goes negative there, while a car the game is driving
    /// itself holds a large positive torque at near constant rpm.
    /// </summary>
    private const float MaxLimiterTorque = 0f;

    /// <summary>How recent the fuel cut has to be, independent of the rpm hold window.</summary>
    private const uint TorqueCutWindowMs = 500;

    /// <summary>A plateau below this fraction of the stored value may mean the engine changed.</summary>
    private const float LowerTolerance = 0.985f;

    /// <summary>Separate pulls that must agree before a stored value is lowered.</summary>
    private const int LowerConfirmations = 3;

    private readonly ForzaRevLimiterCache _cache = new();
    private readonly ForzaRevLimiterCacheJson _storedLimiters;

    private string _carKey = string.Empty;
    private bool _wasRaceOn;

    private uint _lastTimestampMs;
    private uint _lastGearChangeMs;
    private int _lastGear;
    private int _pullCount;

    private uint _peakRaisedMs;
    private int _peakRaisedRpm;
    private float _peakRaisedSpeed;
    private uint _torqueCutMs;
    private bool _torqueCutSeen;
    private int _pullPeakRpm;
    private int _pullFloorRpm;

    private int _confirmedRpm;
    private int _lowerCandidateRpm;
    private int _lowerHits;
    private int _lowerHitPull = -1;

    /// <summary>
    /// Zero until the engine has been held against the limiter at least once,
    /// either in this session or in an earlier one.
    /// </summary>
    public int RevLimiterRpm => _confirmedRpm;

    public ForzaRevLimiterDetector()
    {
        _storedLimiters = _cache.Get(false) ?? _cache.Default();
        _storedLimiters.RevLimiters ??= [];
    }

    public void Update(in ForzaMotorsportsData.SledData sled, in ForzaMotorsportsData.DashData dash)
    {
        // EngineMaxRpm is part of the key because it follows the engine rather than the
        // chassis, so tuning that leaves the engine alone keeps using the known limiter.
        // The title is in there as well: the same car carries the same ordinal in Horizon
        // and in Motorsport, but the limiter does not have to match.
        string carKey = ForzaRevLimiterCache.BuildKey(GameManager.CurrentGame, sled.CarOrdinal, (int)sled.EngineMaxRpm);
        bool raceOn = sled.IsRaceOn > 0;

        if (carKey != _carKey)
        {
            _carKey = carKey;
            _confirmedRpm = _storedLimiters.RevLimiters.TryGetValue(carKey, out int storedRpm) ? storedRpm : 0;
            ResetStint();
        }

        // A return to the track can mean a new tune, but an engine change moves the key,
        // so only the per stint state is dropped here.
        if (raceOn && !_wasRaceOn) ResetStint();

        _wasRaceOn = raceOn;
        if (!raceOn) return;

        int rpm = (int)sled.CurrentEngineRpm;
        uint now = sled.TimestampMs;

        if (dash.Gear != _lastGear)
        {
            _lastGear = dash.Gear;
            _lastGearChangeMs = now;
        }

        uint elapsed = now - _lastTimestampMs;

        // The same physics frame can arrive more than once. Such a packet carries no new
        // information, so it is skipped without touching the plateau state: resetting on it
        // would clear the band every other frame and no plateau would ever be held.
        if (elapsed == 0) return;
        _lastTimestampMs = now;

        // The gear is deliberately not part of this condition: its encoding differs between
        // the Forza titles, and full throttle against the limiter reads the same rpm in any
        // gear, in neutral included.
        bool sampleUsable = dash.Accelerator / 255f >= ThrottleThreshold
            && rpm >= sled.EngineMaxRpm * MinRpmFraction
            && now - _lastGearChangeMs >= GearSettleMs
            && elapsed <= MaxSampleGapMs;

        if (sampleUsable)
            TrackPlateau(rpm, now, sled.EngineMaxRpm, dash.Speed, dash.Torque);
        else
            StartPull(rpm, now, dash.Speed);
    }

    private void TrackPlateau(int rpm, uint now, float engineMaxRpm, float speed, float torque)
    {
        // What identifies the limiter is that the engine stops gaining rpm while the throttle
        // stays pinned, so only upward moves are timed and oscillation around the limiter is
        // ignored. The comparison is against the rpm of the last raise rather than against
        // the running peak: a peak that creeps up with the engine would never clear the
        // threshold, and a steady climb would read as a stall.
        if (rpm > _peakRaisedRpm + PeakClimbRpm)
        {
            _peakRaisedRpm = rpm;
            _peakRaisedMs = now;
            _peakRaisedSpeed = speed;
        }

        // Tracked by time rather than within the hold window: the rpm of an engine on its
        // limiter wanders, and a reset on every wander would lose the cut that proves it.
        if (torque <= MaxLimiterTorque)
        {
            _torqueCutMs = now;
            _torqueCutSeen = true;
        }

        if (rpm > _pullPeakRpm) _pullPeakRpm = rpm;
        if (rpm < _pullFloorRpm) _pullFloorRpm = rpm;

        if (now - _peakRaisedMs < PlateauHoldMs) return;

        // The engine has to have climbed to this point. Without that, the near constant rpm
        // of a car the game is driving itself reads as a limiter.
        if (_pullPeakRpm - _pullFloorRpm < engineMaxRpm * MinPullClimbFraction) return;

        // Terminal speed in the top gear stalls the rpm as well, at an rpm below the limiter.
        // Against the limiter the car is still pulling away, so that is what separates them.
        if (speed - _peakRaisedSpeed < MinStallSpeedGain) return;

        // And the engine has to have been cut recently, which is what the limiter does and
        // what a game controlled car at steady rpm never does.
        if (!_torqueCutSeen || now - _torqueCutMs > TorqueCutWindowMs) return;

        RegisterPlateau(_pullPeakRpm);
    }

    /// <summary>
    /// Starts a new pull. Called whenever the throttle, the gear or the stream breaks the
    /// current one, which is what keeps the rpm spike of a downshift out of the result.
    /// </summary>
    private void StartPull(int rpm, uint now, float speed)
    {
        _pullCount++;
        _peakRaisedRpm = rpm;
        _peakRaisedSpeed = speed;
        _pullPeakRpm = rpm;
        _pullFloorRpm = rpm;
        _peakRaisedMs = now;
    }

    private void RegisterPlateau(int candidate)
    {
        // Anything above what is known is the limiter by definition, the engine reached it.
        if (candidate > _confirmedRpm)
        {
            Store(candidate);
            return;
        }

        if (candidate >= _confirmedRpm * LowerTolerance) return;

        // Terminal speed in the top gear produces the same plateau below the limiter, so a
        // lower value is only accepted after several pulls agree on it. One pull can raise
        // the value but not lower it.
        if (_pullCount == _lowerHitPull) return;
        _lowerHitPull = _pullCount;

        if (Math.Abs(candidate - _lowerCandidateRpm) > _lowerCandidateRpm * (1f - LowerTolerance))
        {
            _lowerCandidateRpm = candidate;
            _lowerHits = 1;
            return;
        }

        _lowerCandidateRpm = Math.Max(_lowerCandidateRpm, candidate);
        if (++_lowerHits >= LowerConfirmations)
            Store(_lowerCandidateRpm);
    }

    private void Store(int rpm)
    {
        _confirmedRpm = rpm;
        _lowerCandidateRpm = 0;
        _lowerHits = 0;

        if (_storedLimiters.RevLimiters.TryGetValue(_carKey, out int storedRpm) && storedRpm == rpm)
            return;

        _storedLimiters.RevLimiters[_carKey] = rpm;
        _cache.Save(_storedLimiters);
    }

    private void ResetStint()
    {
        _lowerCandidateRpm = 0;
        _lowerHits = 0;
        _lowerHitPull = -1;
        _lastGear = 0;
        _lastTimestampMs = 0;
        _lastGearChangeMs = 0;
        _peakRaisedMs = 0;
        _peakRaisedRpm = 0;
        _peakRaisedSpeed = 0;
        _torqueCutMs = 0;
        _torqueCutSeen = false;
        _pullPeakRpm = 0;
        _pullFloorRpm = 0;
    }
}
