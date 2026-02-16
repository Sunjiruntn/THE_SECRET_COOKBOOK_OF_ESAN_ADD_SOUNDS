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
        if (GameDataController.Instance == null) return;

        // ดึงค่าว่าจังหวัดนี้ เล่นถึงด่านไหนแล้ว?
        int currentLevel = GameDataController.Instance.playerData.currentLevelInProvince[provinceIndex];

        // วนลูปเช็คทุกปุ่ม
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool isUnlocked = (i <= currentLevel);
            levelButtons[i].interactable = isUnlocked;
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

    // ปุ่มกดกลับไปแมพใหญ่ (ประเทศไทย)
    public void BackToMainMap()
    {
        // เล่นเสียงย้อนกลับ
        PlaySfx(backSfx);

        SceneManager.LoadScene("MapSelect"); // แก้ชื่อให้ตรงกับหน้าแมพใหญ่ของคุณ
    }
}