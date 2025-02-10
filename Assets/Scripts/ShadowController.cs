using System;
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
    [SerializeField] private bool spearPressed;
    [SerializeField] private bool boostPressedthisFrame;
    [SerializeField] private bool homingPressed;
    [SerializeField] private bool QuickStepFlag;
    [SerializeField] private float QuickStepDirection;
    [SerializeField] private bool ChaosControlPressed;


    [Header("Movement Stuff")]
	public bool ground;
	public bool jump;
	public bool canSpear;
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

	public PlayState playStyle = PlayState.RegularShadow;
	public Action action = Action.None;
    public Rail rail;
    public int railDirection = 1;
	public bool isFrontiersControls = true;
    [SerializeField] float LockTimer;
    [SerializeField] bool keepGravity;
    [SerializeField] PlayState BeforeConstrained;

    [Header("Game Stuff")]
    public uint ringCount;
    public Transform HomingTarget;
    public AudioClip[] gameplayVoices;
	public AudioClip[] SonicInterClips;
    public FootstepCollection[] footstepCollections;
    Vector3 iniPos;
    Collider GravityPlatform;

    public ParticleSystem[] AllVFX;

	public enum PlayState
	{
		RegularShadow,
		Wing,
		Squid,
		Surf,
		SkatingMechanic,
        InterConstrained,
        LockedInPlace,
        Rail,
        Autorun
	}

    public enum Action
    {
        None,
        Boost,
        AirBoost,
        HomingAttack,
        ChaosSnap,
        Slide,
        Brake,
        Stomp,
        Hurt,
    }

	public void ChangePlayStle(PlayState state) { playStyle = state; }

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
        UseChaosSpear();
        ActivateChaosControl();

		UpdateAnimations();
        LaunchHoming();
        if (GamePad.GetButtonTrigger(N3dsButton.Start))
        {
            CommitDie();
        }
	}

    void CommitDie()
    {
        transform.position = iniPos;
        if (playStyle != PlayState.LockedInPlace)
            ShadowsMouth.PlayOneShot(gameplayVoices[3]);
        ChangePlayStle(PlayState.RegularShadow);
        BoostGauge = 100f;
        ringCount = 0;
        gameManager.UpdateRings(0);
        gameManager.UpdateBoost(100);
        gameManager.StopTimer();
        gameManager.Start();
    }

    Vector3 GetDir(Vector3 dir, Vector3 CamDir, float inputVal)
    {
        Vector3 normal = jump ? jumpNomral : GroundNormal;
        Vector3 Out = Quaternion.FromToRotation(dir, CamDir) * dir;
        if (Vector3.ProjectOnPlane(Out, normal).magnitude > 0.6f)
            return Out * inputVal;
        return Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.Euler(0, normal.x * -90 + normal.y * -90 + normal.z * -2 * 90, 0f) * dir * inputVal;
    }

	void GatherInput()
	{
        move = (GamePad.CirclePad + new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized).normalized;

        Vector3 normal = jump ? jumpNomral : GroundNormal;
        MoveDirection = Vector3.ProjectOnPlane(GetDir(Vector3.right, ViewCam.right, move.x) + GetDir(Vector3.forward, ViewCam.forward, move.y), normal).normalized;

        //MoveDirection = Vector3.ProjectOnPlane(move.x * RotateVector(ViewCam.right) + move.y * RotateVector(ViewCam.forward), GroundNormal).normalized;

        if (GamePad.GetButtonTrigger(N3dsButton.B) || Input.GetKeyDown(KeyCode.Space))
			jumpPressed = true;
		else if (GamePad.GetButtonRelease(N3dsButton.B) || Input.GetKeyUp(KeyCode.Space))
			jumpPressed = false;

		stompPressed = GamePad.GetButtonHold(N3dsButton.A) || Input.GetKey(KeyCode.LeftControl);
		spearPressed = (GamePad.GetButtonRelease(N3dsButton.X) || Input.GetMouseButtonUp(1)) && canSpear;

        if (!GamePad.IsCirclePadProConnected())
		{
            if (GamePad.GetButtonTrigger(N3dsButton.Y) || Input.GetKeyDown(KeyCode.LeftShift))
                boostPressed = true;
            else if (GamePad.GetButtonRelease(N3dsButton.Y) || Input.GetKeyUp(KeyCode.LeftShift))
                boostPressed = false;
            QuickStepFlag = GamePad.GetButtonHold(N3dsButton.R);
			homingPressed = GamePad.GetButtonTrigger(N3dsButton.B);
			boostPressedthisFrame = GamePad.GetButtonTrigger(N3dsButton.Y) || boostPressedthisFrame || Input.GetKeyDown(KeyCode.LeftShift);
			ChaosControlPressed = GamePad.GetButtonTrigger(N3dsButton.L) || Input.GetKeyDown(KeyCode.Tab);
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
                boostPressedthisFrame = GamePad.GetButtonTrigger(N3dsButton.ZR) || boostPressedthisFrame || Input.GetKeyDown(KeyCode.LeftShift);
			}
			else
			{
                if (GamePad.GetButtonTrigger(N3dsButton.Y))
                    boostPressed = true;
                else if
                    (GamePad.GetButtonRelease(N3dsButton.Y))
                    boostPressed = false;
                homingPressed = GamePad.GetButtonTrigger(N3dsButton.B);
                boostPressedthisFrame = GamePad.GetButtonTrigger(N3dsButton.Y) || boostPressedthisFrame || Input.GetKeyDown(KeyCode.LeftShift);
            }
        }
    }


    void LaunchHoming()
    {
        if (HomingTarget != null && !ground && action != Action.HomingAttack)
        {
            UnityEngine.Debug.Log("Hominged something");
            StartCoroutine(HomingAttack());
        }
    }

    void StartBoosting()
    {
        if (boostPressedthisFrame && BoostGauge > 0f && CurrentAirBoostTime > 0f)
        {
            AllVFX[0].Play(true);
            boostPressedthisFrame = false;
            action = ground ? Action.Boost : Action.AirBoost;
            HorzVel = MoveDirection * BoostSpeed;
            if (action == Action.AirBoost)
                HorzVel += -Gravity.normalized * BoostSpeed * .2f; 
            BallActivate(false);
            BoostGauge -= 5f;
        }
    }

    IEnumerator HomingAttack()
    {
        float timeout = 0;
        while (Vector3.Distance(transform.position, HomingTarget.transform.position) < 1 && timeout < 2f)
        {
            yield return new WaitForFixedUpdate();
            timeout += Time.fixedDeltaTime;
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

        animator.SetFloat("Speed", HorzVel.magnitude + (action == Action.Boost ? 50f : 0));
        animator.SetFloat("Direction", Mathf.Clamp(Mathf.MoveTowards(animator.GetFloat("Direction"), -Mathf.Sign(Vector3.Cross(MoveDirection, HorzVel.normalized).y) * (1f - Vector3.Dot(MoveDirection, HorzVel.normalized)), Time.deltaTime), -0.1f, .1f));
        animator.SetBool("ground", ground);
        animator.SetBool("stomp", action == Action.Stomp);
        animator.SetBool("brake", action == Action.Brake);
        animator.SetBool("Rail", playStyle == PlayState.Rail);
        animator.SetBool("airBoost", action == Action.AirBoost);
	}

	// Update is called once per frame
	void FixedUpdate() 
	{
        GetInititals();
        VelocityDefine();
        switch (playStyle)
        {
            case PlayState.RegularShadow:

                switch (action) 
                {
                    case Action.Hurt:
                        if (ground)
                        {
                            action = Action.None;
                            HorzVel = -HorzVel;
                        }
                        GravitayionalPull();
                        break;
                    case Action.HomingAttack:
                        break;
                    case Action.ChaosSnap:
                        break;
                    case Action.Brake:
                        if (HorzVel.magnitude < 5f || !ground)
                            action = Action.None;
                        break;
                    case Action.Stomp:
                        if (ground)
                            action = Vector3.Angle(GroundNormal, -Gravity) > 25f ? Action.None : Action.Slide;
                        else
                            VertVel = Gravity * 100f;
                        break;
                    default:
                        GravitayionalPull();
                        CalculateHorzVel();
                        SnapToTheGround();
                        Jump();
                        break;
                }

                break;
            case PlayState.Rail:
                switch (action)
                {
                    case Action.Hurt:
                        boostPressed = false;
                        jumpPressed = false;
                        QuickStepFlag = false;
                        break;
                    default:
                        RailLogic();
                        break;
                }
                break;
            case PlayState.InterConstrained:
                if (keepGravity)
                    GravitayionalPull();
                LockTimer -= Time.deltaTime;
                if (LockTimer < 0)
                    ChangePlayStle(BeforeConstrained);
                break;
		}
        Rotate();
        ApplyVelocity();
    }

	void GetInititals()
	{
        UnityEngine.Debug.DrawRay(transform.position, -GroundNormal * .5f, Color.red, 0f, true);
		if (Physics.Raycast(transform.position, -GroundNormal, out groundRayHit, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore) && groundRayHit.distance < .5f)
		{
			ground = Vector3.Dot(VertVel, -Gravity) <= 0f && (HorzVel.magnitude > 0 || Vector3.Angle(GroundNormal, -Gravity.normalized) < 75f) && (ground || Vector3.Angle(GroundNormal, groundRayHit.normal) <= 45f);
			GroundNormal = groundRayHit.normal;
            rail = null;
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

    private void UseChaosSpear()
    {
        if (spearPressed)
        {
            canSpear = false;
            animator.SetTrigger("SpearUse");
            BallActivate(false);
            ShadowsMouth.PlayOneShot(SonicInterClips[2]);
            StartCoroutine(ChaosSpearCooldown());
        }
    }

    IEnumerator ChaosSpearCooldown()
    {
        yield return new WaitForSecondsRealtime(.25f);
        canSpear = true;
    }

    public void SnapToTheGround()
    {
        if (!ground)
            return;

        transform.position = groundRayHit.point + GroundNormal * .4f;
        VertVel = Vector3.zero;
        jump = false;
        CurrentAirBoostTime = AirBoostTime;
        if (action == Action.AirBoost)
            action = Action.Boost;
        else if (action == Action.Stomp)
            action = Action.None;
		timesJumped = 0;
		BallActivate(false);
        if (action == Action.Slide && Vector3.Angle(-Gravity, groundRayHit.normal) < 5f)
        {
            HorzVel = Vector3.MoveTowards(HorzVel, Vector3.zero, 6);
            if (HorzVel.magnitude < 3f)
            {
                action = Action.None;
            }
        }
    }

    void CalculateHorzVel()
    {
        if (!canSpear)
            return;

        
        //MoveDirection = Vector3.ProjectOnPlane((Quaternion.FromToRotation(-Gravity, normal) * ((ViewCam.forward * move.y + ViewCam.right * move.x).normalized) * 3f).normalized, normal).normalized;
        

        if (boostPressed && MoveDirection.sqrMagnitude == 0)
            MoveDirection = transform.forward;

        if (ground || HorzVel.magnitude < 10)
            HorzVel += MoveDirection * acceleration;
        StartBoosting();
        switch (action) 
        {
            case Action.Slide:
                HorzVel = Vector3.ClampMagnitude(HorzVel, 20f);
                break;
            case Action.Boost:
                HorzVel = Vector3.MoveTowards(HorzVel, HorzVel.normalized * BoostSpeed, 1.5f);
                BoostGauge -= .02f;

                gameManager.UpdateBoost(BoostGauge);
                StopBoosting();
                break;
            case Action.AirBoost:
                HorzVel = HorzVel.normalized * BoostSpeed * (1 - AirBoostTime + CurrentAirBoostTime);
                CurrentAirBoostTime -= Time.deltaTime;
                gameManager.UpdateBoost(BoostGauge);
                StopBoosting();
                    
                break;
            default:
                if (MoveDirection.magnitude >= .7f && HorzVel.magnitude >= RunSpeed)
                    HorzVel = Vector3.MoveTowards(HorzVel, HorzVel.normalized * RunSpeed, 1);
                else if (MoveDirection.magnitude < .1f)
                    HorzVel = Vector3.MoveTowards(HorzVel, Vector3.zero, deceleration);
                else if (MoveDirection.magnitude < .7f)
                    HorzVel = HorzVel.normalized * RunSpeed * MoveDirection.magnitude * MoveDirection.magnitude;
                break;
        }

        if (!ground && HorzVel.magnitude > 20)
            HorzVel = Vector3.MoveTowards(HorzVel, HorzVel.normalized, AirDragCurve.Evaluate(HorzVel.magnitude));

        HorzVel = Vector3.RotateTowards(HorzVel, MoveDirection * HorzVel.magnitude, TurnCurve.Evaluate(HorzVel.magnitude), 0);

        if (Vector3.Dot(MoveDirection, HorzVel.normalized) < .9f)
            HorzVel = Vector3.MoveTowards(HorzVel, Vector3.zero, TurnDecelCurve.Evaluate(HorzVel.magnitude));
        else if (Vector3.Dot(MoveDirection, HorzVel.normalized) < -0.5f && HorzVel.magnitude > 20f)
            action = Action.Brake;

        HorzVel = Vector3.ClampMagnitude(HorzVel, BoostSpeed);
    }

    private void StopBoosting()
    {
        if (boostPressed && BoostGauge > 0f && CurrentAirBoostTime > 0f)
            return;

        AllVFX[0].Stop(true);
        action = Action.None;
    }

    public void Jump()
    {
        bool canJump = timesJumped < 2;

        if (canJump && !jump && jumpPressed)
        {
            action = Action.None;
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
            CurrentJumpTime -= Time.unscaledDeltaTime;
            VertVel += jumpNomral * jumpForce * (CurrentJumpTime / JumpTime);
            GroundNormal = -Gravity.normalized;
        }
        else
        {
            jumpPressed = false;
            jump = false;
        }
    }
    
    private void UsingChaosSpear()
    {
        if (spearPressed && canSpear)
        {
            canSpear = false;
            animator.SetTrigger("SpearUse");
        }
    }

    private void GravitayionalPull()
    {
        if (ground)
            return;

        if (action == Action.AirBoost && !canSpear)
            VertVel += Gravity;

        if (VertVel.magnitude < 100)
            VertVel += Gravity;

        if (stompPressed && action != Action.Stomp)
        {
            if (Vector3.Dot(VertVel, -Gravity) >= 0f)
                VertVel = Vector3.zero;
            action = Action.Stomp;
            BallActivate(false);
            HorzVel = Vector3.zero;
            VertVel += Gravity * 14f;
        }

    }



    public void Rotate()
    {
        if (action == Action.Hurt)
            return;
        Vector3 normal = ground ? GroundNormal : (jump ? jumpNomral : -Gravity.normalized);

        Quaternion ToRotate = Quaternion.LookRotation(Vector3.ProjectOnPlane(HorzVel.normalized == Vector3.zero ? transform.forward : HorzVel.normalized, normal).normalized, normal);
        transform.rotation = Quaternion.Slerp(transform.rotation, ToRotate, 14f * Time.unscaledDeltaTime);
    }

    void ApplyVelocity() { rigidbody.velocity = HorzVel + VertVel; }

    public IEnumerator ChaosControl()
	{
		ShadowsMouth.PlayOneShot(gameplayVoices[0]);
		BoostGauge = 100f;
		Time.timeScale = 0f;
		yield return new WaitForSecondsRealtime(6f);
		Time.timeScale = 1;
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
            if (playStyle != PlayState.InterConstrained)
                BeforeConstrained = playStyle;
            if (playStyle == PlayState.Rail || playStyle == PlayState.Autorun)
                BeforeConstrained = playStyle == PlayState.Wing ? PlayState.Wing : PlayState.RegularShadow;
            ChangePlayStle(PlayState.InterConstrained);
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
                    action = Action.None;
                    HorzVel = Vector3.zero;
                    LockTimer = sonInter.LockTime;
                    VertVel = collider.transform.up * sonInter.Power;
                    break;
                case 2:
                    keepGravity = true;
                    transform.position = collider.transform.position;
                    LockTimer = sonInter.LockTime;
                    VertVel = collider.transform.up * sonInter.Power;
                    break;
                case 3:
                    ChaosMeter = 1;
                    Destroy(collider.gameObject);
                    break;
                case 4:
                    Gravity = -collider.transform.up * 0.5f;
                    GravityPlatform = collider;
                    GroundNormal = collider.transform.up;
                    VertVel = Vector3.Project(VertVel, GroundNormal);
                    ground = true;
                    ChangePlayStle(BeforeConstrained);
                    break;
                case 5:
                    ChangePlayStle(BeforeConstrained);
                    if (playStyle == PlayState.Autorun)
                    {
                        ChangePlayStle(PlayState.Autorun);
                        rail = collider.transform.parent.GetComponent<Rail>();
                    }
                    break;
                case 6:

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
            ChangePlayStle(PlayState.LockedInPlace);
            GameObject.Find("TopCanvas").transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Your time is " + gameManager.SecondsPassed + " seconds. Press Start to try again";
        }
        else if (collider.CompareTag("Section"))
            StartCoroutine(collider.GetComponent<SectionActivator>().Activate());
        else if (collider.CompareTag("Hurt"))
        {
            if (playStyle == PlayState.Wing)
                return;
            action = Action.Hurt;
            BallActivate(false);
            switch (collider.name)
            {
                case "bomb":
                    Transform bomb = collider.transform.parent;
                    bomb.GetChild(0).gameObject.SetActive(false);
                    bomb.GetChild(1).gameObject.SetActive(true);
                    break;
                default:
                    break;
            }

            if (ringCount == 0)
            {
                CommitDie();
                return;
            }
            else
            {
                if (ringCount >= 60)
                    ringCount -= 60;
                else ringCount = 0;
            }

            ShadowsMouth.PlayOneShot(gameplayVoices[1]);
               

            gameManager.UpdateRings(ringCount);

            if (playStyle == PlayState.RegularShadow)
            {

                
                transform.position += -Gravity.normalized * .5f;
                rigidbody.velocity = (-transform.forward - Gravity) * 9f;
                MoveDirection = transform.forward;
            }

            animator.SetTrigger("Hurt");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Rail") && !rail)
        {
            
            BeforeConstrained = playStyle == PlayState.Wing ? PlayState.Wing : PlayState.RegularShadow;
            ChangePlayStle(PlayState.Rail);
            rail = collision.collider.GetComponent<Rail>();
            RailSnap();
            rigidbody.velocity = Vector3.zero;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("CameraPanOut"))
            collider.transform.GetChild(0).gameObject.SetActive(false);
        else if (collider.CompareTag("SonicInter"))
            if (collider == GravityPlatform)
            {
                Gravity = Vector3.down * 0.5f;
                GravityPlatform = null;
            }
    }

    public void RailLogic()
    {
        Vector3 toMove = rail.position + rail.points[rail.i] + transform.up * .3f;
        VertVel = Vector3.zero;
        //UnityEngine.Debug.Log(Mathf.Sqrt(DistanceSquared(transform.position, toMove)) + " " + Vector3.Distance(transform.position, toMove));
        if (Vector3.Distance(transform.position, toMove) > .5f)
        {
            HorzVel = (toMove - transform.position).normalized * (action == Action.Boost ? BoostSpeed : RunSpeed);
            transform.forward = HorzVel;
        }
        else
        {
            rail.i += railDirection;
            if (rail.i < 0 || rail.i >= rail.points.Count)
                ground = false;
        }
    }

    private void RailSnap()
    {
        float[] distances = new float[rail.points.Count];
        int clI = 0, i = 0;
        for (; i < distances.Length; i++)
            distances[i] = DistanceSquared(transform.position - transform.up * .3f, rail.position + rail.points[i]);

        for (i = 0; i < distances.Length; ++i)
        {
            if (distances[i] < distances[clI] && Vector3.Dot(transform.forward, rail.position + rail.points[i] - transform.position - transform.up * .3f) > 0f)
                clI = i;
        }
        if (clI == 0)
            clI = 1;
        rail.i = clI;
        ground = true;
        BallActivate(false);
    }


    float DistanceSquared(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(b.sqrMagnitude - a.sqrMagnitude);
    }

    private void BallActivate(bool isTrue)
	{
		ballForm.SetActive(isTrue);
		mainBody.SetActive(!isTrue);
	}
}
