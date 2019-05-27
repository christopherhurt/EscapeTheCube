using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MocapCameraController : MonoBehaviour {

	// drag the MocapReceiver object from your scene Heirarchy into this slot in the Inspector for this object
	public MotionCaptureStreamingReceiver mocapReceiver;

	// in the Inspector, type the name of the rigidbody you would like to correspond with this object
	public string rbName;

    // the main Oculus camera that receives rotation data from the IMU
    public Transform rotatingCamera;  

	void Start () {
		// tell the MocapReceiver to call the GetWandPosition method from this script every time it gets new data for the rigidbody named in the first argument
		mocapReceiver.RegisterRigidBodyDelegate (rbName, SetTransform);
	}

	// this is the method that MocapReceiver will call when it gets new data for the rigidbody you named
	public void SetTransform(Vector3 position, Quaternion rotation){
        transform.position = position - rotatingCamera.up * 0.25f + rotatingCamera.forward * 0.1f;
		//transform.rotation = rotation;  // while rotation data is being received from HMD IMU, so do not use this line
	}
}
