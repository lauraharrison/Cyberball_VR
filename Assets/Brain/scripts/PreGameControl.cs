using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

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
	
	Text msgToStartUI;
	Text msgToLookUI;
	Text msgToLookShadUI;
	int lookLearned;
	public int lookLearnTreshold = 100;
	public float msgFadeTime = 0.5f;
	

	Animator leftAnim;

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
		
		msgToStartUI = GameObject.Find("GUI/msgToStart").GetComponent<Text>();
		msgToStartUI.gameObject.SetActive(false);
		
		msgToLookUI = GameObject.Find("GUI/msgToLook").GetComponent<Text>();
		msgToLookUI.enabled = true;
		msgToLookShadUI = GameObject.Find("GUI/msgToLookShadow").GetComponent<Text>();
		msgToLookShadUI.enabled = true;
		
		StartCoroutine(Startup());
	}
	
	// Update is called once per frame
	void Update () {
		if(inPreGame){
			myTransform.Rotate(0f,CrossPlatformInputManager.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime, 0f);
			if(walking){
				Vector3 cameraPos = CameraTransform.position;
				cameraPos.y += Mathf.Sin(bobFreq*Time.time)*bobAmp;
				CameraTransform.position = cameraPos;
			}
			
			if(lookLearned < lookLearnTreshold){
				if(CrossPlatformInputManager.GetAxis("Horizontal") != 0)
					lookLearned++;
			}
			else{
				msgToLookUI.CrossFadeAlpha(0.0f,msgFadeTime,false);
				msgToLookShadUI.CrossFadeAlpha(0.0f, msgFadeTime, false);
			}
			
			//if(Input.GetKeyDown("space"))
			//	ActivatePlayers();
		
			if(Input.GetKeyDown(KeyCode.S)){
				navAgent.enabled = false;
				myTransform.position = playerNavPoints[playerNavPoints.Length-1].position;
				watchingToStart = true;
				walking = false;
				if(!playersActive)
					StartCoroutine(ActivatePlayers());
			}				
			
			//update navPoint in case it is lesser than the max number (4)
			//otherwise, activate players
			if (!navAgent.pathPending) {
				if (navAgent.remainingDistance <= 2*navAgent.stoppingDistance) {
					//if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f) {
						if(navPointIndex < playerNavPoints.Length){
							navAgent.SetDestination(playerNavPoints[navPointIndex].position);
							if (navPointIndex == 3)
								StartCoroutine (GreetPlayer());
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
					msgToStartUI.gameObject.SetActive(false);
					GetComponent<BallTosser>().ballUI.gameObject.SetActive(true);
					inPreGame = false;
					Debug.Log("<color=blue>Started throwing game</color>");
					gameManager.ThrowBall();
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
		msgToStartUI.gameObject.SetActive(true);
	}
}
