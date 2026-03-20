using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class KitchenGameManager : MonoBehaviour
{
    [Header("Save Data Settings")]
    public int provinceIndex = 2; 
    public string nextSceneName = "PoundScene";
    
    [Header("UI Settings - Gauge & Progress")]
    public Slider heatSlider; 
    public Image progressCircleFill;

    [Header("Win/Lose Panels")]
    public GameObject gameOverPanel;
    public GameObject successPanel;

    [Header("New UI & Audio Settings")]
    public GameObject tutorialTextUI; 
    public AudioSource voiceSource; 
    public AudioSource musicSource; 
    public AudioClip introVoiceClip; 
    public AudioClip spacebarClickClip; 
    public AudioClip bgmClip; 

    [Header("Stove Sprites")]
    public SpriteRenderer stoveRenderer; 
    public Sprite normalSprite; 
    public Sprite outSprite; 
    public Sprite fireSprite; 

    [Header("VFX & Animation")]
    public Animator grandmaAnimator; 
    public ParticleSystem riceParticles; 

    [Header("Game Balance")]
    public float currentHeat = 50f; 
    public float dropSpeed = 15f; 
    public float tapIntensity = 8f; 

    [Header("Progress Settings")]
    public float currentProgress = 0f; 
    public float cookSpeed = 10f; 
    public float safeZoneMin = 30f; 
    public float safeZoneMax = 70f; 

    private bool isGameOver = true; 
    private bool isIntroPlayed = false;

    void Start()
    {
        gameOverPanel.SetActive(false);
        successPanel.SetActive(false);
        heatSlider.maxValue = 100f;
        progressCircleFill.fillAmount = 0f;

        if (riceParticles != null) riceParticles.Stop();
        if (grandmaAnimator != null) grandmaAnimator.speed = 0f;

        StartCoroutine(StartGameSequence());
    }

    IEnumerator StartGameSequence()
    {
        if (tutorialTextUI != null) tutorialTextUI.SetActive(true);
        yield return new WaitForSeconds(5f);
        if (tutorialTextUI != null) tutorialTextUI.SetActive(false);

        if (voiceSource != null && introVoiceClip != null)
        {
            voiceSource.PlayOneShot(introVoiceClip);
            yield return new WaitForSeconds(introVoiceClip.length);
        }

        if (musicSource != null && bgmClip != null)
        {
            musicSource.clip = bgmClip;
            musicSource.loop = true;
            musicSource.Play();
        }

        isGameOver = false;
        if (riceParticles != null) riceParticles.Play();
    }

    void Update()
    {
        if (isGameOver) return;

        currentHeat -= dropSpeed * Time.deltaTime;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentHeat += tapIntensity;
            if (voiceSource != null && spacebarClickClip != null)
            {
                voiceSource.PlayOneShot(spacebarClickClip);
            }
        }

        currentHeat = Mathf.Clamp(currentHeat, 0f, 100f);
        heatSlider.value = currentHeat;

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

        if (grandmaAnimator != null)
        {
            grandmaAnimator.speed = 0.5f + (currentHeat / 100f);
        }

        CheckGameStatus();

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
        
        StopAllSounds(); // หยุดเสียงทั้งหมดเมื่อแพ้
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

        // หมายเหตุ: หากต้องการให้เพลงคลอไประหว่างหน้า Success ไม่ต้องใส่ StopAllSounds ตรงนี้
        // แต่ต้องใส่ใน GoToNextScene แทน
    }

    // ฟังก์ชันสำหรับหยุดเสียงทั้งหมดใน Script นี้
    private void StopAllSounds()
    {
        if (voiceSource != null) voiceSource.Stop();
        if (musicSource != null) musicSource.Stop();
    }

    // สั่งให้หยุดเสียงเมื่อ Object นี้ถูกทำลาย (เช่นตอนเปลี่ยน Scene)
    void OnDisable()
    {
        StopAllSounds();
    }

    public void RestartGame()
    {
        StopAllSounds(); // หยุดเสียงก่อนโหลด Scene ใหม่
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToNextScene(string sceneName)
    {
        StopAllSounds(); // หยุดเสียงก่อนไปฉากถัดไป
        SceneManager.LoadScene("PoundScene");
    }
}