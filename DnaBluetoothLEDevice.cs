using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BluetoothModule
{
    public class DnaBluetoothLEDevice
    {
        public DateTimeOffset BroadcastTime { get; }

        public ulong Address { get; }

        public string Name { get; }

        public short SignalStrengthDB { get; }
        /// <summary>
        /// Indicates if we are connected to this device
        /// </summary>
        public bool Connected { get; }

        /// <summary>
        /// Indicates if this device supports pairing
        /// </summary>
        public bool CanPair { get; }

        /// <summary>
        /// Indicates if we are currently paired to this device
        /// </summary>
        public bool Paired { get; }

        /// <summary>
        /// The permanent unique Id of this device
        /// </summary>
        public string DeviceId { get; }

        public int Battery { get; set; }


        public DnaBluetoothLEDevice(
            ulong address,
            string name,
            short rssi,
            DateTimeOffset broadcastTime,
            bool connected,
            bool canPair,
            bool paired,
            string deviceId

            )
        {
            Address = address;
            Name = name;
            SignalStrengthDB = rssi;
            BroadcastTime = broadcastTime;
            Connected = connected;
            CanPair = canPair;
            Paired = paired;
            DeviceId = deviceId;
            GetBattery();
        }

        public override string ToString()
        {
            return $"{(string.IsNullOrEmpty(Name) ? "[No Name]" : Name)}\t[{DeviceId}]\t({SignalStrengthDB}) ";
        }

        public async void GetBattery()
        {
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(Address, BluetoothAddressType.Random);
            var gatt = await device.GetGattServicesAsync();

            while (gatt.Status != GattCommunicationStatus.Success)
            {
                gatt = await device.GetGattServicesAsync();
                Console.WriteLine(gatt.Status);
            }
            if (gatt.Status == GattCommunicationStatus.Success)
            {
                foreach (var service in gatt.Services)
                {
                    if (service.Uuid.Equals(EmberCharacteristics.EmberService))
                    {

                        var result = await service.GetCharacteristicsAsync();
                        foreach (var character in result.Characteristics)
                        {
                            //if (character.Uuid.Equals(EmberCharacteristics.Battery))
                            //{
                            //    try
                            //    {
                            //        var Readresult = await character.ReadValueAsync();
                            //        if (Readresult.Status == GattCommunicationStatus.Success)
                            //        {
                            //            DataReader reader = DataReader.FromBuffer(Readresult.Value);
                            //            byte[] input = new byte[reader.UnconsumedBufferLength];
                            //            reader.ReadBytes(input);
                            //            // Utilize the data as needed


                            //            // battery level is encoded as a percentage value in the first byte according to
                            //            // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.battery_level.xml
                            //            Console.WriteLine($"Battery Level: " + input[0].ToString() + "%");
                            //            Battery = input[0];
                            //            // Console.WriteLine("Elapsed={0}", sw.Elapsed);
                            //        }
                            //    }
                            //    catch (ArgumentException)
                            //    {
                            //        throw new Exception("Battery Level: (unable to parse)");
                            //    }
                            //}
                            //if (character.Uuid.Equals(EmberCharacteristics.CurrentTemperature))
                            //{
                            //    try
                            //    {
                            //        var Readresult = await character.ReadValueAsync();
                            //        if (Readresult.Status == GattCommunicationStatus.Success)
                            //        {
                            //            DataReader reader = DataReader.FromBuffer(Readresult.Value);
                            //            byte[] input = new byte[reader.UnconsumedBufferLength];
                            //            reader.ReadBytes(input);
                            //            double temp = input[0];

                            //            Console.WriteLine("Temperature Level: " + input[0].ToString());
                            //            Battery = input[0];
                            //        }
                            //    }
                            //    catch (ArgumentException)
                            //    {
                            //        throw new Exception("Temp: (unable to parse)");
                            //    }
                            //}


                            try
                            {
                                GattReadResult Readresult = await character.ReadValueAsync();
                                if (Readresult.Status == GattCommunicationStatus.Success)
                                {
                                    DataReader reader = DataReader.FromBuffer(Readresult.Value);
                                    reader.ByteOrder = ByteOrder.LittleEndian;
                                    byte[] input = new byte[reader.UnconsumedBufferLength];
                                    reader.ReadBytes(input);
                                    Console.WriteLine("******************" + character.Uuid + "******************");
                                    PrintByteArray(input);
                                }
                            }catch (ArgumentException)
                                {
                                throw new Exception("Temp: (unable to parse)");
                            }
                        }

                    
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No Gatt Services Found");
            }
        }
        public void PrintByteArray(byte[] input)
        {
            string output = default; 
            foreach(var b in input)
            {
                output += " " + b.ToString();
            }
            Console.WriteLine(output);        }
    }
   
}
