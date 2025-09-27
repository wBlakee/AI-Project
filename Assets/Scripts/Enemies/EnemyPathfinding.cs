using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float obstacleDetectionRange = 1f;  // Distance to check for obstacles
    [SerializeField] private LayerMask obstacleLayer;            // LayerMask for obstacles
    
    [SerializeField] private float stuckThreshold = 0.01f;  // Minimum distance to consider moving
    [SerializeField] private float stuckTimeLimit = 0.1f;    // Time before considering "stuck"

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Knockback knockback;
    private SpriteRenderer spriteRenderer;

    private Vector2 lastPosition;
    private float stuckTimer = 0f;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
    }

private void FixedUpdate()
{
    if (knockback.GettingKnockedBack) return;

    AvoidObstacles();
    rb.MovePosition(rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));

    DetectStuck(); // Check for stuck behavior
    FlipSprite();
}

private void DetectStuck()
{
    float distanceMoved = Vector2.Distance(rb.position, lastPosition);

    if (distanceMoved < stuckThreshold)
    {
        stuckTimer += Time.fixedDeltaTime;
    }
    else
    {
        stuckTimer = 0f;  // Reset if enemy is moving
    }

    lastPosition = rb.position;

    if (stuckTimer >= stuckTimeLimit)
    {
        //Debug.Log("Enemy stuck: Triggering recovery.");
        RecalculatePath();
    }
}


    private void RecalculatePath()
    {
        // Small random adjustment to avoid repeating the same stuck path
        moveDir = Quaternion.Euler(0, 0, Random.Range(-20f, 20f)) * moveDir;
        
        //Debug.Log("Recalculating path to avoid obstacle.");
        
        stuckTimer = 0f;  // Reset the stuck timer
    }



    public void MoveTo(Vector2 targetPosition)
    {
        Vector2 desiredDir = (targetPosition - rb.position).normalized;

        // Only update moveDir if no obstacles in the way
        if (Physics2D.Raycast(transform.position, desiredDir, obstacleDetectionRange, obstacleLayer) == false)
        {
            moveDir = desiredDir;
        }
    }




    public void StopMoving()
    {
        moveDir = Vector2.zero; // Stop all movement
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    private void AvoidObstacles()
    {
        RaycastHit2D centerHit = Physics2D.Raycast(transform.position, moveDir, obstacleDetectionRange, obstacleLayer);

        // Debug Raycasts
        //Debug.DrawRay(transform.position, moveDir * obstacleDetectionRange, Color.red);

        if (centerHit.collider != null)
        {
            // Obstacle ahead, try different directions
            Vector2 newDirection = Vector2.zero;

            // Attempt angles: -45, 45, -90, 90, until a clear path is found
            float[] angles = { -90f, 90f, -180f, 180f };
            foreach (float angle in angles)
            {
                newDirection = Quaternion.Euler(0, 0, angle) * moveDir;
                if (!Physics2D.Raycast(transform.position, newDirection, obstacleDetectionRange, obstacleLayer))
                {
                    //Debug.DrawRay(transform.position, newDirection * obstacleDetectionRange, Color.green); // Debug the new direction
                    moveDir = Vector2.Lerp(moveDir, newDirection, Time.deltaTime * 5f).normalized;
                    return;
                }
            }

            // If no clear direction found, reverse slightly
            moveDir = -moveDir;
        }
    }

    private void FlipSprite()
    {
        if (moveDir.x < 0)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }
}
