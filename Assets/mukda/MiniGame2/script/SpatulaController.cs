using UnityEngine;

public class SpatulaController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Camera mainCam;
    private bool isDragging = false;

    [Header("Movement Settings")]
    public float maxSpeed = 35f;

    [Header("Stirring Settings (ระบบคน)")]
    public Transform spatulaTipPosition; // 🟢 ลาก GameObject 'SpatulaTip' มาใส่ช่องนี้
    public float stirRadius = 1.5f;      // รัศมีวงกลมที่จะเช็คถั่ว (ปรับขนาดตรงนี้)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        mainCam = Camera.main;
    }

    void OnMouseDown() { isDragging = true; }
    void OnMouseUp() { isDragging = false; }

    void FixedUpdate()
    {
        // 1. ระบบเคลื่อนที่ (โค้ดเดิมของคุณ)
        if (isDragging)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 currentPos = rb.position;
            Vector2 targetPos = (Vector2)mousePos;
            Vector2 direction = (targetPos - currentPos);
            float distance = direction.magnitude;
            float distanceToMove = Mathf.Min(distance, maxSpeed * Time.fixedDeltaTime);
            rb.MovePosition(currentPos + (direction.normalized * distanceToMove));

            // 2. ✅ เพิ่มระบบ "กวาดหาถั่ว" ตรงนี้! (ทำงานเฉพาะตอนลากเมาส์)
            CheckForBeans();
        }
    }

    // ฟังก์ชันเรดาร์หาถั่ว (ไม่ง้อ OnCollision)
    void CheckForBeans()
    {
        // ถ้าลืมลาก SpatulaTip มาใส่ ให้ใช้ตำแหน่งตัวเองแทน
        Vector2 checkPoint = (spatulaTipPosition != null) ? spatulaTipPosition.position : transform.position;

        // สร้างวงกลมที่มองไม่เห็น กวาดหา Collider ทุกอันในรัศมี
        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPoint, stirRadius);

        foreach (Collider2D hit in hits)
        {
            // เช็คว่าสิ่งที่เจอ มีสคริปต์ BeanStatus แปะอยู่มั้ย?
            // (ใช้ GetComponentInParent เผื่อไปเจอ Collider ลูกของถั่ว)
            BeanStatus bean = hit.GetComponent<BeanStatus>();
            if (bean == null) bean = hit.GetComponentInParent<BeanStatus>();

            if (bean != null)
            {
                // เจอถั่วแล้ว! สั่งลดความร้อนทันที
                if (HeatManager.Instance != null)
                {
                    HeatManager.Instance.ReduceHeat();
                    // Debug.Log("🔥❄️ ลดไฟแล้ว! (เจอถั่ว)"); 
                }

                // เจอแค่เม็ดเดียวก็พอแล้ว (break ออกเลย เดี๋ยวลดเยอะเกิน)
                break;
            }
        }
    }

    // วาดวงกลมสีเหลืองให้เห็นในหน้า Scene (จะได้กะระยะถูก)
    void OnDrawGizmos()
    {
        if (spatulaTipPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spatulaTipPosition.position, stirRadius);
        }
    }
}