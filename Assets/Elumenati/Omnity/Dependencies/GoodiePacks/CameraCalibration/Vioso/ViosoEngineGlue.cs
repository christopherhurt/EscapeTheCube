using System;
using System.Runtime.InteropServices;

namespace Navegar {

    internal static class ViosoEngine {

        private static class NativeMethods {

            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(IntPtr hModule);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UnmanagedGlue_VIOSOEngineInit([MarshalAs(UnmanagedType.LPStr)]string iniFile);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UnmanagedGlue_VIOSOEngineDestroy();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UnmanagedGlue_VIOSOEngineTotalMesh();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UnmanagedGlue_VIOSOEngineGetMesh(int meshNumber);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UnmanagedGlue_VIOSOEngineBlendDataSize(int blendNumber);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UnmanagedGlue_VIOSOEngineBlendData(int blendNumber);

        private static IntPtr DllHandle = IntPtr.Zero;

        private static UnmanagedGlue_VIOSOEngineInit UG_VIOSOEngineInit = null;
        private static UnmanagedGlue_VIOSOEngineDestroy UG_VIOSOEngineDestroy = null;
        private static UnmanagedGlue_VIOSOEngineTotalMesh UG_VIOSOEngineTotalMesh = null;
        private static UnmanagedGlue_VIOSOEngineGetMesh UG_VIOSOEngineGetMesh = null;
        private static UnmanagedGlue_VIOSOEngineBlendDataSize UG_VIOSOEngineBlendDataSize = null;
        private static UnmanagedGlue_VIOSOEngineBlendData UG_VIOSOEngineBlendData = null;

        public static bool IsLoaded() {
            return (DllHandle != IntPtr.Zero);
        }

        private static string DllName = @"ViosoEngine.dll";
#if UNITY_EDITOR
        private static string DllNamePackaged = @"ViosoEngine_dll";
#endif

        private static string pathToDLLs {
            get {
                return UnityEngine.Application.streamingAssetsPath + "\\Vioso";
            }
        }

        private static string pathToDLLs32 {
            get {
                return pathToDLLs + "\\32bits";
            }
        }

        private static string pathToDLLs64 {
            get {
                return pathToDLLs + "\\64bits";
            }
        }

        private static string pathToDLL_Packaged_32 {
            get {
                return "Assets\\Elumenati\\Omnity\\Dependencies\\GoodiePacks\\CameraCalibration\\Vioso\\DLLs\\32bits";
            }
        }

        private static string pathToDLL_Packaged_64 {
            get {
                return "Assets\\Elumenati\\Omnity\\Dependencies\\GoodiePacks\\CameraCalibration\\Vioso\\DLLs\\64bits";
            }
        }

        public static void InstallDLLS() {
#if UNITY_EDITOR

            if (!System.IO.Directory.Exists(pathToDLLs32) || !System.IO.Directory.Exists(pathToDLLs64)) {
                System.IO.Directory.CreateDirectory(UnityEngine.Application.streamingAssetsPath);
                System.IO.Directory.CreateDirectory(pathToDLLs);
                System.IO.Directory.CreateDirectory(pathToDLLs32);
                System.IO.Directory.CreateDirectory(pathToDLLs64);


                string iniFIleWithExtensionSource = pathToDLL_Packaged_64 + @"\\calibration.ini";
                string iniFIleWithExtensionDest32 = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, @"Vioso\32bits\calibration.ini").Replace("/", "\\");
                string iniFIleWithExtensionDest64 = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, @"Vioso\64bits\calibration.ini").Replace("/", "\\");

