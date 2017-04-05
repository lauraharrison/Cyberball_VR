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
	int throwIndex;
	bool freeToThrow;
	public Transform leftPlayer;
	public Transform rightPlayer;
	Transform playerTransform;
	
	BallTosser leftTosser;
	BallTosser rightTosser;

	
	void Awake() {
		leftTosser = leftPlayer.GetComponent<BallTosser>();
		rightTosser = rightPlayer.GetComponent<BallTosser>();
		playerTransform = transform;
		freeToThrow=true;
	}
	
	// Update is called once per frame
	void Update () {
		if( (leftTosser.haveBall || rightTosser.haveBall) && throwIndex < throwSequence.Length && freeToThrow)
		{
			StartCoroutine(ThrowBall());
		}
	}
	
	IEnumerator ThrowBall(){		
		freeToThrow=false;
		
		yield return new WaitForSeconds(throwSequence[throwIndex].waitTime);
		Transform throwTarget;
		
		if(leftTosser.haveBall){
			if(throwSequence[throwIndex].throwToPlayer)
				throwTarget = rightPlayer;
			else
				throwTarget = playerTransform;
			
			StartCoroutine(leftTosser.ThrowBall(throwTarget, !throwSequence[throwIndex].throwToPlayer, throwSequence[throwIndex].gazeToOther, throwSequence[throwIndex].gazeTime));
		}
		else{
			if(throwSequence[throwIndex].throwToPlayer)
				throwTarget = leftPlayer;
			else
				throwTarget = playerTransform;
			
			StartCoroutine(rightTosser.ThrowBall(throwTarget, !throwSequence[throwIndex].throwToPlayer, throwSequence[throwIndex].gazeToOther, throwSequence[throwIndex].gazeTime));
		}
		throwIndex += 1;
		freeToThrow=true;		
	}
}
