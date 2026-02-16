using UnityEngine;

public class BeanRescuer : MonoBehaviour
{
    // กำหนดขอบเขตความปลอดภัย (ถ้าออกไปไกลกว่านี้ถือว่าหลุด)
    public float safeX = 2f;
    public float safeY = 2f;

    void Update()
    {
        // เช็คว่าถั่วหลุดออกไปไกลเกินรึยัง (ทั้งแกน X และ Y)
        if (Mathf.Abs(transform.position.x) > safeX || Mathf.Abs(transform.position.y) > safeY)
        {
            ResetPosition();
        }
    }

    void ResetPosition()
    {
        // 1. หยุดความเร็วทันที (กันไม่ให้พุ่งต่อ)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 2. วาร์ปกลับมาที่ "กลางกระทะ" (หรือจุด Spawn)
        // สุ่มตำแหน่งนิดหน่อยจะได้ไม่ทับกัน
        transform.position = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
    }
}