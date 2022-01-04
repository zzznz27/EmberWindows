using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BluetoothModule
{
    public class GattServiceIds : IReadOnlyCollection<GattService>
    {

        private readonly IReadOnlyCollection<GattService> mCollection; 

        #region PublicProperties 

        public int Count => throw new NotImplementedException();

        public GattServiceIds()
        {
            mCollection = new List<GattService>(new[]
            {
                new GattService("name", "id", 0x1800, "GSS")
            });
        }
        #endregion

        #region Ireadonly methods
        public IEnumerator<GattService> GetEnumerator() => mCollection.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => mCollection.GetEnumerator();
        
        #endregion
    }
}
