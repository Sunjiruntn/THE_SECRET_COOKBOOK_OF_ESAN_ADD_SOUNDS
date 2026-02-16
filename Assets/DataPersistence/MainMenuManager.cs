using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Settings")]
    public string mapSceneName = "MapSelect"; 

    [Header("UI References")]
    public Button continueButton;

    [Header("Audio Settings")]
    public AudioSource backgroundMusic; // ลาก AudioSource ที่ใส่เพลง BGM มาวาง
    public AudioSource sfxSource;       // ลาก AudioSource สำหรับเสียงเอฟเฟคมาวาง
    public AudioClip buttonClickSound;  // ลากไฟล์เสียงคลิกมาใส่

    void Start()
    {
        // เล่นเพลงพื้นหลังทันทีที่เริ่ม (ถ้ายังไม่ได้กด Play On Awake ใน Inspector)
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }

        // --- เช็คว่ามีเซฟไหม? เพื่อเปิด/ปิดปุ่ม Continue ---
        if (GameDataController.Instance != null)
        {
            string lastScene = GameDataController.Instance.playerData.lastSceneName;

            if (string.IsNullOrEmpty(lastScene))
            {
                continueButton.interactable = false;
            }
            else
            {
                continueButton.interactable = true; 
            }
        }
    }

    // ✅ ปุ่ม START (เริ่มใหม่)
    public void OnClickNewGame()
    {
        PlayClickSound(); // เล่นเสียงกดปุ่ม

        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.NewGame();
            GameDataController.Instance.SaveCurrentScene(mapSceneName);
        }

        SceneManager.LoadScene(mapSceneName);
    }

    // ✅ ปุ่ม CONTINUE (เล่นต่อ)
    public void OnClickContinue()
    {
        PlayClickSound(); // เล่นเสียงกดปุ่ม
        
        SceneManager.LoadScene(mapSceneName);
    }

    public void OnClickExit()
    {
        PlayClickSound(); // เล่นเสียงกดปุ่ม
        Application.Quit();
    }

    // ฟังก์ชันช่วยเล่นเสียงคลิก
    private void PlayClickSound()
    {
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }
}