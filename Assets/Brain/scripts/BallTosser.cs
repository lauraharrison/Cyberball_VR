using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BallTosser : MonoBehaviour {
    public Transform targetLeft;
    public Transform targetRight;
	
	public Transform throwTargetLeft;
	public Transform throwTargetRight;
	
	BallTosser leftTosser;
	BallTosser rightTosser;	

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
	public float catchTime = 0.8f;
	public float prepareTime = 0.8f;
	public float prepareTimePlayer = 0.8f;
	public float smoothRot = 0.5f;
 	Animator myAnim;
	Transform myTransform;
	public Vector3 newForth = new Vector3();
	public bool lookingTarget;
	
	public float minWaitThrow = 2.0f;
	public float maxWaitThrow = 5.0f;
	public float exclusionLookTime = 2.0f;

	public Transform headTransform;
	public Transform lookTo;
	public float lookSpeed = 5.0f;
	public float offsetY = 1f;
	public float idleFloat;
	public float idleFloatSpeed = 10f;
	bool paused;
	public RawImage ballUI;
	public Texture ballImage;
	public Texture ballGhostImage;
	
	
	SaveToCSV saveFile;
    
    // Use this for initialization
    void Start () {
		myTransform = transform;
		
		leftTosser = targetLeft.GetComponent<BallTosser>();
		rightTosser = targetRight.GetComponent<BallTosser>();
		
		GameObject saveFileGameObject = GameObject.FindGameObjectWithTag("SaveFile");
        if(saveFileGameObject == null)
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
		
        if(!isplayer){
			myAnim = GetComponent<Animator>();
		}
        else{
			myAnim = GameObject.Find(gameObject.name+"/meshPlayer").GetComponent<Animator>();
			ballUI = GameObject.Find("GUI/tenisBallUI").GetComponent<RawImage>();
			ballUI.gameObject.SetActive(false);
            //StartCoroutine(GetReady2Play());
		}
		
		myBall.SetActive(haveBall);
    }
	
	// Update is called once per frame
	void Update () {
		if(isplayer)
		{
			if(Input.GetKeyDown("escape")){
				if(paused){
					paused=false;
					Time.timeScale = 1f;
				}
				else{
					paused=true;
					Time.timeScale = 0f;
				}
			}
		}
		
        if (haveBall)
        {
            if(isplayer)
            {
                if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
					myBall.SetActive(false);
					haveBall = false;
                    StartCoroutine(AnimateThrow(false));
					saveFile.WriteToFile("Player to Remy");
                }
                if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
					myBall.SetActive(false);
					haveBall = false;
                    StartCoroutine(AnimateThrow(true));
					saveFile.WriteToFile("Player to Stefani");
                }
            }
            //else{				
            //    StartCoroutine(DecideAndThrow());
            //}
        }
		if(lookingTarget && !isplayer){
			myTransform.forward = Vector3.Lerp (myTransform.forward, newForth, smoothRot*Time.deltaTime);			
		}
		if(!isplayer){
			float idlecurr = myAnim.GetFloat("idle");
			if(idlecurr != idleFloat){
				idlecurr += Mathf.Sign(idleFloat)*idleFloatSpeed*Time.deltaTime;
				if(Mathf.Abs(idlecurr) > 1f)
					idlecurr = 1f*Mathf.Sign(idleFloat);
				myAnim.SetFloat("idle",idlecurr);
			}			
		}
	}
	
	void LateUpdate(){
		if(lookingTarget && !isplayer && lookTo){
			Quaternion targetRotation = Quaternion.LookRotation(lookTo.position - headTransform.position);
			// Smoothly rotate towards the target point.
			//headTransform.rotation = Quaternion.Slerp(headTransform.rotation, targetRotation, lookSpeed * Time.deltaTime);
			//headTransform.LookAt(lookTo);
		}
	}

	public IEnumerator ThrowBall(Transform gazeTarget, bool throwLeft, bool gaze, float gazeTime) {
        haveBall = false;
		
		if(gaze){			
			//newForth = gazeTarget.position - myTransform.position;
			if(throwLeft){
				myAnim.SetInteger("gazeValue",1);
				//newForth = targetRight.position - myTransform.position;
			}
			else{
				myAnim.SetInteger("gazeValue",-1);
				//newForth = targetLeft.position - myTransform.position;
			}
			//lookingTarget = true;
			
			yield return new WaitForSeconds(gazeTime);
			myAnim.SetInteger("gazeValue",0);
		}
		
		StartCoroutine(AnimateThrow(throwLeft));
	}
	
    IEnumerator DecideAndThrow() {
        haveBall = false;
		
		//Random wait before throwing the next ball
        yield return new WaitForSeconds(Random.Range(minWaitThrow, maxWaitThrow));
		
		bool throwLeft=false;
		
        if(Random.Range(0.0f, 1.0f) < includePlayerPercentage){
			// look to other player in exclusion mode:		
			if(includePlayerPercentage < 0.01f)
			{
				newForth = targetLeft.position - myTransform.position;
				//lookingTarget = true;
				//wait this exclusion look
				yield return new WaitForSeconds(exclusionLookTime);
			}
			
			throwLeft = false;			
		}
        else{
			// look to other player in exclusion mode:		
			if(includePlayerPercentage < 0.01f)
			{
				newForth = targetRight.position - myTransform.position;
				//lookingTarget = true;
				//wait this exclusion look
				yield return new WaitForSeconds(exclusionLookTime);
			}
			
			throwLeft = true;
		}
		
		StartCoroutine(AnimateThrow(throwLeft));
    }
	
	IEnumerator AnimateThrow(bool left){
		if (left)
        {
            newForth = targetLeft.position - myTransform.position;
			myAnim.SetInteger("throwValue",-1);
			rightTosser.idleFloat = 1.0f;
		}
		else{
			newForth = targetRight.position - myTransform.position;
			myAnim.SetInteger("throwValue",1);
			leftTosser.idleFloat = -1.0f;
		}
		//lookingTarget = true;		
		
		yield return new WaitForSeconds(throwTime);
		ThrowBallNow(left);
		myAnim.SetInteger("throwValue",0);
		
		if(left)
			myAnim.SetFloat("idle",-1.0f);
		else
			myAnim.SetFloat("idle",1.0f);
		
	}
    void ThrowBallNow(bool left) {
        Vector3 origin;
        Transform target;
		Vector3 throwpos;
        if (left)
        {
            origin = leftHandLoc.position;
            target = targetLeft;
			if(throwTargetLeft)
				throwpos = throwTargetLeft.position;
			else
				throwpos = target.position;
			//make right player look to left player
			rightTosser.newForth = targetLeft.position - targetRight.position;
			//rightTosser.lookingTarget = true;
        }
        else
        {
            origin = rightHandLoc.position;
            target = targetRight;
			if(throwTargetRight)
				throwpos = throwTargetRight.position;
			else
				throwpos = target.position;
			leftTosser.newForth = targetRight.position - targetLeft.position;
			//leftTosser.lookingTarget = true;
        }

        GameObject ball = (GameObject)GameObject.Instantiate(ballPrefab, origin, Quaternion.identity);
				
        Transform ballTrans = ball.transform;
        ballTrans.LookAt(target);
		
		if (left)
        {
			rightTosser.lookTo = ballTrans;
		}
		else{
			leftTosser.lookTo = ballTrans;
		}

        //calculating trajectory
        float gravityAcel = Physics.gravity.y;
        tossSpeedy = (-1*(1-offsetY)) * gravityAcel * (Vector3.Distance(throwpos,origin)) / (2 * tossSpeedx);
        ball.GetComponent<Rigidbody>().velocity = ballTrans.forward * tossSpeedx + ballTrans.up * tossSpeedy;
        
		//float impactTime = Vector3.Distance(target.position,origin)/tossSpeedx;
		target.GetComponent<BallTosser>().prepareToTakeBall(isplayer,left);
		//Debug.DrawRay(ballTrans.position, target.position - ballTrans.position, new Color (1f, 1f, 0f));
		//Time.timeScale = 0;
		
		haveBall = false;
        myBall.SetActive(false);
		if(isplayer)
			ballUI.texture = ballGhostImage;
		lookingTarget = false;

        //while there are no other players to throw the ball back...
        //StartCoroutine(RecuperateBall());        
    }
    IEnumerator RecuperateBall() {
        yield return new WaitForSeconds(0.8f);
        haveBall = true;		
        myBall.SetActive(true);
		if(isplayer)
			ballUI.texture = ballImage;
    }
    IEnumerator GetReady2Play()
    {
        yield return new WaitForSeconds(4f);
        haveBall = true;
        myBall.SetActive(true);
		if(isplayer)
			ballUI.texture = ballImage;
    }
	void prepareToTakeBall(bool fromPlayer, bool left){
		//lookingTarget = false;
		//newForth = targetRight.position - myTransform.position;
		//lookingTarget = true;
		/*
		if(left)
			myAnim.SetInteger("gazeValue",-1);
		else
			myAnim.SetInteger("gazeValue",1);
		*/
		StartCoroutine(CatchBall(fromPlayer, left));
	}
	IEnumerator CatchBall(bool fromPlayer, bool left) {
		//yield return new WaitForSeconds(impactIn/2);
		if(fromPlayer)
			yield return new WaitForSeconds(prepareTimePlayer);
		else
			yield return new WaitForSeconds(prepareTime);
		
		if(left)
			myAnim.SetInteger("catchValue",1);
		else
			myAnim.SetInteger("catchValue",-1);
		
        yield return new WaitForSeconds(catchTime);
		myAnim.SetInteger("catchValue",0);
		myAnim.SetFloat("idle",0f);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "cyberball")
        {            		
            Destroy(other.gameObject);
			
			if(isplayer){
				ballUI.texture = ballImage;
			}
			//else{
				myBall.SetActive(true);
			//}
			haveBall = true;
        }
        Debug.Log(gameObject.name + " was triggered by "+other.gameObject.name);
    }
    void OnCollisionEnter(Collision other)
    {
        Debug.Log(gameObject.name + " was collided with " + other.gameObject.name);
    }
}
