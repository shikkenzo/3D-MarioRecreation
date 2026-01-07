using Unity.VisualScripting;
using UnityEngine;

public abstract class Item : MonoBehaviour, IRestartGameElement
{
    private void Start()
    {
        GameManager.GetGameManager().AddRestartGameElement(this);
    }

    public virtual void Pick()
    {
        gameObject.SetActive(false);
    }

    public abstract bool CanPick();

    public void RestartGame()
    {
        gameObject.SetActive(true);
    }
}