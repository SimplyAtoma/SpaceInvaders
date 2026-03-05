using System.Collections;
using UnityEngine;

/// <summary>
/// Mystery ship that periodically crosses the top of the screen.
/// Attach to a GameObject with:
///   - BoxCollider (Is Trigger = true)
///   - Enemy component (type = Mystery)
///   - Rigidbody (Is Kinematic = true, Use Gravity = false, freeze Z + all rotation)
/// Tag as "Enemy".
/// </summary>
public class MysteryShip : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed         =  5f;
    [SerializeField] private float spawnInterval = 20f;
    [SerializeField] private float startX        = -12f;
    [SerializeField] private float endX          =  12f;
    [SerializeField] private float yPosition     =  4.5f;

    private bool moving = false;

    private void Start()
    {
        gameObject.SetActive(false);
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            Activate();
        }
    }

    private void Activate()
    {
        // Z = 0 to stay on the gameplay plane
        transform.position = new Vector3(startX, yPosition, 0f);
        gameObject.SetActive(true);
        moving = true;
    }

    private void Update()
    {
        if (!moving) return;

        Vector3 pos = transform.position;
        pos.x += speed * Time.deltaTime;
        pos.z  = 0f;  // enforce Z lock
        transform.position = pos;

        if (pos.x >= endX)
        {
            moving = false;
            gameObject.SetActive(false);
        }
    }
}
