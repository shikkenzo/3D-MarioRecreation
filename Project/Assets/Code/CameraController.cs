using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class CameraController : MonoBehaviour
{
    private PlayerController m_player;
    float m_yaw = 0.0f;
    float m_pitch = 0.0f;
    public float m_CameraDistance = 5.0f;
    public float m_YawSpeed = 720.0f;
    public float m_PitchSpeed = 720.0f;
    public float m_MinPitch = 60.0f;
    public float m_MaxPitch = 80.0f;
    public float m_MinDistance = 3.0f;
    public float m_MaxDistance = 12.0f;
    public LayerMask m_LayerMask;
    public float m_CollisionDistanceOffset = 0.1f;

    private float m_startYaw;
    private float m_startPitch;

    public float m_CameraResetTime = 2.0f;
    private float m_cameraResetCurrentTime;
    private float m_cameraLastIdleYaw;
    private float m_cameraLastIdlePitch;

    float m_horizontalAxisInput;
    float m_verticalAxisInput;

    private void Start()
    {
        m_player = GameManager.GetGameManager().m_PlayerController;
        m_yaw = transform.eulerAngles.y;

        m_startYaw = m_yaw;
        m_startPitch = m_pitch;
    }

    public void LateUpdate()
    {
        Vector3 l_lookAt = m_player.m_LookAt.transform.position;

        float l_distance = Vector3.Distance(l_lookAt, transform.position);

        m_horizontalAxisInput = Input.GetAxis("Mouse X");
        m_verticalAxisInput = Input.GetAxis("Mouse Y");

        if (GameManager.GetGameManager().IsGamepadConnected())
        {
            //Debug.Log("Gamepad Camera");
            m_horizontalAxisInput += GameManager.GetGameManager().m_PlayerController.GetCameraGamepadInput().x;
            m_verticalAxisInput += GameManager.GetGameManager().m_PlayerController.GetCameraGamepadInput().y;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            m_horizontalAxisInput = 0.0f;
            m_verticalAxisInput = 0.0f;
        }

        m_yaw += m_horizontalAxisInput * m_YawSpeed * Time.deltaTime;
        m_pitch += m_verticalAxisInput * m_PitchSpeed * Time.deltaTime;
        m_pitch = Mathf.Clamp(m_pitch, m_MinPitch, m_MaxPitch);

        if (m_player.m_resetCamera)
        {
            m_yaw = transform.eulerAngles.y;

            m_cameraResetCurrentTime += Time.deltaTime;

            m_cameraLastIdleYaw = m_yaw; 
            m_cameraLastIdlePitch = m_pitch; 

            m_yaw = Mathf.Lerp(m_yaw, m_startYaw, (m_cameraResetCurrentTime / m_CameraResetTime) * Time.deltaTime);
            m_pitch = Mathf.Lerp(m_pitch, m_startPitch, (m_cameraResetCurrentTime / m_CameraResetTime) * Time.deltaTime);
        }
        else
        {
            m_cameraResetCurrentTime = 0.0f;
        }

        float l_yawRadians = m_yaw * Mathf.Deg2Rad;
        float l_pitchRadians = m_pitch * Mathf.Deg2Rad;

        Vector3 l_direction = new Vector3(Mathf.Cos(l_pitchRadians) * Mathf.Sin(l_yawRadians), Mathf.Sin(l_pitchRadians), Mathf.Cos(l_pitchRadians) * Mathf.Cos(l_yawRadians));
        l_distance = Mathf.Clamp(l_distance, m_MinDistance, m_MaxDistance);

        Vector3 l_desiredPosition = l_lookAt - l_direction * l_distance;
        Ray l_ray = new Ray(l_lookAt, -l_direction);
        if (Physics.Raycast(l_ray, out RaycastHit l_raycastHit, l_distance, m_LayerMask.value, QueryTriggerInteraction.Ignore))
        {
            l_desiredPosition = l_raycastHit.point + l_direction * m_CollisionDistanceOffset;
        }

        transform.position = l_desiredPosition;
        transform.LookAt(l_lookAt);
    }

    public bool IsCameraGettingInput()
    {
        return (m_horizontalAxisInput != 0.0f || m_verticalAxisInput != 0.0f);
    }
}