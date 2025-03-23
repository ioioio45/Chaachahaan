using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    Animator anim;
    public bool isAttacking;
    public float attackRange = 1.5f; // Дальность атаки
    public float attackDamage = 25f; // Урон
    public LayerMask enemyLayer; // Слой врагов
    private float attackCooldown = 0.5f;
    private float lastAttackTime = 0f;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            if (Input.GetMouseButtonDown(0) && !isAttacking)
            {
                StartCoroutine(Attack(1));
            }
            else if (Input.GetMouseButtonDown(1) && !isAttacking)
            {
                StartCoroutine(Attack(2));
            }
        }
    }

    private IEnumerator Attack(int attackType)
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        anim.SetBool("isAttacking", true);
        anim.SetInteger("attackType", attackType);
        yield return new WaitForSeconds(0.3f); // Ждём немного перед нанесением урона
        DealDamage();
        yield return new WaitForSeconds(0.5f); // Время на завершение атаки
        anim.SetBool("isAttacking", false);
        isAttacking = false;
    }

    private void DealDamage()
    {
        Vector3 attackPoint = transform.position + Vector3.up * 1.5f + transform.forward * attackRange; // Поднимаем выше
        float radius = 1f;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint, radius, enemyLayer);
        Debug.Log("Найдено врагов: " + hitEnemies.Length);

        foreach (Collider enemy in hitEnemies)
        {
            Debug.Log("Попал по: " + enemy.name);
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
                Debug.Log("Нанесён урон: " + attackDamage);
            }
        }
    }
}
