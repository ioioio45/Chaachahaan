using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float health = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log(gameObject.name + " получил урон: " + damage);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " погиб!");
        Destroy(gameObject); // Удаляет врага
    }
}
