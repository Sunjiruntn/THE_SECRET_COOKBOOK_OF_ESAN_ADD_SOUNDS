using UnityEngine;

public class StagePourTrigger : MonoBehaviour
{
    private void OnMouseDown()
    {
        // สั่งให้ระบบเททำงานทันทีเมื่อคลิกที่ตัวเนื้อขนมสเตจนี้
        PouringSystem pourSys = FindObjectOfType<PouringSystem>();
        if (pourSys != null)
        {
            Debug.Log("คลิกที่เนื้อขนมผสมเสร็จแล้ว! กำลังเตรียมเท...");
            pourSys.StartPour();

            // ปิดตัวเองเพื่อกันคนรัวๆ
            this.enabled = false;
        }
    }
}