                try {
                    System.IO.File.Copy(pathToDLL_Packaged_32 + "\\" + DllNamePackaged, pathToDLLs32 + "\\" + DllName);
                    System.IO.File.Copy(pathToDLL_Packaged_32 + "\\" + "VIOSOWarpBlend32_dll", pathToDLLs32 + "\\" + "VIOSOWarpBlend32.dll");
                } catch (System.Exception e) {
                    UnityEngine.Debug.LogError(e.Message);
                }
                try {
                    System.IO.File.Copy(pathToDLL_Packaged_64 + "\\" + DllNamePackaged, pathToDLLs64 + "\\" + DllName);
                    System.IO.File.Copy(pathToDLL_Packaged_64 + "\\" + "VIOSOWarpBlend64_dll", pathToDLLs64 + "\\" + "VIOSOWarpBlend64.dll");
                } catch (System.Exception e) {
                    UnityEngine.Debug.LogError(e.Message);
                }
                try {
                    System.IO.File.Copy(iniFIleWithExtensionSource, iniFIleWithExtensionDest32);
                } catch (System.Exception e) {
                    UnityEngine.Debug.LogError("Error could not copy ini file " + e.Message);
                }
                try {
                    System.IO.File.Copy(iniFIleWithExtensionSource, iniFIleWithExtensionDest64);
                } catch (System.Exception e) {
                    UnityEngine.Debug.LogError("Error could not copy ini file " + e.Message);
                }
            }
#endif
        }

        /*        static bool is32Bit {
                    get {
                        return IntPtr.Size == 4;
                    }
                }
                */

