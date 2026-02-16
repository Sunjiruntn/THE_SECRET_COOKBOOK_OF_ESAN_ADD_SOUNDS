using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections; // ✅ เพิ่มเพื่อใช้ Coroutine

[System.Serializable] public class CoconutStage { public string name; public Sprite coconutSprite; [Range(0, 100)] public float showAtPercent; }
[System.Serializable] public class FlakeStage { public string name; public GameObject flakeObject; [Range(0, 100)] public float appearAtPercent; }

public class CatHandController : MonoBehaviour
{
    // ==========================================
    // [แทรกใหม่] Audio System Settings
    // ==========================================
    [Header("--- Audio Settings (เพิ่มใหม่) ---")]
    public AudioSource bgmSource;        // สำหรับเพลงพื้นหลัง
    public AudioSource sfxSource;        // สำหรับเสียงปุ่ม/คลิก
    public AudioSource voiceSource;      // สำหรับเสียงพากย์แนะนำ
    public AudioSource grateSource;      // สำหรับเสียงขูด (ติ๊ก Loop ใน Inspector)

    public AudioClip bgmClip;            
    public AudioClip clickSfx;           
    public AudioClip introVoice;         
    public AudioClip retryVoice;         
    public AudioClip grateSfx;           
    // ==========================================

    [Header("--- Win System & Level Settings (เพิ่มใหม่) ---")]
    public GameObject winPanel;
    public Button nextLevelButton;
    public string nextSceneName = "CookingStageSurin";
    public int provinceIndex = 2;

    [Header("--- Dialogue System (ระบบบทพูด) ---")]
    public GameObject dialoguePanel; 
    public TextMeshProUGUI dialogueText; 

    [TextArea(2, 3)]
    public string[] introSentences; 

    [TextArea(2, 3)]
    public string retrySentence = "ยายเอามะพร้าวใหม่มาเปลี่ยนให้แล้ว ขูดใหม่เด้อ"; 

    private static bool isRetryRound = false;
    private Queue<string> sentencesQueue = new Queue<string>();
    private bool isDialogueActive = false;

    [Header("--- Visual Effects (ลากใส่ใหม่ด้วยนะ!) ---")]
    public ParticleSystem whiteFlakesPS;
    public ParticleSystem brownFlakesPS;

    [Header("--- Coconut Visuals ---")]
    public SpriteRenderer coconutRenderer;
    public CoconutStage[] coconutStages;

    [Header("--- Cat Hand Settings ---")]
    public float maxSafeHoldTime = 0.8f;
    public float tensionRecoverySpeed = 2.0f;

    [Header("UI Status")]
    public TextMeshProUGUI statusText;
    public Image circularProgressImage;
    public Image tensionBarImage;
    public GameObject tensionBarWholeObject;
    public GameObject failPopupPanel;
    public GameObject restartButtonObject;

    [Header("References")]
    public Transform handTransform;
    public Transform coconutTarget;
    public GameObject[] dirtyStrikeVisuals;
    public int maxDirtyStrikes = 3;
    public FlakeStage[] trayFlakes;

    [Header("Zone Settings")]
    public float safeZoneRadius = 1.5f;
    public float effortPerCoconut = 100f;

    private float globalEffort = 0f;
    private bool isOnBlade = false;
    private bool isTooRough = false;
    private float currentHoldTimer = 0f;
    private bool isGameFinished = false;
    private bool isGameFailed = false;
    private bool isDragging = false;
    private int currentDirtyStrikes = 0;
    private bool wasTooRoughLastFrame = false;
    private Vector3 dragOffset;
    private Camera mainCamera;
    private Vector3 initialHandPos;

    void Start()
    {
        mainCamera = Camera.main;
        if (handTransform) initialHandPos = handTransform.position;
        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.SaveCurrentScene(SceneManager.GetActiveScene().name);
        }
        UpdateCoconutVisuals();
        UpdateTrayFlakes();
        if (winPanel != null) winPanel.SetActive(false);
        if (restartButtonObject) restartButtonObject.SetActive(false);
        if (failPopupPanel) failPopupPanel.SetActive(false);
        if (dirtyStrikeVisuals != null) foreach (var d in dirtyStrikeVisuals) if (d) d.SetActive(false);
        if (trayFlakes != null) foreach (var f in trayFlakes) if (f.flakeObject) f.flakeObject.SetActive(false);

        ForceStopParticle(whiteFlakesPS);
        ForceStopParticle(brownFlakesPS);

