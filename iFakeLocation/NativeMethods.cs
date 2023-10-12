using System;
using System.Reflection;
using System.Runtime.InteropServices;
using iMobileDevice.Plist;

namespace iFakeLocation
{
    internal static class NativeMethods
    {
        static NativeMethods() {
            EnsureRegistered();
        }

        private static void EnsureRegistered() {
#if !NETCOREAPP2_0 && !NETSTANDARD2_0 && !NET45
            // Special glue which fixes the DllImport library name and resolves the library handle using
            // existing logic in iMobileDevice-net (have to resort to reflection as its internal)
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(),
                (DllImportResolver) Delegate.CreateDelegate(typeof(DllImportResolver),
                    typeof(PlistNativeMethods).Assembly.GetType("iMobileDevice.LibraryResolver")
                        .GetMethod("DllImportResolver", BindingFlags.NonPublic | BindingFlags.Static)));
#endif
        }
        
        [DllImport(PlistNativeMethods.LibraryName, EntryPoint="plist_new_data", CallingConvention=CallingConvention.Cdecl)]
        private static extern unsafe PlistHandle plist_new_data(byte* val, ulong length);

        public static unsafe PlistHandle plist_new_data(byte[] val, int length) {
            fixed (byte* ptr = val)
                return plist_new_data(ptr, (ulong) length);
        }
    }
}
