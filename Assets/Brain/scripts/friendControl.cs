﻿using System.Collections;
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

	// Use this for initialization
	void Start () {
        myTransform = transform;
        navAgent = GetComponent<NavMeshAgent>();
        myAnim = GetComponent<Animator>();
        myTosser = GetComponent<BallTosser>();

        navAgent.SetDestination(playingLocation.position);
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance(myTransform.position, navAgent.destination) > closeDist)
        {
            myAnim.SetFloat("speed", navAgent.velocity.magnitude / navAgent.speed);
            //Debug.Log("animation parameter: " + (navAgent.velocity.magnitude / navAgent.speed).ToString());
        }
        else {
            myAnim.SetFloat("speed", 0f);
            if(!ready)
                StartCoroutine(GetReady2Play());
        }
	}

    IEnumerator GetReady2Play()
    {
        ready = true;
        yield return new WaitForSeconds(Random.Range(0.0f,1.0f));
        myAnim.SetBool("bored",true);
        yield return new WaitForSeconds(2f);
        myAnim.SetBool("bored", false);
    }
}
