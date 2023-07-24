using System.Runtime.InteropServices;
using iMobileDevice.Plist;

namespace iFakeLocation
{
    internal static class NativeMethods
    {
        [DllImport(PlistNativeMethods.LibraryName, EntryPoint="plist_new_data", CallingConvention=CallingConvention.Cdecl)]
        private static extern unsafe PlistHandle plist_new_data(byte* val, ulong length);

        public static unsafe PlistHandle plist_new_data(byte[] val, int length) {
            fixed (byte* ptr = val)
                return plist_new_data(ptr, (ulong) length);
        }
    }
}
