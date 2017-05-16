using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class throwDetail{
	public bool throwToPlayer;
	public float waitTime;
	public bool gazeToOther;
	public float gazeTime;
}

public class Manager : MonoBehaviour {
	public List<throwDetail> throwSequence;
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
	string sequenceFilePath = @"LogFiles/throwSequence.csv";
	public bool readCSVsequence;
	
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

		//read throwSequenceFile
		if(readCSVsequence){
			throwSequence.Clear();
			FileStream fileStream = File.Open(sequenceFilePath, FileMode.Open);
			StreamReader sr = new StreamReader(fileStream);
			bool readingData = false;
			while (!sr.EndOfStream){
				string newLine = sr.ReadLine();
				Debug.Log(newLine);
				if (readingData) {
					string[] elements = newLine.Split (';');
					throwDetail newThrowDetail = new throwDetail ();
					for (int i = 0; i < elements.Length; i++) {
						switch (i) {
						case 0:
							newThrowDetail.throwToPlayer = bool.Parse (elements [i].Trim ());
							break;
						case 1:
							newThrowDetail.waitTime = float.Parse (elements [i].Trim ());
							break;
						case 2:
							float gazeSpan = float.Parse (elements [i].Trim ());
							newThrowDetail.gazeTime = gazeSpan;
							if (gazeSpan > 0)
								newThrowDetail.gazeToOther = true;
							break;
						}
						//Debug.Log ("element: " + elements [i].Trim ());
					}
					throwSequence.Add (newThrowDetail);

				}
				readingData = true;
			}
			fileStream.Close ();
			Debug.Log("End of file reached. ThrowSequence populated from csv file.");
		}
	}

	void Start() {
		skyColor = skyDome.GetComponent<Renderer> ().material.GetColor ("_SkyColor");
		horizonColor = skyDome.GetComponent<Renderer> ().material.GetColor ("_HorizonColor");

		nightColor = new Color(11f/255,89f/255,159f/255);
		stepR = (nightColor.r - skyColor.r) / throwSequence.Count;
		stepG = (nightColor.g - skyColor.g) / throwSequence.Count;
		stepB = (nightColor.b - skyColor.b) / throwSequence.Count;
	}
	
	// Update is called once per frame
	void Update () {
		if( (leftTosser.haveBall || rightTosser.haveBall) && throwIndex < throwSequence.Count && freeToThrow)
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
		Transform gazeTarget;
		
		if(leftTosser.haveBall){
			if(toPlayer){
				gazeTarget = rightPlayer;
			}
			else{
				gazeTarget = playerTransform;
				rightTosser.idleFloat = -1.0f;
			}
			
			StartCoroutine(leftTosser.ThrowBall(gazeTarget, !toPlayer, gazeToOther, 2.0f));
		}
		else{
			if(toPlayer){
				gazeTarget = leftPlayer;
			}
			else{
				gazeTarget = playerTransform;
				leftTosser.idleFloat = 1.0f;
			}
			
			StartCoroutine(rightTosser.ThrowBall(gazeTarget, toPlayer, gazeToOther, 2.0f));
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
		Transform gazeTarget;
		
		if(leftTosser.haveBall){
			if(throwSequence[throwIndex].throwToPlayer){
				gazeTarget = rightPlayer;
				StartCoroutine(CallOut(throwSequence[throwIndex].gazeTime, true));
				saveFile.WriteToFile("Stefani to Player",throwSequence[throwIndex].gazeTime);
			}
			else{
				gazeTarget = playerTransform;
				rightTosser.idleFloat = -1.0f;
				saveFile.WriteToFile("Stefani to Remy",throwSequence[throwIndex].gazeTime);
			}
			
			StartCoroutine(leftTosser.ThrowBall(gazeTarget, !throwSequence[throwIndex].throwToPlayer, throwSequence[throwIndex].gazeToOther, throwSequence[throwIndex].gazeTime));
		}
		else{
			if(throwSequence[throwIndex].throwToPlayer){
				gazeTarget = leftPlayer;
				StartCoroutine(CallOut(throwSequence[throwIndex].gazeTime, false));
				saveFile.WriteToFile("Remy to Player",throwSequence[throwIndex].gazeTime);
			}
			else{
				gazeTarget = playerTransform;
				leftTosser.idleFloat = 1.0f;
				saveFile.WriteToFile("Remy to Stefani",throwSequence[throwIndex].gazeTime);
			}
			
			StartCoroutine(rightTosser.ThrowBall(gazeTarget, throwSequence[throwIndex].throwToPlayer, throwSequence[throwIndex].gazeToOther, throwSequence[throwIndex].gazeTime));
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
