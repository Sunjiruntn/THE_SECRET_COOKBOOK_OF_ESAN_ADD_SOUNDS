using UnityEngine;
using System.IO; // จำเป็นสำหรับการอ่าน/เขียนไฟล์

public class GameDataController : MonoBehaviour
{
    // Singleton: ตัวแปรนี้จะทำให้เราเรียกใช้ GameDataController ได้จากทุกที่
    public static GameDataController Instance;

    // ข้อมูลผู้เล่น (เรียกใช้จากไฟล์ PlayerData.cs)
    public PlayerData playerData;

    // ที่อยู่ของไฟล์เซฟในเครื่อง
    private string saveFilePath;

    void Awake()
    {
        // --- ส่วนตั้งค่า Singleton (ให้มีตัวเดียวตลอดเกม) ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ห้ามทำลายเมื่อเปลี่ยน Scene

            // กำหนดที่อยู่ไฟล์เซฟ (SaveData.json)
            saveFilePath = Application.persistentDataPath + "/SaveData.json";

            // โหลดข้อมูลทันทีที่เริ่มเกม
            LoadGame();
        }
        else
        {
            // ถ้ามีตัวซ้ำเกิดขึ้น ให้ทำลายตัวใหม่ทิ้งทันที
            Destroy(gameObject);
        }
    }

    // =========================================================
    // 💾 โซนบันทึกและโหลด (Core System)
    // =========================================================

    public void SaveGame()
    {
        // แปลงข้อมูลเป็นข้อความ JSON
        string json = JsonUtility.ToJson(playerData);

        // เขียนลงไฟล์
        File.WriteAllText(saveFilePath, json);

        // Debug.Log("💾 บันทึกเกมเรียบร้อย!");
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            // ถ้ามีไฟล์เซฟเก่า -> อ่านข้อมูลออกมา
            string json = File.ReadAllText(saveFilePath);
            playerData = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("📂 โหลดเซฟสำเร็จ: เล่นค้างไว้ที่ " + playerData.lastSceneName);
        }
        else
        {
            // ถ้าไม่มีไฟล์เซฟ -> สร้างใหม่ (New Game)
            Debug.Log("⚠️ ไม่พบเซฟเก่า สร้างข้อมูลผู้เล่นใหม่...");
            NewGame();
        }
    }

    // ฟังก์ชันเริ่มเกมใหม่ (ล้างข้อมูลเก่าทั้งหมด)
    public void NewGame()
    {
        playerData = new PlayerData(); // สร้าง object ใหม่ (ค่าเริ่มต้นจะถูกเซ็ตใน Constructor ของ PlayerData)
        SaveGame(); // บันทึกทันที
    }

    // =========================================================
    // 🛠️ โซนฟังก์ชันช่วยเหลือ (Helper Functions)
    // เอาไว้ให้ GameManager เรียกใช้ได้ง่ายๆ
    // =========================================================

    // 1. เรียกเมื่อผู้เล่นเล่นผ่านมินิเกมย่อย (เช่น ขุดปู, ตำปู)
    public void PassLevel(int provinceIndex)
    {
        // 1. ตรวจสอบว่ามีข้อมูลจังหวัดนี้จริงไหม (กัน Error)
        if (provinceIndex < 0 || provinceIndex >= playerData.currentLevelInProvince.Length)
        {
            Debug.LogError($"❌ Error: ใส่เลข Province Index ผิด! คุณใส่เลข {provinceIndex} แต่ระบบมีแค่ 0-{playerData.currentLevelInProvince.Length - 1}");
            return;
        }

        // 2. ดึงเลเวลปัจจุบันออกมาดู
        int currentLv = playerData.currentLevelInProvince[provinceIndex];

        // 3. เพิ่มเลเวลขึ้น 1 (เพื่อให้ด่านถัดไปเปิด)
        // เราจะเช็คก่อนว่า เลเวลมันตันหรือยัง (สมมติว่าแต่ละจังหวัดมี 3 ด่าน คือ 0, 1, 2 -> จบที่ 3)
        // ถ้ายังไม่ตัน ก็บวกเพิ่มได้
        int maxLevelsPerProvince = 10;
        if (currentLv < maxLevelsPerProvince)
        {
            playerData.currentLevelInProvince[provinceIndex]++;
            Debug.Log($"✅ อัพเลเวลจังหวัด {provinceIndex} จาก {currentLv} -> เป็น {playerData.currentLevelInProvince[provinceIndex]}");
        }
        else
        {
            Debug.Log($"⚠️ จังหวัด {provinceIndex} เลเวลเต็มแล้ว (เล่นซ้ำได้ แต่เลเวลไม่เพิ่ม)");
        }

        // 4. 🔥🔥🔥 หัวใจสำคัญที่สุด: สั่งบันทึกลงไฟล์ทันที! 🔥🔥🔥
        // ถ้าไม่มีบรรทัดนี้ พอปิดเกมมันจะลืมหมดครับ
        SaveGame();

        Debug.Log("💾 บันทึกเกมเรียบร้อย! (Game Saved)");
    }

    // 2. เรียกเมื่อผู้เล่นทำอาหารสำเร็จ (จบจังหวัด)
    public void CompleteProvince(int provinceIndex)
    {
        // 1. ติ๊กถูกว่าได้สูตรอาหารแล้ว
        playerData.cookbookCollected[provinceIndex] = true;

        // 2. ปลดล็อคจังหวัดถัดไป (ถ้ามี)
        // เช็คก่อนว่าไม่ใช่จังหวัดสุดท้าย (Index 4)
        if (provinceIndex + 1 < playerData.provinceUnlocked.Length)
        {
            playerData.provinceUnlocked[provinceIndex + 1] = true;
            Debug.Log($"ปลดล็อคจังหวัดที่ {provinceIndex + 1} แล้ว!");
        }

        // 3. (เสริม) เซ็ต Level ให้สุด เผื่อไว้อ้างอิง
        playerData.currentLevelInProvince[provinceIndex] = 100; // หรือค่าที่กำหนดว่าจบแล้ว

        SaveGame();
    }

    // 3. เรียกเมื่อเข้าฉากใหม่ (เอาไว้ใช้กับปุ่ม Continue)
    public void SaveCurrentScene(string sceneName)
    {
        // ไม่บันทึกฉากเมนู หรือฉากโหลด
        if (sceneName != "MainMenu" && sceneName != "LoadingScreen")
        {
            playerData.lastSceneName = sceneName;
            SaveGame();
            // Debug.Log("📍 จำตำแหน่งฉากล่าสุด: " + sceneName);
        }
    }

    // 4. (แถม) ฟังก์ชันเช็คว่าจังหวัดนี้ปลดล็อคหรือยัง?
    public bool IsProvinceUnlocked(int provinceIndex)
    {
        if (provinceIndex >= 0 && provinceIndex < playerData.provinceUnlocked.Length)
        {
            return playerData.provinceUnlocked[provinceIndex];
        }
        return false;
    }
    public void ResetData()
    {
        NewGame(); // ให้มันไปเรียกตัว NewGame อีกที
    }
    public void CollectCookbook(int provinceIndex)
    {
        // 1. เช็คว่า index ที่ส่งมา ถูกต้องไหม (ไม่เกินจำนวนอาเรย์ 5 ช่อง)
        if (provinceIndex >= 0 && provinceIndex < playerData.cookbookCollected.Length)
        {
            // 2. ถ้ายังไม่เคยเก็บได้มาก่อน ให้ทำการบันทึก
            if (!playerData.cookbookCollected[provinceIndex])
            {
                playerData.cookbookCollected[provinceIndex] = true;
                Debug.Log($"📖 ได้รับสูตรอาหารของจังหวัด {provinceIndex} เรียบร้อยแล้ว!");

                // 3. บันทึกเกมทันที
                SaveGame();
            }
        }
        else
        {
            Debug.LogError("ส่ง Index ผิด! ไม่มีจังหวัดลำดับที่ " + provinceIndex);
        }
    }
}