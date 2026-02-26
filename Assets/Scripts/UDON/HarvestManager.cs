using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HarvestManager : MonoBehaviour
{
    [Header("Save Data Settings")]
    public int provinceIndex = 2; // จังหวัดอุดรธานี Index = 4
    public string nextSceneName = "ThreshingScene";
    [Header("UI Windows")]
    public TMP_Text scoreText;
    public GameObject successPanel; // ลาก Success Group (ภาพยินดี+ปุ่มไปต่อ) มาใส่
    public GameObject failurePanel; // ลาก Failure Group (ภาพตกใจ+ปุ่มลองใหม่) มาใส่

    [Header("Game Settings")]
    public int targetScore = 5;
    private int currentScore = 0;
    private bool isGameOver = true; // เริ่มต้นเป็น true เพื่อรอยายสอนจบ

    void Start()
    {
        // บังคับปิด UI ทั้งหมดทันทีที่กดเริ่มเกม
        if (successPanel != null) successPanel.SetActive(false);
        if (failurePanel != null) failurePanel.SetActive(false);
        UpdateUI();
    }

    // ฟังก์ชันถูกเรียกโดย RiceTutorialManager เมื่อยายลอยออกไปแล้ว
    public void StartGame()
    {
        isGameOver = false;
        Debug.Log("ระบบเกี่ยวข้าวเปิดทำงาน!");
    }

    public void CheckRice(RiceGrain rice)
    {
        if (isGameOver) return;

        if (rice.riceType == 1)
        { // 1 = ข้าวระยะเม่า (ถูก)
            currentScore++;
            rice.gameObject.SetActive(false); // หายไปเมื่อเกี่ยว
            UpdateUI();

            if (currentScore >= targetScore)
            {
                ShowWin();
            }
        }
        else
        { // กรณีคลิกข้าวสีอื่น (0 หรือ 2)
            ShowFail();
        }
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "เก็บรวงข้าวระยะเม่า: " + currentScore + " / " + targetScore;
    }

    void ShowWin()
    {
        isGameOver = true;
        if (successPanel != null) successPanel.SetActive(true);
        if (GameDataController.Instance != null)
        {
            Debug.Log($"Saving Progress: Province {provinceIndex} - Level Passed");

            GameDataController.Instance.PassLevel(provinceIndex);
            GameDataController.Instance.SaveCurrentScene(nextSceneName);
            GameDataController.Instance.SaveGame();
        }
    }

    void ShowFail()
    {
        isGameOver = true;
        if (failurePanel != null) failurePanel.SetActive(true);
        PlayerPrefs.SetInt("HasFailedRice", 1); // บันทึกว่าแพ้
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToNextGame()
    {
        SceneManager.LoadScene("ThreshingScene"); // ระบุชื่อฉากมินิเกมที่ 2
    }
}