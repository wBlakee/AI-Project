using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float roamChangeDirFloat = 2f;
    [SerializeField] private float attackRange = 5f;  // Distance to start chasing the player
    [SerializeField] private float defaultAttackRange = 5f;  // Distance to start chasing the player

    [SerializeField] private float mushroomDetectionRange = 6.5f;  // Range to detect mushrooms
    [SerializeField] private float protectDistance = 2.5f;  // Distance to position in front of the mushroom
    [SerializeField] private float protectSpeedMultiplier = 0.65f;
    [SerializeField] private int damageAmount = 1;

    private float defaultMoveSpeed;
    private Transform currentMushroom;  // Reference to the mushroom being protected
    private PlayerHealth playerHealth;
    private bool isAttackRangeBoosted = false;

    private float timeSurvivedWithoutDamage = 0f;
    private float previousHealth;

    private DifficultyMode difficultyMode = DifficultyMode.Passive;
    private State state;
    private EnemyPathfinding enemyPathfinding;

    private enum State
    {
        Roaming,
        Chasing,
        Protecting
    }

    private enum DifficultyMode
    {
        Passive,
        Aggressive
    }

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        state = State.Roaming;
        playerHealth = FindObjectOfType<PlayerHealth>(); // Get PlayerHealth component
    }

    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        defaultMoveSpeed = enemyPathfinding.GetMoveSpeed();
        previousHealth = playerHealth.GetCurrentHealth();
        StartCoroutine(StateRoutine());
    }

    private void Update()
    {
        HandleState();
        TrackPlayerSurvival();
        //yield return new WaitForSeconds(10f);
        AdjustDifficultyModeBasedOnPerformance();
    }

    private void HandleState()
    {
        currentMushroom = FindNearestMushroom();

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        int currentSlimeHealth = GetComponent<EnemyHealth>().GetCurrentHealth();  // Assuming EnemyHealth is attached to slime

        if (currentMushroom != null)
        {
            int mushroomHealth = currentMushroom.GetComponent<EnemyHealth>().GetCurrentHealth();  // Assuming mushrooms have EnemyHealth script

            if (currentSlimeHealth <= mushroomHealth)
            {
                // Switch to chasing behavior when health is less than or equal to mushroom
                state = State.Chasing;
                enemyPathfinding.SetMoveSpeed(defaultMoveSpeed + 1.0f);  // Add extra speed for charging
                enemyPathfinding.MoveTo(PlayerController.Instance.transform.position);
                return;  // Exit early to prevent Protecting state
            }

            state = State.Protecting;
            enemyPathfinding.SetMoveSpeed(defaultMoveSpeed + protectSpeedMultiplier);

            Collider2D slimeCollider = GetComponent<Collider2D>();
            Collider2D mushroomCollider = currentMushroom.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(slimeCollider, mushroomCollider, true);
        }
        else if (distanceToPlayer < attackRange)
        {
            state = State.Chasing;
            enemyPathfinding.SetMoveSpeed(defaultMoveSpeed);
        }
        else
        {
            state = State.Roaming;
            enemyPathfinding.SetMoveSpeed(defaultMoveSpeed);

            if (currentMushroom != null)
            {
                Collider2D slimeCollider = GetComponent<Collider2D>();
                Collider2D mushroomCollider = currentMushroom.GetComponent<Collider2D>();
                Physics2D.IgnoreCollision(slimeCollider, mushroomCollider, false);
            }
        }
    }

    private void AdjustDifficultyModeBasedOnPerformance()
    {
        if (playerHealth == null) return;

        bool isPlayerPerformingWell = false;

        if (playerHealth.GetCurrentHealth() >= 3 && difficultyMode == DifficultyMode.Aggressive)
        {
            Debug.Log("Player is Still Performing Well");
            isPlayerPerformingWell = true;
        }

        if (timeSurvivedWithoutDamage >= 15f)
        {
            isPlayerPerformingWell = true;
        }

        if (isPlayerPerformingWell)
        {
            SetDifficultyMode(DifficultyMode.Aggressive);
        }
        else
        {
            SetDifficultyMode(DifficultyMode.Passive);
        }
    }

    private void SetDifficultyMode(DifficultyMode mode)
    {
        difficultyMode = mode;

        if (mode == DifficultyMode.Aggressive)
        {
            enemyPathfinding.SetMoveSpeed(defaultMoveSpeed + 1f);
            attackRange = defaultAttackRange + 2f;  // Increased chasing distance
            damageAmount = 2;
        }
        else // Passive
        {
            enemyPathfinding.SetMoveSpeed(defaultMoveSpeed + 0f);  // Slower movement
            attackRange = defaultAttackRange;
            damageAmount = 1;
        }

        attackRange = Mathf.Max(1f, attackRange);  // Ensure attack range never goes below 1
    }

    private void TrackPlayerSurvival()
    {
        if (playerHealth.GetCurrentHealth() == previousHealth)
        {
            Debug.Log("No Damage");
            timeSurvivedWithoutDamage += Time.deltaTime;
        }
        else
        {
            timeSurvivedWithoutDamage = 0f;
            previousHealth = playerHealth.GetCurrentHealth();
        }
    }


    private IEnumerator BoostAttackRange()
    {
        isAttackRangeBoosted = true;
        attackRange += 4f;  // Increase attack range by 4f
        //Debug.Log("Attack range boosted");

        yield return new WaitForSeconds(4f);

        attackRange -= 4f;  // Revert the attack range
        //Debug.Log("Attack range reverted");
        isAttackRangeBoosted = false;
    }

    private void FaceAwayFromMushroom()
    {
        if (currentMushroom == null) return;

        Vector2 directionToMushroom = (currentMushroom.position - transform.position).normalized;

        // Flip sprite to face away from the mushroom
        if (directionToMushroom.x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;  // Face right, away from mushroom
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;  // Face left, away from mushroom
        }
    }

    private IEnumerator StateRoutine()
    {
        while (true)
        {
            switch (state)
            {
                case State.Roaming:
                    Roaming();
                    yield return new WaitForSeconds(roamChangeDirFloat);
                    break;

                case State.Chasing:
                    ChasePlayer();
                    yield return null;  // Update every frame
                    break;

                case State.Protecting:
                    ProtectMushroom();
                    yield return null;  // Update every frame
                    break;
            }
        }
    }

    private void Roaming()
    {
        Vector2 roamPosition = GetRoamingPosition() + (Vector2)transform.position;
        enemyPathfinding.MoveTo(roamPosition);
    }

    private void ChasePlayer()
    {
        Vector2 playerPosition = PlayerController.Instance.transform.position;
        enemyPathfinding.MoveTo(playerPosition);
    }


    private void ProtectMushroom()
    {
        if (currentMushroom == null) return;

        // Calculate position in front of the mushroom, facing the player
        Vector2 playerPosition = PlayerController.Instance.transform.position;
        Vector2 mushroomPosition = currentMushroom.position;

        // Direction from mushroom to player
        Vector2 directionToPlayer = (playerPosition - mushroomPosition).normalized;

        // Position the slime in front of the mushroom
        Vector2 protectPosition = (Vector2)mushroomPosition + directionToPlayer * protectDistance;

        // Move slime to the protect position
        enemyPathfinding.MoveTo(protectPosition);

        // Ensure the slime faces away from the mushroom
        FaceAwayFromMushroom();
    }


    private Vector2 GetRoamingPosition()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private Transform FindNearestMushroom()
    {
        RangedEnemyAI[] mushrooms = FindObjectsOfType<RangedEnemyAI>();  // Detects all RangedEnemyAI instances
        Transform nearestMushroom = null;
        float shortestDistance = mushroomDetectionRange;

        foreach (RangedEnemyAI mushroom in mushrooms)
        {
            float distanceToMushroom = Vector2.Distance(transform.position, mushroom.transform.position);
            if (distanceToMushroom < shortestDistance)
            {
                nearestMushroom = mushroom.transform;
                shortestDistance = distanceToMushroom;
            }
        }

        return nearestMushroom;
    }
    public int GetDamageAmount()
    {
        return damageAmount;
    }
}
