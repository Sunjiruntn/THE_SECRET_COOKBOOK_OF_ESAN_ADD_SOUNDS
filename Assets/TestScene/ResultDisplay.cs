using UnityEngine;
using TMPro; // สำคัญ! ถ้าไม่มีบรรทัดนี้จะ Error เพราะใช้ TextMeshPro
using UnityEngine.SceneManagement;

public class ResultDisplay : MonoBehaviour
{
    [Header("UI Elements (ลาก TextMeshPro มาใส่ให้ครบ)")]
    public TextMeshProUGUI dishNameText; // ช่องแสดงชื่อโจทย์ (ด้านบน)
    public TextMeshProUGUI scoreText;    // ช่องคะแนนรวม
    public TextMeshProUGUI mistakesText; // ช่องข้อผิดพลาด
    public TextMeshProUGUI timeText;     // ช่องเวลาที่ใช้
    public TextMeshProUGUI gradeText;    // ช่องตราปั๊มเกรด

    void Start()
    {
        // 1. ดึงข้อมูลที่บันทึกไว้จากการสอบ
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        int mistakes = PlayerPrefs.GetInt("Mistakes", 0);
        float timeUsed = PlayerPrefs.GetFloat("TimeUsed", 0);
        string grade = PlayerPrefs.GetString("FinalGrade", "-");

        // [เพิ่มใหม่] ดึงชื่อ Scene ที่เพิ่งสอบไป 
        // ⚠️ (อย่าลืมให้ TestGameManager สั่ง PlayerPrefs.SetString("TestedScene", ชื่อด่าน) ตอนสอบเสร็จด้วยนะครับ)
        string testedScene = PlayerPrefs.GetString("TestedScene", "");

        // 2. แสดงผลชื่อโจทย์ (แปลงจากชื่อ Scene เป็นภาษาไทย)
        if (dishNameText != null)
        {
            dishNameText.text = "โจทย์ที่ได้รับ " + GetThaiDishName(testedScene);
        }

        // 3. แสดงคะแนนและข้อผิดพลาด 
        if (scoreText != null)
        {
            scoreText.text = "คะแนน " + finalScore.ToString();
        }

        if (mistakesText != null)
        {
            mistakesText.text = "ความผิดพลาด " + mistakes.ToString() + " ครั้ง";
        }
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(timeUsed / 60F);
            int seconds = Mathf.FloorToInt(timeUsed - minutes * 60);

            timeText.text = string.Format("เวลาที่ใช้ {0:00}:{1:00}", minutes, seconds);
        }

        if (gradeText != null)
        {
            gradeText.text = "ระดับฝีมือ " + grade;
        }
    }

    // ==========================================
    // ฟังก์ชันแปลงชื่อ Scene เป็นชื่อเมนูอาหารภาษาไทย
    // ==========================================
    private string GetThaiDishName(string sceneName)
    {
        switch (sceneName)
        {
            case "CookingStage": return "เมนูลาบปูนา จังหวัดร้อยเอ็ด";
            case "CookingStageSurin": return "เมนูขนมเนียล จังหวัดสุรินทร์";
            case "Mukdahan_Cooking": return "เมนูข้าวต้มพันตองหนองสูง จังหวัดมุกดาหาร";
            case "KitchenScene": return "เมนูข้าวโจ้โรยงา จังหวัดขอนแก่น";

            default: return "สุ่มเมนูอีสานพาแซ่บ";
        }
    }

    public void BackToMenu()
    {
        // ตามที่เราคุยกัน กลับไปหน้าแมพของสุรินทร์
        SceneManager.LoadScene("SubMap_Surin");
    }
}