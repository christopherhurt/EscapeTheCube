#if UNITY_STANDALONE_WIN

using SpoutNamespace;
using System.Xml.XPath;
using UnityEngine;

public class OmniSpout : OmnityTabbedFeature {
    static public OmnityPluginsIDs _myOmnityPluginsID = OmnityPluginsIDs.OmniSpout;
    override public OmnityPluginsIDs myOmnityPluginsID { get { return _myOmnityPluginsID; } }

    static public OmniSpout OmniCreateSingleton(GameObject go) {
        return GetSingleton<OmniSpout>(ref singleton, go);
    }

    static public OmniSpout singleton = null;

    private void Awake() {
        if (VerifySingleton<OmniSpout>(ref singleton)) {
            Omnity.onReloadEndCallbackPriority.AddLoaderFunction(PriorityEventHandler.Ordering.Order_OmniSpout, CoroutineLoader);
            Omnity.onReloadStartCallback += (Omnity anOmnity) => { Unload(); };
            Omnity.onSaveCompleteCallback += Save;
        }
    }

    public override string BaseName {
        get {
            return "OmniSpout";
        }
    }

    public override System.Collections.IEnumerator PostLoad() {
        if (null == SpoutNamespace.Spout.instance) {
            Debug.LogError("Spout Failed to initalize");
            yield break;
        }
        foreach (var cam in Omnity.anOmnity.finalPassCameras) {
            if (cam.renderTextureSettings.enabled) {
                if (cam.renderTextureSettings.mipmap == true) {
                    Debug.LogError("finalPassCamera.renderTextureSettings.mipmap  must be false for spout to work. Please Save and Reload");
                    cam.renderTextureSettings.mipmap = false;
                }
                SpoutSender sender = cam.myCameraTransform.gameObject.AddComponent<SpoutSender>();
                senders.Add(cam, sender);
                if (senders.Count == 0) {
                    sender.name = sender.sharingName = spoutName;
                } else {
                    sender.name = sender.sharingName = spoutName + senders.Count.ToString(); ;
                }
                sender.texture = cam.myCamera.targetTexture;
            }
        }
        yield break;
    }

    public override void Unload() {
        foreach (var kvp in senders) {
            kvp.Value.texture = null;
            GameObject.Destroy(kvp.Value);
        }
        senders.Clear();
    }

    private System.Collections.Generic.Dictionary<FinalPassCamera, SpoutSender> senders = new System.Collections.Generic.Dictionary<FinalPassCamera, SpoutSender>();

    /// <param name="xmlWriter">The XML writer.</param>
    override public void WriteXMLDelegate(System.Xml.XmlTextWriter xmlWriter) {
        xmlWriter.WriteElementString("spoutName", spoutName);
    }

    private string spoutName = "OmniSpout";

    public override void ReadXMLDelegate(XPathNavigator nav) {
        spoutName = OmnityHelperFunctions.ReadElementStringDefault(nav, ".//spoutName", "OmniSpout");
    }

    public override void MyGuiCallback(Omnity anOmnity) {
        if (!anOmnity.PluginEnabled(OmnityPluginsIDs.OmniSpout)) {
            return;
        }
        spoutName = OmnityHelperFunctions.TextBoxInputReset("spoutName", spoutName, "OmniSpout");
        GUILayout.TextArea("To add omnispout to your config, one final pass camera is required that has a render texture on it.  This will be sent as \"OmniSpout\" and will not show up in this application's display.  A second final pass camera is recommended to provide a view within this application's window. Changes to name/mipmapping need a reload.");
    }
}

#endif