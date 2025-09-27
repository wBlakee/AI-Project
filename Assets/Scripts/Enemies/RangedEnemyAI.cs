using System.Collections;
using UnityEngine;

public class RangedEnemyAI : MonoBehaviour
{
    [SerializeField] private float roamChangeDirFloat = 2f;  // Time to change roaming direction
    [SerializeField] private float attackRange = 8f;         // Max range to start attacking
    [SerializeField] private float stopRange = 5f;           // Distance to stop moving toward player
    [SerializeField] private GameObject projectilePrefab;    
    [SerializeField] private Transform firePoint;            // Spawn point for projectiles
    [SerializeField] private float attackCooldown = 2f;      // Time between attacks
    [SerializeField] private float stopRangeBuffer = 1f; // Buffer to prevent rapid state switching


    private enum State
    {
        Roaming,
        Attacking,
        Fleeing
    }

    private State state;
    private EnemyPathfinding enemyPathfinding;
    private bool canAttack = true;

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        state = State.Roaming;
    }

    private void Start()
    {
        StartCoroutine(RoamingRoutine());
    }

    private void Update()
    {
        HandleState();
    }

private void HandleState()
{
    float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

    if (distanceToPlayer < attackRange)
    {
        state = State.Attacking;

        if (distanceToPlayer < stopRange - stopRangeBuffer)
        {
            Debug.Log("Fleeing");

            // Move away from the player if too close
            Vector2 directionAwayFromPlayer = (transform.position - PlayerController.Instance.transform.position).normalized;
            Vector2 fleeTarget = (Vector2)transform.position + directionAwayFromPlayer * stopRange;

            enemyPathfinding.MoveTo(fleeTarget);
        }
        else if (distanceToPlayer > stopRange + stopRangeBuffer)
        {
            // Move toward the player if within attack range but outside stop range
            enemyPathfinding.MoveTo(PlayerController.Instance.transform.position);
        }
        else
        {
            // Slight buffer zone, stop movement to prevent jitter
            enemyPathfinding.StopMoving();
        }

        // Always face the player, even when fleeing
        FacePlayer();
    }
    else
    {
        state = State.Roaming;
    }
}





    private IEnumerator RoamingRoutine()
    {
        while (true)
        {
            if (state == State.Roaming)
            {
                Vector2 roamPosition = GetRoamingPosition() + (Vector2)transform.position;
                enemyPathfinding.MoveTo(roamPosition);
                yield return new WaitForSeconds(roamChangeDirFloat);
            }
            else if (canAttack == true)
            {

                    Debug.Log("Attempting to fire");
                    FireProjectile();
                    canAttack = false; // Prevents immediate re-fire
                    yield return new WaitForSeconds(attackCooldown); // Waits for cooldown
                    canAttack = true; // Resets canAttack after cooldown
            }

            yield return null; // This ensures it loops every frame when attacking
        }
    }

    private void FacePlayer()
    {
        Vector2 directionToPlayer = PlayerController.Instance.transform.position - transform.position;

        if (directionToPlayer.x < 0)
        {
            // Face left
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            // Face right
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }


    private void FireProjectile()
    {
        //Debug.Log("Bruh");

        //Debug.Log("FireProjectile called"); // Added this for debugging
        // Instantiate the projectile at the Fire Point's position and rotation
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Get the direction to the player
        Vector2 direction = (PlayerController.Instance.transform.position - firePoint.position).normalized;

        // Pass the direction to the projectile's behavior script
        projectile.GetComponent<ProjectileBehavior>().SetDirection(direction);

        //Debug.Log("Projectile fired at player!");
        canAttack = false;
        
    }

    private Vector2 GetRoamingPosition()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }
}
