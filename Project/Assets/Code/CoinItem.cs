using System.Diagnostics;

public class CoinItem : Item
{
    public override void Pick()
    {
        base.Pick();
        GameManager.GetGameManager().m_PlayerController.AddCoin();
    }

    public override bool CanPick()
    {
        if (GameManager.GetGameManager().m_PlayerController.GetCoins() >= GameManager.GetGameManager().m_PlayerController.m_MaxCoins)
            return false;
        else
            return true;
    }
}