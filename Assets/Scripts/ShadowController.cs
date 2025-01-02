using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.N3DS;

public class ShadowController : MonoBehaviour {

	GameManager gameManager;
	Rigidbody rigidbody;
	Transform ViewCam;
	AudioSource ShadowsMouth;
	Transform TelegraphShadow;
	Animator animator;
	GameObject mainBody;
	GameObject ballForm;
	[Header("Input")]
	
	[SerializeField] private Vector2 move;
    [SerializeField] private bool jumpPressed;
    [SerializeField] private bool stompPressed;
    [SerializeField] private bool boostPressed;
    [SerializeField] private bool homingPressed;
    [SerializeField] private bool QuickStepFlag;
    [SerializeField] private float QuickStepDirection;
    [SerializeField] private bool ChaosControlPressed;


    [Header("Movement Stuff")]
	public bool ground;
	public bool jump;
	public bool stomp;
	public bool slide;
	public bool homing;
	public bool boosting;
	public bool AirBoosted;
	public float acceleration;
	public float deceleration;
	public float RunSpeed;
	public float BoostSpeed;
	[Range(0, 100)] public float BoostGauge;
	public AnimationCurve TurnCurve;
	public Vector3 MoveDirection;
	public Vector3 HorzVel;
	public Vector3 VertVel;
	public Vector3 jumpNomral;
	public byte timesJumped;
	public float JumpTime = 1f;
	private float CurrentJumpTime;
	public float jumpForce = 4f;
	RaycastHit groundRayHit;
	public Vector3 GroundNormal = Vector3.up;
	public LayerMask groundMask;
	public Vector3 Gravity = Vector3.down * 9.81f;
	[Range(0, 1)] public float ChaosMeter;

	public State PlayStyle = State.RegularShadow;
	public bool isFrontiersControls = true;

	public AudioClip[] gameplayVoices;

	public enum State
	{
		RegularShadow,
		Wing,
		Squid,
		Surf,
		SkatingMechanic
	}

	public void ChangePlayStle(State state) { PlayStyle = state; }

	void Start() 
	{
		rigidbody = GetComponent<Rigidbody>();
		ShadowsMouth = GetComponent<AudioSource>();
		ViewCam = Camera.main.transform;
		TelegraphShadow = transform.GetChild(2);
        mainBody = transform.GetChild(0).gameObject;
        animator = mainBody.GetComponent<Animator>();
		ballForm = transform.GetChild(1).gameObject;
		gameManager = GameObject.Find("UIGameplayCanvas").GetComponent<GameManager>();
	}
	
	void Update()
	{
		GatherInput();
		ActivateChaosControl();
		UpdateAnimations();
	}

	void GatherInput()
	{
        move = GamePad.CirclePad + new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (GamePad.GetButtonTrigger(N3dsButton.B) || Input.GetKeyDown(KeyCode.Space))
			jumpPressed = true;
		else if (GamePad.GetButtonRelease(N3dsButton.B) || Input.GetKeyUp(KeyCode.Space))
			jumpPressed = false;

		stompPressed = GamePad.GetButtonHold(N3dsButton.A) || Input.GetKeyDown(KeyCode.LeftControl);

        if (!GamePad.IsCirclePadProConnected())
		{
            if (GamePad.GetButtonTrigger(N3dsButton.Y) || Input.GetKeyDown(KeyCode.LeftShift))
                boostPressed = true;
            else if (GamePad.GetButtonRelease(N3dsButton.Y) || Input.GetKeyUp(KeyCode.LeftShift))
                boostPressed = false;
            QuickStepFlag = GamePad.GetButtonHold(N3dsButton.R);
			homingPressed = GamePad.GetButtonTrigger(N3dsButton.B);
			ChaosControlPressed = GamePad.GetButtonTrigger(N3dsButton.L);
        }
		else
		{
            ChaosControlPressed = GamePad.GetButtonTrigger(N3dsButton.ZL);

            if (isFrontiersControls)
			{
                if (GamePad.GetButtonTrigger(N3dsButton.ZR))
                    boostPressed = true;
                else if (GamePad.GetButtonRelease(N3dsButton.ZR))
                    boostPressed = false;
                homingPressed = GamePad.GetButtonTrigger(N3dsButton.Y);
			}
			else
			{
                if (GamePad.GetButtonTrigger(N3dsButton.Y))
                    boostPressed = true;
                else if (GamePad.GetButtonRelease(N3dsButton.Y))
                    boostPressed = false;
                homingPressed = GamePad.GetButtonTrigger(N3dsButton.B);
            }
        }
    }



    void ActivateChaosControl()
    {
		if (!ChaosControlPressed || ChaosMeter != 1)
			return;
		StartCoroutine(ChaosControl());
    }

    void UpdateAnimations()
	{
        TelegraphShadow.position = groundRayHit.point + GroundNormal * .03f;
        TelegraphShadow.localRotation = Quaternion.LookRotation(Vector3.forward, GroundNormal);

        animator.SetFloat("Speed", HorzVel.magnitude);
	}

	// Update is called once per frame
	void FixedUpdate() 
	{
		switch (PlayStyle)
		{
			case State.RegularShadow:
				GetInititals();
				VelocityDefine();
                GravitayionalPull();
                CalculateHorzVel();
                SnapToTheGround();
                Jump();
                Rotate();
				ApplyVelocity();
				break;
		}	
	}

