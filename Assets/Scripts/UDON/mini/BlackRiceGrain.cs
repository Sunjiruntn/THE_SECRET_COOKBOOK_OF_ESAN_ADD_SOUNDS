using UnityEngine;

public class BlackRiceGrain : MonoBehaviour
{
    private Vector3 centerPosition;
    private Rigidbody2D rb;
    private bool isSwirling = false;
    private float timer;

    [Header("Movement Settings")]
    public float swirlRadius = 0.08f;
    public float swirlSpeed = 2.5f;
    public float rotationSpeed = 40f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Explode(Vector2 direction, float force)
    {
        // เริ่มต้นด้วย Dynamic เพื่อให้พุ่งกระจายได้
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        timer = Random.Range(0f, 10f);

        // รอ 1.2 วินาทีให้แรงเฉื่อยหมดลง แล้วเริ่มการลอยวน
        Invoke("StartSwirling", 1.2f);
    }

    void StartSwirling()
    {
        isSwirling = true;
        centerPosition = transform.position;

        // แก้ไขจุดนี้: เปลี่ยนเป็น Kinematic แทนการปิด Simulated
        // เพื่อให้ Collider ยังทำงาน (คลิกได้) แต่ไม่ไหลตามแรงฟิสิกส์
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (isSwirling)
        {
            timer += Time.deltaTime * swirlSpeed;

            // เคลื่อนที่แบบวงกลมรอบจุด centerPosition
            float x = Mathf.Cos(timer) * swirlRadius;
            float y = Mathf.Sin(timer) * swirlRadius;
            transform.position = centerPosition + new Vector3(x, y, 0);

            // หมุนตัวเมล็ดข้าว
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }

    void OnMouseDown()
    {
        // ตรวจสอบว่า GameManager มีตัวตนอยู่จริง
        if (GameManagerUdon.Instance != null)
        {
            GameManagerUdon.Instance.RemoveRice();
        }

        // ทำลายเมล็ดข้าวทิ้งทันทีที่คลิก
        Destroy(gameObject);
    }
}