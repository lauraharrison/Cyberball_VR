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
    public bool haveBall=false;

    public GameObject myBall;
    public bool isplayer;
    public float includePlayerPercentage = 0.5f;
	
	public float throwTime = 0.8f;
	public float smoothRot = 0.5f;
 	Animator myAnim;
	Transform myTransform;
	Vector3 newForth = new Vector3();
	bool lookingTarget;

    // Use this for initialization
    void Start () {
		myTransform = transform;
		
        if(!isplayer){
			myAnim = GetComponent<Animator>();
            myBall.SetActive(false);
		}
        else{
			myAnim = GameObject.Find(gameObject.name+"/meshPlayer").GetComponent<Animator>();
            StartCoroutine(GetReady2Play());
		}
    }
	
	// Update is called once per frame
	void Update () {
        if (haveBall)
        {
            if (isplayer)
            {
                if (Input.GetKeyDown(KeyCode.L))
                {
					myBall.SetActive(false);
                    StartCoroutine(AnimateThrow(false));
                }
                if (Input.GetKeyDown(KeyCode.J))
                {
					myBall.SetActive(false);
                    StartCoroutine(AnimateThrow(true));
                }
            }
            else {				
                StartCoroutine(DecideAndThrow());
            }
        }
		if(lookingTarget && !isplayer){
			myTransform.forward = Vector3.Lerp (myTransform.forward, newForth, smoothRot*Time.deltaTime);
		}
	}

    IEnumerator DecideAndThrow() {
        haveBall = false;
		
		//Random wait before throwing the next ball
        yield return new WaitForSeconds(Random.Range(0.0f, 5.0f));
        if(Random.Range(0.0f, 1.0f) <= includePlayerPercentage)
            StartCoroutine(AnimateThrow(false));
        else
            StartCoroutine(AnimateThrow(true));
    }
	
	IEnumerator AnimateThrow(bool left){
		if (left)
        {
            newForth = targetLeft.position - myTransform.position;
		}
		else{
			newForth = targetRight.position - myTransform.position;
		}
		lookingTarget = true;
		
		myAnim.SetBool("throw",true);
		yield return new WaitForSeconds(throwTime);
		ThrowBall(left);
		myAnim.SetBool("throw",false);
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
        
		float impactTime = Vector3.Distance(target.position,origin)/tossSpeedx;
		target.GetComponent<BallTosser>().prepareToTakeBall(impactTime);
		//Debug.DrawRay(ballTrans.position, target.position - ballTrans.position, new Color (1f, 1f, 0f));
		//Time.timeScale = 0;
		
		haveBall = false;
        myBall.SetActive(false);
		lookingTarget = false;

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
	void prepareToTakeBall(float impactIn){
		StartCoroutine(CatchBall(impactIn));
	}
	IEnumerator CatchBall(float impactIn) {
		yield return new WaitForSeconds(impactIn/2);
		myAnim.SetBool("catch",true);
        yield return new WaitForSeconds(0.8f);
		myAnim.SetBool("catch",false);
		myBall.SetActive(true);
        haveBall = true;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "cyberball")
        {            		
            Destroy(other.gameObject);
			//prepareToTakeBall(CatchBall());
        }
        Debug.Log(gameObject.name + " was triggered by "+other.gameObject.name);
    }
    void OnCollisionEnter(Collision other)
    {
        Debug.Log(gameObject.name + " was collided with " + other.gameObject.name);
    }
}
