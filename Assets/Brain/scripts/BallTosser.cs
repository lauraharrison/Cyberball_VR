using UnityEngine;
using System.Collections;

public class BallTosser : MonoBehaviour {
    public Transform targetLeft;
    public Transform targetRight;

    public Transform rightHandLoc;
    public Transform leftHandLoc;

    public GameObject ballPrefab;
    public float tossSpeedx = 4f;
    float tossSpeedy = 5f;
    public bool haveBall=true;

    public GameObject myBall;
    public bool isplayer;
    public float includePlayerPercentage = 0.5f;


    // Use this for initialization
    void Start () {
        if(!isplayer)
            myBall.SetActive(false);
        else
            StartCoroutine(GetReady2Play());
    }
	
	// Update is called once per frame
	void Update () {
        if (haveBall)
        {
            if (isplayer)
            {
                if (Input.GetKeyDown(KeyCode.L))
                {
                    ThrowBall(false);
                }
                if (Input.GetKeyDown(KeyCode.J))
                {
                    ThrowBall(true);
                }
            }
            else {
                StartCoroutine(DecideAndThrow());
            }
        }
	}

    IEnumerator DecideAndThrow() {
        haveBall = false;
        yield return new WaitForSeconds(Random.Range(0.0f, 5.0f));
        if(Random.Range(0.0f, 1.0f) <= includePlayerPercentage)
            ThrowBall(false);
        else
            ThrowBall(true);
    }
    void ThrowBall(bool left) {
        Vector3 origin;
        Transform target;
        if (left)
        {
            origin = leftHandLoc.position;
            target = targetLeft;
        }
        else
        {
            origin = rightHandLoc.position;
            target = targetRight;
        }

        GameObject ball = (GameObject)GameObject.Instantiate(ballPrefab, origin, Quaternion.identity);
				
        Transform ballTrans = ball.transform;
        ballTrans.LookAt(target);

        //calculating trajectory
        float gravityAcel = Physics.gravity.y;
        tossSpeedy = (-1) * gravityAcel * (Vector3.Distance(target.position,origin)) / (2 * tossSpeedx);
        ball.GetComponent<Rigidbody>().velocity = ballTrans.forward * tossSpeedx + ballTrans.up * tossSpeedy;
        
		//Debug.DrawRay(ballTrans.position, target.position - ballTrans.position, new Color (1f, 1f, 0f));
		//Time.timeScale = 0;
		
		haveBall = false;
        myBall.SetActive(false);

        //while there are no other players to throw the ball back...
        //StartCoroutine(RecuperateBall());        
    }
    IEnumerator RecuperateBall() {
        yield return new WaitForSeconds(0.8f);
        haveBall = true;
        myBall.SetActive(true);
    }
    IEnumerator GetReady2Play()
    {
        yield return new WaitForSeconds(4f);
        haveBall = true;
        myBall.SetActive(true);        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "cyberball")
        {
            haveBall = true;
            myBall.SetActive(true);
            Destroy(other.gameObject);
        }
        Debug.Log(gameObject.name + " was triggered by "+other.gameObject.name);
    }
    void OnCollisionEnter(Collision other)
    {
        Debug.Log(gameObject.name + " was collided with " + other.gameObject.name);
    }
}
