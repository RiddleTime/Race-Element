using Device.Net;
using DualSenseAPI.State;
using DualSenseAPI.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DualSenseAPI
{
    /// <summary>
    /// Interaction logic for DualSense controllers.
    /// </summary>
    public class DualSense
    {
        // IO parameters
        private readonly IDevice underlyingDevice;
        private readonly int? readBufferSize;
        private readonly int? writeBufferSize;


        public bool IsPolling { get; private set; }

        // async polling
        private IDisposable? pollerSubscription;

        /// <summary>
        /// State event handler for asynchronous polling.
        /// </summary>
        /// <seealso cref="BeginPolling(uint)"/>
        /// <seealso cref="EndPolling"/>
        public event StatePolledHandler? OnStatePolled;

        /// <summary>
        /// Button state changed event handler for asynchronous polling.
        /// </summary>
        /// <seealso cref="BeginPolling(uint)"/>
        /// <seealso cref="EndPolling"/>
        public event ButtonStateChangedHandler? OnButtonStateChanged;

        /// <summary>
        /// The I/O mode the controller is connected by.
        /// </summary>
        public IoMode IoMode { get; private set; }

        /// <summary>
        /// Configurable dead zone for gamepad joysticks. A joystick axis with magnitude less than this value will
        /// be returned as 0.
        /// </summary>
        public float JoystickDeadZone { get; set; } = 0;

        /// <summary>
        /// This controller's output state.
        /// </summary>
        public DualSenseOutputState OutputState { get; set; } = new DualSenseOutputState();

        /// <summary>
        /// This controller's most recently polled input state.
        /// </summary>
        public DualSenseInputState InputState { get; private set; } = new DualSenseInputState();

        /// <summary>
        /// Private constructor for <see cref="EnumerateControllers"/>.
        /// </summary>
        /// <param name="underlyingDevice">The underlying low-level device.</param>
        /// <param name="readBufferSize">The device's declared read buffer size.</param>
        /// <param name="writeBufferSize">The device's declared write buffer size.</param>
        private DualSense(IDevice underlyingDevice, int? readBufferSize, int? writeBufferSize)
        {
            this.underlyingDevice = underlyingDevice;
            this.readBufferSize = readBufferSize;
            this.writeBufferSize = writeBufferSize;
            IoMode = readBufferSize switch
            {
                64 => IoMode.USB,
                78 => IoMode.Bluetooth,
                _ => IoMode.Unknown
            };
            if (IoMode == IoMode.Unknown)
            {
                throw new InvalidOperationException("Can't initialize device - supported IO modes are USB and Bluetooth.");
            }
        }

        /// <summary>
        /// Acquires the controller.
        /// </summary>
        public void Acquire()
        {
            if (!underlyingDevice.IsInitialized)
            {
                underlyingDevice.InitializeAsync().Wait();
            }
        }

        /// <summary>
        /// Releases the controller.
        /// </summary>
        public void Release()
        {
            if (underlyingDevice.IsInitialized)
            {
                underlyingDevice?.Close();
                underlyingDevice?.Dispose();
            }
        }

        private async Task<DualSenseInputState> ReadWriteOnceAsync()
        {
            TransferResult result = await underlyingDevice.WriteAndReadAsync(GetOutputDataBytes());
            if (result.BytesTransferred == readBufferSize)
            {
                // this can effectively determine which input packet you've recieved, USB or bluetooth, and offset by the right amount
                int offset = result.Data[0] switch
                {
                    0x01 => 1, // USB packet flag
                    0x31 => 2, // Bluetooth packet flag
                    _ => 0
                };
                return new DualSenseInputState(result.Data.Skip(offset).ToArray(), IoMode, JoystickDeadZone);
            }
            else
            {
                throw new IOException("Failed to read data - buffer size mismatch");
            }
        }

        /// <summary>
        /// Updates the input and output states once. This operation is blocking.
        /// </summary>
        /// <returns>The polled state, for convenience. This is also updated on the controller instance.</returns>
        public DualSenseInputState ReadWriteOnce()
        {
            Task<DualSenseInputState> stateTask = ReadWriteOnceAsync();
            stateTask.Wait();
            InputState = stateTask.Result;
            return InputState;
        }

        /// <summary>
        /// Process a state event. Wraps around user-provided handler since Reactive needs an Action<>.
        /// </summary>
        /// <param name="nextState">The receieved input state</param>
        private void ProcessEachState(DualSenseInputState nextState)
        {
            DualSenseInputState prevState = InputState;
            InputState = nextState;
            // don't take up the burden to diff the changes unless someone cares
            if (OnButtonStateChanged != null)
            {
                DualSenseInputStateButtonDelta delta = new DualSenseInputStateButtonDelta(prevState, nextState);
                if (delta.HasChanges)
                {
                    OnButtonStateChanged.Invoke(this, delta);
                }
            }
            OnStatePolled?.Invoke(this);
        }

        /// <summary>
        /// Begins asynchously updating the output state and polling the input state at the specified interval.
        /// </summary>
        /// <param name="pollingIntervalMs">How long to wait between each I/O loop, in milliseconds</param>
        /// <remarks>
        /// Instance state is not thread safe. In other words, when using polling, updating instance state 
        /// (such as <see cref="OutputState"/>) both inside and outside of <see cref="OnStatePolled"/>
        /// may create unexpected results. When using polling, it is generally expected you will only make
        /// modifications to state inside the <see cref="OnStatePolled"/> handler in response to input, or
        /// outside of the handler in response to external events (for example, game logic). It's also
        /// expected that you will only use the <see cref="DualSense"/> instance passed as an argument to 
        /// the sender, rather than external references to instance.
        /// </remarks>
        public void BeginPolling(uint pollingIntervalMs)
        {
            if (pollerSubscription != null)
            {
                throw new InvalidOperationException("Can't begin polling after it's already started.");
            }

            IObservable<DualSenseInputState> stateObserver = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(pollingIntervalMs))
                .SelectMany(Observable.FromAsync(() => ReadWriteOnceAsync()));
            // TODO: figure how we can leverage DistinctUntilChanged (or similar) so we can do filtered eventing (e.g. button pressed only)
            // how would we allow both to modify state in a smart way (i.e. without overriding each other?) if needed?
            // this also applies for consumers - logically they should not need to worry about race conditions if they're subscribing to both

            pollerSubscription = stateObserver.Subscribe(ProcessEachState);
        }

        /// <summary>
        /// Stop asynchronously updating the output state and polling for new inputs.
        /// </summary>
        public void EndPolling()
        {
            if (pollerSubscription == null)
            {
                throw new InvalidOperationException("Can't end polling without starting polling first");
            }
            pollerSubscription.Dispose();
            pollerSubscription = null;
        }

        /// <summary>
        /// Builds the output byte array that will be sent to the controller.
        /// </summary>
        /// <returns>An array of bytes to send to the controller</returns>
        private byte[] GetOutputDataBytes()
        {
            byte[] bytes = new byte[writeBufferSize ?? 0];
            byte[] hidBuffer = OutputState.BuildHidOutputBuffer();
            if (IoMode == IoMode.USB)
            {
                bytes[0] = 0x02;
                Array.Copy(hidBuffer, 0, bytes, 1, 47);
            }
            else if (IoMode == IoMode.Bluetooth)
            {
                bytes[0] = 0x31;
                bytes[1] = 0x02;
                Array.Copy(hidBuffer, 0, bytes, 2, 47);
                // make a 32 bit checksum of the first 74 bytes and add it at the end
                uint crcChecksum = CRC32Utils.ComputeCRC32(bytes, 74);
                byte[] checksumBytes = BitConverter.GetBytes(crcChecksum);
                Array.Copy(checksumBytes, 0, bytes, 74, 4);
            }
            else
            {
                throw new InvalidOperationException("Can't send data - supported IO modes are USB and Bluetooth.");
            }
            return bytes;
        }

        public override string ToString()
        {
            return $"DualSense Controller ({IoMode})";
        }

        /// <summary>
        /// Enumerates available controllers.
        /// </summary>
        /// <returns>Enumerable of available controllers.</returns>
        public static IEnumerable<DualSense> EnumerateControllers()
        {
            foreach (ConnectedDeviceDefinition deviceDefinition in HidScanner.Instance.ListDevices())
            {
                IDevice device = HidScanner.Instance.GetConnectedDevice(deviceDefinition);
                yield return new DualSense(device, deviceDefinition.ReadBufferSize, deviceDefinition.WriteBufferSize);
            }
        }
    }
}
