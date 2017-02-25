using UnityEngine;
using System.Collections;

public class FlyingOBJcontrol : MonoBehaviour {
	public GameObject mosquito;
	public GameObject lamp;
	public GameObject clock;
	public GameObject flowerVase;

	public Transform target;
	public float maxDistance=10f;
	public float minDistance=1f;
	public float maxSpeed=100f;
	float speed;
	public float acel=10f;
	float currAcel;

	GameObject currFlyingOBJ;
	Vector3 flyingPos;
	Vector3 oppositeDir;
	bool paused;
	int statue=1;

	// Use this for initialization
	void Start () {
		flyingPos = new Vector3 ();
		flyingPos = target.position + 0.01f*maxDistance*target.forward;
		oppositeDir = new Vector3();

		speed = 0f;
		currAcel = acel;
		currFlyingOBJ = (GameObject)GameObject.Instantiate(mosquito, flyingPos, target.rotation);
		
		Debug.Log("Use 1-4 to choose the object, P pauses their movement, J or K increase/decrease IPD");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Alpha1))changeFlyingObj (1);
		if (Input.GetKeyDown (KeyCode.Alpha2))changeFlyingObj (2);
		if (Input.GetKeyDown (KeyCode.Alpha3))changeFlyingObj (3);
		if (Input.GetKeyDown (KeyCode.Alpha4))changeFlyingObj (4);

		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (paused) {
				paused = false;
				Time.timeScale = 1f;
			} else {
				paused = true;
				Time.timeScale = 0f;
			}		
		}
		if (Input.GetKeyDown (KeyCode.P)) {
			if (statue==1) {
				statue = 0;
			} else {
				statue = 1;
			}		
		}
		if (Vector3.Distance (target.position, flyingPos) >= maxDistance) {
			currAcel = -acel;
		}
		if (Vector3.Distance (target.position, flyingPos) <= minDistance) {
			currAcel = acel;
		}
		speed += currAcel * statue * Time.deltaTime;
		if (speed >= maxSpeed) {
			speed = maxSpeed;
		}
		if(speed<= -maxSpeed){
			speed = -maxSpeed;
		}

		oppositeDir = flyingPos - target.position;
		flyingPos += speed * statue * Time.deltaTime * oppositeDir.normalized;
		currFlyingOBJ.transform.position = flyingPos;
	}
	void changeFlyingObj(int objID){
		Destroy (currFlyingOBJ);
		switch(objID){
		case 1:
			currFlyingOBJ = (GameObject)GameObject.Instantiate (mosquito,flyingPos,target.rotation);
			break;
		case 2:
			currFlyingOBJ = (GameObject)GameObject.Instantiate (lamp,flyingPos,target.rotation);
			break;
		case 3:
			currFlyingOBJ = (GameObject)GameObject.Instantiate (clock,flyingPos,target.rotation);
			break;
		case 4:
			currFlyingOBJ = (GameObject)GameObject.Instantiate (flowerVase,flyingPos,target.rotation);
			break;
		}
	}
}
