using BluetoothModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLE
{
    public class DnaBluetoothLEAdvertisementWatcher
    {

        #region Private

        private readonly BluetoothLEAdvertisementWatcher watcher;
        private readonly Dictionary<string, DnaBluetoothLEDevice> mDiscoveredDevices = new Dictionary<string, DnaBluetoothLEDevice>();
        private readonly object mThreadLock = new object();
        
        #endregion

        #region Public
        public int HeartbeatTimeout { get; set; } = 30;
        public bool Listening => watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;
       
        #endregion

        #region PublicEvents

        public event Action StoppedListening = () => { };
        public event Action StartedListening = () => { };
        public event Action<DnaBluetoothLEDevice> NewDeviceDiscovered = (device) => { };
        public event Action<DnaBluetoothLEDevice> DeviceDiscovered = (device) => { };
        public event Action<DnaBluetoothLEDevice> DeviceNameChanged = (device) => { };
        public event Action<DnaBluetoothLEDevice> DeviceTimeout = (device) => { };

        #endregion

        

        public IReadOnlyCollection<DnaBluetoothLEDevice> DiscoveredDevices
        {
            get
            {
                CleanupTimeouts();
                lock (mThreadLock)
                {
                    return mDiscoveredDevices.Values.ToList().AsReadOnly();
                }
            }
        }

        public DnaBluetoothLEAdvertisementWatcher()
        {
            watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            watcher.Received += WatcherAdvertisementReceivedAsync;

            watcher.Stopped += (watcher, e) =>
            {
                StoppedListening();
            };
        }

        private async void WatcherAdvertisementReceivedAsync(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            CleanupTimeouts();
            var device = await GetBluetoothLEDeviceAsync(args.BluetoothAddress,
                args.Timestamp,
                args.RawSignalStrengthInDBm);

            if (device == null)
                return;

            // Is new discovery?
            var newDiscovery = false;
            var existingName = default(string);
            var nameChanged = false;


            lock (mThreadLock)
            {
                // Check if this is a new discovery
                newDiscovery = !mDiscoveredDevices.ContainsKey(device.DeviceId);

                // If this is not new...
                if (!newDiscovery)
                    // Store the old name
                    existingName = mDiscoveredDevices[device.DeviceId].Name;

                // Name changed?
                nameChanged =
                    // If it already exists
                    !newDiscovery &&
                    // And is not a blank  name
                    !string.IsNullOrEmpty(device.Name) &&
                    // And the name is different
                    existingName != device.Name;

                // If we are no longer listening...
                if (!Listening)
                    // Don't bother adding to the list and do nothing
                    return;

                // Add/update the device in the dictionary
                mDiscoveredDevices[device.DeviceId] = device;

            }
            DeviceDiscovered(device);

            if (nameChanged)
                DeviceNameChanged(device);

            if (newDiscovery)
                NewDeviceDiscovered(device);

        }

        private async Task<DnaBluetoothLEDevice> GetBluetoothLEDeviceAsync(ulong address, DateTimeOffset broadcastTime, short rssi)
        {
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address, BluetoothAddressType.Random);
            if (device == null)
                return null;

            // Return the new device information
            return new DnaBluetoothLEDevice
            (
                // Device Id
                deviceId: device.DeviceId,
                // Bluetooth Address
                address: device.BluetoothAddress,
                // Device Name
                name: device.Name,
                // Broadcast Time
                broadcastTime: broadcastTime,
                // Signal Strength
                rssi: rssi,
                // Is Connected?
                connected: device.ConnectionStatus == BluetoothConnectionStatus.Connected,
                // Can Pair?
                canPair: device.DeviceInformation.Pairing.CanPair,
                // Is Paired?
                paired: device.DeviceInformation.Pairing.IsPaired
            );
        }

        private void CleanupTimeouts()
        {
            lock (mThreadLock)
            {
                var threshold = DateTime.UtcNow - TimeSpan.FromSeconds(HeartbeatTimeout);

                mDiscoveredDevices.Where(f => f.Value.BroadcastTime < threshold).ToList().ForEach(device =>
                {
                    mDiscoveredDevices.Remove(device.Key);
                    DeviceTimeout(device.Value);
                });
            }
        }

        public void StartListening()
        {
            lock (mThreadLock)
            {
                if (Listening)
                    return;

                watcher.Start();

            }
            StartedListening();
        }

        public void StopListening()
        {
            lock (mThreadLock)
            {
                if (!Listening)
                    return;

                watcher.Stop();
                mDiscoveredDevices.Clear();


            }
        }
    }
}

