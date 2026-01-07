using System.Diagnostics;

public class StarItem : Item
{
    public override void Pick()
    {
        base.Pick();
        GameManager.GetGameManager().m_PlayerController.AddHealth();
    }

    //public override void OnEnable()
    //{
    //    base.OnEnable();
    //}

    //public override void Update()
    //{
    //    base.Update();
    //}

    public override bool CanPick()
    {
        if (GameManager.GetGameManager().m_PlayerController.GetHealth() >= GameManager.GetGameManager().m_PlayerController.m_MaxHealth)
            return false;
        else
            return true;
    }
}