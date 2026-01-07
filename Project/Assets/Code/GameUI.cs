using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Text m_CoinsText;
    public Text m_GlobalHPText;
    public Image m_LifeBar;

    [Header("Animation")]
    public Animation m_Animation;
    public AnimationClip m_InAnimationClip;
    public AnimationClip m_OutAnimationClip;
    public AnimationClip m_StayInAnimationClip;
    public AnimationClip m_StayOutAnimationClip;
    public float m_ShowUIWaitTime = 1.0f;
    private float m_showUIRemainingTime;
    private bool m_canHide;

    private void Start()
    {
        SetCoins(0);
        SetLifeBar(1.0f);
        SetGlobalHP(GameManager.GetGameManager().m_PlayerController.m_StartingGlobalHP);
        m_Animation.Play(m_StayOutAnimationClip.name);
        m_Animation.Sample(); 

        DependencyInjector.GetDependency<CoinController>().m_OnCoinsChanged += OnCoinsChanged;
        DependencyInjector.GetDependency<HealthController>().m_OnHealthChanged += OnHealthChanged;
        DependencyInjector.GetDependency<HealthController>().m_OnHealthChanged += OnGlobalHPChanged;

    }
    private void OnDestroy()
    {
        DependencyInjector.GetDependency<CoinController>().m_OnCoinsChanged -= OnCoinsChanged;
        DependencyInjector.GetDependency<HealthController>().m_OnHealthChanged -= OnHealthChanged;
        DependencyInjector.GetDependency<HealthController>().m_OnHealthChanged -= OnGlobalHPChanged;
    }
    private void SetCoins(int coins)
    {
        m_CoinsText.text = coins.ToString();
    }
    private void SetLifeBar(float lifeNormalized)
    {
        m_LifeBar.fillAmount = lifeNormalized;
    }
    private void SetGlobalHP(int _globalHP)
    {
        m_GlobalHPText.text = _globalHP.ToString();
    }
    public void ShowUI()
    {
        if (m_Animation.IsPlaying(m_StayOutAnimationClip.name))
        {
            m_Animation.Play(m_InAnimationClip.name);
            m_Animation.PlayQueued(m_StayInAnimationClip.name);
            m_Animation.Sample();
        }
    }

    public void HideUI()
    {
        if (m_Animation.IsPlaying(m_StayInAnimationClip.name))
        {
            m_Animation.Play(m_OutAnimationClip.name);
            m_Animation.PlayQueued(m_StayOutAnimationClip.name);
            m_Animation.Sample();

        }
    }
    void OnCoinsChanged(CoinController _coinController)
    {
        SetCoins(_coinController.GetValue());
        ShowUI();
        m_showUIRemainingTime = m_ShowUIWaitTime;
    }
    void OnHealthChanged(HealthController _healthController)
    {
        SetLifeBar(_healthController.GetValue() / (float) GameManager.GetGameManager().m_PlayerController.m_MaxHealth);
        ShowUI();
        m_showUIRemainingTime = m_ShowUIWaitTime;
    }
    void OnGlobalHPChanged(HealthController _healthController)
    {
        SetGlobalHP(_healthController.GetGlobalHP());
        m_showUIRemainingTime = m_ShowUIWaitTime;
    }
    private void Update()
    {
        if (m_Animation.IsPlaying(m_StayInAnimationClip.name))
        {
            m_showUIRemainingTime -= Time.deltaTime;
            if (m_showUIRemainingTime < 0.0f) m_showUIRemainingTime = 0.0f;

            if (m_showUIRemainingTime == 0.0f)
            {
                m_canHide = true;
            }
        }
        
        if (m_canHide)
        {
            HideUI();
            m_canHide = false;
        }
    }
}
