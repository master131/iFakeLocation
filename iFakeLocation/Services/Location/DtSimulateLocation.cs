using System;
using System.Globalization;
using System.Text;
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.Service;

namespace iFakeLocation.Services.Location
{
    internal class DtSimulateLocation : LocationService
    {
        public DtSimulateLocation(DeviceInformation device) : base(device) {
        }

        private static byte[] ToBytesBE(int i) {
            var b = BitConverter.GetBytes((uint) i);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }

        public override void SetLocation(PointLatLng? target) {
            iDeviceHandle deviceHandle = null;
            LockdownClientHandle lockdownHandle = null;
            LockdownServiceDescriptorHandle simulateDescriptor = null;
            ServiceClientHandle serviceClientHandle = null;

            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var service = LibiMobileDevice.Instance.Service;

            try {
                // Get device handle
                var err = idevice.idevice_new_with_options(out deviceHandle, _device.UDID, (int) (_device.IsNetwork ? iDeviceOptions.LookupNetwork : iDeviceOptions.LookupUsbmux));
                if (err != iDeviceError.Success)
                    throw new Exception("Unable to connect to the device. Make sure it is connected.");

                // Obtain a lockdown client handle
                if (lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "iFakeLocation") !=
                    LockdownError.Success)
                    throw new Exception("Unable to connect to lockdownd.");

                // Start the simulatelocation service
                if (lockdown.lockdownd_start_service(lockdownHandle, "com.apple.dt.simulatelocation",
                        out simulateDescriptor) != LockdownError.Success ||
                    simulateDescriptor.IsInvalid)
                    throw new Exception("Unable to start simulatelocation service.");

                // Create new service client
                if (service.service_client_new(deviceHandle, simulateDescriptor, out serviceClientHandle) !=
                    ServiceError.Success)
                    throw new Exception("Unable to create simulatelocation service client.");

                if (!target.HasValue) {
                    // Send stop
                    var stopMessage = ToBytesBE(1); // 0x1 (32-bit big-endian uint)
                    uint sent = 0;
                    if (service.service_send(serviceClientHandle, stopMessage, (uint) stopMessage.Length, ref sent) !=
                        ServiceError.Success)
                        throw new Exception("Unable to send stop message to device.");
                }
                else {
                    // Send start
                    var startMessage = ToBytesBE(0); // 0x0 (32-bit big-endian uint)
                    var lat = Encoding.ASCII.GetBytes(target.Value.Lat.ToString(CultureInfo.InvariantCulture));
                    var lng = Encoding.ASCII.GetBytes(target.Value.Lng.ToString(CultureInfo.InvariantCulture));
                    var latLen = ToBytesBE(lat.Length);
                    var lngLen = ToBytesBE(lng.Length);
                    uint sent = 0;

                    if (service.service_send(serviceClientHandle, startMessage, (uint) startMessage.Length, ref sent) !=
                        ServiceError.Success ||
                        service.service_send(serviceClientHandle, latLen, (uint) latLen.Length, ref sent) !=
                        ServiceError.Success ||
                        service.service_send(serviceClientHandle, lat, (uint) lat.Length, ref sent) !=
                        ServiceError.Success ||
                        service.service_send(serviceClientHandle, lngLen, (uint) lngLen.Length, ref sent) !=
                        ServiceError.Success ||
                        service.service_send(serviceClientHandle, lng, (uint) lng.Length, ref sent) !=
                        ServiceError.Success) {
                        throw new Exception("Unable to send co-ordinates to device.");
                    }
                }
            }
            finally {
                // Cleanup
                if (serviceClientHandle != null)
                    serviceClientHandle.Close();

                if (simulateDescriptor != null)
                    simulateDescriptor.Close();

                if (lockdownHandle != null)
                    lockdownHandle.Close();

                if (deviceHandle != null)
                    deviceHandle.Close();
            }
        }
    }
}
