using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PreGameControl : MonoBehaviour {
	Transform myTransform;
	public Transform leftPlayer;
	public Transform rightPlayer;
	friendControl leftFriend;
	friendControl rightFriend;
	Manager gameManager;
	
	public float rotateSpeed = 10f;
	public float startupTime = 2.5f;
	public bool inPreGame = true;
	public bool wakingUp = true;
	bool invited2play;
	
	bool watchingToStart;
	AudioSource leftPlayerSound;
	AudioSource rightPlayerSound;
	
	public float lookAngle = 15f;

	Vector3 middleTargetDir;
	UnityEngine.AI.NavMeshAgent navAgent;
	public Transform[] playerNavPoints;
	int navPointIndex;
	public AudioClip greeting1;
	public AudioClip greeting2;
	bool playersActive;
	
	public Transform CameraTransform;
	public float bobAmp = 0.01f;
	public float bobFreq = 10f;
	bool walking = true;
	
	Text msgToStartUI_right;
	Text msgToLookUI_right;
	Text msgToLookShadUI_right;
	Text msgToStartGameUI_right;
	Text msgToStartGameShadUI_right;
	
	Text msgToStartUI_left;
	Text msgToLookUI_left;
	Text msgToLookShadUI_left;
	Text msgToStartGameUI_left;
	Text msgToStartGameShadUI_left;
	
	int lookLearned;
	public int lookLearnTreshold = 100;
	public float msgFadeTime = 0.5f;
	
	Animator leftAnim;
	
	float horizontal;
	float timer;
	float fadeTime = 1.0f;
	float startFade;
	bool fadingin;
	Image fadeTex_right;
	Image fadeTex_left;
	Color fadeColor;

	starterData starterData;	
		
	// Use this for initialization
	void Start () {
		Cursor.visible = false;
		myTransform = transform;
		gameManager = GetComponent<Manager>();
		navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		leftFriend = leftPlayer.GetComponent<friendControl>();
		leftPlayerSound = leftPlayer.GetComponent<AudioSource> ();
		leftAnim = leftPlayer.GetComponent<Animator>();

		rightFriend = rightPlayer.GetComponent<friendControl>();
		rightPlayerSound = rightPlayer.GetComponent<AudioSource>();
		
		starterData = GetComponent<starterData>();
		if(starterData.pseudoGame){
			GameObject.Find("GUI_right/label").SetActive(true);
			GameObject.Find("GUI_left/label").SetActive(true);
		}
		else{
			GameObject.Find("GUI_right/label").SetActive(false);
			GameObject.Find("GUI_left/label").SetActive(false);
		}
		
		msgToStartUI_right = GameObject.Find("GUI_right/msgToStart").GetComponent<Text>();
		msgToStartUI_right.gameObject.SetActive(false);
		msgToStartUI_left = GameObject.Find("GUI_left/msgToStart").GetComponent<Text>();
		msgToStartUI_left.gameObject.SetActive(false);
		
		
		msgToLookUI_right = GameObject.Find("GUI_right/msgToLook").GetComponent<Text>();
		msgToLookUI_right.enabled = true;
		msgToLookUI_left = GameObject.Find("GUI_left/msgToLook").GetComponent<Text>();
		msgToLookUI_left.enabled = true;
		
		msgToLookShadUI_right = GameObject.Find("GUI_right/msgToLookShadow").GetComponent<Text>();
		msgToLookShadUI_right.enabled = true;
		msgToLookShadUI_left = GameObject.Find("GUI_left/msgToLookShadow").GetComponent<Text>();
		msgToLookShadUI_left.enabled = true;
		
		msgToStartGameUI_right = GameObject.Find("GUI_right/msgToStartGame").GetComponent<Text>();
		msgToStartGameUI_right.enabled = false;
		msgToStartGameUI_left = GameObject.Find("GUI_left/msgToStartGame").GetComponent<Text>();
		msgToStartGameUI_left.enabled = false;
		msgToStartGameShadUI_right = GameObject.Find("GUI_right/msgToStartGameShad").GetComponent<Text>();
		msgToStartGameShadUI_right.enabled = false;
		msgToStartGameShadUI_left = GameObject.Find("GUI_left/msgToStartGameShad").GetComponent<Text>();
		msgToStartGameShadUI_left.enabled = false;
		
		//StartCoroutine(Startup());
		fadeTex_right = GameObject.Find("GUI_right/fadeTexture").GetComponent<Image>();
		fadeTex_left = GameObject.Find("GUI_left/fadeTexture").GetComponent<Image>();
		fadeColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
		fadeTime = gameManager.fadeTime;
		fadingin = true;
		startFade = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.R)){
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			SceneManager.LoadScene("startScreen");
		}
			
		if(Input.GetKey(KeyCode.Q))
			Application.Quit();
		
		timer += Time.deltaTime;
		if(fadingin){
			if(timer <= startFade + fadeTime){
				fadeColor.a = 1.0f - (timer-startFade)/fadeTime;
				//Debug.Log("fade alpha: "+fadeColor.a.ToString());
				fadeTex_right.color = fadeColor;
				fadeTex_left.color = fadeColor;
			}
			else{
				fadeColor.a = 0.0f;
				fadeTex_right.color = fadeColor;
				fadeTex_left.color = fadeColor;
				fadingin = false;				
			}				
		}
		
		horizontal = 0.0f;
		if(Input.GetKey(KeyCode.Keypad1) || Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			horizontal = -1.0f;
		if(Input.GetKey(KeyCode.Keypad2) || Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			horizontal = 1.0f;

		if(lookLearned < lookLearnTreshold){			
			if(horizontal != 0)
				lookLearned++;
		}
		else{
			msgToLookUI_right.CrossFadeAlpha(0.0f,msgFadeTime,false);
			msgToLookUI_left.CrossFadeAlpha(0.0f,msgFadeTime,false);
			msgToLookShadUI_right.CrossFadeAlpha(0.0f, msgFadeTime, false);
			msgToLookShadUI_left.CrossFadeAlpha(0.0f, msgFadeTime, false);
		}
		if(wakingUp){
			myTransform.Rotate(0f,horizontal * rotateSpeed * Time.deltaTime, 0f);
			//check if spot other players to activate them
			if(invited2play){
				if((Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) || (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow)) || 
				(Input.GetKey(KeyCode.Keypad1) && Input.GetKey(KeyCode.Keypad2)) || (Input.GetKey(KeyCode.Alpha1) && Input.GetKey(KeyCode.Alpha2))){
					wakingUp = false;
					inPreGame = true;
					
					//fade msg to Start game
					msgToStartGameUI_right.CrossFadeAlpha(0.0f,msgFadeTime,false);
					msgToStartGameUI_left.CrossFadeAlpha(0.0f,msgFadeTime,false);
					msgToStartGameShadUI_right.CrossFadeAlpha(0.0f, msgFadeTime, false);
					msgToStartGameShadUI_left.CrossFadeAlpha(0.0f, msgFadeTime, false);
					
					if(!playersActive)
						StartCoroutine(ActivatePlayers());
				}				
			}
			
			if(Vector3.Angle(leftPlayer.position - myTransform.position, myTransform.forward) <= 1.5*lookAngle){
				if(!invited2play){
					invited2play = true;
					
					//greet player inviting him to play
					StartCoroutine(GreetPlayer());
					//show message to start game
					msgToStartGameUI_right.enabled = true;
					msgToStartGameUI_left.enabled = true;
					msgToStartGameShadUI_right.enabled = true;
					msgToStartGameShadUI_left.enabled = true;
					
					msgToLookUI_right.CrossFadeAlpha(0.0f,msgFadeTime,false);
					msgToLookUI_left.CrossFadeAlpha(0.0f,msgFadeTime,false);
					msgToLookShadUI_right.CrossFadeAlpha(0.0f, msgFadeTime, false);
					msgToLookShadUI_left.CrossFadeAlpha(0.0f, msgFadeTime, false);
				}
			}
			/*
			if(Input.GetKeyDown("space")){
				if(!playersActive)
					StartCoroutine(ActivatePlayers());
			}
			*/
		}
		else{
			if(inPreGame){
				myTransform.Rotate(0f,horizontal * rotateSpeed * Time.deltaTime, 0f);
				if(walking){
					Vector3 cameraPos = CameraTransform.position;
					cameraPos.y += Mathf.Sin(bobFreq*Time.time)*bobAmp;
					CameraTransform.position = cameraPos;
				}		
				/*
				if(Input.GetKeyDown("space")){
					if(!playersActive)
						StartCoroutine(ActivatePlayers());
				}
							
				if(Input.GetKeyDown(KeyCode.S)){
					navAgent.enabled = false;
					myTransform.position = playerNavPoints[playerNavPoints.Length-1].position;
					watchingToStart = true;
					walking = false;
					if(!playersActive)
						StartCoroutine(ActivatePlayers());
				}	
				*/
				
				//update navPoint in case it is less than the max number (4)
				//otherwise, activate players
				if (!navAgent.pathPending) {
					if (navAgent.remainingDistance <= 2*navAgent.stoppingDistance) {
						//if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f) {
							if(navPointIndex < playerNavPoints.Length){
								navAgent.SetDestination(playerNavPoints[navPointIndex].position);
								if (navPointIndex == 3)
									StartCoroutine(GreetPlayer());
								navPointIndex++;
								if(navPointIndex == playerNavPoints.Length)
									walking = false;
							}
							else{
								if(!playersActive)
									StartCoroutine(ActivatePlayers());
							}							
						//}
					}
				}		
			
				if(watchingToStart){
					Debug.DrawRay(myTransform.position, middleTargetDir, Color.black, 100f);
					Debug.DrawRay(myTransform.position, myTransform.forward, Color.red, 100f);
					//check if player is looking to both the other players
					if(Vector3.Angle(middleTargetDir, myTransform.forward) <= lookAngle){
						msgToStartUI_right.gameObject.SetActive(false);
						msgToStartUI_left.gameObject.SetActive(false);
						GetComponent<BallTosser>().ballUI_right.gameObject.SetActive(true);
						GetComponent<BallTosser>().ballUI_left.gameObject.SetActive(true);
						inPreGame = false;
						Debug.Log("<color=blue>Started throwing game</color>");
						gameManager.ThrowBall();
						
						gameObject.GetComponent<PreGameControl>().enabled = false;
					}
				}
			}
		}
	}
	
	IEnumerator ActivatePlayers(){
		playersActive = true;
		gameManager.enabled = true;
		leftFriend.Move2Loc();
		rightFriend.Move2Loc();

		yield return new WaitForSeconds(startupTime);
		leftPlayerSound.PlayOneShot(greeting2);
	}
	
	IEnumerator Startup(){
		navAgent.SetDestination(playerNavPoints[navPointIndex].position);
		navPointIndex++;
		
		yield return new WaitForSeconds(startupTime);
	}

	IEnumerator GreetPlayer(){
		leftAnim.SetBool("bored", true);
		leftPlayerSound.PlayOneShot(greeting1);
		yield return new WaitForSeconds(1.0f);
		leftAnim.SetBool("bored", false);
	}

	public void CalculateMiddlePoint(){
		middleTargetDir = leftPlayer.position - myTransform.position + (rightPlayer.position - leftPlayer.position)/2f;
		watchingToStart = true;
		msgToStartUI_right.gameObject.SetActive(true);
		msgToStartUI_left.gameObject.SetActive(true);
	}
}
