using UnityEngine;
using TMPro; // สำคัญ! ถ้าไม่มีบรรทัดนี้จะ Error เพราะใช้ TextMeshPro
using UnityEngine.SceneManagement;

public class ResultDisplay : MonoBehaviour
{
    // ตัวแปรสำหรับเชื่อมกับ UI
    public TextMeshProUGUI scoreText; // ต้องลาก TextMeshPro - Text (UI) มาใส่
    public TextMeshProUGUI gradeText; // ต้องลาก TextMeshPro - Text (UI) มาใส่

    void Start()
    {
        // 1. ดึงข้อมูลตัวเลข
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        int mistakes = PlayerPrefs.GetInt("Mistakes", 0);
        float timeUsed = PlayerPrefs.GetFloat("TimeUsed", 0);

        // 2. [แก้ตรงนี้] ไม่ต้องคำนวณเกรดใหม่แล้ว! ให้ดึงเกรดที่ TestGameManager บันทึกไว้มาเลย
        // (ถ้าไม่มีบันทึกไว้ ให้ค่าเริ่มต้นเป็น "-")
        string grade = PlayerPrefs.GetString("FinalGrade", "-");

        // 3. แสดงผล
        if (scoreText != null)
        {
            scoreText.text = $"คะแนนรวม: {finalScore}\nผิดพลาด: {mistakes} ครั้ง\nเวลา: {timeUsed:F2} วินาที";
        }

        if (gradeText != null)
        {
            gradeText.text = $"Grade: {grade}";
        }
    }

    public void BackToMenu()
    {
        // เปลี่ยน "MainMenu" เป็นชื่อ Scene หน้าแรกของคุณ
        SceneManager.LoadScene("MainMenu");
    }
}