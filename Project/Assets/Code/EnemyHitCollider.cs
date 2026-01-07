using UnityEngine;

public class EnemyHitCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HitCollider"))
        {
            PlayerController l_player = GameManager.GetGameManager().m_PlayerController;
            Vector3 l_normal = (transform.parent.position - l_player.transform.position).normalized;
            GetComponentInParent<EnemyController>().StartBounce(l_normal, true);
        }
    }
}