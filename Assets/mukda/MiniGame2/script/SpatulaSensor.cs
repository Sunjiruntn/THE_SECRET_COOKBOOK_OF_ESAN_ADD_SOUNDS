using UnityEngine;

public class SpatulaSensor : MonoBehaviour
{
    // ฟังก์ชันนี้จะทำงานเมื่อมีอะไรเข้ามาในเขต Trigger ของ Sensor
    void OnTriggerEnter2D(Collider2D other)
    {
        // แทนที่จะเช็ค Tag เราเช็คว่า "สิ่งที่ชนมีสคริปต์ BeanStatus แปะอยู่ไหม?"
        // วิธีนี้ชัวร์กว่า Tag ล้านเท่า! เพราะถั่วทุกเม็ดต้องมีสคริปต์นี้
        BeanStatus bean = other.GetComponent<BeanStatus>();

        if (bean != null)
        {
            // เจอถั่วแล้ว! สั่งลดความร้อนเลย
            if (HeatManager.Instance != null)
            {
                HeatManager.Instance.ReduceHeat();
                Debug.Log("🔥❄️ ลดความร้อนสำเร็จ! (Sensor เจอถั่ว)");
            }
        }
    }
}