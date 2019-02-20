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
	bool throwSide;

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
	public RawImage ballUI_right;
	public RawImage ballUI_left;
	public Texture ballImage;
	public Texture ballGhostImage;
	
	public Transform smoothLookTarget;
	bool smoothLookTargeted = true;
	Vector3 smoothLookPos;
	public float smoothLookSpeed = 10.0f;
	public float smoothTreshold = 5.0f;
	Vector3 ballPosTarget;
	Vector3 ballPos;
	public float ballPosSpan = 0.1f;
	public float handBallSpeed = 10f;
	public float riseBallcueDelay=0.5f;
	
	
	SaveToCSV saveFile;
    
    // Use this for initialization
    void Start () {
		myTransform = transform;
		smoothLookPos = new Vector3();
		
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
			myBall.SetActive(haveBall);
		}
        else{
			myAnim = GameObject.Find(gameObject.name+"/meshPlayer").GetComponent<Animator>();
			ballUI_right = GameObject.Find("GUI_right/tenisBallUI").GetComponent<RawImage>();
			ballUI_right.gameObject.SetActive(false);
			ballUI_left = GameObject.Find("GUI_left/tenisBallUI").GetComponent<RawImage>();
			ballUI_left.gameObject.SetActive(false);
            //StartCoroutine(GetReady2Play());
			ballPos = myBall.transform.localPosition;
			ballPosTarget = ballPos;
		}		
    }
	
	// Update is called once per frame
	void Update () {
		if(isplayer)
		{
			
			//update player ball pos
			ballPos = Vector3.Lerp(ballPos, ballPosTarget, handBallSpeed*Time.deltaTime);
			myBall.transform.localPosition = ballPos;
			
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
		else{
			if(!smoothLookTargeted){
				smoothLookPos += (lookTo.position - smoothLookPos).normalized*smoothLookSpeed*Time.deltaTime;
				smoothLookTarget.position = smoothLookPos;
				if(Vector3.Distance(smoothLookPos,lookTo.position) < smoothTreshold){
					smoothLookTargeted = true;
				}
			}			
		}
		
        if (haveBall)
        {
            if(isplayer)
            {
                if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
                {
					ballPosTarget = ballPos - Vector3.up*ballPosSpan;
					//myBall.SetActive(false);
					haveBall = false;
                    StartCoroutine(AnimateThrow(false));
					saveFile.WriteToFile("Player to Remy");
                }
                if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
                {
					ballPosTarget = ballPos - Vector3.up*ballPosSpan;
					//myBall.SetActive(false);
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
		/*
		if(!isplayer){
			float idlecurr = myAnim.GetFloat("idle");
			if(idlecurr != idleFloat){
				idlecurr += Mathf.Sign(idleFloat)*idleFloatSpeed*Time.deltaTime;
				if(Mathf.Abs(idlecurr) > 1f)
					idlecurr = 1f*Mathf.Sign(idleFloat);
				myAnim.SetFloat("idle",idlecurr);
			}			
		}
		*/
	}
	
	void LateUpdate(){
		if(!isplayer && lookTo){
			if(smoothLookTargeted){
				Quaternion targetRotation = Quaternion.LookRotation(lookTo.position - headTransform.position);
				// Smoothly rotate towards the target point.
				headTransform.rotation = Quaternion.Slerp(headTransform.rotation, targetRotation, lookSpeed * Time.deltaTime);	
			}
			else{
				Quaternion targetRotation = Quaternion.LookRotation(smoothLookTarget.position - headTransform.position);
				// Smoothly rotate towards the target point.
				headTransform.rotation = Quaternion.Slerp(headTransform.rotation, targetRotation, lookSpeed * Time.deltaTime);
			}			
		}
	}

	public IEnumerator ThrowBall(Transform gazeTarget, bool throwLeft, bool gaze, float gazeTime) {
        haveBall = false;
		
		if(gaze){	
			UpdateLookTo(gazeTarget);
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
			UpdateLookTo(targetLeft);
		}
		else{
			newForth = targetRight.position - myTransform.position;
			myAnim.SetInteger("throwValue",1);
			leftTosser.idleFloat = -1.0f;
			UpdateLookTo(targetRight);
		}
		//lookingTarget = true;		
		throwSide = left;
		
		yield return new WaitForSeconds(throwTime);
		//ThrowBallNow(left);
		myAnim.SetInteger("throwValue",0);
		
		if(left)
			myAnim.SetFloat("idle",-1.0f);
		else
			myAnim.SetFloat("idle",1.0f);
		
	}
    public void ThrowBallNow() {
        Vector3 origin;
        Transform target;
		Vector3 throwpos;
        if (throwSide)
        {
            origin = leftHandLoc.position;
            
			if(throwTargetLeft)
				target = throwTargetLeft;
			else
				target = targetLeft;
			
			throwpos = target.position;
			//make right player look to left player
			rightTosser.newForth = targetLeft.position - targetRight.position;
			//rightTosser.lookingTarget = true;
        }
        else
        {
            origin = rightHandLoc.position;
            
			if(throwTargetRight)
				target = throwTargetRight;
			else
				target = targetRight;
				
			throwpos = target.position;
			leftTosser.newForth = targetRight.position - targetLeft.position;
			//leftTosser.lookingTarget = true;
        }

        GameObject ball = (GameObject)GameObject.Instantiate(ballPrefab, origin, Quaternion.identity);
		//paused=true;
		//Time.timeScale = 0f;
					
        Transform ballTrans = ball.transform;
        ballTrans.LookAt(target);
		
		if (throwSide)
        {
			rightTosser.UpdateLookTo(ballTrans);
			leftTosser.prepareToTakeBall(isplayer,throwSide);
		}
		else{
			leftTosser.UpdateLookTo(ballTrans);
			rightTosser.prepareToTakeBall(isplayer,throwSide);
		}
		
        //calculating trajectory
        float gravityAcel = Physics.gravity.y;
        tossSpeedy = (-1*(1-offsetY)) * gravityAcel * (Vector3.Distance(throwpos,origin)) / (2 * tossSpeedx);
        ball.GetComponent<Rigidbody>().velocity = ballTrans.forward * tossSpeedx + ballTrans.up * tossSpeedy;
        
		//float impactTime = Vector3.Distance(target.position,origin)/tossSpeedx;
		
		//Debug.DrawRay(ballTrans.position, target.position - ballTrans.position, new Color (1f, 1f, 0f));
		//Time.timeScale = 0;
		
		haveBall = false;
        if(isplayer){
			ballUI_right.texture = ballGhostImage;
			ballUI_left.texture = ballGhostImage;
		}
		else{
			myBall.SetActive(false);	
		}
		lookingTarget = false;

        //while there are no other players to throw the ball back...
        //StartCoroutine(RecuperateBall());        
    }
    IEnumerator RecuperateBall() {
        yield return new WaitForSeconds(0.8f);
        haveBall = true;		
        
		if(isplayer){
			ballUI_right.texture = ballImage;
			ballUI_left.texture = ballImage;
			ballPosTarget = ballPos + Vector3.up*ballPosSpan;
		}
		else{
				myBall.SetActive(true);
		}
    }
    IEnumerator GetReady2Play()
    {
        yield return new WaitForSeconds(4f);
        haveBall = true;
        myBall.SetActive(true);
		if(isplayer){
			ballUI_right.texture = ballImage;
			ballUI_left.texture = ballImage;
		}
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
	public void UpdateLookTo(Transform newLookTo){
		if(!isplayer){
			if(lookTo){
				smoothLookPos = lookTo.position;
				smoothLookTarget.position = smoothLookPos;
			}
			else{
				smoothLookPos = myTransform.position + myTransform.forward * 100.0f;
				smoothLookTarget.position = smoothLookPos;			
			}
		}
		smoothLookTargeted = false;
		lookTo = newLookTo;		
	}
	IEnumerator RaiseBallcue(){
		yield return new WaitForSeconds(riseBallcueDelay);
		ballPosTarget = ballPos + Vector3.up*ballPosSpan;
	}
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "cyberball")
        {            		
            Destroy(other.gameObject);
			
			if(isplayer){
				ballUI_right.texture = ballImage;
				ballUI_left.texture = ballImage;
				StartCoroutine(RaiseBallcue());
			}
			else{
				myBall.SetActive(true);
			}
			haveBall = true;
			
			//make other players look to me
			rightTosser.UpdateLookTo(myTransform);
			leftTosser.UpdateLookTo(myTransform);
			//look forward in case you are the player
        }		
		
        Debug.Log(gameObject.name + " was triggered by "+other.gameObject.name);
    }
    void OnCollisionEnter(Collision other)
    {
        Debug.Log(gameObject.name + " was collided with " + other.gameObject.name);
    }
}
