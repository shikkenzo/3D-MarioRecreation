using Unity.VisualScripting;
using UnityEngine;

public class HealthController
{
    int m_healthPoints;
    int m_globalHP;

    public delegate void OnHealthChangedFn(HealthController _healthController);
    public event OnHealthChangedFn m_OnHealthChanged;

    public HealthController()
    {
        DependencyInjector.AddDependency<HealthController>(this);
    }
    public void AddHealthPoints(int health)
    {
        m_healthPoints += health;
        m_OnHealthChanged.Invoke(this);
    }
    public void HitGlobalHP()
    {
        m_globalHP--;
        if (m_globalHP < 0) m_globalHP = 0;

        m_OnHealthChanged.Invoke(this);
    }
    public int GetValue()
    {
        return m_healthPoints;
    }
    public int GetGlobalHP()
    {
        return m_globalHP;
    }
    public void ResetValue(int value)
    {
        m_healthPoints = value;
        m_OnHealthChanged.Invoke(this);
    }
    public void ResetGlobalHP(int value)
    {
        m_globalHP = value;
        m_OnHealthChanged.Invoke(this);
    }
}