using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnidentifiedMarkersExample : MonoBehaviour {

    // drag the MocapReceiver object from your scene Heirarchy into this slot in the Inspector for this object
    public MotionCaptureStreamingReceiver mocapReceiver;

    void Start() {
        // tell the MocapReceiver to call the GetWandPosition method from this script every time it gets new data for the rigidbody named in the first argument
        mocapReceiver.RegisterUnidentifiedMarkersDelegate(GetUnidentifiedMarkers);
    }

    // this is the method that MocapReceiver will call when it gets new data for the rigidbody you named
    public void GetUnidentifiedMarkers(List<Vector3> positions) {
        for (int i = 0; i < positions.Count; i++) {
            Debug.Log("position #" + i + ": " + positions[i]);
        }
    }
}
