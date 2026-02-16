using UnityEngine;
// --- 2. BeanSpawner.cs ---
public class BeanSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] beanPrefabs;
    public int amount = 80;
    public float spawnRadius = 2f;

    public void SpawnBeans() // เปลี่ยนเป็น public เพื่อให้ GameManager เรียกใช้ได้
    {
        if (beanPrefabs.Length == 0) return;

        for (int i = 0; i < amount; i++)
        {
            Vector2 randomPos = (Vector2)transform.position + (Random.insideUnitCircle * spawnRadius);
            int randomIndex = Random.Range(0, beanPrefabs.Length);
            GameObject selectedPrefab = beanPrefabs[randomIndex];
            Instantiate(selectedPrefab, randomPos, Quaternion.identity);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
