using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class DisableTracking : MonoBehaviour {
    private Camera cam;
    //private Vector3 startPos;

    void Start () {
        cam = GetComponentInChildren<Camera>();
        //startPos = transform.localPosition;
    }
	
    void Update () {
        //transform.localPosition = startPos - cam.transform.localPosition;
		UnityEngine.VR.InputTracking.disablePositionalTracking = true;
        transform.localRotation = Quaternion.Inverse(cam.transform.localRotation);
    }
}
