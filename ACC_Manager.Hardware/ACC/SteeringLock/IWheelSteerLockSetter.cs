namespace RaceElement.Hardware.ACC.SteeringLock
{
    internal interface IWheelSteerLockSetter
    {
        string ControllerName { get; }

        bool Test(string productGuid);
        bool Apply(int angle, bool isReset, out int appliedValue);

        int MaximumSteerLock { get; }
        int MinimumSteerLock { get; }
    }
}
