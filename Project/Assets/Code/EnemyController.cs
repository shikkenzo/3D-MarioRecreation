using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour, IRestartGameElement
{
    enum IState 
    { 
        IDLE = 0, 
        PATROL, 
        ALERT, 
        ATTACK, 
        WAIT_TO_ATTACK, 
        BOUNCING 
    }

    public enum IType 
    { 
        GOOMBA, 
        KOOPA 
    }

    [Header("Type")]
    public IType m_EnemyType;
    public GameObject m_Shell;

    IState m_State, m_LastState;
    NavMeshAgent m_navMeshAgent;
    CollisionFlags m_collisionFlags;
    Vector3 m_startPosition;
    Quaternion m_startRotation;
    CharacterController m_characterController;
    Animator m_animator;

    [Header("Distances")]
    public float m_MaxEarDistance = 9.0f;
    public LayerMask m_LayerMask;

    [Header("Patrol")]
    public List<Transform> m_PatrolPoints;
    int m_CurrentPatrolPointId = 0;

    [Header("Alert")]
    public float m_AlertRotateTime = 0.3f;
    float m_StartYaw;
    float m_TargetYaw;
    float m_YawTimer;

    [Header("Attack")]
    public float m_attackTime = 3.0f;
    public Transform m_LookAt;
    public float m_WallRaycastDistance = 1.5f;
    public float m_WallAngleToBouncing = 25.0f;

    float m_attackTimer;
    Vector3 m_attackDirection = Vector3.zero;
    Vector3 m_targetPlayerPosition = Vector3.zero;
    public float m_AttackSpeed = 8.0f;
    public float m_TimeToAttack = 2.0f;

    [Header("Rebound")]
    Vector3 m_normalBounce;
    float m_verticalSpeed = 0f;
    public float m_bounceForce = 5f;
    public float m_bounceHeight = 2f;
    public float m_gravity = -9.81f;
    bool m_isBouncing = false;

    [Header("WaitingAttack")]
    public float m_timeAfterHit = 3.0f;
    float m_timerToAttack;

    bool m_isDashing = false;
    bool m_canUpdate = true;

    float m_initialSpeed = 3.5f;
    Vector3 m_dashDestination;

    private void Awake()
    {
        m_characterController = GetComponent<CharacterController>();
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameManager.GetGameManager().AddRestartGameElement(this);
        m_CurrentPatrolPointId = 0;

        if (m_PatrolPoints != null && m_PatrolPoints.Count > 0)
        {
            m_startPosition = m_PatrolPoints[m_CurrentPatrolPointId].position;
            m_startRotation = m_PatrolPoints[m_CurrentPatrolPointId].rotation;
        }
        else
        {
            m_startPosition = transform.position;
            m_startRotation = transform.rotation;
        }

        m_initialSpeed = m_navMeshAgent.speed;
        SetIdleState();
    }

    private void Update()
    {
        if (!m_canUpdate) return;

        switch (m_State)
        {
            case IState.IDLE: 
                UpdateIdleState(); 
                break;

            case IState.PATROL: 
                UpdatePatrolState(); 
                break;

            case IState.ALERT: 
                UpdateAlertState(); 
                break;

            case IState.ATTACK: 
                UpdateAttackState(); 
                break;

            case IState.WAIT_TO_ATTACK: 
                UpdateWaitToAttackState(); 
                break;
        }
    }

    void SetIdleState()
    {
        m_State = IState.IDLE;
        if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
        {
            m_navMeshAgent.isStopped = true;
            m_navMeshAgent.speed = m_initialSpeed;
        }
    }

    void UpdateIdleState()
    {
        SetPatrolState();
    }

    void SetPatrolState()
    {
        m_State = IState.PATROL;
        if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
            m_navMeshAgent.isStopped = false;

        m_animator.SetTrigger("IsWalking");
        MoveToNextPatrolPosition();
    }

    void UpdatePatrolState()
    {
        if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
        {
            if (!m_navMeshAgent.hasPath && m_navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
                MoveToNextPatrolPosition();
        }

        if (HearsPlayer()) 
            SetAlertState();
    }

    void MoveToNextPatrolPosition()
    {
        if (m_PatrolPoints == null || m_PatrolPoints.Count == 0) return;

        Vector3 l_Destination = m_PatrolPoints[m_CurrentPatrolPointId].position;

        if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
            m_navMeshAgent.SetDestination(l_Destination);

        m_CurrentPatrolPointId++;
        if (m_CurrentPatrolPointId >= m_PatrolPoints.Count)
            m_CurrentPatrolPointId = 0;
    }

    void MoveToNextNearestPosition()
    {
        if (m_PatrolPoints == null || m_PatrolPoints.Count == 0) return;

        float m_smallestDistance = Mathf.Infinity;
        for (int i = 0; i < m_PatrolPoints.Count; i++)
        {
            float m_currentDistance = Vector3.Distance(m_PatrolPoints[i].position, transform.position);
            if (m_currentDistance < m_smallestDistance)
            {
                m_smallestDistance = m_currentDistance;
                m_CurrentPatrolPointId = i;
            }
        }
    }

    void SetAlertState()
    {
        m_State = IState.ALERT;
        if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
            m_navMeshAgent.isStopped = true;

        m_YawTimer = 0.0f;
        m_StartYaw = transform.eulerAngles.y;

        Vector3 l_playerPos = GameManager.GetGameManager().m_PlayerController.transform.position;
        Vector3 l_direction = l_playerPos - transform.position;
        l_direction.y = 0;
        m_TargetYaw = Quaternion.LookRotation(l_direction).eulerAngles.y;

        m_animator.SetTrigger("IsAlert");

        if (m_EnemyType == IType.KOOPA)
            SetAttackState();
    }

    void UpdateAlertState()
    {
        m_YawTimer += Time.deltaTime;
        float l_time = Mathf.Clamp01(m_YawTimer / m_AlertRotateTime);
        float l_currentYaw = Mathf.LerpAngle(m_StartYaw, m_TargetYaw, l_time);
        transform.rotation = Quaternion.Euler(0.0f, l_currentYaw, 0.0f);
    }

    void SetAttackState()
    {
        m_State = IState.ATTACK;

        m_targetPlayerPosition = GameManager.GetGameManager().m_PlayerController.transform.position;
        m_attackDirection = m_targetPlayerPosition - transform.position;
        m_attackDirection.y = 0f;

        if (m_attackDirection.sqrMagnitude > 0.0001f)
            m_attackDirection.Normalize();
        else
            m_attackDirection = transform.forward;

        m_animator.SetBool("IsAttacking", true);
        m_isDashing = false;
        m_attackTimer = 0f;

        if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
            m_navMeshAgent.isStopped = true;

        StartCoroutine(ChargeThenDashCoroutine());
    }

    IEnumerator ChargeThenDashCoroutine()
    {
        m_StartYaw = transform.eulerAngles.y;
        float elapsed = 0f;

        while (elapsed < m_TimeToAttack)
        {
            if (m_State == IState.BOUNCING) 
                yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / m_TimeToAttack);

            Vector3 playerPos = GameManager.GetGameManager().m_PlayerController.transform.position;
            Vector3 dir = playerPos - transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude > 0.0001f) 
                dir.Normalize();
            else 
                dir = transform.forward;

            m_TargetYaw = Quaternion.LookRotation(dir).eulerAngles.y;
            float currentYaw = Mathf.LerpAngle(m_StartYaw, m_TargetYaw, t);
            transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

            yield return null;
        }

        m_targetPlayerPosition = GameManager.GetGameManager().m_PlayerController.transform.position;
        m_attackDirection = m_targetPlayerPosition - transform.position;
        m_attackDirection.y = 0f;

        if (m_attackDirection.sqrMagnitude > 0.0001f) 
            m_attackDirection.Normalize();
        else 
            m_attackDirection = transform.forward;

        StartDash();
    }

    void StartDash()
    {
        float dashDistance = m_AttackSpeed * m_attackTime;
        Vector3 desiredDestination = transform.position + m_attackDirection * dashDistance;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(desiredDestination, out hit, 2.0f, NavMesh.AllAreas))
        {
            m_dashDestination = hit.position;

            if (m_navMeshAgent != null)
            {
                m_navMeshAgent.enabled = true;
                if (m_navMeshAgent.isOnNavMesh)
                {
                    m_navMeshAgent.Warp(transform.position);
                    m_navMeshAgent.speed = m_AttackSpeed;
                    m_navMeshAgent.isStopped = false;
                    m_navMeshAgent.SetDestination(m_dashDestination);
                }
            }
        }
        else
        {
            m_dashDestination = transform.position + m_attackDirection * 0.1f;

            if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
                m_navMeshAgent.isStopped = true;
        }

        m_isDashing = true;
        m_attackTimer = 0f;
    }

    void UpdateAttackState()
    {
        if (m_isBouncing || m_State == IState.BOUNCING) 
            return;

        if (m_isDashing)
        {
            Vector3 rayOrigin = m_LookAt.position;
            float rayDistance = m_WallRaycastDistance;

            if (Physics.Raycast(rayOrigin, m_attackDirection, out RaycastHit hit, rayDistance, m_LayerMask.value, QueryTriggerInteraction.Ignore))
            {
                if ((Vector3.Dot(hit.normal, transform.up)) < Mathf.Cos(m_WallAngleToBouncing * Mathf.Deg2Rad))
                {
                    if (hit.collider.CompareTag("Player"))
                        hit.collider.GetComponent<PlayerController>().Hit(-1, -hit.normal);

                    StartBounce(hit.normal, false);
                    return;
                }
            }

            transform.position += m_attackDirection * m_AttackSpeed * Time.deltaTime;
            m_attackTimer += Time.deltaTime;

            if (m_attackTimer >= m_attackTime) 
                EndDashWithoutCollision();
        }
    }

    void EndDashWithoutCollision()
    {
        if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
        {
            m_navMeshAgent.isStopped = true;
            m_navMeshAgent.Warp(transform.position);
            m_navMeshAgent.velocity = Vector3.zero;
        }

        m_isDashing = false;
        m_animator.SetBool("IsAttacking", false);
        SetWaitToAttackState();
    }

    public void StartBounce(Vector3 normal, bool hasToDie)
    {
        if (m_isBouncing) return;

        m_LastState = m_State;
        m_State = IState.BOUNCING;
        m_isDashing = false;

        if (m_navMeshAgent != null)
        {
            m_navMeshAgent.isStopped = true;
            m_navMeshAgent.enabled = false;
            m_navMeshAgent.velocity = Vector3.zero;
        }

        m_animator.SetBool("IsAttacking", false);
        m_normalBounce = normal.normalized;

        StartCoroutine(BounceCoroutine(hasToDie));
    }

    IEnumerator BounceCoroutine(bool hasToDie)
    {
        m_isBouncing = true;
        m_verticalSpeed = m_bounceHeight;

        while (true)
        {
            Vector3 horizontalMovement = m_normalBounce * m_bounceForce * Time.deltaTime;
            m_verticalSpeed += m_gravity * Time.deltaTime;
            Vector3 verticalMovement = Vector3.up * m_verticalSpeed * Time.deltaTime;

            CollisionFlags flags = m_characterController.Move(horizontalMovement + verticalMovement);

            if ((flags & CollisionFlags.CollidedBelow) != 0 && m_verticalSpeed < 0f) 
                break;

            yield return null;
        }

        m_isBouncing = false;

        if (m_navMeshAgent != null)
        {
            m_navMeshAgent.enabled = true;

            if (m_navMeshAgent.isOnNavMesh)
                m_navMeshAgent.isStopped = true;

            m_navMeshAgent.velocity = Vector3.zero;
        }

        m_animator.SetBool("IsAttacking", false);

        if (hasToDie) 
            Kill();
        else 
            SetWaitToAttackState();
    }

    void SetWaitToAttackState()
    {
        m_State = IState.WAIT_TO_ATTACK;

        if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
        {
            m_navMeshAgent.isStopped = true;
            m_navMeshAgent.velocity = Vector3.zero;
            m_navMeshAgent.speed = m_initialSpeed;
        }

        m_animator.SetTrigger("IsStatic");
        m_timerToAttack = 0.0f;
    }

    void UpdateWaitToAttackState()
    {
        m_timerToAttack += Time.deltaTime;

        if (m_timerToAttack >= m_timeAfterHit)
        {
            if (!HearsPlayer())
            {
                MoveToNextNearestPosition();
                SetIdleState();
            }
            else 
                SetAlertState();
        }
    }

    bool HearsPlayer()
    {
        Vector3 l_PlayerPosition = GameManager.GetGameManager().m_PlayerController.transform.position;
        float l_Distance = Vector3.Distance(l_PlayerPosition, transform.position);
        return l_Distance <= m_MaxEarDistance;
    }

    public void Kill()
    {
        gameObject.SetActive(false);

        if (m_EnemyType == IType.KOOPA)
        {
            m_Shell.transform.position = transform.position;
            m_Shell.SetActive(true);
        }
    }

    public void RestartGame()
    {
        m_CurrentPatrolPointId = 0;
        m_characterController.enabled = false;
        transform.position = m_startPosition;
        transform.rotation = m_startRotation;
        m_characterController.enabled = true;

        gameObject.SetActive(true);

        if (m_EnemyType == IType.KOOPA) 
            m_Shell.SetActive(false);

        SetIdleState();
        m_canUpdate = true;
    }

    //EVENT
    void ToChaseState()
    {
        SetAttackState();
    }

    private void OnDrawGizmos()
    {
        if (m_LookAt != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(m_LookAt.position, m_LookAt.forward * m_WallRaycastDistance);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, m_MaxEarDistance);
    }
}