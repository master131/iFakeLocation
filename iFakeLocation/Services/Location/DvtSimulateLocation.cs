using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iFakeLocation.Services.Location
{
    internal class DvtSimulateLocation : LocationService
    {
        public DvtSimulateLocation(DeviceInformation device) : base(device) {
        }

        public override void SetLocation(PointLatLng? target) {
            
        }
    }
}
