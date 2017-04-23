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
    SaveToCSV saveFile;

    void Awake() {
		leftTosser = leftPlayer.GetComponent<BallTosser>();
		rightTosser = rightPlayer.GetComponent<BallTosser>();
		playerTransform = transform;
		freeToThrow=true;

        GameObject saveFileGameObject = GameObject.FindGameObjectWithTag("SaveFile");
        if (saveFileGameObject == null)
        {
            Debug.LogError("GameObject with SaveToCSV scripts needs to be tagged with SaveFile tag");
        }
        else
        {
            saveFile = saveFileGameObject.GetComponent<SaveToCSV>();
            if (saveFile == null)
            {
                Debug.LogError("Assign SaveToCSV script to SaveFile GameObject");
            }
        }
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
        bool[] events = new bool[saveFile.numEvents];
        if (leftTosser.haveBall){           
            if (throwSequence[throwIndex].throwToPlayer)
            {
                //Stefani throws to player
                events[2] = true;                
                throwTarget = rightPlayer;
            }

            else
            {
                //Stefani throws to Remy
                events[3] = true;
                throwTarget = playerTransform;
            }
            if (saveFile != null)
            {
                events[6] = throwSequence[throwIndex].gazeToOther;
                saveFile.WriteToFile(events);
            }
            StartCoroutine(leftTosser.ThrowBall(throwTarget, !throwSequence[throwIndex].throwToPlayer, throwSequence[throwIndex].gazeToOther, throwSequence[throwIndex].gazeTime));
		}
		else{
            if (throwSequence[throwIndex].throwToPlayer)
            {
                //Stefani throws to player
                events[4] = true;
                throwTarget = leftPlayer;
            }
				
			else
            {
                //Stefani throws to Remy
                events[5] = true;
                throwTarget = playerTransform;
            }

            if (saveFile != null)
            {
                events[7] = throwSequence[throwIndex].gazeToOther;
                saveFile.WriteToFile(events);
            }
            StartCoroutine(rightTosser.ThrowBall(throwTarget, !throwSequence[throwIndex].throwToPlayer, throwSequence[throwIndex].gazeToOther, throwSequence[throwIndex].gazeTime));
		}
		throwIndex += 1;
		freeToThrow=true;		
	}
}