	void GetInititals()
	{
        UnityEngine.Debug.DrawRay(transform.position + GroundNormal * .2f, -GroundNormal * .5f, Color.red, 0f, true);
		if (Physics.Raycast(transform.position + GroundNormal * .2f, -GroundNormal, out groundRayHit, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore) && groundRayHit.distance < .5f)
		{
			ground = Vector3.Dot(VertVel, -Gravity) <= 0f && (HorzVel.magnitude > 0 || Vector3.Angle(GroundNormal, -Gravity.normalized) < 75f) && (ground || Vector3.Angle(GroundNormal, groundRayHit.normal) <= 45f);
			AirBoosted = false;
			GroundNormal = groundRayHit.normal;
		}
		else
		{
			GroundNormal = -Gravity.normalized;
			ground = false;
		}


    }

    private void VelocityDefine()
    {
        Vector3 Normal = jump ? jumpNomral : GroundNormal;
        VertVel = Vector3.Project(rigidbody.velocity, Normal);
        HorzVel = ground
            ? Vector3.ProjectOnPlane(rigidbody.velocity, GroundNormal)
            : Vector3.ProjectOnPlane(rigidbody.velocity, Normal);
    }

    public void SnapToTheGround()
    {
        if (!ground)
            return;

        transform.position = groundRayHit.point;
        VertVel = Vector3.zero;
        jump = false;
		timesJumped = 0;
		BallActivate(false);
        if (slide && Vector3.Angle(-Gravity, groundRayHit.normal) < 5f)
        {
            HorzVel = Vector3.MoveTowards(HorzVel, Vector3.zero, 6);
            if (HorzVel.magnitude < 3f)
            {
                slide = false;
            }
        }
    }

    void CalculateHorzVel()
	{
        Vector3 normal = jump ? jumpNomral : GroundNormal;
		MoveDirection = Vector3.ProjectOnPlane((Quaternion.FromToRotation(-Gravity, normal) * (ViewCam.forward * move.y + ViewCam.right * move.x).normalized).normalized, normal).normalized;
        boosting = boostPressed && BoostGauge > 0f;

        if (boosting && MoveDirection == Vector3.zero)
            MoveDirection = transform.forward;

        HorzVel += MoveDirection * acceleration;
        if (HorzVel.magnitude >= RunSpeed && !boosting && !slide)
            HorzVel = Vector3.MoveTowards(HorzVel, HorzVel.normalized * RunSpeed, ground ? 1 : 3);
        else if (boosting)
        {
            BoostGauge = Mathf.Clamp(BoostGauge, 0, 100);
            BoostGauge -= 10 * Time.deltaTime;
            gameManager.UpdateBoost(BoostGauge);

            if (!AirBoosted)
            {
                HorzVel = HorzVel.normalized * BoostSpeed;
                VertVel += -Gravity.normalized * 5;
            }

            if (!ground)
            {
                BallActivate(false);
                AirBoosted = true;
                boosting = false;
            }
        }


        HorzVel = Vector3.RotateTowards(HorzVel, MoveDirection * HorzVel.magnitude, TurnCurve.Evaluate(HorzVel.magnitude), 0f);

        if (!slide && MoveDirection == Vector3.zero)
		{
			HorzVel = Vector3.MoveTowards(HorzVel, Vector3.zero, deceleration);
		}

        HorzVel = Vector3.ClampMagnitude(HorzVel, BoostSpeed);
    }

    public void Jump()
    {
        bool canJump = timesJumped < 2;

        if (canJump && !jump && jumpPressed)
        {
            slide = false;
            jumpNomral = GroundNormal;
			boostPressed = false;
            jump = true;
            VertVel = Vector3.zero;
            CurrentJumpTime = JumpTime;
            timesJumped += 1;
            ground = false;
            ShadowsMouth.PlayOneShot(gameplayVoices[2]);
            BallActivate(true);
        }

        if (jump && jumpPressed && CurrentJumpTime > 0f)
        {
            CurrentJumpTime -= Time.deltaTime;
            VertVel += jumpNomral * jumpForce * (CurrentJumpTime / JumpTime);
            GroundNormal = -Gravity.normalized;
        }
        else
        {
            jumpPressed = false;
            jump = false;
        }
    }

    private void GravitayionalPull()
    {
        if (ground)
            return;

        if (VertVel.magnitude < 100)
            VertVel += Gravity;

    }



    public void Rotate()
    {
        Vector3 normal = ground ? GroundNormal : (jump ? jumpNomral : -Gravity.normalized);

        Quaternion ToRotate = Quaternion.LookRotation(Vector3.ProjectOnPlane(HorzVel.normalized == Vector3.zero ? transform.forward : HorzVel.normalized, normal).normalized, normal);
        transform.rotation = Quaternion.Slerp(transform.rotation, ToRotate, 14f * Time.deltaTime);
    }

    void ApplyVelocity() { rigidbody.velocity = HorzVel + VertVel; }

    public IEnumerator ChaosControl()
	{
		ShadowsMouth.PlayOneShot(gameplayVoices[0]);
		BoostGauge = 100f;
		//Time.timeScale = 0f;
		yield return new WaitForSecondsRealtime(6f);
		//Time.timeScale = 1;
	}


	private void OnTriggerEnter(Collider collider)
	{
        if (collider.CompareTag("Ring"))
        {
			ShadowsMouth.PlayOneShot(gameplayVoices[1]);
			BoostGauge += 6;
			gameManager.UpdateBoost(BoostGauge);
			Destroy(collider.gameObject);
        }
        else if (collider.CompareTag("CameraPanOut"))
        {
            collider.transform.GetChild(0).gameObject.SetActive(true);
            collider.transform.GetChild(0).GetComponent<PannedCamera>().GetFollowData(transform);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("CameraPanOut"))
        {
            collider.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private void BallActivate(bool isTrue)
	{
		ballForm.SetActive(isTrue);
		mainBody.SetActive(!isTrue);
	}
}
