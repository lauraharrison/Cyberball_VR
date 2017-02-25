using UnityEngine;
using System.Collections;

public class EyeSettings : MonoBehaviour {

	public float IPD = 0.06f;
	public Camera leftEye;
	public Camera rightEye;

	public float ipdSpeed=5f;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.J)) {
			IPD -= ipdSpeed * Time.deltaTime;
			if (IPD < 0f)
				IPD = 0f;
		}
		if (Input.GetKey (KeyCode.K)) {
			IPD += ipdSpeed * Time.deltaTime;
		}

		leftEye.transform.localPosition = new Vector3 (-IPD / 2f, 0f, 0f);
		rightEye.transform.localPosition = new Vector3 (IPD / 2f, 0f, 0f);	
	}
}
