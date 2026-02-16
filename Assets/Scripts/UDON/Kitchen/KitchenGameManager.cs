using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // เพิ่มเพื่อให้ใช้ Coroutine ได้
using TMPro; // เพิ่มถ้าใช้ TextMeshPro

public class KitchenGameManager : MonoBehaviour
{
    [Header("Save Data Settings")]
    public int provinceIndex = 4; // จังหวัดอุดรธานี Index = 4
    public string nextSceneName = "PoundScene";
    
    [Header("UI Settings - Gauge & Progress")]
    public Slider heatSlider;             // แถบความร้อนแนวตั้ง
    public Image progressCircleFill;      // แถบวงกลมสะสมความสำเร็จ (Image Type: Filled)

    [Header("Win/Lose Panels")]
    public GameObject gameOverPanel;      // หน้าต่างตอนแพ้
    public GameObject successPanel;       // หน้าต่างตอนชนะ

    [Header("New UI & Audio Settings")]
    public GameObject tutorialTextUI;     // UI Text ที่จะแสดง 5 วินาทีแรก
    public AudioSource voiceSource;       // สำหรับเสียงแนะนำด่าน
    public AudioSource musicSource;       // สำหรับเพลง Intro/Background
    public AudioClip introVoiceClip;      // ไฟล์เสียงแนะนำด่าน
    public AudioClip spacebarClickClip;   // ไฟล์เสียงตอนกด Space
    public AudioClip bgmClip;             // ไฟล์เพลง Background

    [Header("Stove Sprites")]
    public SpriteRenderer stoveRenderer;  // ตัวแสดงภาพเตา
    public Sprite normalSprite;           // ภาพเตาปกติ
    public Sprite outSprite;              // ภาพเตาไฟดับ
    public Sprite fireSprite;             // ภาพเตาไฟไหม้

    [Header("VFX & Animation")]
    public Animator grandmaAnimator;      // ตัวคุมท่าทางคุณยาย
    public ParticleSystem riceParticles;  // ตัวคุมเมล็ดข้าวดีด

    [Header("Game Balance")]
    public float currentHeat = 50f;       // ค่าความร้อนเริ่มต้น
    public float dropSpeed = 15f;         // ไฟลดลงกี่หน่วยต่อวินาที
    public float tapIntensity = 8f;       // กด Space 1 ครั้ง ไฟเพิ่มกี่หน่วย

    [Header("Progress Settings")]
    public float currentProgress = 0f;    // ความสุกของอาหาร (0-100)
    public float cookSpeed = 10f;         // ความเร็วในการทำสุก (เมื่ออยู่ในโซนเขียว)
    public float safeZoneMin = 30f;       // ขีดเริ่มโซนเขียว
    public float safeZoneMax = 70f;       // ขีดจบโซนเขียว

    private bool isGameOver = true;       // เริ่มต้นเป็น true เพื่อ "หยุดเกม" รอ Tutorial จบ
    private bool isIntroPlayed = false;

    void Start()
    {
        // ตั้งค่าเริ่มต้นตอนเริ่มเกม
        gameOverPanel.SetActive(false);
        successPanel.SetActive(false);
        heatSlider.maxValue = 100f;
        progressCircleFill.fillAmount = 0f;

        // เริ่มต้นด้วยการหยุด Particle และ Animation
        if (riceParticles != null) riceParticles.Stop();
        if (grandmaAnimator != null) grandmaAnimator.speed = 0f;

        // เริ่ม Coroutine ลำดับการแนะนำด่าน
        StartCoroutine(StartGameSequence());
    }

    // --- ลำดับเหตุการณ์ช่วงเริ่มเกม ---
    IEnumerator StartGameSequence()
    {
        // 1. แสดง Text แนะนำ 5 วินาที
        if (tutorialTextUI != null) tutorialTextUI.SetActive(true);
        yield return new WaitForSeconds(5f);
        if (tutorialTextUI != null) tutorialTextUI.SetActive(false);

        // 2. เล่นเสียงแนะนำด่าน
        if (voiceSource != null && introVoiceClip != null)
        {
            voiceSource.PlayOneShot(introVoiceClip);
            // รอจนเสียงแนะนำจบ
            yield return new WaitForSeconds(introVoiceClip.length);
        }

        // 3. เริ่มเล่นเพลง Intro / Background
        if (musicSource != null && bgmClip != null)
        {
            musicSource.clip = bgmClip;
            musicSource.loop = true;
            musicSource.Play();
        }

        // 4. เริ่มเกม (ปลดล็อกระบบ Update)
        isGameOver = false;
        if (riceParticles != null) riceParticles.Play();
    }

    void Update()
    {
        // ถ้าอยู่ในช่วง Intro หรือเกมจบแล้ว จะไม่ทำงานข้างล่างนี้
        if (isGameOver) return;

        // 1. จัดการระดับไฟ
        currentHeat -= dropSpeed * Time.deltaTime;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentHeat += tapIntensity;
            // --- เล่นเสียงเมื่อกด Spacebar ---
            if (voiceSource != null && spacebarClickClip != null)
            {
                voiceSource.PlayOneShot(spacebarClickClip);
            }
        }

        // จำกัดค่าไฟไม่ให้เกิน 0-100
        currentHeat = Mathf.Clamp(currentHeat, 0f, 100f);
        heatSlider.value = currentHeat;

        // 2. จัดการความสุก (Progress)
        if (currentHeat >= safeZoneMin && currentHeat <= safeZoneMax)
        {
            currentProgress += cookSpeed * Time.deltaTime;
            var emission = riceParticles.emission;
            emission.enabled = true;
        }
        else
        {
            var emission = riceParticles.emission;
            emission.enabled = false;
        }

        progressCircleFill.fillAmount = currentProgress / 100f;

        // 3. ปรับความเร็ว Animation ยายตามความแรงไฟ
        if (grandmaAnimator != null)
        {
            grandmaAnimator.speed = 0.5f + (currentHeat / 100f);
        }

        // 4. เช็คสถานะเกม
        CheckGameStatus();

        // 5. เช็คเมื่อชนะ
        if (currentProgress >= 100f)
        {
            WinGame();
        }
    }

    void CheckGameStatus()
    {
        if (currentHeat <= 0)
        {
            stoveRenderer.sprite = outSprite;
            EndGame();
        }
        else if (currentHeat >= 100)
        {
            stoveRenderer.sprite = fireSprite;
            EndGame();
        }
        else
        {
            stoveRenderer.sprite = normalSprite;
        }
    }

    void EndGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        gameOverPanel.SetActive(true);
        if (riceParticles != null) riceParticles.Stop();
        if (musicSource != null) musicSource.Stop(); // หยุดเพลงเมื่อแพ้
    }

    void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        successPanel.SetActive(true);
        if (riceParticles != null) riceParticles.Stop();
        
        if (GameDataController.Instance != null)
        {
            Debug.Log($"Saving Progress: Province {provinceIndex} - Washing Game Complete");
            GameDataController.Instance.PassLevel(provinceIndex);
            GameDataController.Instance.SaveCurrentScene(nextSceneName);
            GameDataController.Instance.SaveGame();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToNextScene(string sceneName)
    {
        SceneManager.LoadScene("PoundScene");
    }
}