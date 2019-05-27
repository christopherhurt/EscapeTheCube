
using System;
using System.Runtime.InteropServices;




namespace Navegar
{
    public static class IO
    {
        private static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(IntPtr hModule);
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarRemoteIO_InitServer();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarRemoteIO_DestroyServer();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavegarRemoteIO_InitClient();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavegarRemoteIO_DestroyClient();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarRemoteStream_InitServer();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarRemoteStream_DestroyServer();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavegarRemoteStream_InitClient();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavegarRemoteStream_DestroyClient();


        private static IntPtr DllHandle = IntPtr.Zero;

        private static NavegarRemoteIO_InitServer UG_NavegarRemoteIO_InitServer = null;
        private static NavegarRemoteIO_DestroyServer UG_NavegarRemoteIO_DestroyServer = null;
        private static NavegarRemoteIO_InitClient UG_NavegarRemoteIO_InitClient = null;
        private static NavegarRemoteIO_DestroyClient UG_NavegarRemoteIO_DestroyClient = null;
        private static NavegarRemoteStream_InitServer UG_NavegarRemoteStream_InitServer = null;
        private static NavegarRemoteStream_DestroyServer UG_NavegarRemoteStream_DestroyServer = null;
        private static NavegarRemoteStream_InitClient UG_NavegarRemoteStream_InitClient = null;
        private static NavegarRemoteStream_DestroyClient UG_NavegarRemoteStream_DestroyClient = null;



        public static bool IsLoaded()
        {
            return (DllHandle != IntPtr.Zero);
        }


        private static string NFDllName = @"NavegarRemoteIO.DLL";
        private static string NFDllNamePackaged = @"NavegarRemoteIO_DLL";

        private static string pathToDLLs
        {
            get
            {
                return UnityEngine.Application.streamingAssetsPath + "\\NavegarRemoteIO";
            }
        }

        private static string pathToDLLs32
        {
            get
            {
                return pathToDLLs + "\\32bits";
            }
        }

        private static string pathToDLLs64
        {
            get
            {
                return pathToDLLs + "\\64bits";
            }
        }

        private static string pathToDLL_Packaged_32
        {
            get
            {
                return "Assets\\Elumenati\\Omnity\\GoodiePacks\\GoodiePackCobra\\DLLs\\32bits";
            }
        }

        private static string pathToDLL_Packaged_64
        {
            get
            {
                return "Assets\\Elumenati\\Omnity\\GoodiePacks\\GoodiePackCobra\\DLLs\\64bits";
            }
        }

        public static void InstallDLLS()
        {
            if (!System.IO.Directory.Exists(pathToDLLs32) || !System.IO.Directory.Exists(pathToDLLs64))
            {
                System.IO.Directory.CreateDirectory(UnityEngine.Application.streamingAssetsPath);
                System.IO.Directory.CreateDirectory(pathToDLLs);
                System.IO.Directory.CreateDirectory(pathToDLLs32);
                System.IO.Directory.CreateDirectory(pathToDLLs64);
                System.IO.File.Copy(pathToDLL_Packaged_32 + "\\" + NFDllNamePackaged, pathToDLLs32 + "\\" + NFDllName);
                System.IO.File.Copy(pathToDLL_Packaged_64 + "\\" + NFDllNamePackaged, pathToDLLs64 + "\\" + NFDllName);
            }
        }


        public static bool Constructor() {
            IntPtr funcPtr;

            if (IsLoaded()) return true;

            DllHandle = NativeMethods.LoadLibrary(NFDllName);
            if (!IsLoaded()) {
                string saveCurrentDir = Environment.CurrentDirectory;
                try {
                    Environment.CurrentDirectory = pathToDLLs32;
                } catch {
                    UnityEngine.Debug.LogError("ERROR COBRA INTERFACE DLL NOT INSTALLED INTO STREAMING ASSETS");
                }
                DllHandle = NativeMethods.LoadLibrary(NFDllName);

                Environment.CurrentDirectory = saveCurrentDir;
            }
            if (!IsLoaded()) {
                string saveCurrentDir = Environment.CurrentDirectory;
                try {
                    Environment.CurrentDirectory = pathToDLLs64;
                } catch {
                    UnityEngine.Debug.LogError("ERROR COBRA INTERFACE DLL NOT INSTALLED INTO STREAMING ASSETS");
                }
                DllHandle = NativeMethods.LoadLibrary(NFDllName);
                Environment.CurrentDirectory = saveCurrentDir;
            }
            if (!IsLoaded()) return false;

            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarRemoteIO_InitServer");
            if (funcPtr == IntPtr.Zero) return false;
            UG_NavegarRemoteIO_InitServer = (NavegarRemoteIO_InitServer)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarRemoteIO_InitServer));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarRemoteIO_DestroyServer");
            if (funcPtr == IntPtr.Zero) return false;
            UG_NavegarRemoteIO_DestroyServer = (NavegarRemoteIO_DestroyServer)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarRemoteIO_DestroyServer));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarRemoteIO_InitClient");
            if (funcPtr == IntPtr.Zero) return false;
            UG_NavegarRemoteIO_InitClient = (NavegarRemoteIO_InitClient)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarRemoteIO_InitClient));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarRemoteIO_DestroyClient");
            if (funcPtr == IntPtr.Zero) return false;
            UG_NavegarRemoteIO_DestroyClient = (NavegarRemoteIO_DestroyClient)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarRemoteIO_DestroyClient));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarRemoteStream_InitServer");
            if (funcPtr == IntPtr.Zero) return false;
            UG_NavegarRemoteStream_InitServer = (NavegarRemoteStream_InitServer)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarRemoteStream_InitServer));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarRemoteStream_DestroyServer");
            if (funcPtr == IntPtr.Zero) return false;
            UG_NavegarRemoteStream_DestroyServer = (NavegarRemoteStream_DestroyServer)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarRemoteStream_DestroyServer));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarRemoteStream_InitClient");
            if (funcPtr == IntPtr.Zero) return false;
            UG_NavegarRemoteStream_InitClient = (NavegarRemoteStream_InitClient)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarRemoteStream_InitClient));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarRemoteStream_DestroyClient");
            if (funcPtr == IntPtr.Zero) return false;
            UG_NavegarRemoteStream_DestroyClient = (NavegarRemoteStream_DestroyClient)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarRemoteStream_DestroyClient));

            return true;
        }


        public static void Destructor()
        {
            if (!IsLoaded()) return;
            NativeMethods.FreeLibrary(DllHandle);
            DllHandle = IntPtr.Zero;
        }




        public static class API
        {
            public static void initServer()
            {
                if (!IO.IsLoaded()) return;
                UG_NavegarRemoteStream_InitServer();
                UG_NavegarRemoteIO_InitServer();
            }


            public static void destroyServer()
            {
                if (!IO.IsLoaded()) return;
                UG_NavegarRemoteStream_DestroyServer();
                UG_NavegarRemoteIO_DestroyServer();
            }


            public static void initClient() {
                if (!IO.IsLoaded()) return;
                UG_NavegarRemoteStream_InitClient();
                UG_NavegarRemoteIO_InitClient();
            }


            public static void destroyClient() {
                if (!IO.IsLoaded()) return;
                UG_NavegarRemoteStream_DestroyClient();
                UG_NavegarRemoteIO_DestroyClient();
            }
        }
    }
}
