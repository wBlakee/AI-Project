using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 3;

    private Vector2 direction;

    private void Start()
    {
        Destroy(gameObject, lifetime); // Destroy after set time
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
            Debug.Log("Found Player");
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log("Took Damage");
                playerHealth.TakeDamage(damage, transform); // Call TakeDamage in PlayerHealth
            }
            Destroy(gameObject); // Destroy the projectile after hitting the player

    }
}
