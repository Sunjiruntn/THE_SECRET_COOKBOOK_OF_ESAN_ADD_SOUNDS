using UnityEngine;

public class CoconutProgress : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] coconutStages;

    [Header("Sensitivity Settings (ความเปราะบาง)")]
    // ใส่ค่าความเร็วที่ "ปลอดภัย" สำหรับแต่ละระยะ
    // เลขมาก = ขูดแรงได้ (ปลอดภัย), เลขน้อย = ต้องขูดเบาๆ
    // จำนวนต้องเท่ากับจำนวนรูปภาพ (4 ระยะ)
    public float[] safeSpeedLimits = { 100f, 80f, 50f, 20f };
    [Header("Difficulty")]
    public int currentStageIndex = 0;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    public void AdvanceStage()
    {
        if (currentStageIndex < coconutStages.Length - 1)
        {
            currentStageIndex++;
            UpdateSprite();
            Debug.Log("Coconut Stage: " + currentStageIndex + " (Max Speed: " + GetCurrentSafeSpeed() + ")");
        }
    }

    void UpdateSprite()
    {
        if (coconutStages.Length > 0 && currentStageIndex < coconutStages.Length)
        {
            spriteRenderer.sprite = coconutStages[currentStageIndex];
        }
    }

    // ฟังก์ชันให้ Controller เรียกถามว่า "ตอนนี้ขูดแรงได้แค่ไหน?"
    public float GetCurrentSafeSpeed()
    {
        if (currentStageIndex < safeSpeedLimits.Length)
        {
            return safeSpeedLimits[currentStageIndex];
        }
        return 10f; // ค่า Default ถ้าหาไม่เจอ
    }
}