using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Updated for Part 2:
/// - Notifies EnemyAnimator on each discrete step (synchronized animation)
/// - Notifies EnemyAnimator on each enemy shoot
/// - Triggers scene transition when all enemies are defeated
/// </summary>
public class EnemyFormation : MonoBehaviour
{
    [Header("Formation Setup")]
    [SerializeField] private GameObject[] enemyPrefabs;
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

    private List<Enemy> activeEnemies   = new List<Enemy>();
    private int         totalEnemies;
    private float       currentStepInterval;
    private int         moveDirection   = 1;
    private bool        needsToDescend  = false;

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
        int[] prefabIndexByRow = { 0, 1, 1, 2, 2 };
        for (int row = 0; row < rows; row++)
        {
            int prefabIdx = prefabIndexByRow[Mathf.Clamp(row, 0, prefabIndexByRow.Length - 1)];
            for (int col = 0; col < columns; col++)
            {
                float x   = transform.position.x + (col - columns / 2) * spacingX;
                float y   = transform.position.y - row * spacingY;
                Vector3 pos = new Vector3(x, y, 0f);

                GameObject go    = Instantiate(enemyPrefabs[prefabIdx], pos, Quaternion.identity, transform);
                Enemy      enemy = go.GetComponent<Enemy>();
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

            pos.z              = 0f;
            transform.position = pos;

            // Notify all enemies of step — syncs idle animation
            NotifyAllEnemiesStep();
        }
    }

    private void NotifyAllEnemiesStep()
    {
        foreach (Enemy e in activeEnemies)
        {
            if (e == null) continue;
            e.GetComponent<EnemyAnimator>()?.NotifyStep();
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
        {
            GameManager.Instance?.NotifyAllEnemiesDefeated();
            // Transition to Credits
            SceneController.Instance?.GoToCredits();
        }
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
                Vector3 firePos = activeEnemies[idx].transform.position;
                firePos.z = 0f;
                Instantiate(enemyBulletPrefab, firePos, Quaternion.identity);

                // Notify that enemy it just shot
                activeEnemies[idx].GetComponent<EnemyAnimator>()?.NotifyShoot();
            }
        }
    }
}