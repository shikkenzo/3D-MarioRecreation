using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    static GameManager m_GameManager;
    List<IRestartGameElement> m_RestartGameElements = new List<IRestartGameElement>();

    public PlayerController m_PlayerController;
    public Canvas m_RespawnCanvas;
    public Canvas m_GameOverCanvas;

    private void Awake()
    {
        if (m_GameManager != null)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        m_GameManager = this;
        DontDestroyOnLoad(gameObject);

        InputSystem.onDeviceChange += AssignControllerSticks;
    }
    private void Start()
    {
        HideRestartUI();
        AssignControllerSticks();
    }
    public static GameManager GetGameManager()
    {
        return m_GameManager;
    }

    public void AddRestartGameElement(IRestartGameElement restartGameElement)
    {
        m_RestartGameElements.Add(restartGameElement);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            m_PlayerController.Kill();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            m_PlayerController.Hit(-1);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            m_PlayerController.AddCoin();
        }
    }

    public void RespawnUI()
    {
        m_RespawnCanvas.gameObject.SetActive(true);
    }
    public void GameOverUI()
    {
        m_GameOverCanvas.gameObject.SetActive(true);
    }
    public void RestartScreen(bool canRespawn)
    {
        if (canRespawn)
        {
            RespawnUI();
        }
        else
        {
            GameOverUI();
        }
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartValues()
    {
        m_PlayerController.ResetCheckpoints();
        m_PlayerController.ResetGlobalHP();
        m_PlayerController.ResetCoins();
    }

    public void HideRestartUI()
    {
        m_RespawnCanvas.gameObject.SetActive(false);
        m_GameOverCanvas.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
    }

    public bool IsUIActive()
    {
        return (m_RespawnCanvas.isActiveAndEnabled || m_GameOverCanvas.isActiveAndEnabled);
    }

    public bool IsGamepadConnected()
    {
        return (Gamepad.current != null);
    }

    public void AssignControllerSticks(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            Debug.Log("NewGamepad: " + Gamepad.all.Count);
            m_PlayerController.m_MovementStick = Gamepad.all[0].leftStick;
            m_PlayerController.m_CameraStick = Gamepad.all[0].rightStick;
        }
    }

    public void AssignControllerSticks()
    {
        if (Gamepad.all.Count > 0)
        {
            Debug.Log("NewGamepad: " + Gamepad.current.name);
            m_PlayerController.m_MovementStick = Gamepad.all[0].leftStick;
            m_PlayerController.m_CameraStick = Gamepad.all[0].rightStick;
        }
    }

    public void RestartGame()
    {
        foreach (IRestartGameElement l_RestartGameElement in m_RestartGameElements)
        {
            l_RestartGameElement.RestartGame();
        }
    }
}
