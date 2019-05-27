#if MOUSEDDEBUG || UNITY_EDITOR
#define SHOWMOUSEDDEBUG
#endif

using System;
using System.Runtime.InteropServices;

namespace Navegar {

    internal static class WindowEngine {

        private static class NativeMethods {

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT {
                public int X;
                public int Y;
            }

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetModuleHandle(string dllToLoad);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.U4)]
            public static extern UInt32 GetCursorPos(out POINT lpPoint);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr NavegarWindowEngineUnity_build();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_loadWindow(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string pathToLoader, [MarshalAs(UnmanagedType.LPStr)]string configPath, [MarshalAs(UnmanagedType.LPStr)]string title, [MarshalAs(UnmanagedType.LPStr)]string id, int sendInput, int remoteWindow);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_destroy(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_monitor(IntPtr handle, Int32 mon);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_title(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string title);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_position(IntPtr handle, Int32 x, Int32 y);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_size(IntPtr handle, Int32 w, Int32 h);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_width(IntPtr handle, Int32 w);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_height(IntPtr handle, Int32 h);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_border(IntPtr handle, Int32 t);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_alwaysOnTop(IntPtr handle, Int32 t);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_resizable(IntPtr handle, Int32 t);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_fullscreen(IntPtr handle, Int32 t);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_show(IntPtr handle, Int32 t);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_reset(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavegarWindowEngineUnity_touchScreen();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavegarWindowEngineUnity_mouseButtonLeft();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavegarWindowEngineUnity_mouseButtonRight();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavegarWindowEngineUnity_mouseButtonMiddle();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavegarWindowEngineUnity_currentWindowWidth();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavegarWindowEngineUnity_currentWindowHeight();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavegarWindowEngineUnity_currentWindowId();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr NavegarWindowEngineUnity_startBuildRenderTexture(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavegarWindowEngineUnity_endBuildRenderTexture(IntPtr handle, IntPtr destText);

        private static IntPtr DllHandle = IntPtr.Zero;

        private static NavegarWindowEngineUnity_build UG_build = null;
        private static NavegarWindowEngineUnity_loadWindow UG_loadWindow = null;
        private static NavegarWindowEngineUnity_destroy UG_destroy = null;
        private static NavegarWindowEngineUnity_monitor UG_monitor = null;
        private static NavegarWindowEngineUnity_title UG_title = null;
        private static NavegarWindowEngineUnity_position UG_position = null;
        private static NavegarWindowEngineUnity_size UG_size = null;
        private static NavegarWindowEngineUnity_width UG_width = null;
        private static NavegarWindowEngineUnity_height UG_height = null;
        private static NavegarWindowEngineUnity_border UG_border = null;
        private static NavegarWindowEngineUnity_alwaysOnTop UG_alwaysOnTop = null;
        private static NavegarWindowEngineUnity_resizable UG_resizable = null;
        private static NavegarWindowEngineUnity_fullscreen UG_fullscreen = null;
        private static NavegarWindowEngineUnity_show UG_show = null;
        private static NavegarWindowEngineUnity_reset UG_reset = null;

        private static NavegarWindowEngineUnity_touchScreen UG_touchScreen = () => 0;
        private static NavegarWindowEngineUnity_mouseButtonLeft UG_mouseButtonLeft = () => 0;
        private static NavegarWindowEngineUnity_mouseButtonRight UG_mouseButtonRight = () => 0;
        private static NavegarWindowEngineUnity_mouseButtonMiddle UG_mouseButtonMiddle = () => 0;
        private static NavegarWindowEngineUnity_currentWindowWidth UG_winWidth = () => UnityEngine.Screen.width;
        private static NavegarWindowEngineUnity_currentWindowHeight UG_winHeight = () => UnityEngine.Screen.height;
        private static NavegarWindowEngineUnity_currentWindowId UG_winId = () => -1;

        private static NavegarWindowEngineUnity_startBuildRenderTexture UG_startBuildRenderTexture = null;
        private static NavegarWindowEngineUnity_endBuildRenderTexture UG_endBuildRenderTexture = null;

        public static bool IsLoaded() {
            return (DllHandle != IntPtr.Zero);
        }

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else

        [DllImport("NavegarWindowEngine")]
#endif
        private static extern void NavegarWindowEngineUnity_Link();

        public static bool Constructor() {
            IntPtr funcPtr = IntPtr.Zero;

            NavegarWindowEngineUnity_Link();

            if (IsLoaded()) return true;

            DllHandle = NativeMethods.GetModuleHandle(@"NavegarWindowEngine.dll");

            if (!IsLoaded()) return true;

            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_build");
            if (funcPtr == IntPtr.Zero) return false;
            UG_build = (NavegarWindowEngineUnity_build)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_build));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_loadWindow");
            if (funcPtr == IntPtr.Zero) return false;
            UG_loadWindow = (NavegarWindowEngineUnity_loadWindow)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_loadWindow));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_destroy");
            if (funcPtr == IntPtr.Zero) return false;
            UG_destroy = (NavegarWindowEngineUnity_destroy)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_destroy));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_monitor");
            if (funcPtr == IntPtr.Zero) return false;
            UG_monitor = (NavegarWindowEngineUnity_monitor)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_monitor));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_title");
            if (funcPtr == IntPtr.Zero) return false;
            UG_title = (NavegarWindowEngineUnity_title)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_title));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_position");
            if (funcPtr == IntPtr.Zero) return false;
            UG_position = (NavegarWindowEngineUnity_position)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_position));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_size");
            if (funcPtr == IntPtr.Zero) return false;
            UG_size = (NavegarWindowEngineUnity_size)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_size));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_width");
            if (funcPtr == IntPtr.Zero) return false;
            UG_width = (NavegarWindowEngineUnity_width)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_width));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_height");
            if (funcPtr == IntPtr.Zero) return false;
            UG_height = (NavegarWindowEngineUnity_height)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_height));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_border");
            if (funcPtr == IntPtr.Zero) return false;
            UG_border = (NavegarWindowEngineUnity_border)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_border));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_alwaysOnTop");
            if (funcPtr == IntPtr.Zero) return false;
            UG_alwaysOnTop = (NavegarWindowEngineUnity_alwaysOnTop)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_alwaysOnTop));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_resizable");
            if (funcPtr == IntPtr.Zero) return false;
            UG_resizable = (NavegarWindowEngineUnity_resizable)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_resizable));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_fullscreen");
            if (funcPtr == IntPtr.Zero) return false;
            UG_fullscreen = (NavegarWindowEngineUnity_fullscreen)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_fullscreen));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_show");
            if (funcPtr == IntPtr.Zero) return false;
            UG_show = (NavegarWindowEngineUnity_show)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_show));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_reset");
            if (funcPtr == IntPtr.Zero) return false;
            UG_reset = (NavegarWindowEngineUnity_reset)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_reset));

            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_touchScreen");
            if (funcPtr == IntPtr.Zero) return false;
            UG_touchScreen = (NavegarWindowEngineUnity_touchScreen)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_touchScreen));

            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_mouseButtonLeft");
            if (funcPtr == IntPtr.Zero) return false;
            UG_mouseButtonLeft = (NavegarWindowEngineUnity_mouseButtonLeft)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_mouseButtonLeft));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_mouseButtonRight");
            if (funcPtr == IntPtr.Zero) return false;
            UG_mouseButtonRight = (NavegarWindowEngineUnity_mouseButtonRight)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_mouseButtonRight));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_mouseButtonMiddle");
            if (funcPtr == IntPtr.Zero) return false;
            UG_mouseButtonMiddle = (NavegarWindowEngineUnity_mouseButtonMiddle)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_mouseButtonMiddle));

            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_currentWindowWidth");
            if (funcPtr == IntPtr.Zero) return false;
            UG_winWidth = (NavegarWindowEngineUnity_currentWindowWidth)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_currentWindowWidth));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_currentWindowHeight");
            if (funcPtr == IntPtr.Zero) return false;
            UG_winHeight = (NavegarWindowEngineUnity_currentWindowHeight)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_currentWindowHeight));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_currentWindowId");
            if (funcPtr == IntPtr.Zero) return false;
            UG_winId = (NavegarWindowEngineUnity_currentWindowId)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_currentWindowId));

            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_startBuildRenderTexture");
            if (funcPtr == IntPtr.Zero) return false;
            UG_startBuildRenderTexture = (NavegarWindowEngineUnity_startBuildRenderTexture)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_startBuildRenderTexture));
            funcPtr = NativeMethods.GetProcAddress(DllHandle, "NavegarWindowEngineUnity_endBuildRenderTexture");
            if (funcPtr == IntPtr.Zero) return false;
            UG_endBuildRenderTexture = (NavegarWindowEngineUnity_endBuildRenderTexture)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(NavegarWindowEngineUnity_endBuildRenderTexture));

            return true;
        }

        public static void Destructor() {
            if (!IsLoaded()) return;
            DllHandle = IntPtr.Zero;
        }

        public class API {

            public delegate IntPtr OnCreateTexture(API me, int width, int height);

            public int passCamera = 0;
            public bool linkedFunction = false;
            public OnCreateTexture funcCreateTexture = null;

            [StructLayout(LayoutKind.Sequential)]
            private struct TextureDescription {
                public int width;
                public int height;
            }

            private IntPtr handle = IntPtr.Zero;

            private static UnityEngine.Vector3 _mousePostion = new UnityEngine.Vector3();

            [System.NonSerialized]
            private static ComputeOncePerFrame<UnityEngine.Vector3> _mousePosition = new ComputeOncePerFrame<UnityEngine.Vector3>(
                () => {
                    NativeMethods.POINT pt = new NativeMethods.POINT { X = 0xEADBEAF, Y = 0xEADBEAF };
                    UInt32 _retMousePosition = NativeMethods.GetCursorPos(out pt);
                    if (_retMousePosition == 0xDEADBEAF) {
                        // we should check if the window hops and set it to zero.
                        mouseDelta = new UnityEngine.Vector2(pt.X - _mousePostion.x, pt.Y - _mousePostion.y);
                        _mousePostion.x = pt.X;
                        _mousePostion.y = pt.Y;
                        return _mousePostion;
                    } else {
                        mouseDelta = new UnityEngine.Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));
                        return UnityEngine.Input.mousePosition;
                    }
                });

            static public UnityEngine.Vector3 mousePosition {
                get {
                    return _mousePosition.value;
                }
            }
            static public UnityEngine.Vector2 mouseDelta;


            public static bool touchScreen {
                get {
                    return UG_touchScreen() != 0;
                }
            }

            public static bool mouseButtonLeft {
                get {
                    return UG_mouseButtonLeft() != 0;
                }
            }

            public static bool mouseButtonRight {
                get {
                    return UG_mouseButtonRight() != 0;
                }
            }

            public static bool mouseButtonMiddle {
                get {
                    return UG_mouseButtonMiddle() != 0;
                }
            }

            public static int remoteWidth {
                get {
                    int w = UG_winWidth();
                    if (w < 0) {
                        return UnityEngine.Screen.width;
                    } else {
                        return w;
                    }
                }
            }

            public static int remoteHeight {
                get {
                    int h = UG_winHeight();
                    if (h < 0) {
                        return UnityEngine.Screen.height;
                    } else {
                        return h;
                    }
                }
            }

            public static int remoteID {
                get {
                    if (UG_winId == null) {
                        return -1;
                    }
                    return UG_winId();
                }
            }

            public bool build() {
                if (!WindowEngine.IsLoaded()) {
                    return true;
                }
                destructor();
                handle = UG_build();
                return true;
            }

            public void loadWindow(string configPath, string title, string id, bool sendInput, bool remoteWindow, bool remoteWindowMirror) {
                UG_loadWindow(handle, UnityEngine.Application.streamingAssetsPath, configPath, title, id, sendInput == true ? 1 : 0, remoteWindowMirror == true ? 2 : (remoteWindow == true ? 1 : 0));
            }

            public void destructor() {
                if (!WindowEngine.IsLoaded()) return;
                linkedFunction = false;
                funcCreateTexture = null;
                UG_destroy(handle);
                handle = IntPtr.Zero;
            }

            public void monitor(Int32 m) {
                UG_monitor(handle, m);
            }

            public void title(string t) {
                UG_title(handle, t);
            }

            public void position(Int32 x, Int32 y) {
                UG_position(handle, x, y);
            }

            public void size(Int32 w, Int32 h) {
                UG_size(handle, w, h);
            }

            public void width(Int32 w) {
                UG_width(handle, w);
            }

            public void height(Int32 h) {
                UG_height(handle, h);
            }

            public void border(bool t) {
                if (t) UG_border(handle, 1);
                else UG_border(handle, 0);
            }

            public void alwaysOnTop(bool t) {
                if (t) UG_alwaysOnTop(handle, 1);
                else UG_alwaysOnTop(handle, 0);
            }

            public void resizable(bool t) {
                if (t) UG_resizable(handle, 1);
                else UG_resizable(handle, 0);
            }

            public void fullscreen(bool t) {
                if (t) UG_fullscreen(handle, 1);
                else UG_fullscreen(handle, 0);
            }

            public void show(bool t) {
                if (t) UG_show(handle, 1);
                else UG_show(handle, 0);
            }

            public void reset() {
                UG_reset(handle);
            }

            public void createRenderTexture() {
                if (funcCreateTexture == null) {
                    return;
                }

                IntPtr retValue = UG_startBuildRenderTexture(handle);

                if (retValue != IntPtr.Zero) {
                    IntPtr texture = IntPtr.Zero;
                    TextureDescription textDesc = (TextureDescription)Marshal.PtrToStructure(retValue, typeof(TextureDescription));
                    texture = funcCreateTexture(this, textDesc.width, textDesc.height);
                    UG_endBuildRenderTexture(handle, texture);
                    // UnityEngine.Debug.Log("texture " + texture);
                }
            }
        }
    }
}