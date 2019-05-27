using System.Xml.XPath;
using UnityEngine;

#if USE_OMNITYDLLSOURCE
namespace DLLExports {
#else
#endif

public class EasyMultiDisplay3 : OmnityTabbedFeature {
    static public OmnityPluginsIDs _myOmnityPluginsID = OmnityPluginsIDs.EasyMultiDisplay3;
    override public OmnityPluginsIDs myOmnityPluginsID { get { return _myOmnityPluginsID; } }
    public const OmnityPluginsIDs pluginID = OmnityPluginsIDs.EasyMultiDisplay3;

    static private EasyMultiDisplay3 singleton = null;

    static public EasyMultiDisplay3 OmniCreateSingleton(GameObject go) {
        return GetSingleton<EasyMultiDisplay3>(ref singleton, go);
    }

    public static EasyMultiDisplay3 Get() {
        return singleton;
    }

    private void Awake() {
        if (VerifySingleton<EasyMultiDisplay3>(ref singleton)) {
            OmnityInputHelper.fnMouseAxisX = () => Navegar.WindowEngine.API.mouseDelta.x;
            OmnityInputHelper.fnMouseAxisY = () => Navegar.WindowEngine.API.mouseDelta.y;
            OmnityInputHelper.fnMousePosition = () => Navegar.WindowEngine.API.mousePosition;
            OmnityInputHelper.fnMouseRemoteID = () => Navegar.WindowEngine.API.remoteID;

            // todo add more functions 

            Omnity.onReloadEndCallbackPriority.AddLoaderFunction(PriorityEventHandler.Ordering.Order_EasyMultiDisplay3, CoroutineLoader);
        }
    }

    private void OnDestroy() {
        for (int i = 0; i < _window.Count; i++) {
            _window[i].destructor();
        }
        _window.Clear();
        singleton = null;
        Omnity.onReloadEndCallbackPriority.RemoveLoaderFunction(PriorityEventHandler.Ordering.Order_EasyMultiDisplay3, CoroutineLoader);
    }

    override public string BaseName {
        get {
            return "EasyMultiDisplay3";
        }
    }

    private static System.Collections.Generic.List<Navegar.WindowEngine.API> _window = new System.Collections.Generic.List<Navegar.WindowEngine.API>();

    public override void ReadXMLDelegate(XPathNavigator nav) {
    }

    override public void WriteXMLDelegate(System.Xml.XmlTextWriter xmlWriter) {
    }

    override public void MyGuiCallback(Omnity anOmnity) {
    }

    static public bool isEZ3(int i) {
        return Omnity.anOmnity.finalPassCameras[i].name.ContainsCaseInsensitiveSimple("ez3");
    }

    static public bool isInput(int i) {
        return Omnity.anOmnity.finalPassCameras[i].name.ContainsCaseInsensitiveSimple("input");
    }

    static public bool isRemote(int i) {
        return Omnity.anOmnity.finalPassCameras[i].name.ContainsCaseInsensitiveSimple("remote");
    }

    static public bool isRemoteAndMirror(int i) {
        return Omnity.anOmnity.finalPassCameras[i].name.ContainsCaseInsensitiveSimple("remotemirror");
    }

    static public string passCameraName(int i) {
        return Omnity.anOmnity.finalPassCameras[i].name.ToLower().Replace("ez3", "").Replace("input", "").Replace("remotemirror", "").Replace("remote", "").TrimEnd(' ');
    }

    override public System.Collections.IEnumerator Load(Omnity anOmnity) {
        yield return anOmnity.StartCoroutine(  base.Load(anOmnity));

        Navegar.WindowEngine.Constructor();

        for (int i = 0; i < Omnity.anOmnity.finalPassCameras.Length; i++) {
            if (isEZ3(i)) {
                Navegar.WindowEngine.API wnd = new Navegar.WindowEngine.API();
                wnd.build();
                wnd.funcCreateTexture = CreateRenderTexture;
                wnd.passCamera = i;
                wnd.loadWindow(Omnity.anOmnity.configFileChooser.absolutePathToOmnityConfigFolder.Replace('/', '\\') + "\\" + Omnity.anOmnity.configFileChooser.configXMLFilename2, passCameraName(i), i.ToString(), isInput(i), isRemote(i), isRemoteAndMirror(i));
                wnd.size(1024, 1024);
                wnd.monitor(-2);
                wnd.show(true);
                _window.Add(wnd);
            }
        }
    }

    private System.IntPtr CreateRenderTexture(Navegar.WindowEngine.API me, int width, int height) {
        if (Omnity.anOmnity == null) return System.IntPtr.Zero;

        var finalPassCam = Omnity.anOmnity.finalPassCameras[me.passCamera];
        if (finalPassCam != null) {
            var rts = finalPassCam.renderTextureSettings;

            if (finalPassCam.myCamera != null) {
                finalPassCam.myCamera.targetTexture = null;
            }

            rts.FreeRT();
            rts.height = height;
            rts.width = width;
            finalPassCam.myCamera.targetTexture = rts.GenerateRenderTexture();
            finalPassCam.renderTextureSettings.rt.Create();

            return finalPassCam.renderTextureSettings.rt.GetNativeTexturePtr();
        }

        return System.IntPtr.Zero;
    }

    static public void ResetAll() {
        Omnity anOmnity = Omnity.anOmnity;
        if (anOmnity == null || !anOmnity.PluginEnabled(pluginID)) {
            return;
        }
        for (int i = 0; i < _window.Count; i++) {
            _window[i].reset();
        }
    }

    // Update is called once per frame
    override public void Update() {
        base.Update();
        Omnity anOmnity = Omnity.anOmnity;
        if (anOmnity == null || !anOmnity.PluginEnabled(pluginID)) {
            return;
        }
        for (int i = 0; i < _window.Count; i++) {
            _window[i].createRenderTexture();
        }
        //bool dumpText = false;
        //if (dumpText) {
        //    var finalPassCam = Omnity.anOmnity.finalPassCameras[2];
        //    ClipMap.SaveToFile(finalPassCam.renderTextureSettings.rt, @"d:\temp.png");
        //}
    }

    public override void Unload() {
        for (int i = 0; i < _window.Count; i++) {
            _window[i].destructor();
        }
        _window.Clear();
        base.Unload();
    }
}

#if USE_OMNITYDLLSOURCE
}
#endif