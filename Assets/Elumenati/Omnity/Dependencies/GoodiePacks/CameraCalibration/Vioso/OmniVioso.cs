#if UNITY_STANDALONE_WIN

using OmnityMono;
using OmnityMono.Navegar;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using UnityEngine;

public class OmniVioso : OmnityTabbedFeature {
    static public OmnityPluginsIDs _myOmnityPluginsID = OmnityPluginsIDs.OmniVioso;
    override public OmnityPluginsIDs myOmnityPluginsID { get { return _myOmnityPluginsID; } }

    static public OmniVioso OmniCreateSingleton(GameObject go) {
        return GetSingleton<OmniVioso>(ref singleton, go);
    }

    public static OmniVioso singleton;

    private Material newViosoMaterial {
        get {
            return FinalPassSetup.ApplySettings(textureSource == TextureSource.PerspectiveCamera ? FinalPassShaderType.ViosoShader : FinalPassShaderType.ViosoShaderFlat);
        }
    }

    public enum TextureSource {
        TextureFromApplication = 0,
        PerspectiveCamera = 1,
        FinalPassCamera = 2,
    }

    public TextureSource textureSource = TextureSource.PerspectiveCamera;
    public bool isBlendUsed = true;
    public bool wasCalibrationSpherical = false;
    public float angleAtDomePerimeterDegrees = 90;

    static public System.Func<Texture> getTextureFromApp = () => {
        Debug.LogError("Replace with callback from app");
        return null;
    };

    static public System.Func<bool> getTextureFlipFromApp = () => {
        Debug.LogError("Replace with callback from app");
        return false;
    };

    static public OmniVioso Get() {
        return singleton;
    }

    private void Awake() {
        if (VerifySingleton<OmniVioso>(ref singleton)) {
            Omnity.onReloadEndCallbackPriority.AddLoaderFunction(PriorityEventHandler.Ordering.Order_OmniVioso, CoroutineLoader);
            onNewTextureCallback += () => {
                if (!Omnity.anOmnity.PluginEnabled(OmnityPluginsIDs.OmniVioso)) {
                    this.enabled = false;
                    return;
                } else {
                    this.enabled = true;
                    ReconnectTextures(Omnity.anOmnity);
                }
            };

            Omnity.onSaveCompleteCallback += Save;
        }
    }

    private bool loadedOnce = false;

    public override void ReadXMLDelegate(XPathNavigator nav) {
        textureSource = OmnityHelperFunctions.ReadElementEnumDefault<TextureSource>(nav, "(.//textureSource)", TextureSource.TextureFromApplication);
        Rect r = OmnityHelperFunctions.ReadElementRectDefault(nav, ".//screenMapper", new Rect(-90, -45, 180, 90));
        screenMapperData = new ScreenMapper { rect = r };
        isBlendUsed = OmnityHelperFunctions.ReadElementBoolDefault(nav, "(.//isBlendUsed)", true);
        wasCalibrationSpherical = OmnityHelperFunctions.ReadElementBoolDefault(nav, "(.//wasCalibrationSpherical)", false);
        angleAtDomePerimeterDegrees = OmnityHelperFunctions.ReadElementFloatDefault(nav, "(.//angleAtDomePerimeterDegrees)", 90f);
    }

    public override void WriteXMLDelegate(XmlTextWriter xmlWriter) {
        xmlWriter.WriteElementString("textureSource", textureSource.ToString());

        xmlWriter.WriteElementString("screenMapper", screenMapperData.rect.ToString());
        xmlWriter.WriteElementString("isBlendUsed", isBlendUsed.ToString());
        xmlWriter.WriteElementString("wasCalibrationSpherical", wasCalibrationSpherical.ToString());
        xmlWriter.WriteElementString("angleAtDomePerimeterDegrees", angleAtDomePerimeterDegrees.ToString());
    }

    public override IEnumerator PostLoad() {
        StopAllCoroutines();
        yield return StartCoroutine(CoLoad(Omnity.anOmnity));
    }

    public void OnDestroy() {
        ViosoEngine.Destructor();
    }

