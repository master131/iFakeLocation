namespace iFakeLocation.Services.Location
{
    internal abstract class LocationService
    {
        protected readonly DeviceInformation _device;

        protected LocationService(DeviceInformation device) {
            _device = device;
        }

        public abstract void SetLocation(PointLatLng? target);
    }
}
