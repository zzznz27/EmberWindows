using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothModule
{
    public class GattService
    {
        public string Name { get;  }

        public string UniformTypeIdentifier { get; }

        public ushort AssignedNumber { get;  }

        public string ProfileSpecification { get; }

        public GattService(string name, string uniformIdentifier, ushort assignedNumber, string profileSpec)
        {
            Name = name;
            UniformTypeIdentifier = uniformIdentifier;
            AssignedNumber = assignedNumber;
            ProfileSpecification = profileSpec;
        }
    }
}