        StartDialogueSequence();
    }

    void StartDialogueSequence()
    {
        sentencesQueue.Clear();
        AudioClip selectedVoice = null; // ✅ เพิ่มเพื่อเก็บเสียงที่จะเล่น

        if (isRetryRound)
        {
            sentencesQueue.Enqueue(retrySentence);
            selectedVoice = retryVoice; // ✅ เลือกเสียง Retry
        }
        else
        {
            foreach (string sentence in introSentences)
            {
                sentencesQueue.Enqueue(sentence);
            }
            selectedVoice = introVoice; // ✅ เลือกเสียง Intro
        }

        // ✅ [แทรก] เล่นเสียงพากย์และรอเริ่ม BGM
        if (voiceSource != null && selectedVoice != null)
        {
            voiceSource.clip = selectedVoice;
            voiceSource.Play();
            StartCoroutine(WaitAndPlayBGM(selectedVoice.length));
        }
        else { PlayBGM(); }

        if (sentencesQueue.Count > 0)
        {
            isDialogueActive = true;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            DisplayNextSentence();
        }
        else
        {
            EndDialogue();
        }
    }

    // ✅ [แทรกใหม่] Coroutine รอเสียงพากย์จบแล้วต่อ BGM
    IEnumerator WaitAndPlayBGM(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayBGM();
    }

    void PlayBGM()
    {
        if (bgmSource != null && bgmClip != null && !bgmSource.isPlaying)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void DisplayNextSentence()
    {
        if (sentencesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentencesQueue.Dequeue();
        if (dialogueText != null) dialogueText.text = sentence;
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (isDialogueActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // ✅ [แทรก] เสียงคลิกตอนคุย
                if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);
                DisplayNextSentence();
            }
            return; 
        }

        if (isGameFinished || isGameFailed)
        {
            ForceStopParticle(whiteFlakesPS);
            ForceStopParticle(brownFlakesPS);
            // ✅ [แทรก] หยุดเสียงขูดถ้าเกมจบ
            if (grateSource != null && grateSource.isPlaying) grateSource.Stop();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = GetMouseWorldPos();
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && hit.transform == handTransform)
            {
                isDragging = true;
                dragOffset = handTransform.position - mousePos;
                dragOffset.z = 0;
            }
        }
        if (Input.GetMouseButtonUp(0)) isDragging = false;

        if (isDragging && Input.GetMouseButton(0))
        {
            MoveHandToMouse();
            CheckCatHandMechanic();

            if (isOnBlade && !isTooRough)
            {
                globalEffort += Time.deltaTime * 15f;

                if (globalEffort >= effortPerCoconut)
                {
                    globalEffort = effortPerCoconut;
                    isGameFinished = true;
                    GameFinishedSuccess();
                }

                UpdateCoconutVisuals();
                UpdateTrayFlakes();

                // ✅ [แทรก] เล่นเสียงขูดมะพร้าว
                if (grateSource != null && !grateSource.isPlaying && grateSfx != null)
                {
                    grateSource.clip = grateSfx;
                    grateSource.Play();
                }
            }
            else
            {
                // ✅ [แทรก] หยุดเสียงขูดถ้าไม่ได้ขูดจริง (หรือขูดแรงไป)
                if (grateSource != null && grateSource.isPlaying) grateSource.Stop();
            }
            CheckPenaltyLogic();
        }
        else
        {
            currentHoldTimer -= Time.deltaTime * tensionRecoverySpeed;
            if (currentHoldTimer < 0) currentHoldTimer = 0;
            isTooRough = false;
            wasTooRoughLastFrame = false;

            // ✅ [แทรก] หยุดเสียงขูดเมื่อปล่อยมือ
            if (grateSource != null && grateSource.isPlaying) grateSource.Stop();
        }

        HandleGratingFX();
        UpdateUI();
    }

    public void RestartGame()
    {
        // ✅ [แทรก] เสียงปุ่มตอน Restart
        if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);
        isRetryRound = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GameFinishedSuccess()
    {
        Debug.Log("🎉 ภารกิจสำเร็จ! ขูดมะพร้าวเสร็จแล้ว");
        if (winPanel != null) winPanel.SetActive(true);
        if (tensionBarWholeObject != null) tensionBarWholeObject.SetActive(false);
        if (handTransform != null) handTransform.gameObject.SetActive(false);
        if (statusText != null) statusText.text = "";
        if (whiteFlakesPS != null) whiteFlakesPS.gameObject.SetActive(false);
        if (brownFlakesPS != null) brownFlakesPS.gameObject.SetActive(false);
        
        // ✅ [แทรก] หยุดเสียงขูดเมื่อชนะ
        if (grateSource != null) grateSource.Stop();

        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.PassLevel(provinceIndex);
            GameDataController.Instance.SaveCurrentScene(nextSceneName);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(GoToNextLevel);
        }
    }

    public void GoToNextLevel()
    {
        // ✅ [แทรก] เสียงปุ่มตอนไปด่านต่อไป
        if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);
        SceneManager.LoadScene(nextSceneName);
    }

    void HandleGratingFX()
    {
        bool shouldShow = isDragging && isOnBlade && Input.GetMouseButton(0);
        if (isDragging && handTransform != null)
        {
            Vector3 targetPos = handTransform.position;
            targetPos.z = -5f;
            if (whiteFlakesPS) whiteFlakesPS.transform.position = targetPos;
            if (brownFlakesPS) brownFlakesPS.transform.position = targetPos;
        }

        if (shouldShow)
        {
            if (isTooRough)
            {
                SetParticleRate(brownFlakesPS, 5f);
                SetParticleRate(whiteFlakesPS, 0f);
            }
            else
            {
                SetParticleRate(whiteFlakesPS, 5f);
                SetParticleRate(brownFlakesPS, 0f);
            }
        }
        else
        {
            SetParticleRate(whiteFlakesPS, 0f);
            SetParticleRate(brownFlakesPS, 0f);
        }
    }

    void SetParticleRate(ParticleSystem ps, float rate)
    {
        if (ps != null)
        {
            var emission = ps.emission;
            emission.rateOverTime = rate;
            if (!ps.isPlaying) ps.Play();
        }
    }

    void ForceStopParticle(ParticleSystem ps)
    {
        if (ps != null)
        {
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            ps.Play();
        }
    }

    void CheckCatHandMechanic()
    {
        if (!coconutTarget || !handTransform) return;
        float distance = Vector2.Distance(handTransform.position, coconutTarget.position);
        isOnBlade = (distance <= safeZoneRadius);
        if (isOnBlade && isDragging)
        {
            currentHoldTimer += Time.deltaTime;
            isTooRough = (currentHoldTimer > maxSafeHoldTime);
        }
    }

    void CheckPenaltyLogic()
    {
        if (isOnBlade && isTooRough)
        {
            if (!wasTooRoughLastFrame) AddDirtyStrike();
            wasTooRoughLastFrame = true;
        }
    }

    void AddDirtyStrike()
    {
        currentDirtyStrikes++;
        if (dirtyStrikeVisuals != null && currentDirtyStrikes <= dirtyStrikeVisuals.Length)
        {
            int index = currentDirtyStrikes - 1;
            if (dirtyStrikeVisuals[index]) dirtyStrikeVisuals[index].SetActive(true);
        }
        if (currentDirtyStrikes >= maxDirtyStrikes) GameOverFailed();
    }

    void GameOverFailed()
    {
        isGameFailed = true;
        isDragging = false;
        if (failPopupPanel) failPopupPanel.SetActive(true);
        if (restartButtonObject) restartButtonObject.SetActive(true);
        
        // ✅ [แทรก] หยุดเสียงขูดเมื่อแพ้
        if (grateSource != null) grateSource.Stop();
    }

    void UpdateUI()
    {
        if (circularProgressImage) circularProgressImage.fillAmount = Mathf.Clamp01(globalEffort / effortPerCoconut);
        if (tensionBarImage)
        {
            float ratio = Mathf.Clamp01(currentHoldTimer / maxSafeHoldTime);
            tensionBarImage.fillAmount = ratio;
            tensionBarImage.color = (ratio > 0.8f) ? Color.red : (ratio > 0.5f ? Color.yellow : Color.green);
        }
        if (statusText)
        {
            if (isGameFailed) return;
            if (isGameFinished) { statusText.text = "ภารกิจสำเร็จ!"; return; }
            statusText.text = $"ความเสียหาย: {currentDirtyStrikes}/{maxDirtyStrikes}";
        }
    }

    void MoveHandToMouse()
    {
        if (!handTransform) return;
        Vector3 targetPos = GetMouseWorldPos() + dragOffset;
        targetPos.z = 0;
        handTransform.position = Vector3.Lerp(handTransform.position, targetPos, Time.deltaTime * 20f);
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 p = Input.mousePosition;
        p.z = -mainCamera.transform.position.z;
        Vector3 w = mainCamera.ScreenToWorldPoint(p);
        w.z = 0;
        return w;
    }

    void UpdateCoconutVisuals()
    {
        if (coconutStages == null || !coconutRenderer) return;
        float percent = (globalEffort / effortPerCoconut) * 100f;
        foreach (var s in coconutStages) if (percent >= s.showAtPercent) coconutRenderer.sprite = s.coconutSprite;
    }

    void UpdateTrayFlakes()
    {
        if (trayFlakes == null) return;
        float percent = (globalEffort / effortPerCoconut) * 100f;
        foreach (var f in trayFlakes) if (f.flakeObject && percent >= f.appearAtPercent) f.flakeObject.SetActive(true);
    }

    void OnDrawGizmos() { if (coconutTarget) { Gizmos.color = Color.green; Gizmos.DrawWireSphere(coconutTarget.position, safeZoneRadius); } }
}