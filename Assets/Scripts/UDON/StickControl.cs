using UnityEngine;

public class StickControl : MonoBehaviour {
    private Vector3 startPos;
    private ThreshingManager manager;

    void Start() {
        startPos = transform.position;
        manager = Object.FindFirstObjectByType<ThreshingManager>();
    }

    void OnMouseDrag() {
        if (manager != null && !manager.canPlay) return;
        
        // ลากไม้ตามเมาส์
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;
    }

    void OnMouseUp() {
        // เช็คว่าปล่อยไม้โดนรวงข้าวไหม
        Collider2D hit = Physics2D.OverlapPoint(transform.position);
        if (hit != null && hit.CompareTag("Rice")) {
            manager.HitRice();
        }
        
        // กลับไปจุดเริ่มต้น
        transform.position = startPos;
    }
}