using UnityEngine;
using System.Collections;

public class ThreshingManager : MonoBehaviour
{

    [Header("Save Data Settings")]
    public int provinceIndex = 2; // จังหวัดอุดรธานี Index = 4
    public string nextSceneName = "DryingRiceScene"; // จังหวัดอุดรธานี Index = 4
                                                     // public string nextSceneName = "ThreshingScene";
    [Header("Rice Settings")]
    public GameObject riceStalks;     // รวงข้าว
    public GameObject fullRice;       // ข้าวเต็มกระด้ง (ปิดไว้ก่อน)
    public ParticleSystem riceParticles; // เมล็ดข้าวกระเด็น

    [Header("UI Settings")]
    public GameObject victoryUI;      // ภาพแสดงความยินดีและปุ่มไปต่อ

    [Header("Game State")]
    public bool canPlay = false;      // ยายสอนจบหรือยัง (ตัวนี้ต่อกับสคริปต์ยาย)
    private int hitCount = 0;
    private bool isDone = false;

    // ฟังก์ชันนี้จะถูกเรียกจากไม้ตี
    public void HitRice()
    {
        if (!canPlay || isDone) return;

        hitCount++;

        // 1. สั่งให้ข้าวกระเด็น
        if (riceParticles != null) riceParticles.Play();

        // 2. สั่งให้รวงข้าวสั่น
        StartCoroutine(ShakeRice());

        // 3. ตรวจสอบว่าครบ 10 ครั้งหรือยัง
        if (hitCount >= 10)
        {
            FinishGame();
        }
    }

    IEnumerator ShakeRice()
    {
        Vector3 originalPos = riceStalks.transform.position;
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            riceStalks.transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.1f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        riceStalks.transform.position = originalPos;
    }

    void FinishGame()
    {
        isDone = true;
        riceStalks.SetActive(false); // ซ่อนรวงข้าว
        fullRice.SetActive(true);    // โชว์ข้าวที่ตีเสร็จแล้ว
        victoryUI.SetActive(true);   // โชว์ภาพยินดี

        if (GameDataController.Instance != null)
        {
            Debug.Log($"Saving Progress: Province {provinceIndex} - Level Passed");

            // สั่งผ่านด่าน (Level ในอุดรฯ จะขยับจาก 0 -> 1)
            GameDataController.Instance.PassLevel(provinceIndex);

            // เซฟชื่อฉากถัดไปไว้เผื่อกด Continue
            GameDataController.Instance.SaveCurrentScene(nextSceneName);

            // บันทึกลงไฟล์ทันที
            GameDataController.Instance.SaveGame();
        }
    }
}