        public static bool ConstructorPart1() {
            if (IsLoaded()) return true;

            DllHandle = NativeMethods.LoadLibrary(DllName);

            if (!IsLoaded()) {
                string saveCurrentDir = Environment.CurrentDirectory;
                try {
                    //     if (is32Bit) {
                    //       UnityEngine.Debug.LogWarning("WARNING 32 VIOSO NOT YET TESTED");
                    //     Environment.CurrentDirectory = pathToDLLs32;
                    //} else {
                    Environment.CurrentDirectory = pathToDLLs64;
                    //}
                } catch {
                    UnityEngine.Debug.LogError("ERROR VIOSO ENGINE DLL NOT INSTALLED INTO STREAMING ASSETS");
                }
                DllHandle = NativeMethods.LoadLibrary(DllName);
                Environment.CurrentDirectory = saveCurrentDir;
            }

            if (!IsLoaded()) return false;

            IntPtr funcPtr;
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "VIOSOEngineInit");
            if (funcPtr == IntPtr.Zero) return false;
            UG_VIOSOEngineInit = (UnmanagedGlue_VIOSOEngineInit)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(UnmanagedGlue_VIOSOEngineInit));

            funcPtr = NativeMethods.GetProcAddress(DllHandle, "VIOSOEngineDestroy");
            if (funcPtr == IntPtr.Zero) return false;
            UG_VIOSOEngineDestroy = (UnmanagedGlue_VIOSOEngineDestroy)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(UnmanagedGlue_VIOSOEngineDestroy));

            funcPtr = NativeMethods.GetProcAddress(DllHandle, "VIOSOEngineTotalMesh");
            if (funcPtr == IntPtr.Zero) return false;
            UG_VIOSOEngineTotalMesh = (UnmanagedGlue_VIOSOEngineTotalMesh)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(UnmanagedGlue_VIOSOEngineTotalMesh));

            funcPtr = NativeMethods.GetProcAddress(DllHandle, "VIOSOEngineGetMesh");
            if (funcPtr == IntPtr.Zero) return false;
            UG_VIOSOEngineGetMesh = (UnmanagedGlue_VIOSOEngineGetMesh)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(UnmanagedGlue_VIOSOEngineGetMesh));

            funcPtr = NativeMethods.GetProcAddress(DllHandle, "VIOSOEngineBlendDataSize");
            if (funcPtr == IntPtr.Zero) return false;
            UG_VIOSOEngineBlendDataSize = (UnmanagedGlue_VIOSOEngineBlendDataSize)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(UnmanagedGlue_VIOSOEngineBlendDataSize));

            funcPtr = NativeMethods.GetProcAddress(DllHandle, "VIOSOEngineBlendData");
            if (funcPtr == IntPtr.Zero) return false;
            UG_VIOSOEngineBlendData = (UnmanagedGlue_VIOSOEngineBlendData)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(UnmanagedGlue_VIOSOEngineBlendData));

            return true;
        }

        //
        // VWB_ERROR_* are defined on VIOSO API
        // VIOSOENGINE_ERROR_* are defined for viosoengine.dll
        // ERROR CODES:
        //
        private enum errorcodevioso {
            VWB_ERROR_NONE = 0,                        /// No error, we succeeded
            VWB_ERROR_GENERIC = -1,                    /// a generic error, this might be anything, check log file
            VWB_ERROR_PARAMETER = -2,                  /// a parameter error, provided parameter are missing or inappropriate
            VWB_ERROR_INI_LOAD = -3,                   /// ini could notbe loaded
            VWB_ERROR_BLEND = -4,                      /// blend invalid or coud not be loaded to graphic hardware, check log file
            VWB_ERROR_WARP = -5,                       /// warp invalid or could not be loaded to graphic hardware, check log file
            VWB_ERROR_SHADER = -6,                     /// shader program failed to load, usually because of not supported hardware, check log file
            VWB_ERROR_VWF_LOAD = -7,                   /// mappings file broken or version mismatch
            VWB_ERROR_VWF_FILE_NOT_FOUND = -8,         /// cannot find mapping file
            VWB_ERROR_FALSE = -16,                     /// No error, but nothing has been done
            VIOSOENGINE_ERROR_D3D9FAIL = -20,          /// D3d9 initialization failed
            VIOSOENGINE_ERROR_MEMORY = -21,            /// malloc failed to reserve memory
            VIOSOENGINE_ERROR_RENDERFAIL = -22,        /// Vioso api couldn't render mesh
            VIOSOENGINE_ERROR_MONITOR_MISSMATCH = -23, /// Hardware monitor doesn't match any definition on VWF file
        };

        public static bool ConstructorPart2(string iniFileWithoutExtension) {
            if (!ViosoEngine.IsLoaded()) {
                UnityEngine.Debug.LogError("VIOSO Not loaded");
                return false;
            }
            if (!System.IO.File.Exists(iniFileWithoutExtension + ".ini")) {
                UnityEngine.Debug.LogError(iniFileWithoutExtension + ".ini Missing");
            }
            errorcodevioso returnval = (errorcodevioso)UG_VIOSOEngineInit(iniFileWithoutExtension);

            if (returnval != errorcodevioso.VWB_ERROR_NONE) {
                UnityEngine.Debug.LogError("UG_VIOSOEngineInit(" + iniFileWithoutExtension + ") failed, error " + returnval.ToString());
            }
            return returnval == errorcodevioso.VWB_ERROR_NONE;
        }

        public static void Destructor() {
            if (!IsLoaded()) return;
            NativeMethods.FreeLibrary(DllHandle);
            DllHandle = IntPtr.Zero;
        }

        public static class API {
            public const int VIOSOENGINE_Mesh_Divisions = 160;

            [StructLayout(LayoutKind.Sequential)]
            public struct VIOSO_MESH_Vertex {
                public float x;
                public float y;
                public float z;
                public float tu;
                public float tv;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct VIOSO_DATA {

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = VIOSOENGINE_Mesh_Divisions * VIOSOENGINE_Mesh_Divisions * 2)]
                public VIOSO_MESH_Vertex[] data;

                public int totalVertices;
                public int monitorWidth;
                public int monitorHeight;
                public int monitorOffsetX;
                public int monitorOffsetY;

                public VIOSO_DATA Duplicate() {
                    VIOSO_DATA newMem = new VIOSO_DATA();
                    newMem.data = (VIOSO_MESH_Vertex[])data.Clone();
                    newMem.totalVertices = totalVertices;
                    newMem.monitorWidth = monitorWidth;
                    newMem.monitorHeight = monitorHeight;
                    newMem.monitorOffsetX = monitorOffsetX;
                    newMem.monitorOffsetY = monitorOffsetY;
                    return newMem;
                }
            }

            static internal void Log(VIOSO_DATA d) {
                UnityEngine.Debug.Log("totalVertices " + d.totalVertices);
                //                   newMem.deviceName = (char[])deviceName.Clone();
                UnityEngine.Debug.Log("monitorWidth " + d.monitorWidth.ToString());
                UnityEngine.Debug.Log("monitorHeight" + d.monitorHeight.ToString());
                UnityEngine.Debug.Log("monitorOffsetX " + d.monitorOffsetX.ToString());
                UnityEngine.Debug.Log("monitorOffsetY " + d.monitorOffsetY.ToString());

                for (int i = 0; i < 10; i++) {
                    UnityEngine.Debug.LogError( i + " : " + d.data[i].x + " " + d.data[i].y + " " + d.data[i].tu + " " + d.data[i].tv);
                }
            }

            public static void Destructor() {
                if (!ViosoEngine.IsLoaded()) return;
                UG_VIOSOEngineDestroy();
            }

            public static int GetMeshTotal() {
                return UG_VIOSOEngineTotalMesh();
            }

            public static VIOSO_DATA GetMesh(int meshNumber) {
                IntPtr v = UG_VIOSOEngineGetMesh(meshNumber);
                VIOSO_DATA mashaledData = (VIOSO_DATA)Marshal.PtrToStructure(v, typeof(VIOSO_DATA));
                return mashaledData;
            }

            // blend data format is rgba 8bits per channel, so arraysize if w*h*4
            public static byte[] GetBlendData(int blendNumber) {
                IntPtr pointer = UG_VIOSOEngineBlendData(blendNumber);
                if (pointer.ToInt32() == 0) {
                    UnityEngine.Debug.Log("UG_VIOSOEngineBlendData returns " + pointer.ToInt32().ToString());
                }
                int arraySize = UG_VIOSOEngineBlendDataSize(blendNumber);
                byte[] result = new byte[arraySize];
                Marshal.Copy(pointer, result, 0, arraySize * sizeof(byte));
                return result;
            }

            public static UnityEngine.Texture2D GetBlendTexture(int blendNumber, VIOSO_DATA mesh) {
                byte[] result = GetBlendData(blendNumber);
                int blendSize = UG_VIOSOEngineBlendDataSize(blendNumber);

                int arraySize = mesh.monitorWidth * mesh.monitorHeight;
                UnityEngine.Color32[] colors = new UnityEngine.Color32[arraySize];

                UnityEngine.Texture2D image = new UnityEngine.Texture2D(mesh.monitorWidth, mesh.monitorHeight, UnityEngine.TextureFormat.RGBA32, false);

                if (blendSize != (arraySize * 4)) {
                    UnityEngine.Debug.LogWarning("ERROR MISMATCH");
                    UnityEngine.Debug.Log("blendSize size is " + blendSize);
                    UnityEngine.Debug.Log("arraySize4 size is " + arraySize * 4);
                    UnityEngine.Debug.Log("arraySize size is " + arraySize);
                    UnityEngine.Debug.Log("Colors size is " + colors.Length);
                    UnityEngine.Debug.Log("image size is " + image.height + " X " + image.width + " = " + image.height * image.width);
                }

                for (int i = 0; i < arraySize; i++) {
                    colors[i] = new UnityEngine.Color32(result[i * 4 + 0], result[i * 4 + 1], result[i * 4 + 2], result[i * 4 + 3]);
                }

                image.SetPixels32(colors);
                image.Apply();

                return image;
            }

            static public UnityEngine.Mesh GetUnityMesh(VIOSO_DATA memSafe) {
                UnityEngine.Mesh m = new UnityEngine.Mesh();
                System.Collections.Generic.List<UnityEngine.Vector3> verts = new System.Collections.Generic.List<UnityEngine.Vector3>();
                System.Collections.Generic.List<UnityEngine.Vector2> uv = new System.Collections.Generic.List<UnityEngine.Vector2>();
                System.Collections.Generic.List<int> list = new System.Collections.Generic.List<int>();

                int totalTriangles = memSafe.totalVertices - 2;

                for (int i = 0; i < memSafe.totalVertices; i++) {
                    verts.Add(new UnityEngine.Vector3(memSafe.data[i].x, memSafe.data[i].y, memSafe.data[i].z));
                    uv.Add(new UnityEngine.Vector2(memSafe.data[i].tu, memSafe.data[i].tv));
                }

                for (int i = 0; i < totalTriangles; i++) {
                    if (1 == (i % 2)) {
                        list.Add(i);
                        list.Add(i + 2);
                        list.Add(i + 1);
                    } else {
                        list.Add(i + 2);
                        list.Add(i);
                        list.Add(i + 1);
                    }
                }

                m.vertices = verts.ToArray();
                m.uv = uv.ToArray();
                m.triangles = list.ToArray();
                OmnityPlatformDefines.OptimizeMesh(m);
                return m;
            }
        }
    }
}