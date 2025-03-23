using UnityEngine;
using System.Collections;
using EN;
using UnityEngine.AI;

public class EnemySkeleton : MonoBehaviour
{
    public float health = 50f;
    public float moveSpeed = 2f;
    public float attackRange = 2f;
    public float detectionRange = 10f;

    public Transform attackPoint; // Точка атаки
    private Transform player;
    private Animator anim;
    private bool isAttacking;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange && !isAttacking)
        {
            StartCoroutine(Attack());
        }
        else if (distance <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            anim.SetFloat("Speed", 0); // Остановить анимацию, если игрок далеко
        }
    }

    private void ChasePlayer()
    {
        if (isAttacking) return;

        agent.SetDestination(player.position);
        anim.SetFloat("Speed", 0.5f);
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        anim.SetFloat("Speed", 0f);
        anim.SetTrigger("RightAttack");  // Это должен быть параметр типа Trigger

        yield return new WaitForSeconds(0.5f);

        if (player == null)
        {
            isAttacking = false;
            yield break;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            player.GetComponent<PlayerManager>()?.TakeDamage(10f);
        }

        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        anim.SetTrigger("Die");
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 2f);
    }
}
