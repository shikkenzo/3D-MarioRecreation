public class CoinController 
{ 
    int m_coinCount;

    public delegate void OnCoinsChangedFn(CoinController _coinController);
    public event OnCoinsChangedFn m_OnCoinsChanged;

    public CoinController()
    {
        DependencyInjector.AddDependency<CoinController>(this);
    }
    public void AddCoins(int coins)
    {
        m_coinCount += coins;
        m_OnCoinsChanged.Invoke(this);
    }
    public int GetValue()
    {
        return m_coinCount;
    }
    public void ResetValue(int value)
    {
        m_coinCount = value;
        m_OnCoinsChanged.Invoke(this);
    }
}