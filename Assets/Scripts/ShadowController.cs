using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Director;
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
    [SerializeField] private bool boostPressedthisFrame;
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
	public bool InAirBoost;
    public float AirBoostTime = .2f;
    [SerializeField] private float CurrentAirBoostTime = .2f;
	public float acceleration;
	public float deceleration;
	public float RunSpeed;
	public float BoostSpeed;
	[Range(0, 100)] public float BoostGauge;
	public AnimationCurve TurnCurve;
	public AnimationCurve TurnDecelCurve;
	public AnimationCurve AirDragCurve;
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
    [SerializeField] float LockTimer;
    [SerializeField] bool keepGravity;
    [SerializeField] State BeforeConstrained;

    [Header("Game Stuff")]
    public uint ringCount;
    public AudioClip[] gameplayVoices;
	public AudioClip[] SonicInterClips;
    Vector3 iniPos;
	public enum State
	{
		RegularShadow,
		Wing,
		Squid,
		Surf,
		SkatingMechanic,
        InterConstrained,
        LockedInPlace
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
        iniPos = transform.position;
	}
	
	void Update()
	{
		GatherInput();
		ActivateChaosControl();
		UpdateAnimations();
        if (GamePad.GetButtonTrigger(N3dsButton.Start))
        {
            transform.position = iniPos;
            if (PlayStyle != State.LockedInPlace)
                ShadowsMouth.PlayOneShot(gameplayVoices[3]);
            ChangePlayStle(State.RegularShadow);
            BoostGauge = 100f;
            ringCount = 0;
            gameManager.UpdateRings(0);
            gameManager.UpdateBoost(100);
            gameManager.StopTimer();
            gameManager.Start();
        }
	}

	void GatherInput()
	{
        move = GamePad.CirclePad + new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (GamePad.GetButtonTrigger(N3dsButton.B) || Input.GetKeyDown(KeyCode.Space))
			jumpPressed = true;
		else if (GamePad.GetButtonRelease(N3dsButton.B) || Input.GetKeyUp(KeyCode.Space))
			jumpPressed = false;

		stompPressed = GamePad.GetButtonTrigger(N3dsButton.A) || Input.GetKeyDown(KeyCode.LeftControl);

        if (!GamePad.IsCirclePadProConnected())
		{
            if (GamePad.GetButtonTrigger(N3dsButton.Y) || Input.GetKeyDown(KeyCode.LeftShift))
                boostPressed = true;
            else if (GamePad.GetButtonRelease(N3dsButton.Y) || Input.GetKeyUp(KeyCode.LeftShift))
                boostPressed = false;
            QuickStepFlag = GamePad.GetButtonHold(N3dsButton.R);
			homingPressed = GamePad.GetButtonTrigger(N3dsButton.B);
			boostPressedthisFrame = GamePad.GetButtonTrigger(N3dsButton.Y) || Input.GetKeyDown(KeyCode.LeftShift);
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
                boostPressedthisFrame = GamePad.GetButtonTrigger(N3dsButton.ZR) || Input.GetKeyDown(KeyCode.LeftShift);
			}
			else
			{
                if (GamePad.GetButtonTrigger(N3dsButton.Y))
                    boostPressed = true;
                else if (GamePad.GetButtonRelease(N3dsButton.Y))
                    boostPressed = false;
                homingPressed = GamePad.GetButtonTrigger(N3dsButton.B);
                boostPressedthisFrame = GamePad.GetButtonTrigger(N3dsButton.Y) || Input.GetKeyDown(KeyCode.LeftShift);
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
        TelegraphShadow.up = GroundNormal;

        animator.SetFloat("Speed", HorzVel.magnitude);
        animator.SetBool("ground", ground);
        animator.SetBool("airBoost", CurrentAirBoostTime > 0 && AirBoosted && Vector3.Dot(VertVel, -Gravity) >= 0);
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
            case State.InterConstrained:
                GetInititals();
                VelocityDefine();
                GravitayionalPull();
                Rotate();
                ApplyVelocity();
                LockTimer -= Time.deltaTime;
                if (LockTimer <= 0)
                    ChangePlayStle(BeforeConstrained);
                break;
            case State.LockedInPlace:
                rigidbody.velocity = Vector3.zero;
                break;
		}	
	}

	void GetInititals()
	{
        UnityEngine.Debug.DrawRay(transform.position + transform.up * .2f, -GroundNormal * .5f, Color.red, 0f, true);
		if (Physics.Raycast(transform.position + transform.up * .2f, -GroundNormal, out groundRayHit, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore) && groundRayHit.distance < .5f)
		{
			ground = Vector3.Dot(VertVel, -Gravity) <= 0f && (HorzVel.magnitude > 0 || Vector3.Angle(GroundNormal, -Gravity.normalized) < 75f) && (ground || Vector3.Angle(GroundNormal, groundRayHit.normal) <= 45f);

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
        AirBoosted = false;
        InAirBoost = false;
        CurrentAirBoostTime = 0f;
        stomp = false;
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
        if (stomp)
            return;

        Vector3 normal = jump ? jumpNomral : GroundNormal;
		MoveDirection = Vector3.ProjectOnPlane((Quaternion.FromToRotation(-Gravity, normal) * (ViewCam.forward * move.y + ViewCam.right * move.x).normalized).normalized, normal).normalized;
        boosting = boostPressed && BoostGauge > 0f;


        if (boostPressed && MoveDirection == Vector3.zero)
            MoveDirection = transform.forward;

        HorzVel += MoveDirection * acceleration * (ground ? 1 : 0.4f);
        if (HorzVel.magnitude >= RunSpeed && !boosting && !slide)
            HorzVel = Vector3.MoveTowards(HorzVel, HorzVel.normalized * RunSpeed, 1);
        else if (boosting)
        {
            if (!ground)
            {

                if (!AirBoosted && boostPressedthisFrame)
                {
                    BallActivate(false);
                    HorzVel = MoveDirection * HorzVel.magnitude;
                    AirBoosted = true;
                    BoostGauge -= 10;
                    gameManager.UpdateBoost(BoostGauge);
                    CurrentAirBoostTime = AirBoostTime;
                }

                if (CurrentAirBoostTime > 0 && AirBoosted)
                {
                    HorzVel = HorzVel.normalized * BoostSpeed * Mathf.Sin(Mathf.PI * (CurrentAirBoostTime) / AirBoostTime/2f);
                    VertVel = Gravity.normalized * 8 * Mathf.Cos(Mathf.PI * (CurrentAirBoostTime) / AirBoostTime);
                    CurrentAirBoostTime -= Time.deltaTime;
                }
                else if (CurrentAirBoostTime < 0) 
                {
                    boostPressed = false;
                    CurrentAirBoostTime = 0;

                }
            }
            else
            {
                HorzVel = HorzVel.normalized * BoostSpeed;
            }

            BoostGauge = Mathf.Clamp(BoostGauge, 0, 100);
            BoostGauge -= 10 * Time.deltaTime;
            gameManager.UpdateBoost(BoostGauge);
        }

        if (!boostPressed && AirBoosted )
        {
            boostPressed = false;
            CurrentAirBoostTime = -1f;
        }

        HorzVel = Vector3.RotateTowards(HorzVel, MoveDirection * HorzVel.magnitude, TurnCurve.Evaluate(HorzVel.magnitude), 0f);
        if (Vector3.Dot(MoveDirection, HorzVel.normalized) < .8f)
            HorzVel = Vector3.MoveTowards(HorzVel, Vector3.zero, TurnDecelCurve.Evaluate(HorzVel.magnitude));
        if (!ground)
            HorzVel = Vector3.MoveTowards(HorzVel, Vector3.zero, AirDragCurve.Evaluate(HorzVel.magnitude));

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

        if (stompPressed && !stomp)
        {
            if (Vector3.Dot(VertVel, -Gravity) >= 0f)
                VertVel = Vector3.zero;
            stomp = true;
            BallActivate(false);
            HorzVel = Vector3.zero;
            VertVel += Gravity * 14f;
        }

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
            ringCount++;
            ShadowsMouth.PlayOneShot(gameplayVoices[1]);
            BoostGauge += 6;
            gameManager.UpdateBoost(BoostGauge);
            gameManager.UpdateRings(ringCount);
            collider.gameObject.SetActive(false);
        }
        else if (collider.CompareTag("CameraPanOut"))
        {
            collider.transform.GetChild(0).gameObject.SetActive(true);
            collider.transform.GetChild(0).GetComponent<PannedCamera>().GetFollowData(transform);
        }
        else if (collider.CompareTag("SonicInter"))
        {
            SonicInteractables sonInter = collider.GetComponent<SonicInteractables>();
            if (PlayStyle != State.InterConstrained)
                BeforeConstrained = PlayStyle;
            ChangePlayStle(State.InterConstrained);
            BallActivate(false);
            if ((int)sonInter.ObjectType != 4)
                ShadowsMouth.PlayOneShot(SonicInterClips[(int)sonInter.ObjectType]);
            switch ((int)sonInter.ObjectType)
            {
                case 0:
                    transform.position = collider.transform.position;
                    HorzVel = collider.transform.forward * sonInter.Power;
                    LockTimer = sonInter.LockTime;
                    VertVel = Vector3.zero;
                    break;
                case 1:
                    transform.position = collider.transform.position;
                    keepGravity = true;
                    stomp = false;
                    HorzVel = Vector3.zero;
                    LockTimer = sonInter.LockTime;
                    VertVel = collider.transform.up * sonInter.Power;
                    break;
                case 2:
                    keepGravity = false;
                    HorzVel = Vector3.zero;
                    LockTimer = sonInter.LockTime;
                    VertVel = collider.transform.up * sonInter.Power;
                    break;
                case 3:
                    ChaosMeter = 1;
                    Destroy(collider.gameObject);
                    break;
                case 4:
                    Gravity = -collider.transform.up * 0.5f;
                    GroundNormal = collider.transform.up;
                    transform.up = GroundNormal;
                    ground = true;
                    break;
            }
            ApplyVelocity();
        }
        else if (collider.CompareTag("Speak"))
        {
            collider.GetComponent<InGameVoices>().StartTalking();
        }
        else if (collider.CompareTag("Finish"))
        {
            gameManager.StopTimer();
            ChangePlayStle(State.LockedInPlace);
            GameObject.Find("TopCanvas").transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Your time is " + gameManager.SecondsPassed + " seconds. Press Start to try again";
        }
        else if (collider.CompareTag("Section"))
            StartCoroutine(collider.GetComponent<SectionActivator>().Activate());
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("CameraPanOut"))
            collider.transform.GetChild(0).gameObject.SetActive(false);
        else if (collider.CompareTag("SonicInter"))
            if (!Physics.Raycast(transform.position, Gravity, out groundRayHit, 1f, groundMask, QueryTriggerInteraction.Collide) || groundRayHit.collider.tag != "PlayerInteractable")
                Gravity = Vector3.down * 0.5f;
    }

    private void BallActivate(bool isTrue)
	{
		ballForm.SetActive(isTrue);
		mainBody.SetActive(!isTrue);
	}
}
