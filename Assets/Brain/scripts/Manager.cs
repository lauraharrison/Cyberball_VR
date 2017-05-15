using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class throwDetail{
	public bool throwToPlayer;
	public float waitTime;
	public bool gazeToOther;
	public float gazeTime;
}

public class Manager : MonoBehaviour {
	public throwDetail[] throwSequence;
	public GameObject skyDome;

	int throwIndex;
	bool freeToThrow;
	public Transform leftPlayer;
	public Transform rightPlayer;
	Transform playerTransform;
	
	BallTosser leftTosser;
	BallTosser rightTosser;
	AudioSource leftAudio;
	AudioSource rightAudio;

	SaveToCSV saveFile;
	
	Color horizonColor;
	Color skyColor;
	Color nightColor;
	float stepR;
	float stepG;
	float stepB;
	
	void Awake() {
		leftTosser = leftPlayer.GetComponent<BallTosser> ();
		rightTosser = rightPlayer.GetComponent<BallTosser> ();

		leftAudio = leftPlayer.GetComponent<AudioSource>();
		rightAudio = rightPlayer.GetComponent<AudioSource>();

		playerTransform = transform;
		freeToThrow = true;

		if (!skyDome) {
			Debug.Log ("SkyDome object was not assigned.");
			Application.Quit ();
		}
		
		GameObject saveFileGameObject = GameObject.FindGameObjectWithTag ("SaveFile");
		if (saveFileGameObject == null) {
			Debug.LogError ("GameObject with SaveToCSV scripts needs to be tagged with SaveFile tag");
		} else {
			saveFile = saveFileGameObject.GetComponent<SaveToCSV> ();
			if (saveFile == null) {
				Debug.LogError ("Assign SaveToCSV script to SaveFile GameObject");
			}
		}
	}

	void Start() {
		skyColor = skyDome.GetComponent<Renderer> ().material.GetColor ("_SkyColor");
		horizonColor = skyDome.GetComponent<Renderer> ().material.GetColor ("_HorizonColor");

		nightColor = new Color(11f/255,89f/255,159f/255);
		stepR = (nightColor.r - skyColor.r) / throwSequence.Length;
		stepG = (nightColor.g - skyColor.g) / throwSequence.Length;
		stepB = (nightColor.b - skyColor.b) / throwSequence.Length;
	}
	
	// Update is called once per frame
	void Update () {
		if( (leftTosser.haveBall || rightTosser.haveBall) && throwIndex < throwSequence.Length && freeToThrow)
		{
			StartCoroutine(WaitNThrowBall());
		}
		else{
			if((leftTosser.haveBall || rightTosser.haveBall) && freeToThrow){
				if( Input.GetKeyDown("p") ){
					freeToThrow = false;
					if(Input.GetKey("g"))					
						UserThrow(true,true);
					else
						UserThrow(true,false);
				}
				if( Input.GetKeyDown("o") ){
					freeToThrow = false;
					if(Input.GetKey("g"))
						UserThrow(false,true);
					else
						UserThrow(false,false);
				}
			}
		}
	}
	
	void UserThrow(bool toPlayer, bool gazeToOther){
		Transform throwTarget;
		
		if(leftTosser.haveBall){
			if(toPlayer){
				throwTarget = rightPlayer;
			}
			else{
				throwTarget = playerTransform;
				rightTosser.idleFloat = -1.0f;
			}
			
			StartCoroutine(leftTosser.ThrowBall(throwTarget, !toPlayer, gazeToOther, 2.0f));
		}
		else{
			if(toPlayer){
				throwTarget = leftPlayer;
			}
			else{
				throwTarget = playerTransform;
				leftTosser.idleFloat = 1.0f;
			}
			
			StartCoroutine(rightTosser.ThrowBall(throwTarget, toPlayer, gazeToOther, 2.0f));
		}
		
		freeToThrow = true;
	}

	void NightFallStep(){
		skyColor.r += stepR;
		skyColor.g += stepG;
		skyColor.b += stepB;

		horizonColor.r += stepR;
		horizonColor.g += stepG;
		horizonColor.b += stepB;

		skyDome.GetComponent<Renderer>().material.SetColor ("_SkyColor", skyColor);
		skyDome.GetComponent<Renderer>().material.SetColor ("_HorizonColor", horizonColor);
		Debug.Log("sky was changed");
	}

	IEnumerator WaitNThrowBall(){
		NightFallStep();
		freeToThrow=false;
		yield return new WaitForSeconds(throwSequence[throwIndex].waitTime);
		
		if(throwIndex==0){
			GetComponent<PreGameControl>().CalculateMiddlePoint();
		}
		else{
			ThrowBall();
		}
	}
	
	public void ThrowBall(){
		Transform throwTarget;
		
		if(leftTosser.haveBall){
			if(throwSequence[throwIndex].throwToPlayer){
				throwTarget = rightPlayer;
				StartCoroutine(CallOut(throwSequence[throwIndex].gazeTime, true));
				saveFile.WriteToFile("Stefani to Player",throwSequence[throwIndex].gazeTime);
			}
			else{
				throwTarget = playerTransform;
				rightTosser.idleFloat = -1.0f;
				saveFile.WriteToFile("Stefani to Remy",throwSequence[throwIndex].gazeTime);
			}
			
			StartCoroutine(leftTosser.ThrowBall(throwTarget, !throwSequence[throwIndex].throwToPlayer, throwSequence[throwIndex].gazeToOther, throwSequence[throwIndex].gazeTime));
		}
		else{
			if(throwSequence[throwIndex].throwToPlayer){
				throwTarget = leftPlayer;
				StartCoroutine(CallOut(throwSequence[throwIndex].gazeTime, false));
				saveFile.WriteToFile("Remy to Player",throwSequence[throwIndex].gazeTime);
			}
			else{
				throwTarget = playerTransform;
				leftTosser.idleFloat = 1.0f;
				saveFile.WriteToFile("Remy to Stefani",throwSequence[throwIndex].gazeTime);
			}
			
			StartCoroutine(rightTosser.ThrowBall(throwTarget, throwSequence[throwIndex].throwToPlayer, throwSequence[throwIndex].gazeToOther, throwSequence[throwIndex].gazeTime));
		}
		throwIndex += 1;
		freeToThrow=true;		
	}

	IEnumerator CallOut(float timeSpan, bool leftThrow){
		yield return new WaitForSeconds(1.5f*timeSpan);
		if (leftThrow)
			leftAudio.Play ();
		else
			rightAudio.Play ();

	}
}
