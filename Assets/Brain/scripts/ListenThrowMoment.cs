using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListenThrowMoment : MonoBehaviour {
	
	public BallTosser myBallTosser;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void ThrowBallInstant(){
		Debug.Log("<color=yellow> throw instant fired!</color>");
		myBallTosser.ThrowBallNow();
	}
}