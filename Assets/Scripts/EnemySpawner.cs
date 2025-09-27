using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    //[SerializeField] private GameObject enemyPrefab; // The enemy to spawn
    [SerializeField] private Transform[] spawnPoints; // Spawn locations
    [SerializeField] private float spawnInterval = 5f; // Time between spawns
    [SerializeField] private int maxEnemies = 10; // Maximum number of active enemies
    [SerializeField] private GameObject[] enemyPrefabs;


    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            //Debug.Log("Spawning");

            if (activeEnemies.Count < maxEnemies)
            {
                //Debug.Log()"Attempting to spawn");

                SpawnEnemy();
            }
        }
    }
private void SpawnEnemy()
{
    if (spawnPoints == null || spawnPoints.Length == 0)
    {
        //Debug.LogError("No spawn points");
        return;
    }

    int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
    int randomSpawnIndex = Random.Range(0, spawnPoints.Length);

    Transform spawnPoint = spawnPoints[randomSpawnIndex];
    if (spawnPoint == null)
    {
        //Debug.LogError("Error1 (Spawn Point == null)");
        return;
    }

    GameObject newEnemy = Instantiate(enemyPrefabs[randomEnemyIndex], spawnPoint.position, Quaternion.identity);
    activeEnemies.Add(newEnemy);

    var enemyComponent = newEnemy.GetComponent<Enemy>();
    if (enemyComponent != null)
    {
        enemyComponent.OnEnemyDeath += () => activeEnemies.Remove(newEnemy);
    }
    else
    {
        //Debug.LogError("Error2 (No enemy component)");
    }
}


}
