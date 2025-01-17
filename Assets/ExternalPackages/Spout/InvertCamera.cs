﻿//http://docs.unity3d.com/412/Documentation/ScriptReference/Camera.OnPreCull.html
using UnityEngine;
using System.Collections;

namespace SpoutNamespace{

	[RequireComponent (typeof(Camera))]
	[ExecuteInEditMode]
	public class InvertCamera : MonoBehaviour {
		//public Camera camera;
		void Start () {
			//camera = get
		}
		
		void OnPreCull () {
			GetComponent<Camera>().ResetWorldToCameraMatrix();
			GetComponent<Camera>().ResetProjectionMatrix();
			GetComponent<Camera>().projectionMatrix = GetComponent<Camera>().projectionMatrix * Matrix4x4.Scale(new Vector3(1, -1, 1));
		}
		
		void OnPreRender () {
            OmnityPlatformDefines.SetInvertCulling(true);
		}
		
		void OnPostRender () {
            OmnityPlatformDefines.SetInvertCulling(false);
		}
		
	}

}