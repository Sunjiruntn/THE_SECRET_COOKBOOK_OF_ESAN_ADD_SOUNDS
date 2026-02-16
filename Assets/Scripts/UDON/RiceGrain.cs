using UnityEngine;

public class RiceGrain : MonoBehaviour 
{
    [Header("0=Young, 1=Ready, 2=Old")]
    public int riceType; 
    private HarvestManager manager;

    void Start() {
        manager = FindObjectOfType<HarvestManager>();
        // ตรวจสอบว่ามี Collider หรือไม่
        if (GetComponent<Collider2D>() == null) {
            Debug.LogWarning(gameObject.name + " ลืมใส่ BoxCollider2D นะจ๊ะ!");
        }
    }

    void OnMouseDown() {
        if (manager != null) {
            manager.CheckRice(this); 
        }
    }
}