    public System.Collections.Generic.List<ViosoIG> list = new System.Collections.Generic.List<ViosoIG>();

    private int nMeshes = 0;
    private int originalSSLength = 0;
    private int originalFPCLength = 0;

    private System.Collections.IEnumerator CoLoad(Omnity myOmnity) {
        nMeshes = 0;
        originalSSLength = 0;
        originalFPCLength = 0;
        if (!loadedOnce) {
            if (!ViosoEngine.ConstructorPart1()) {
                Debug.Log("Failed constructing viosio glue");
            }
            loadedOnce = true;
        }
        string vwfFIleWithExtensionSource = @"C:\Users\Public\Documents\VIOSO\Anyblend\Export\calibration.vwf";
        string vwfFIleWithExtensionDest32 = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, @"Vioso\32bits\calibration.vwf").Replace("/", "\\");
        string vwfFIleWithExtensionDest64 = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, @"Vioso\64bits\calibration.vwf").Replace("/", "\\");
        try {
            System.IO.File.Copy(vwfFIleWithExtensionSource, vwfFIleWithExtensionDest64, true);
            System.IO.File.Copy(vwfFIleWithExtensionSource, vwfFIleWithExtensionDest32, true);
        } catch (System.Exception e) {
            Debug.LogError("Error could not copy vwf file " + e.Message);
        }
        string iniFIleWithoutExtension = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, @"Vioso\64bits\calibration").Replace("/", "\\");

        ViosoEngine.ConstructorPart2(iniFIleWithoutExtension);
        nMeshes = ViosoEngine.API.GetMeshTotal();

        list.Clear();

        originalSSLength = myOmnity.screenShapes.Length;
        originalFPCLength = myOmnity.finalPassCameras.Length;
        myOmnity.ScreenShapeArrayResize(myOmnity.screenShapes.Length + nMeshes, false, true, OmnityScreenShapeType.CustomApplicationLoaded, OmnityMono.FinalPassShaderType.Custom_ApplicationLoaded, true, true);
        myOmnity.FinalPassCameraArrayResize(myOmnity.finalPassCameras.Length + nMeshes, false, false, true, true);

        yield return null;

        for (int i = 0; i < nMeshes; i++) {
            var currentSS = myOmnity.screenShapes[originalSSLength + i];
            var currentFPC = myOmnity.finalPassCameras[originalFPCLength + i];

            currentFPC.normalizedViewportRect = new Rect(0, 0, 1, 1);
            CreateMesh(iniFIleWithoutExtension, i, currentSS, currentFPC);

            switch (textureSource) {
                case TextureSource.TextureFromApplication:
                    currentSS.automaticallyConnectTextures = false;
                    break;

                case TextureSource.PerspectiveCamera:
                    currentSS.automaticallyConnectTextures = true;
                    break;

                case TextureSource.FinalPassCamera:
                    currentSS.automaticallyConnectTextures = false;
                    Debug.LogWarning("mode not tested");
                    break;

                default:
                    Debug.LogWarning("mode not found");
                    break;
            }
        }
        SetExtentsNowExpensive();

        switch (textureSource) {
            case TextureSource.TextureFromApplication:
                ReconnectTextures(Omnity.anOmnity);
                break;

            case TextureSource.PerspectiveCamera:
                myOmnity.DoConnectTextures();
                break;

            case TextureSource.FinalPassCamera:
                ReconnectTextures(Omnity.anOmnity);
                Debug.LogWarning("mode not tested");
                break;

            default:
                Debug.LogWarning("mode not found");
                break;
        }

        OmnityWindowsExtensions.FindWindowByAltCache();
        ActivateDisplays();

        ViosoEngine.API.Destructor();

        var ez1 = EasyMultiDisplay.Get();
        if (ez1) {
            ez1.TrySetPositionAndResolutionCallback(Omnity.anOmnity);
        }
    }

    private void ActivateDisplays() {
        list.Sort((a, b) => {
            if (a.monitorOffsetY == 0 && a.monitorOffsetX == 0) {
                return 0.CompareTo(1);
            } else if (b.monitorOffsetY == 0 && b.monitorOffsetX == 0) {
                return 1.CompareTo(0);
            } else if (a.monitorOffsetY != b.monitorOffsetY) {
                return a.monitorOffsetY.CompareTo(b.monitorOffsetY);
            } else if (a.monitorOffsetX != b.monitorOffsetX) {
                return a.monitorOffsetX.CompareTo(b.monitorOffsetX);
            } else {
                return 0;
            }
        });

        bool hasMainDisplay = false;

        for (int i = 0; i < list.Count; i++) {
            if (0 == list[i].monitorOffsetX && 0 == list[i].monitorOffsetY) {
                hasMainDisplay = true;
                break;
            }
        }

        for (int i = 0; i < list.Count; i++) {
            int targetDisplay = hasMainDisplay ? i : i + 1;
            list[i].finalPassCamera.myCamera.targetDisplay = targetDisplay;
            list[i].finalPassCamera.targetDisplay = targetDisplay;

            list[i].finalPassCamera.cullingMask = 1 << list[i].screenShape.layer;
            list[i].finalPassCamera.myCamera.cullingMask = 1 << list[i].screenShape.layer;

            if (targetDisplay > UnityEngine.Display.displays.Length - 1) {
                if (!Application.isEditor) {
                    Debug.LogWarning("Warning target display " + targetDisplay + " overflows displays list " + UnityEngine.Display.displays.Length + " usually this is because a monitor was removed since the calibration happened.");
                }
                targetDisplay = UnityEngine.Display.displays.Length - 1;
            }
            if (targetDisplay < UnityEngine.Display.displays.Length) {
                UnityEngine.Display.displays[targetDisplay].Activate();
            }
        }
    }

    public override string BaseName {
        get {
            return "OmniViosoSettings.xml";
        }
    }

    private float _Gamma1 = 1.0f;

    public override void MyGuiCallback(Omnity anOmnity) {
        if (!anOmnity.pluginIDs.Contains((int)OmnityPluginsIDs.OmniVioso)) {
            return;
        }
        Tabs(anOmnity);

        if (OmnityHelperFunctions.EnumInputResetWasChanged<TextureSource>("SDM.textureSource", "textureSource", ref textureSource, TextureSource.PerspectiveCamera, 1)) {
            Debug.LogError("TODO DEAL WITH THIS CHANGE SDM.textureSource");
        }

        if (OmnityHelperFunctions.FloatInputResetSliderWasChanged("Gamma1", ref _Gamma1, 0.0f, 1.0f, 3.0f)) {
            for (int i = 0; i < anOmnity.screenShapes.Length; i++) {
                anOmnity.screenShapes[i].renderer.sharedMaterial.SetFloat("_Gamma1", _Gamma1);
            }
        }
        isBlendUsed = OmnityHelperFunctions.BoolInputReset("Use Blend", isBlendUsed, true);
        wasCalibrationSpherical = OmnityHelperFunctions.BoolInputReset("Was Image Calibration Spherical", wasCalibrationSpherical, false);
        GUILayout.Label("(set false for rectangular or equirectangular calibration preview image, set true for fulldome calibration preview image)", GUILayout.MaxWidth(400));

        if (wasCalibrationSpherical) {
            angleAtDomePerimeterDegrees = OmnityHelperFunctions.FloatInputResetSlider("AngleAtDomePerimeterDegrees", angleAtDomePerimeterDegrees, 0, 90, 180);
        } else {
            screenMapperData.xMin = OmnityHelperFunctions.FloatInputReset("xMin", screenMapperData.xMin, -90);
            screenMapperData.xMax = OmnityHelperFunctions.FloatInputReset("xMax", screenMapperData.xMax, 90);
            screenMapperData.yMin = OmnityHelperFunctions.FloatInputReset("yMin", screenMapperData.yMin, -90);
            screenMapperData.yMax = OmnityHelperFunctions.FloatInputReset("yMax", screenMapperData.yMax, 90);
        }
        
        GUILayout.Label(@"Make sure to export your latest VIOSO config to C:\Users\Public\Documents\VIOSO\Anyblend\Export\calibration.vwf", GUILayout.MaxWidth(400));
        GUILayout.Label(@"textureSource should be PerspectiveCamera for most applications.  This mode will use the Perspective cameras to generate a cube map like data structure and uses projective perspective mapping to map the captured perspective onto the screen geometry.  Developers can opt for TextureFromApplication if they want to map a single equirectangular or fisheye that passed to Omnity via passing a function that returns a texture to this plugin.  Here is one way to do it.  OmniVioso.Get().getTextureFromApp =()=>{ return myTexture; };", GUILayout.MaxWidth(400));
        


        SetExtentsNowExpensive();
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save")) {
            Save(anOmnity);
        }
        if (GUILayout.Button("Reload")) {
            anOmnity.StartCoroutine(Load(anOmnity));
        }
        GUILayout.EndHorizontal();
    }

    public void Tabs(Omnity anOmnity) {
        GUILayout.BeginHorizontal();
        GUILayout.EndHorizontal();
    }

    // Use this for initialization
    private void Start() {
    }

    private void LateUpdate() {
        if (list != null) {
            foreach (var m in list) {
                m.DrawCamera();
            }
        }
    }

    public ScreenMapper screenMapperData = new ScreenMapper();

    [System.Serializable]
    public class ScreenMapper {
        public float xMin = -90;
        public float xMax = 90;
        public float yMin = -45;
        public float yMax = 45;

        public Rect rect {
            get {
                return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            }
            set {
                xMin = value.xMin;
                xMax = value.xMax;
                yMin = value.yMin;
                yMax = value.yMax;
            }
        }
    }

    public System.Action onNewTextureCallback = delegate {
    };

    public System.Func<Texture> getLatestTextureCallback = () => {
        Texture t = null;
        if (OmniVioso.Get() == null || Omnity.anOmnity == null) {
            return null;
        }
        switch (OmniVioso.Get().textureSource) {
            case TextureSource.TextureFromApplication:
                return getTextureFromApp();

            case TextureSource.FinalPassCamera:
                if (Omnity.anOmnity.finalPassCameras.Length > 0) {
                    t = Omnity.anOmnity.finalPassCameras[0].renderTextureSettings.rt;
                }
                break;

            case TextureSource.PerspectiveCamera:
                if (Omnity.anOmnity.cameraArray.Length > 0) {
                    t = Omnity.anOmnity.cameraArray[0].renderTextureSettings.rt;
                }
                break;

            default:

                Debug.LogError("Unknown type " + OmniVioso.Get().textureSource);
                return null;
        }
        if (t == null) {
            Debug.LogError(OmniVioso.Get().textureSource.ToString() + " Error: Make sure to add a camera with a render texture");
        }
        return t;
    };

    public System.Func<bool> getLatestFlipY = () => {
        switch (OmniVioso.Get().textureSource) {
            case TextureSource.TextureFromApplication:
                return getTextureFlipFromApp();

            case TextureSource.FinalPassCamera:
                return false;

            case TextureSource.PerspectiveCamera:
                return false;

            default:
                Debug.LogError("Unknown " + OmniVioso.Get().textureSource);
                return false;
        }
    };

    public void ReconnectTextures(Omnity anOmnity) {
        if (!anOmnity && anOmnity.screenShapes != null) {
            return;
        }
        if (!anOmnity.PluginEnabled(OmnityPluginsIDs.OmniVioso)) {
            return;
        }

        foreach (var ss in anOmnity.screenShapes) {
            if (!ss.automaticallyConnectTextures) {
                if (ss.renderer != null && ss.renderer.sharedMaterial != null) {
                    ss.renderer.sharedMaterial.SetTexture("_Cam0Tex", getTextureFromApp());
                    if (getLatestFlipY()) {
                        ss.renderer.sharedMaterial.SetTextureOffset("_Cam0Tex", new Vector2(0, 1));
                        ss.renderer.sharedMaterial.SetTextureScale("_Cam0Tex", new Vector2(1, -1));
                    } else {
                        ss.renderer.sharedMaterial.SetTextureOffset("_Cam0Tex", new Vector2(0, 0));
                        ss.renderer.sharedMaterial.SetTextureScale("_Cam0Tex", new Vector2(1, 1));
                    }
                }
            }
        }
    }

    private void CreateMesh(string iniFIleWithoutExtension, int meshNumber, ScreenShape screenshape, FinalPassCamera fpc) {
        ViosoEngine.API.VIOSO_DATA meshFile = ViosoEngine.API.GetMesh(meshNumber);

        var mr = screenshape.trans.gameObject.GetComponent<MeshRenderer>();
        var mf = screenshape.trans.gameObject.GetComponent<MeshFilter>();

        mr.material = newViosoMaterial;

        var mesh = mf.mesh = ViosoEngine.API.GetUnityMesh(meshFile, OmnityPlatformDefines.OptimizeMesh);

        screenshape.layer = 31;
        fpc.projectorType = OmnityProjectorType.FisheyeFullDome;
        fpc.name = fpc.myCameraTransform.gameObject.name = "VIOSOCAM_IG" + meshNumber;
        fpc.myCamera.cullingMask = 1 << screenshape.layer;

        mr.enabled = false;

        mr.sharedMaterial.SetTexture("_BlendTexture", ViosoEngine.API.GetBlendTexture(meshNumber, meshFile));
        mr.sharedMaterial.SetTextureOffset("_BlendTexture", new Vector2(.5f, .5f));
        mr.sharedMaterial.SetTextureScale("_BlendTexture", new Vector2(.5f, -.5f));

        list.Add(new ViosoIG {
            finalPassCamera = fpc,
            screenShape = screenshape,
            mesh = mesh,
            monitorHeight = meshFile.monitorHeight,
            monitorWidth = meshFile.monitorWidth,
            monitorOffsetX = meshFile.monitorOffsetX,
            monitorOffsetY = meshFile.monitorOffsetY
        });
    }

    private void SetExtentsNowExpensive() {
        for (int i = 0; i < nMeshes; i++) {
            int ss = i + originalSSLength;
            if (ss < Omnity.anOmnity.screenShapes.Length && Omnity.anOmnity.screenShapes[ss].trans != null && Omnity.anOmnity.screenShapes[ss].trans.gameObject.GetComponent<MeshRenderer>() != null && Omnity.anOmnity.screenShapes[ss].trans.gameObject.GetComponent<MeshRenderer>().sharedMaterial) {
                var mat = Omnity.anOmnity.screenShapes[ss].trans.gameObject.GetComponent<MeshRenderer>().sharedMaterial;
                mat.SetFloat("leftDegrees", screenMapperData.xMin);
                mat.SetFloat("rightDegrees", screenMapperData.xMax);
                mat.SetFloat("topDegrees", screenMapperData.yMax);
                mat.SetFloat("bottomDegrees", screenMapperData.yMin);
                mat.SetFloat("_wasCalibrationSpherical", OmniVioso.Get().wasCalibrationSpherical ? 1f : 0f);
                mat.SetFloat("_isBlendUsed", OmniVioso.Get().isBlendUsed ? 1f : 0f);
                mat.SetFloat("_angleAtDomePerimeterDegrees", OmniVioso.Get().angleAtDomePerimeterDegrees);
            }
        }
    }
}

[System.Serializable]
public class ViosoIG {

    [System.NonSerialized]
    public FinalPassCamera finalPassCamera;

    [System.NonSerialized]
    public ScreenShape screenShape;

    [System.NonSerialized]
    public Mesh mesh;

    public void DrawCamera() {
        if (screenShape != null && screenShape.renderer != null && finalPassCamera != null) {
            Graphics.DrawMesh(mesh, screenShape.trans.localToWorldMatrix, screenShape.renderer.material, screenShape.layer, finalPassCamera.myCamera);
        }
    }

    public int monitorWidth = 0;
    public int monitorHeight = 0;
    public int monitorOffsetX = 0;
    public int monitorOffsetY = 0;
}

#endif