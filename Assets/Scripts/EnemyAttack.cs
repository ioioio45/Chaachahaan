using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private Animator anim;
    public float attackCooldown = 2f;
    private float nextAttackTime = 0f;

    public float attackRange = 2f;
    public int attackDamage = 10;
    public LayerMask playerLayer;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            Collider[] hitPlayers = Physics.OverlapSphere(transform.position, attackRange, playerLayer);

            if (hitPlayers.Length > 0)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    private void Attack()
    {
        int attackType = Random.Range(0, 2); // 0 или 1

        if (attackType == 0)
            anim.SetTrigger("LeftAttack");
        else
            anim.SetTrigger("RightAttack");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
