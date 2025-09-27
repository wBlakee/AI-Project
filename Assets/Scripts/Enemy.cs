using UnityEngine;

public class Enemy : MonoBehaviour
{
    public System.Action OnEnemyDeath;

    public void Die()
    {
        OnEnemyDeath?.Invoke();
        Destroy(gameObject);
    }
}
