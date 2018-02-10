using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class friendControl : MonoBehaviour {

    public Transform playingLocation;
    public float closeDist = 2.0f;
    Transform myTransform;
    NavMeshAgent navAgent;
    Animator myAnim;
    BallTosser myTosser;
    bool ready;
	bool move2Location=false;
	public Transform leftPos;
	public Transform rightPos;

	// Use this for initialization
	void Start () {
        myTransform = transform;
        navAgent = GetComponent<NavMeshAgent>();
        myAnim = GetComponent<Animator>();
        myTosser = GetComponent<BallTosser>();
	}
	
	// Update is called once per frame
	void Update () {
		if(move2Location){
			if (Vector3.Distance(myTransform.position, navAgent.destination) > closeDist)
			{
				myAnim.SetFloat("speed", navAgent.velocity.magnitude / navAgent.speed);
				//Debug.Log("animation parameter: " + (navAgent.velocity.magnitude / navAgent.speed).ToString());
			}
			else {
				myAnim.SetFloat("speed", 0f);
				move2Location = false;
				navAgent.enabled = false;
				if(!ready)
					StartCoroutine(GetReady2Play());
			}
		}
		Debug.DrawRay(myTransform.position, myTosser.newForth, new Color (0f, 0f, 1f));
	}

    IEnumerator GetReady2Play()
    {
        ready = true;
		//wait so the player arrive at their locations
		//yield return new WaitForSeconds(2.0f);
		
		myTosser.newForth = leftPos.position - myTransform.position + (rightPos.position - leftPos.position)/2f;
		Debug.DrawRay(myTransform.position, myTosser.newForth, new Color (0f, 0f, 1f));
		myTosser.lookingTarget = true;
			
        yield return new WaitForSeconds(Random.Range(0.0f,1.0f));
        myAnim.SetBool("bored",true);
        yield return new WaitForSeconds(2f);
        myAnim.SetBool("bored", false);
		myTosser.lookingTarget = false;
    }
	
	public void Move2Loc(){
		navAgent.SetDestination(playingLocation.position);
		move2Location = true;
	}
}
