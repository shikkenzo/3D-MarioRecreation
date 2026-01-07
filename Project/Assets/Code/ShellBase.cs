using UnityEngine;

public class ShellBase : MonoBehaviour
{
    public int m_MaxBounces = 5;
    public float m_InitialSpeed = 2.0f;
    public float m_RaycastDistane = 2.0f;
    public float m_WallAngleToBouncing = 25.0f;
    public LayerMask m_LayerMask;
    public Transform m_PlayerTransform;
    bool m_isGrabbed = false;

    float m_speed;
    int m_bounceCount = 0;
    bool m_stayInPlace = true;

    Vector3 m_direction = Vector3.zero;
    Rigidbody m_rigidbody;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        m_bounceCount = 0;
        m_stayInPlace = true;
        m_speed = m_InitialSpeed;
        m_direction = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (m_isGrabbed)
        {
            transform.localPosition = Vector3.zero;
        }
        if (m_stayInPlace)
            return;

        if (Physics.Raycast(transform.position, m_direction, out RaycastHit hit, m_RaycastDistane, m_LayerMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Dot(hit.normal, transform.up) < Mathf.Cos(m_WallAngleToBouncing * Mathf.Deg2Rad))
            {
                m_direction = Vector3.Reflect(m_direction, hit.normal).normalized;
                AddBounce();
            }
        }

        Vector3 l_move = m_direction * m_speed * Time.fixedDeltaTime;
        m_rigidbody.MovePosition(m_rigidbody.position + l_move);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision == null)
            return;

        if (!m_stayInPlace)
        {
            if (collision.CompareTag("Player"))
                GameManager.GetGameManager().m_PlayerController.Hit(-1);

            else if (collision.CompareTag("Goomba"))
                collision.GetComponentInParent<EnemyController>().StartBounce(m_direction, true);

            AddBounce();
        }
        else
        {
            if (collision.CompareTag("Player"))
            {
                Grab();
            }
        }
    }

    public void StartMoving()
    {
        m_stayInPlace = false;
    }

    void AddBounce()
    {
        Debug.Log("Bounce " + m_bounceCount);
        if (m_bounceCount++ >= m_MaxBounces)
        {
            m_stayInPlace = true;
            return;
        }
        m_speed *= 0.8f;
    }

    public void Throw(Vector3 direction)
    {
        m_isGrabbed = false;

        GetComponent<Rigidbody>().isKinematic = false;

        Debug.Log(m_InitialSpeed);
        m_speed = m_InitialSpeed;
        m_direction = direction;

        StartMoving();
    }

    public void Grab()
    {
        GameManager.GetGameManager().m_PlayerController.GrabShell(transform);
        GetComponent<Rigidbody>().isKinematic = true;
        m_isGrabbed = true;
    }
}