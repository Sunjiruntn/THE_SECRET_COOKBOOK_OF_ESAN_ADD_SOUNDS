using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndingSystem : MonoBehaviour
{
    public bool canEndGame = false;

    [Header("UI Components")]
    public TextMeshProUGUI outroText;
    public GameObject levelCompletePanel;
    public GameObject dialoguePanel;

    [Header("Game Data Settings")]
    public int provinceIndex = 3;
    public string nextSceneName = "MiniGame1Surin";

    [Header("Dialogue List")]
    [TextArea(3, 10)]
    public string[] sentences;
    public GameObject ingreGroup;
    private int index = 0;

    public TextMeshProUGUI statusText;

    void Awake()
    {
        // 🌟 ซ่อน UI ที่ไม่จำเป็นตอนเริ่มเกมทันที
        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (statusText != null) statusText.gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        // 1. เช็คว่า "ถึงเวลาจบเกมหรือยัง?" 
        // ถ้ายังไม่ถึงเวลาจบ (canEndGame เป็น false) ให้ปิดตัวเองไปก่อนแล้วรอระบบเรียกใหม่
        if (!canEndGame)
        {
            this.gameObject.SetActive(false);
            return;
        }

        // ==========================================
        // 🛑 2. ถ้าถึงเวลาจบเกมแล้ว (canEndGame = true) และเป็น "โหมดสอบ"
        // ==========================================
        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            Debug.Log("✅ [โหมดสอบ] ทำอาหารเสร็จแล้ว! กำลังส่งคะแนนและจบการสอบ...");

            // ปิด UI ทิ้งให้หมด
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (statusText != null) statusText.gameObject.SetActive(false);

            // 🌟 สั่งจบการสอบและเด้งไปหน้าโชว์เกรดทันที! ไม่ต้องรออ่านข้อความคุณย่า
            TestGameManager.Instance.FinishExam();
            return; // หยุดการทำงานของฟังก์ชันนี้เลย
        }

        // ==========================================
        // 🟢 3. ถ้าเป็น "โหมดสอนปกติ" ให้เล่นข้อความจบ
        // ==========================================
        if (statusText != null) statusText.gameObject.SetActive(false);

        index = 0;
        if (sentences.Length > 0 && outroText != null)
            outroText.text = sentences[index];

        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
    }

    void Update()
    {
        // โหมดสอบไม่ต้องรอกดเมาส์แล้ว เพราะมันข้ามไปตั้งแต่ OnEnable
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
            if (outroText != null) outroText.text = sentences[index];
        }
        else
        {
            EndLevelAndSave();
        }
    }

    void EndLevelAndSave()
    {
        Debug.Log("Outro จบแล้ว! กำลังบันทึกและปิดทุกอย่าง...");

        // --- 1. สั่งปิด Ingre ทิ้งเป็นอย่างแรกเลย ---
        if (ingreGroup != null)
        {
            ingreGroup.SetActive(false);
        }
        else
        {
            GameObject findIngre = GameObject.Find("ingre");
            if (findIngre != null) findIngre.SetActive(false);
        }

        // 2. บันทึกเกม
        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.CompleteProvince(provinceIndex);
            GameDataController.Instance.CollectCookbook(provinceIndex);
            GameDataController.Instance.SaveCurrentScene(nextSceneName);
        }

        // 3. เปิดหน้า WinPanel
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            levelCompletePanel.transform.SetAsLastSibling();
        }

        // 4. ปิดหน้า Outro 
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}