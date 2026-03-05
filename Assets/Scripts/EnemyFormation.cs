using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the enemy grid in 3D space with orthographic-style 2D movement.
/// All movement is on the XY plane — Z is always locked to 0.
/// Attach to an empty GameObject at position (0, 0, 0).
/// </summary>
public class EnemyFormation : MonoBehaviour
{
    [Header("Formation Setup")]
    [SerializeField] private GameObject[] enemyPrefabs;  // 0=TypeA, 1=TypeB, 2=TypeC
    [SerializeField] private int   columns     = 11;
    [SerializeField] private int   rows        = 5;
    [SerializeField] private float spacingX    = 1.4f;
    [SerializeField] private float spacingY    = 1.2f;

    [Header("Discrete Step Movement")]
    [SerializeField] private float baseStepInterval = 0.8f;
    [SerializeField] private float stepSize         = 0.3f;
    [SerializeField] private float descendAmount    = 0.5f;
    [SerializeField] private float rightBoundary    =  8f;
    [SerializeField] private float leftBoundary     = -8f;

    [Header("Enemy Shooting")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float minFireInterval = 1.5f;
    [SerializeField] private float maxFireInterval = 3.5f;

    private List<Enemy> activeEnemies    = new List<Enemy>();
    private int         totalEnemies;
    private float       currentStepInterval;
    private int         moveDirection    = 1;  // 1=right, -1=left
    private bool        needsToDescend   = false;

    private void OnEnable()  { Enemy.OnEnemyDied += HandleEnemyDied; }
    private void OnDisable() { Enemy.OnEnemyDied -= HandleEnemyDied; }

    private void Start()
    {
        SpawnFormation();
        currentStepInterval = baseStepInterval;
        StartCoroutine(MoveFormation());
        StartCoroutine(EnemyFireRoutine());
    }

    private void SpawnFormation()
    {
        // Row 0 = TypeA (30pts), rows 1-2 = TypeB (20pts), rows 3-4 = TypeC (10pts)
        int[] prefabIndexByRow = { 0, 1, 1, 2, 2 };

        for (int row = 0; row < rows; row++)
        {
            int prefabIdx = prefabIndexByRow[Mathf.Clamp(row, 0, prefabIndexByRow.Length - 1)];
            for (int col = 0; col < columns; col++)
            {
                float x = transform.position.x + (col - columns / 2) * spacingX;
                float y = transform.position.y - row * spacingY;

                // Z = 0 — keep everything on the XY plane
                Vector3 pos = new Vector3(x, y, 0f);

                GameObject go = Instantiate(enemyPrefabs[prefabIdx], pos, Quaternion.identity, transform);
                Enemy enemy   = go.GetComponent<Enemy>();
                if (enemy != null) activeEnemies.Add(enemy);
            }
        }

        totalEnemies = activeEnemies.Count;
    }

    private IEnumerator MoveFormation()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentStepInterval);
            if (activeEnemies.Count == 0) yield break;

            Vector3 pos = transform.position;

            if (needsToDescend)
            {
                pos.y         -= descendAmount;
                needsToDescend = false;
                moveDirection *= -1;
            }
            else
            {
                pos.x += stepSize * moveDirection;
                CheckBoundary();
            }

            pos.z              = 0f;  // always enforce Z = 0
            transform.position = pos;
        }
    }

    private void CheckBoundary()
    {
        foreach (Enemy e in activeEnemies)
        {
            if (e == null) continue;
            float ex = e.transform.position.x;
            if (moveDirection ==  1 && ex >= rightBoundary) { needsToDescend = true; return; }
            if (moveDirection == -1 && ex <= leftBoundary)  { needsToDescend = true; return; }
        }
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        UpdateSpeed();
        if (activeEnemies.Count == 0)
            GameManager.Instance?.NotifyAllEnemiesDefeated();
    }

    private void UpdateSpeed()
    {
        if (totalEnemies == 0) return;
        float ratio = (float)activeEnemies.Count / totalEnemies;
        currentStepInterval = Mathf.Lerp(0.1f, baseStepInterval, ratio);
    }

    private IEnumerator EnemyFireRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minFireInterval, maxFireInterval));
            if (activeEnemies.Count == 0) yield break;

            int idx = Random.Range(0, activeEnemies.Count);
            if (activeEnemies[idx] != null)
            {
                // Spawn bullet at enemy position, Z = 0
                Vector3 firePos = activeEnemies[idx].transform.position;
                firePos.z = 0f;
                Instantiate(enemyBulletPrefab, firePos, Quaternion.identity);
            }
        }
    }
}
