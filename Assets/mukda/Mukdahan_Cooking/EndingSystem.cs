using UnityEngine;
using TMPro; // อย่าลืมบรรทัดนี้ ไม่งั้นใช้ TextMeshPro ไม่ได้จ้ะ
using UnityEngine.SceneManagement;


public class EndingSystem : MonoBehaviour
{
    public bool canEndGame = false;
    [Header("UI Components")]
    public TextMeshProUGUI outroText; // ลาก Text ใน Panel มาใส่
    public GameObject levelCompletePanel;  // หน้าต่างจบเกม (ที่มีปุ่มไปต่อ)
    public GameObject dialoguePanel;

    [Header("Game Data Settings")]
    public int provinceIndex = 3;
    public string nextSceneName = "MiniGame1Surin";

    [Header("Dialogue List")]
    [TextArea(3, 10)] // ทำให้ช่องพิมพ์ข้อความกว้างขึ้น
    public string[] sentences; // พิมพ์ลิสต์ข้อความจบที่นี่
    public GameObject ingreGroup;
    private int index = 0;

    public TextMeshProUGUI statusText;

    void OnEnable()
    {
        statusText.gameObject.SetActive(false);
        if (!canEndGame)
        {
            this.gameObject.SetActive(false);
            return;
        }
        // ทำงานทันทีที่ Panel ถูกเปิดขึ้นมา
        index = 0;
        if (sentences.Length > 0) outroText.text = sentences[index];

        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
    }

    void Update()
    {
        // กดคลิกเมาส์เพื่อเปลี่ยนข้อความ
        if (Input.GetMouseButtonDown(0))
        {
            NextSentence();
        }
    }

    void NextSentence()
    {
        if (index < sentences.Length - 1)
        {
            index++;
            outroText.text = sentences[index];
        }
        else
        {
            // อ่านครบทุกประโยคแล้ว -> บันทึกและจบเกม
            EndLevelAndSave();
        }
    }
    void EndLevelAndSave()
    {
        Debug.Log("Outro จบแล้ว! กำลังบันทึกและปิดทุกอย่าง...");
        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            TestGameManager.Instance.FinishExam();
            return; // หยุดการทำงานของ WinPanel ด้านล่างทั้งหมด
        }
        // --- 1. สั่งปิด Ingre ทิ้งเป็นอย่างแรกเลย! (สำคัญมาก) ---
        if (ingreGroup != null)
        {
            ingreGroup.SetActive(false);
        }
        else
        {
            // ถ้าลืมลาก ให้มันลองหาเองดู (กันเหนียว)
            GameObject findIngre = GameObject.Find("ingre");
            if (findIngre != null) findIngre.SetActive(false);
        }
        // -----------------------------------------------------

        // 2. บันทึกเกม
        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.CompleteProvince(provinceIndex);
            GameDataController.Instance.CollectCookbook(provinceIndex);
            GameDataController.Instance.SaveCurrentScene(nextSceneName);
        }

        // 3. เปิดหน้า WinPanel (Level Complete)
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            levelCompletePanel.transform.SetAsLastSibling(); // ดันมาหน้าสุด
        }

        // 4. ปิดหน้า Outro (ปิดตัวเองเป็นอย่างสุดท้าย)
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    // ฟังก์ชันสำหรับปุ่มใน LevelCompletePanel (กดแล้วเปลี่ยนฉาก)
    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}