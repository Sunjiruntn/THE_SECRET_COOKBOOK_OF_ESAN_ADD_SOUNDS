using UnityEngine;

public class PotManager : MonoBehaviour
{
    [Header("Content Sprites")]
    public GameObject liquidVisual; // น้ำแป้งเหลว
    public GameObject thickVisual;  // แป้งเริ่มหนืด
    public GameObject cookedVisual; // แป้งสุกเงา
    public GameObject toppingVisual; // ถั่วคั่ว/งา

    void Start()
    {
        // เริ่มต้นให้หม้อว่างเปล่า
        AllContentOff();
    }

    public void AllContentOff()
    {
        liquidVisual.SetActive(false);
        thickVisual.SetActive(false);
        cookedVisual.SetActive(false);
        toppingVisual.SetActive(false);
    }

    // ฟังก์ชันเปลี่ยนสเตจเนื้อขนม
    public void SetPotState(int state)
    {
        AllContentOff();
        switch (state)
        {
            case 1: liquidVisual.SetActive(true); break; // หลังเทเสร็จ
            case 2: thickVisual.SetActive(true); break;  // กวนไปสักพัก
            case 3: cookedVisual.SetActive(true); break; // สุกแล้ว
            case 4: 
                cookedVisual.SetActive(true); 
                toppingVisual.SetActive(true); break; // สุก + โรยหน้า
        }
    }
}