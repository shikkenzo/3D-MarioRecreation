using System.Collections;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class PlayerController : MonoBehaviour, IRestartGameElement
{
    public enum TPunchType
    {
        RIGHT_HAND = 0,
        LEFT_HAND,
        KICK
    }
    public Camera m_Camera;
    CharacterController m_characterController;
    private CollisionFlags m_collisionFlags;
    Animator m_animator;
    Vector3 m_startPosition;
    Quaternion m_startRotation;
    public float m_WalkSpeed;
    public float m_RunSpeed;
    private float m_verticalSpeed = 0.0f;
    public Transform m_LookAt;
    [Range(0.0f, 1.0f)] public float m_RotationLerpPct = 0.0f;
    public float m_AnimationDampTime = 0.2f;

    private bool m_isCollidingWithMap;

    Checkpoint m_currentCheckpoint;

    CoinController m_coinController = new CoinController();
    HealthController m_healthController = new HealthController();

    [Header("Health")]
    public int m_MaxHealth = 8;
    private int m_startingHealth;

    public int m_StartingGlobalHP = 3;

    [Header("Coins")]
    public int m_MaxCoins = 999;
    private int m_startingCoins;

    [Header("Jump")]
    public float m_BaseJumpVerticalSpeed = 6.0f;
    public float m_LongJumpVerticalSpeed = 10.0f;
    public float m_LongJumpHorizontalSpeed = 20.0f;
    public float m_FallingGravityMultiplier = 2.0f;
    private float m_gravityCurrentMultiplier;
    [Range(0.0f, 1.0f)] public float m_ModifyGravityOffsetPct = 0.0f;
    public float m_KillJumpSpeed = 4.0f;
    public float m_MaxAngleToKillGoomba = 60.0f;
    public float m_MaxTimeToChainJump = 0.8f;
    int m_currentJumpId;
    float m_lastJumpLandTime;
    public float m_JumpChainBoostPct = 0.5f;
    public float m_WallJumpVerticalSpeed = 6.0f;
    public float m_WallJumpHorizontalSpeed = 6.0f;
    private Vector3 m_wallJumpDirection;
    public float m_WallJumpImpulseDuration = 1.0f;
    private float m_wallJumpRemainingTime = 0.0f;
    public float m_MaxHorizontalAngleToWallJump = 30.0f;
    public float m_MinVerticalAngleToWallJump = 30.0f;
    public float m_coyoteTime = 0.2f;
    float m_coyoteRemainingTime = 0.0f;

    public float m_HitImpulseDuration = 1.0f;
    private float m_hitImpulseRemainingTime = 0.0f;
    public float m_HitImpulseVerticalSpeed = 6.0f;
    public float m_HitImpulseHorizontalSpeed = 10.0f;
    private Vector3 m_hitImpulseDirection;

    [Header("Punch")]
    public float m_MaxTimeToCombo = 0.8f;
    int m_currentPunchId;
    float m_lastPunchTime;
    public GameObject m_RightHandPunchCollider;
    public GameObject m_LeftHandPunchCollider;
    public GameObject m_KickPunchCollider;

    [Header("Input")]
    public int m_PunchMouseButton = 0;
    public KeyCode m_JumpKeyCode = KeyCode.Space;

    [Header("GamepadInput")]
    public GamepadButton m_PunchGamepad = GamepadButton.East;
    public GamepadButton m_JumpGamepad = GamepadButton.South;
    public GamepadButton m_RunGamepad = GamepadButton.LeftTrigger;
    public StickControl m_MovementStick;
    public StickControl m_CameraStick;

    [Header("Idle")]
    private bool m_isGettingInput;
    private float m_idleCurrentTime;
    public float m_CameraRepositionWaitTime = 5.0f;
    public float m_SpecialIdleWaitTime = 10.0f;
    private AnimationOverrider m_animationOverrider;
    public AnimatorOverrideController m_SpecialIdleOverrideController;
    public AnimatorOverrideController m_DefaultIdleOverrideController;

    public bool m_resetCamera { get; private set; }
    private bool m_specialIdle;

    [Header("Sound")]
    public AudioSource m_LeftFootStepAudioSource;
    public AudioSource m_RightFootStepAudioSource;
    public AudioSource m_JumpAudioSource;
    public AudioSource m_PunchAudioSource;
    public AudioSource m_HitAudioSource;
    public AudioSource m_DeathAudioSource;
    public AudioSource m_CoinAudioSource;
    public AudioSource m_StarAudioSource;

    [Header("Elevator")]
    public float m_MaxAngleToAttachToElevator = 30.0f;
    Collider m_elevatorCollider;
    public float m_BridgeHitForce = 10.0f;

    [Header("Grip")]
    public Transform m_Grip;

    [Header("Debug")]
    public int m_DebugInt;
    public float m_DebugFloat;

    private void Awake()
    {
        m_characterController = GetComponent<CharacterController>();
        m_animator = GetComponent<Animator>();

        m_animationOverrider = GetComponent<AnimationOverrider>();
    }
    private void Start()
    {
        m_lastPunchTime = -m_MaxTimeToCombo;
        m_lastJumpLandTime = -m_MaxTimeToChainJump;

        m_RightHandPunchCollider.SetActive(false);
        m_LeftHandPunchCollider.SetActive(false);
        m_KickPunchCollider.SetActive(false);

        m_startPosition = transform.position;
        m_startRotation = transform.rotation;
        m_startingHealth = m_MaxHealth;
        m_startingCoins = 0;

        ResetHP();
        ResetCoins();
        ResetGlobalHP();

        Cursor.lockState = CursorLockMode.Locked;

        GameManager.GetGameManager().AddRestartGameElement(this);
    }
    private void Update()
    {
        if (GameManager.GetGameManager().IsUIActive()) return;

        Vector3 l_right = m_Camera.transform.right;
        Vector3 l_forward = m_Camera.transform.forward;
        Vector3 l_movement = Vector3.zero;

        l_right.y = 0;
        l_right.Normalize();
        l_forward.y = 0;
        l_forward.Normalize();

        m_isGettingInput = false;

        if (Input.GetKey(KeyCode.D))
        {
            l_movement += l_right;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            l_movement -= l_right;
        }


        if (Input.GetKey(KeyCode.W))
        {
            l_movement += l_forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            l_movement -= l_forward;
        }

        if (GameManager.GetGameManager().IsGamepadConnected())
        {
            l_movement += m_MovementStick.ReadValue().y * l_forward;
            l_movement += m_MovementStick.ReadValue().x * l_right;
        }
        
        m_isGettingInput = (l_movement.magnitude > 0) || m_Camera.GetComponent<CameraController>().IsCameraGettingInput();

        m_resetCamera = m_idleCurrentTime > m_CameraRepositionWaitTime;
        m_specialIdle = m_idleCurrentTime > m_SpecialIdleWaitTime;

        if (m_specialIdle)
        {
            PlaySpecialIdle();
        }
        else
        {
            StopSpecialIdle();
        }

        if (GameManager.GetGameManager().IsGamepadConnected() ? Gamepad.current[m_JumpGamepad].isPressed : Input.GetKey(m_JumpKeyCode))
        {
            if (CanJump())
            {
                if (IsRunning())
                {
                    LongJump();
                }
                else
                {
                    Jump();
                }
            }
            else if (CanWallJump())
            {
                WallJump();
            }

            m_isGettingInput = true;
        }

        l_movement.Normalize();

        float l_SpeedAnimationValue = 0.5f;
        float l_Speed = m_WalkSpeed;

        if (IsRunning())
        {
            l_Speed = m_RunSpeed;
            l_SpeedAnimationValue = 1.0f;
        }
        else if (IsInLongJump())
        {
            l_Speed = m_LongJumpHorizontalSpeed;
            l_SpeedAnimationValue = 0.0f;
        }
        else if (IsInWallJump())
        {
            if (m_wallJumpRemainingTime > 0.0f)
            {
                l_Speed = m_WallJumpHorizontalSpeed;
                l_movement = m_wallJumpDirection.normalized;

                UpdateUpWallJump(); 
            }
        }
        else if (IsHit())
        {
            if (m_hitImpulseRemainingTime > 0.0)
            {
                l_Speed = m_HitImpulseHorizontalSpeed;
                l_movement = m_hitImpulseDirection.normalized;
            }
        }

        if (l_movement.sqrMagnitude == 0.0f)
        {
            m_animator.SetFloat("Speed", 0.0f, m_AnimationDampTime, Time.deltaTime);
            if (m_animator.GetFloat("Speed") < 0.1f) m_animator.SetFloat("Speed", 0.0f, 0.0f, Time.deltaTime);
        }
        else
        {
            m_animator.SetFloat("Speed", l_SpeedAnimationValue, m_AnimationDampTime, Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(l_movement), m_RotationLerpPct);
        }

        l_movement *= l_Speed * Time.deltaTime;
        if (IsDead()) l_movement = Vector3.zero;


        m_verticalSpeed += Physics.gravity.y * Time.deltaTime * m_gravityCurrentMultiplier;
        l_movement.y = m_verticalSpeed * Time.deltaTime;

        if (m_verticalSpeed < m_ModifyGravityOffsetPct * m_BaseJumpVerticalSpeed)
        {
            m_gravityCurrentMultiplier = m_FallingGravityMultiplier;
        }
        else
        {
            m_gravityCurrentMultiplier = 1.0f;
        }


        m_collisionFlags = m_characterController.Move(l_movement);
        if ((m_collisionFlags & CollisionFlags.CollidedBelow) != 0 && m_verticalSpeed < 0.0f)
        {
            m_verticalSpeed = 0.0f;

            m_coyoteRemainingTime = m_coyoteTime;

            if (IsJumping())
            {
                LandJump();
            }
        }
        else if ((m_collisionFlags & CollisionFlags.CollidedAbove) != 0 && m_verticalSpeed > 0.0f)
        {
            m_verticalSpeed = 0.0f;
        }

        UpdatePunch();
        SendVerticalSpeed();

        //TIMERS//
        m_coyoteRemainingTime -= Time.deltaTime;
        if (m_coyoteRemainingTime < 0.0f) m_coyoteRemainingTime = 0.0f;

        if (IsInWallJump())
        {
            m_wallJumpRemainingTime -= Time.deltaTime;
            if (m_wallJumpRemainingTime < 0.0f) m_wallJumpRemainingTime = 0.0f;
        }
        else
        {
            m_wallJumpRemainingTime = m_WallJumpImpulseDuration;
        }

        if (IsHit())
        {
            m_hitImpulseRemainingTime -= Time.deltaTime;
            if (m_hitImpulseRemainingTime < 0.0f) m_hitImpulseRemainingTime = 0.0f;
        }
        else
        {
            m_hitImpulseRemainingTime = m_HitImpulseDuration;
        }

        if (!m_isGettingInput)
        {
            m_idleCurrentTime += Time.deltaTime;
        }
        else
        {
            m_idleCurrentTime = 0.0f;
        }
        //
    }
    private void LateUpdate()
    {
        UpdateElevator();
    }

    private bool IsInMovementState()
    {
        return (!m_animator.IsInTransition(0) && m_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Movement"));
    }
    private bool IsRunning()
    {
        return (IsInMovementState() && (GameManager.GetGameManager().IsGamepadConnected() ? Gamepad.current[m_RunGamepad].isPressed : Input.GetKey(KeyCode.LeftShift)));
    }

    private void UpdatePunch()
    {
        if (CanPunch() && (GameManager.GetGameManager().IsGamepadConnected() ? Gamepad.current[m_PunchGamepad].isPressed : Input.GetMouseButtonDown(m_PunchMouseButton)))
        {
            Punch();
        }
    }
    private bool CanPunch()
    {
        return IsInMovementState();
    }
    private void Punch()
    {
        if (m_currentGrabbedTransform == null)
        {
            float l_DiffPunchTime = Time.time - m_lastPunchTime;

            if (l_DiffPunchTime < m_MaxTimeToCombo)
            {
                m_currentPunchId = (m_currentPunchId + 1) % 3;
            }
            else
            {
                m_currentPunchId = 0;
            }
            m_lastPunchTime = Time.time;

            m_animator.SetTrigger("Punch");
            m_animator.SetInteger("PunchId", m_currentPunchId);
        }
        else
        {
            ThrowShell();
        }
    }
    public void SetActivePunch(TPunchType punchType, bool isActive)
    {
        if (punchType == TPunchType.RIGHT_HAND)
        {
            m_RightHandPunchCollider.SetActive(isActive);
        }
        if (punchType == TPunchType.LEFT_HAND)
        {
            m_LeftHandPunchCollider.SetActive(isActive);
        }
        if (punchType == TPunchType.KICK)
        {
            m_KickPunchCollider.SetActive(isActive);
        }
    }

    bool CanJump()
    {
        return (IsInMovementState() && (m_coyoteRemainingTime > 0));
    }
    void Jump()
    {
        float l_DiffJumpTime = Time.time - m_lastJumpLandTime;

        if (l_DiffJumpTime < m_MaxTimeToChainJump)
        {
            m_currentJumpId = (m_currentJumpId + 1) % 3;
        }
        else
        {
            m_currentJumpId = 0;
        }

        m_verticalSpeed = m_BaseJumpVerticalSpeed + (m_BaseJumpVerticalSpeed * m_JumpChainBoostPct * m_currentJumpId);

        m_coyoteRemainingTime = 0;

        m_animator.SetTrigger("Jump");
        m_animator.SetInteger("JumpId", m_currentJumpId);
    }
    void LongJump()
    {
        m_verticalSpeed = m_LongJumpVerticalSpeed;

        m_coyoteRemainingTime = 0;

        m_animator.SetTrigger("LongJump");
    }
    bool CanWallJump()
    {
        return (!m_animator.IsInTransition(0) && !IsInWallJump() && IsJumping() && m_isCollidingWithMap && ((m_collisionFlags & CollisionFlags.CollidedSides) != 0) && ((Vector3.Dot(m_wallJumpDirection, -m_characterController.transform.forward)) > Mathf.Cos(m_MaxHorizontalAngleToWallJump * Mathf.Deg2Rad))) && ((Vector3.Dot(m_wallJumpDirection, transform.up)) < Mathf.Cos(m_MinVerticalAngleToWallJump * Mathf.Deg2Rad));
    }
    void WallJump()
    {
        m_verticalSpeed = m_WallJumpVerticalSpeed;

        Debug.Log("Wall" + m_DebugInt++);
        m_animator.SetTrigger("WallJump");
    }
    bool IsInJumpChain()
    {
        return (m_animator.GetCurrentAnimatorStateInfo(0).IsTag("JumpChain") && !m_animator.IsInTransition(0));
    }
    bool IsInLongJump()
    {
        return (m_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("LongJump") && !m_animator.IsInTransition(0));
    }
    bool IsInWallJump()
    {
        return (m_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("WallJump") && !m_animator.IsInTransition(0));
    }
    bool IsJumping()
    {
        return (IsInJumpChain() || IsInLongJump() || IsInWallJump());
    }
    void LandJump()
    {
        m_animator.SetTrigger("Land");
        SetLastJumpTime();
    }
    void SetLastJumpTime()
    {
        m_lastJumpLandTime = Time.time;
    }
    void SendVerticalSpeed()
    {
        m_animator.SetFloat("VerticalSpeed", m_verticalSpeed);
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Goomba"))
        {
            EnemyController l_goombaEnemy = hit.collider.GetComponent<EnemyController>();
            if (CanKillWithFeet(hit))
            {
                l_goombaEnemy.Kill();
                JumpOverEnemy();
            }
            else
            {
                Hit(-1, hit.normal);
            }
        }
        else if (hit.collider.CompareTag("Bridge"))
        {
            hit.rigidbody.AddForceAtPosition(-hit.normal * m_BridgeHitForce, hit.point);
        }
        else if (hit.collider.CompareTag("Deadzone"))
        {
            Kill();
        }

        if (hit.collider.CompareTag("Map"))
        {
            m_isCollidingWithMap = true;
            m_wallJumpDirection = hit.normal;
            m_wallJumpDirection.y = 0;
        }
        else
        {
            m_isCollidingWithMap = false;
        }
    }
    void JumpOverEnemy()
    {
        m_verticalSpeed = m_KillJumpSpeed;
    }
    bool CanKillWithFeet(ControllerColliderHit hit)
    {
        float l_dot = Vector3.Dot(hit.normal, Vector3.up);
        return m_verticalSpeed < 0.0f && l_dot > Mathf.Cos(m_MaxAngleToKillGoomba * Mathf.Deg2Rad);
    }
    public void Step(AnimationEvent _animationEvent)
    {
        AudioSource l_currentAudioSource = null;
        if (_animationEvent.stringParameter == "Left")
        {
            l_currentAudioSource = m_LeftFootStepAudioSource;
        }
        else if (_animationEvent.stringParameter == "Right")
        {
            l_currentAudioSource = m_RightFootStepAudioSource;
        }
        AudioClip l_audioClip = (AudioClip)_animationEvent.objectReferenceParameter;
        l_currentAudioSource.clip = l_audioClip;
        l_currentAudioSource.Play();
    }
    public void Jump1(AnimationEvent _animationEvent)
    {
        m_JumpAudioSource.clip = (AudioClip)_animationEvent.objectReferenceParameter;
        m_JumpAudioSource.Play();
    }
    public void Jump2(AnimationEvent _animationEvent)
    {
        m_JumpAudioSource.clip = (AudioClip)_animationEvent.objectReferenceParameter;
        m_JumpAudioSource.Play();
    }
    public void Jump3(AnimationEvent _animationEvent)
    {
        m_JumpAudioSource.clip = (AudioClip)_animationEvent.objectReferenceParameter;
        m_JumpAudioSource.Play();
    }
    public void LongJumpSound(AnimationEvent _animationEvent)
    {
        m_JumpAudioSource.clip = (AudioClip)_animationEvent.objectReferenceParameter;
        m_JumpAudioSource.Play();
    }
    public void PunchSound1(AnimationEvent _animationEvent)
    {
        m_PunchAudioSource.clip = (AudioClip)_animationEvent.objectReferenceParameter;
        m_PunchAudioSource.Play();
    }
    public void PunchSound2(AnimationEvent _animationEvent)
    {
        m_PunchAudioSource.clip = (AudioClip)_animationEvent.objectReferenceParameter;
        m_PunchAudioSource.Play();
    }
    public void PunchSound3(AnimationEvent _animationEvent)
    {
        m_PunchAudioSource.clip = (AudioClip)_animationEvent.objectReferenceParameter;
        m_PunchAudioSource.Play();
    }
    public void FinishPunch(AnimationEvent _animationEvent)
    {
        m_PunchAudioSource.clip = (AudioClip)_animationEvent.objectReferenceParameter;
        m_PunchAudioSource.Play();
    }
    public void HitSound(AnimationEvent _animationEvent)
    {
        m_HitAudioSource.clip = (AudioClip)_animationEvent.objectReferenceParameter;
        m_HitAudioSource.Play();
    }
    public void DeathSound(AnimationEvent _animationEvent)
    {
        m_DeathAudioSource.clip = (AudioClip)_animationEvent.objectReferenceParameter;
        m_DeathAudioSource.Play();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Elevator"))
        {
            if (CanAttachToElevator(other))
            {
                AttachToElevator(other);
            }
        }
        else if (other.CompareTag("Checkpoint"))
        {
            m_currentCheckpoint = other.GetComponent<Checkpoint>();
        }
        else if (other.CompareTag("Item"))
        {
            Item l_Item = other.GetComponentInParent<Item>();

            if (l_Item.CanPick())
                l_Item.Pick();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Elevator"))
        {
            DetachFromElevator();
        }
    }

    bool CanAttachToElevator(Collider elevatorCollider)
    {
        return Vector3.Dot(elevatorCollider.transform.up, Vector3.up) > Mathf.Cos(m_MaxAngleToAttachToElevator * Mathf.Deg2Rad);
    }
    void AttachToElevator(Collider elevatorCollider)
    {
        transform.SetParent(elevatorCollider.transform.parent);
        m_elevatorCollider = elevatorCollider;
    }
    void DetachFromElevator()
    {
        transform.SetParent(null);
        UpdateUpElevator();
        m_elevatorCollider = null;
    }
    void UpdateElevator()
    {
        if (m_elevatorCollider != null)
        {
            UpdateUpElevator();
        }
    }
    void UpdateUpElevator()
    {
        if (m_elevatorCollider != null)
        {
            Vector3 l_direction = transform.forward;
            l_direction.y = 0.0f;
            l_direction.Normalize();
            transform.rotation = Quaternion.LookRotation(l_direction, Vector3.up);
        }
    }
    void UpdateUpWallJump()
    {
        Vector3 l_direction = transform.forward;
        l_direction.y = 0.0f;
        l_direction.Normalize();
        transform.rotation = Quaternion.LookRotation(l_direction, Vector3.up);
    }

    public void AddCoin()
    {
        m_coinController.AddCoins(1);

        m_CoinAudioSource.PlayOneShot(m_CoinAudioSource.clip);
    }

    public int GetCoins()
    {
        return m_coinController.GetValue();
    }

    public void AddHealth()
    {
        m_healthController.AddHealthPoints(1);

        m_StarAudioSource.Play();
    }
    
    public int GetHealth()
    {
        return m_healthController.GetValue();
    }

    private bool CanBeHit()
    {
        return (!(m_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Hit") || IsDead()) && !m_animator.IsInTransition(0));
    }
    public void Hit(int hpChangeValue)
    {
        if (CanBeHit())
        {
            m_hitImpulseRemainingTime = 0.0f;
            HitImpulse(Vector3.zero);

            m_healthController.AddHealthPoints(hpChangeValue);

            m_animator.SetTrigger("Hit");

            CheckDeath();
        }
    }
    public void Hit(int hpChangeValue, Vector3 impulse)
    {
        if (CanBeHit())
        {
            m_hitImpulseRemainingTime = m_HitImpulseDuration;
            HitImpulse(impulse);

            m_healthController.AddHealthPoints(hpChangeValue);

            m_animator.SetTrigger("Hit");

            CheckDeath();
        }
    }
    private void HitImpulse(Vector3 impulseDirection)
    {
        m_verticalSpeed = m_HitImpulseVerticalSpeed;

        m_hitImpulseDirection = impulseDirection;
        m_hitImpulseDirection.y = 0.0f;
    }

    private void CheckDeath()
    {
        if (m_healthController.GetValue() <= 0) Die();
    }
    private void Die()
    {
        m_healthController.HitGlobalHP();

        StartCoroutine(Death());
        m_animator.SetTrigger("Death");
    }
    public void Kill()
    {
        Hit(-m_MaxHealth);
    }

    private IEnumerator Death()
    {
        yield return new WaitForSeconds(2.0f);
        GameManager.GetGameManager().RestartScreen(m_healthController.GetGlobalHP() > 0);
    }
    private bool IsDead()
    {
        return (m_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Die")); 
    }
    private bool IsHit()
    {
        return (m_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Hit"));
    }

    private void PlaySpecialIdle()
    {
        m_animationOverrider.SetAnimations(m_SpecialIdleOverrideController);
    }

    private void StopSpecialIdle()
    {
        m_animationOverrider.SetAnimations(m_DefaultIdleOverrideController);
    }

    public void ResetCheckpoints()
    {
        Debug.Log(m_currentCheckpoint);
        m_currentCheckpoint = null;
        Debug.Log(m_currentCheckpoint);
    }

    public void ResetGlobalHP()
    {
        m_healthController.ResetGlobalHP(m_StartingGlobalHP);
    }

    public void ResetHP()
    {
        m_healthController.ResetValue(m_startingHealth);
    }

    public void ResetCoins()
    {
        m_coinController.ResetValue(m_startingCoins);
    }

    public Vector2 GetCameraGamepadInput()
    {
        return (m_CameraStick.value);
    }
    private Transform m_currentGrabbedTransform;
    public void GrabShell(Transform shellTransform)
    {
        shellTransform.parent = m_Grip;
        m_currentGrabbedTransform = shellTransform;
        shellTransform.position = m_Grip.position;
    }
    public void ThrowShell()
    {
        if (m_currentGrabbedTransform != null)
        {
            Transform l_transform = m_currentGrabbedTransform;
            m_currentGrabbedTransform.parent = null;
            m_currentGrabbedTransform = null;

            l_transform.GetComponent<ShellBase>().Throw(transform.forward);
        }
    }

    public void RestartGame()
    {
        Debug.Log("Restart");

        m_animator.SetTrigger("Restart");

        Vector3 l_respawnPosition = m_startPosition;
        Quaternion l_respawnRotation = m_startRotation;

        if (m_currentCheckpoint != null)
        {
            l_respawnPosition = m_currentCheckpoint.m_RespawnTransform.position;
            l_respawnRotation = m_currentCheckpoint.m_RespawnTransform.rotation;
        }
        m_characterController.enabled = false;
        transform.position = l_respawnPosition;
        transform.rotation = l_respawnRotation;
        m_characterController.enabled = true;

        ResetHP();
    }
}