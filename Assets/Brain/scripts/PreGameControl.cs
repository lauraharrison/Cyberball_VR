using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
		
		StartCoroutine(Startup());
	}
	
	// Update is called once per frame
	void Update () {
		if(inPreGame){
			myTransform.Rotate(0f,CrossPlatformInputManager.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime, 0f);
			
			//if(Input.GetKeyDown("space"))
			//	ActivatePlayers();
		
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
	}
}
