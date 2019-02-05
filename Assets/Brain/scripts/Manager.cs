using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
	public string sequenceFilePath = @"LogFiles/throwSequence.csv";
	bool readCSVsequence;
	
	Color horizonColor;
	Color skyColor;

	Color nightColor;

	Color currentSkyColor;
	Color currentHorizonColor;
	public float nightFallSpeed = 10f;
	public bool doNightFall = false;

	float stepR;
	float stepG;
	float stepB;
	
	bool endAfterThrowSeq;
	float timer;
	public float fadeTime = 1.0f;
	float startFade;
	bool fadingout;
	Image fadeTex_right;
	Image fadeTex_left;
	Color fadeColor;

	starterData starterData;

	void Awake() {
		starterData = GetComponent<starterData>();
		
		//check if pseudoGame
		if(starterData.pseudoGame){
			readCSVsequence = false;
			endAfterThrowSeq = false;			
		}
		else{
			readCSVsequence = true;
			endAfterThrowSeq = true;
		}
		
		sequenceFilePath = starterData.sequenceFilePath + starterData.throwSequence;
		Debug.Log("throwSequence path: "+sequenceFilePath);
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
        if (readCSVsequence){
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
		currentSkyColor = skyColor;
		currentHorizonColor = horizonColor;

		//decrease this number so night is not so dark
		nightColor = new Color(11f/255,89f/255,159f/255);
		stepR = (nightColor.r - skyColor.r) / throwSequence.Count;
		stepG = (nightColor.g - skyColor.g) / throwSequence.Count;
		stepB = (nightColor.b - skyColor.b) / throwSequence.Count;
		
		fadeTex_right = GameObject.Find("GUI_right/fadeTexture").GetComponent<Image>();
		fadeTex_left = GameObject.Find("GUI_left/fadeTexture").GetComponent<Image>();
		fadeColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        saveFile.startFile();
    }
	
	// Update is called once per frame
	void Update(){
		if(Input.GetKey(KeyCode.R)){
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			SceneManager.LoadScene("startScreen");
		}
			
		if(Input.GetKey(KeyCode.Q))
			Application.Quit();
		
		timer += Time.deltaTime;		
		if(fadingout){
			if(timer <= startFade + fadeTime){
				fadeColor.a = (timer-startFade)/fadeTime;
				fadeTex_right.color = fadeColor;
				fadeTex_left.color = fadeColor;
				Debug.Log("fade alpha: "+fadeColor.a.ToString());
			}
			else{
				fadeColor.a = 1.0f;
				fadeTex_right.color = fadeColor;
				fadeTex_left.color = fadeColor;
				fadingout = false;
				
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				SceneManager.LoadScene("startScreen");
			}
		}
		
		if( (leftTosser.haveBall || rightTosser.haveBall) && throwIndex < throwSequence.Count && freeToThrow)
		{
			StartCoroutine(WaitNThrowBall());
		}
		else{
			if((leftTosser.haveBall || rightTosser.haveBall) && freeToThrow){
				if(endAfterThrowSeq){
					if(!fadingout){
						startFade = timer;
					}
					fadingout = true;
				}
				else{
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

		if(doNightFall){
			currentSkyColor.r = Mathf.Lerp(currentSkyColor.r, skyColor.r, nightFallSpeed*Time.deltaTime);
			currentSkyColor.g = Mathf.Lerp(currentSkyColor.g, skyColor.g, nightFallSpeed*Time.deltaTime);
			currentSkyColor.b = Mathf.Lerp(currentSkyColor.b, skyColor.b, nightFallSpeed*Time.deltaTime);

			currentHorizonColor.r = Mathf.Lerp(currentHorizonColor.r, horizonColor.r, nightFallSpeed*Time.deltaTime);
			currentHorizonColor.g = Mathf.Lerp(currentHorizonColor.g, horizonColor.g, nightFallSpeed*Time.deltaTime);
			currentHorizonColor.b = Mathf.Lerp(currentHorizonColor.b, horizonColor.b, nightFallSpeed*Time.deltaTime);
				
			skyDome.GetComponent<Renderer>().material.SetColor ("_SkyColor", currentSkyColor);
			skyDome.GetComponent<Renderer>().material.SetColor ("_HorizonColor", currentHorizonColor);
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

		Debug.Log("sky was changed");
	}

	IEnumerator WaitNThrowBall(){
		NightFallStep();
		freeToThrow=false;
		yield return new WaitForSeconds(throwSequence[throwIndex].waitTime);
		
		if(throwIndex==0){
			//GetComponent<PreGameControl>().CalculateMiddlePoint();
			Debug.Log("<color=blue>Manager started!</color>");
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
				if(throwIndex == 0)
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
				if(throwIndex == 0)
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
