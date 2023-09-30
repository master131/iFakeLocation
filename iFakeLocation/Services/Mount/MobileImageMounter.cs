
namespace iFakeLocation.Services.Mount
{
    internal abstract class MobileImageMounter
    {
        protected readonly DeviceInformation _device;

        protected MobileImageMounter(DeviceInformation device) {
            _device = device;
        }

        public abstract void EnableDeveloperMode(string[] resourcePaths);
    }
}
