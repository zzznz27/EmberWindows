using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothModule
{
    static class EmberCharacteristics
    {
        public static Guid EmberService = new Guid("fc543622-236c-4c94-8fa9-944a3e5353fa");
        public static Guid Battery = new Guid("fc540007-236c-4c94-8fa9-944a3e5353fa");
        public static Guid CurrentTemperature = new Guid("fc540002-236c-4c94-8fa9-944a3e5353fa");
    }
}
