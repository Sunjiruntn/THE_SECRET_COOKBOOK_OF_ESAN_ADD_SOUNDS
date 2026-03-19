using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProvinceMapController : MonoBehaviour
{
    [Header("Province Settings")]
    public int provinceIndex = 0; // ใส่เลขจังหวัด (0=กาฬสินธุ์, 1=ขอนแก่น...)

    [Header("Scene Buttons")]
    public Button[] levelButtons;

    [Header("Scene Names")]
    public string[] levelSceneNames;

    [Header("Audio Settings (เพิ่มใหม่)")]
    public AudioSource sfxSource;      // ตัวเล่นเสียง
    public AudioClip clickSfx;        // เสียงกดเข้าด่าน
    public AudioClip backSfx;         // เสียงกดย้อนกลับ
    [Header("--- Exam System (สำหรับด่านสุรินทร์) ---")]
    public Button btnExam;
    [Tooltip("ใส่หมายเลข Index ของด่านสุดท้ายของจังหวัดนี้")]
    public int lastRegularLevelIndex = 3;
    void Start()
    {
        UpdateButtons();

        // บันทึกว่าอยู่ที่แมพย่อยนี้ เผื่อกด Continue
        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.SaveCurrentScene(SceneManager.GetActiveScene().name);
        }
    }

    // ฟังก์ชันช่วยเล่นเสียง
    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    void UpdateButtons()
    {
        // ดักจับข้อ 2: ถ้าไม่มี GameData โชว์ Error สีแดงเลย!
        if (GameDataController.Instance == null)
        {
            Debug.LogError("❌ GameDataController หายไป! โค้ดปุ่มเลยหยุดทำงาน (ลองเริ่มเล่นจากหน้า Intro หรือ MainMenu ดูครับ)");
            return;
        }

        int currentLevel = GameDataController.Instance.playerData.currentLevelInProvince[provinceIndex];
        Debug.Log("📌 ตอนนี้จังหวัดที่ " + provinceIndex + " เล่นถึงด่านที่: " + currentLevel);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            // ด่านนี้ปลดล็อคหรือยัง? (ถ้า index ของปุ่ม น้อยกว่าหรือเท่ากับ level ที่เล่นถึง = ปลดล็อค)
            bool isUnlocked = (i <= currentLevel);

            // ✅ ใช้ปุ่มเปิด/ปิดการกด (interactable จะจัดการเปลี่ยนสีเทาให้เองถ้าตั้งค่าปุ่มไว้)
            levelButtons[i].interactable = isUnlocked;

            // ✅ ใช้ CanvasGroup คุมความโปร่งใส (ล้างโค้ด SpriteRenderer ทิ้งไปเลย!)
            CanvasGroup cg = levelButtons[i].GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = levelButtons[i].gameObject.AddComponent<CanvasGroup>();
            }

            // ถ้าปลดล็อคแล้ว สีเต็ม (1.0), ถ้ายังล็อคอยู่ ให้จางลงครึ่งนึง (0.5)
            cg.alpha = isUnlocked ? 1.0f : 0.5f;
        }

        if (btnExam != null)
        {
            // ถ้าด่านปัจจุบัน (currentLevel) ทะลุด่านสุดท้ายที่ตั้งไว้ไปแล้ว ให้ปลดล็อคห้องสอบ!
            bool isExamUnlocked = currentLevel > lastRegularLevelIndex;
            btnExam.interactable = isExamUnlocked;

            // ทำให้ปุ่มสอบจางลงด้วยถ้ายังไม่ปลดล็อค
            CanvasGroup cgExam = btnExam.GetComponent<CanvasGroup>();
            if (cgExam == null)
            {
                cgExam = btnExam.gameObject.AddComponent<CanvasGroup>();
            }
            cgExam.alpha = isExamUnlocked ? 1.0f : 0.5f;
        }
    }

    // ฟังก์ชันสำหรับกดปุ่ม (เชื่อมต่อใน Inspector)
    public void LoadLevel(int buttonIndex)
    {
        // เล่นเสียงคลิกเข้าด่าน
        PlaySfx(clickSfx);

        // เช็คว่ามีชื่อ Scene ใส่ไว้ครบไหม
        if (buttonIndex < levelSceneNames.Length)
        {
            SceneManager.LoadScene(levelSceneNames[buttonIndex]);

        }
        else
        {
            Debug.LogError("ลืมใส่ชื่อ Scene ในช่อง levelSceneNames จ้า!");
        }
    }
    public void GoToExam()
    {
        PlaySfx(clickSfx); // เล่นเสียงคลิกด้วย

        // สั่งให้ TestGameManager ทำการสุ่มด่านสอบทันที!
        if (TestGameManager.Instance != null)
        {
            TestGameManager.Instance.StartRandomExam();
        }
        else
        {
            Debug.LogError("หา TestGameManager ไม่เจอ! ลืมวาง Prefab ไว้ใน Scene นี้หรือเปล่า?");
        }
    }

    // ปุ่มกดกลับไปแมพใหญ่ (ประเทศไทย)
    public void BackToMainMap()
    {
        // เล่นเสียงย้อนกลับ
        PlaySfx(backSfx);

        SceneManager.LoadScene("MapSelect"); // แก้ชื่อให้ตรงกับหน้าแมพใหญ่ของคุณ
    }


}