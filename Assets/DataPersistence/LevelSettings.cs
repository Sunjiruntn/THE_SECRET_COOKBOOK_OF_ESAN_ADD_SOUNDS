using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    [Header("ตั้งค่าประจำด่านนี้")]
    [Tooltip("นี่คือจังหวัดลำดับที่เท่าไหร่? (เริ่มที่ 0, 1, 2, 3, 4)")]
    public int provinceIndex = 0;

    [Tooltip("นี่คือด่านทำอาหาร (ด่านสุดท้าย) ใช่หรือไม่?")]
    public bool isCookingGame = false;
}