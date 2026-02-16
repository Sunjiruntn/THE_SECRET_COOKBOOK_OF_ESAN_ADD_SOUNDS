using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseSystem : MonoBehaviour
{
    // ==========================================
    // [แทรกใหม่] Audio System
    // ==========================================
    [Header("--- Audio Settings (เพิ่มใหม่) ---")]
    public AudioSource sfxSource;   // ลาก AudioSource ที่ใช้เล่นเสียง SFX มาใส่
    public AudioClip clickSfx;      // ลากไฟล์เสียงคลิกมาใส่
    // ==========================================

    [Header("UI Components")]
    public GameObject pausePanel;  
    public GameObject pauseButton; 

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string mapSelectScene = "MapSelect";

    private bool isPaused = false;

    void Start()
    {
        ResetGameState();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // --- ฟังก์ชันหลัก ---

    public void PauseGame()
    {
        // [แทรก] เล่นเสียงเมื่อกด Pause
        PlayClickSound();

        isPaused = true;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);

        Time.timeScale = 0f;
        Debug.Log("Game Paused (ESC or Button)");
    }

    public void ResumeGame()
    {
        // [แทรก] เล่นเสียงเมื่อกด Resume
        PlayClickSound();

        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);

        Time.timeScale = 1f;
        Debug.Log("Game Resumed");
    }

    // --- ฟังก์ชันเปลี่ยนฉาก ---

    public void GoToMainMenu()
    {
        // [แทรก] เล่นเสียงก่อนเปลี่ยนฉาก
        PlayClickSound();

        Time.timeScale = 1f; 
        SceneManager.LoadScene(mainMenuScene);
    }

    public void GoToMapSelect()
    {
        // [แทรก] เล่นเสียงก่อนเปลี่ยนฉาก
        PlayClickSound();

        Time.timeScale = 1f; 
        SceneManager.LoadScene(mapSelectScene);
    }

    private void ResetGameState()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
    }

    // ==========================================
    // [แทรกใหม่] ฟังก์ชันช่วยเล่นเสียง
    // ==========================================
    private void PlayClickSound()
    {
        if (sfxSource != null && clickSfx != null)
        {
            // ใช้ PlayOneShot เพื่อให้เสียงเล่นจบแม้จะมีการหยุดเวลาหรือเปลี่ยนคำสั่ง
            // หมายเหตุ: ถ้า Time.timeScale = 0 เสียงบางประเภทอาจไม่ดัง 
            // แนะนำให้ตั้งค่า AudioSource ตรง "Ignore Listener Pause" ใน Unity Inspector ถ้าต้องการเสียงตอนหยุดเกม
            sfxSource.PlayOneShot(clickSfx);
        }
    }
}