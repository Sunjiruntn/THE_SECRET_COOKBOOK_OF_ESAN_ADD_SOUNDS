using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
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

    [Header("Cutscene Settings")]
    public GameObject cutscenePanel;
    public VideoPlayer videoPlayer;
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
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
        }

        // ✅ 4. ดักจับว่า "ถ้าวิดีโอเล่นจบแล้ว ให้ทำฟังก์ชัน OnVideoEnd นะ"
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
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

        //SceneManager.LoadScene(mapSceneName);

        PlayCutscene();
    }

    // ✅ ปุ่ม CONTINUE (เล่นต่อ)
    public void OnClickContinue()
    {
        PlayClickSound(); // เล่นเสียงกดปุ่ม

        //SceneManager.LoadScene(mapSceneName);

        PlayCutscene();
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
    private void PlayCutscene()
    {
        // 1. หยุดเพลงหน้าเมนู เสียงจะได้ไม่ตีกับเสียงในวิดีโอ
        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }

        // 2. เปิดหน้าจอวิดีโอ และสั่งเล่น
        if (cutscenePanel != null && videoPlayer != null)
        {
            cutscenePanel.SetActive(true);
            videoPlayer.Play();
        }
        else
        {
            // กันเหนียว: ถ้าลืมใส่ไฟล์วิดีโอ ให้ข้ามไปโหลดด่านเลย เกมจะได้ไม่ค้าง
            SceneManager.LoadScene(mapSceneName);
        }
    }

    // ฟังก์ชันนี้จะทำงานอัตโนมัติเมื่อวิดีโอเล่นจบเฟรมสุดท้าย
    private void OnVideoEnd(VideoPlayer vp)
    {
        // โหลดเข้าฉาก MapSelect ได้เลย!
        SceneManager.LoadScene(mapSceneName);
    }

    // (แถม) เอาไว้ใช้สร้างปุ่ม "ข้าม (Skip)" ให้ผู้เล่นกด
    public void SkipCutscene()
    {
        if (videoPlayer != null) videoPlayer.Stop();
        SceneManager.LoadScene(mapSceneName);
    }
}