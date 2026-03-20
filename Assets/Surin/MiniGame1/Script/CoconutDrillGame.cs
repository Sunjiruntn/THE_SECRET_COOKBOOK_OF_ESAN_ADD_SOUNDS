using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // ✅ เพิ่มเพื่อใช้ Coroutine

public class CoconutDrillGame : MonoBehaviour
{
    [Header("Level Settings")]
    public int provinceIndex = 4; 
    public string nextSceneName = "MiniGame2Surin";

    // ==========================================
    // [แทรกใหม่] Audio System
    // ==========================================
    [Header("Audio Settings")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource;
    public AudioSource drillSource; // สำหรับเสียงเจาะ (ติ๊ก Loop ใน Inspector)

    public AudioClip bgmClip;
    public AudioClip clickSfx;
    public AudioClip introVoice;
    public AudioClip retryVoice;
    public AudioClip drillingSfx;
    // ==========================================

    [Header("UI References")]
    public Image heatFillImage;
    public Image decorationIcon;

    public GameObject gameOverPanel;
    public GameObject restartButton;

    [Header("Dialogue System")]
    public GameObject dialoguePanel; 
    public TextMeshProUGUI dialogueText; 

    [TextArea(2, 3)]
    public string[] introSentences; 

    [TextArea(2, 3)]
    public string retrySentence = "ยายเอากะลาใบใหม่มาเปลี่ยนให้แล้ว เจาะใหม่เด้อ"; 

    private static bool isRetryRound = false;
    private List<string> currentDialogueQueue = new List<string>();
    private bool isDialogueActive = false;

    [Header("Drill Settings")]
    public SpriteRenderer drillRenderer;
    public Sprite drillFrame1;
    public Sprite drillFrame2;
    public float animationSpeed = 0.1f;

    [Header("Drilling VFX")]
    public ParticleSystem dustParticles;

    [Header("Damage VFX")]
    public float timeToCrack = 2.0f;

    [Header("Hole Levels")]
    public List<SpriteRenderer> allHoles;

    [Header("Drill Movement")]
    public float startOffset = 1.5f;
    public float depthOffset = -1.5f;

    [Header("Game Logic")]
    public float heatUpSpeed = 50f;
    public float coolDownSpeed = 30f;
    public float drillSpeed = 10f;
    public float maxHeat = 100f;
    public float targetDepth = 100f;

    private int currentHoleIndex = 0;
    private float currentHeat = 0f;
    private float currentDepth = 0f;
    private float animTimer;
    private bool isFrame1 = true;
    private bool isFinishedAll = false;
    private bool isMovingToNext = false;
    private bool isGameOver = false;

    private float currentDrillStartY;
    private float currentDrillEndY;
    private GameObject currentHoleRim;
    private GameObject currentHoleCrack;
    private float redZoneTimer = 0f;

    // ==========================================
    // [แก้ไขเพิ่มเติม] หยุดเสียงทั้งหมดเมื่อเปลี่ยนฉาก
    // ==========================================
    private void OnDestroy()
    {
        StopAllSounds();
    }

    private void StopAllSounds()
    {
        if (bgmSource != null) bgmSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
        if (voiceSource != null) voiceSource.Stop();
        if (drillSource != null) drillSource.Stop();
    }
    // ==========================================

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);

        foreach (var hole in allHoles)
        {
            Color c = hole.color;
            c.a = 0f;
            hole.color = c;

            Transform rimTransform = hole.transform.Find("Hole_rim");
            if (rimTransform != null) rimTransform.gameObject.SetActive(false);

            Transform crackTransform = hole.transform.Find("Hole_Crack");
            if (crackTransform != null) crackTransform.gameObject.SetActive(false);
        }

        PrepareForHole(0);
        StartDialogueSequence();

        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.SaveCurrentScene(SceneManager.GetActiveScene().name);
        }
    }

    void StartDialogueSequence()
    {
        currentDialogueQueue.Clear();
        AudioClip selectedVoice = null; // ไว้เก็บเสียงที่จะเล่น

        if (isRetryRound)
        {
            currentDialogueQueue.Add(retrySentence);
            selectedVoice = retryVoice; // เลือกเสียง Retry
        }
        else
        {
            foreach (string sentence in introSentences)
            {
                currentDialogueQueue.Add(sentence);
            }
            selectedVoice = introVoice; // เลือกเสียง Intro
        }

        // [แทรก] เล่นเสียงพากย์และรอเปิด BGM
        if (voiceSource != null && selectedVoice != null)
        {
            voiceSource.clip = selectedVoice;
            voiceSource.Play();
            StartCoroutine(WaitAndPlayBGM(selectedVoice.length));
        }
        else { PlayBGM(); }

        isDialogueActive = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        ShowNextSentence();
    }

    // [แทรก] Coroutine รอเสียงพากย์จบ
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

    void ShowNextSentence()
    {
        if (currentDialogueQueue.Count > 0)
        {
            dialogueText.text = currentDialogueQueue[0];
            currentDialogueQueue.RemoveAt(0);
        }
        else
        {
            EndDialogue();
        }
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
                // [แทรก] เสียงคลิกตอนคุย
                if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);
                ShowNextSentence();
            }
            return;
        }

        if (isFinishedAll || isMovingToNext || isGameOver) 
        {
            // [แทรก] หยุดเสียงเจาะถ้าเกมจบ
            if (drillSource != null && drillSource.isPlaying) drillSource.Stop();
            return;
        }

        if (Input.GetMouseButton(0))
        {
            ProcessDrilling();
            // [แทรก] เริ่มเสียงเจาะ
            if (drillSource != null && !drillSource.isPlaying && drillingSfx != null)
            {
                drillSource.clip = drillingSfx;
                drillSource.Play();
            }
        }
        else
        {
            ProcessCooling();
            // [แทรก] หยุดเสียงเจาะเมื่อปล่อยมือ
            if (drillSource != null && drillSource.isPlaying) drillSource.Stop();
        }

        UpdateHeatUI();
    }

    public void RestartGame()
    {
        // [แทรก] เสียงปุ่มกดตอน Restart
        if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);
        isRetryRound = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void PrepareForHole(int index)
    {
        currentHoleIndex = index;
        currentHeat = 0;
        currentDepth = 0;
        redZoneTimer = 0f;
        if (index >= allHoles.Count)
        {
            isFinishedAll = true;
            drillRenderer.gameObject.SetActive(false);
            StartCoroutine(WinGameRoutine());
            return;
        }

        SpriteRenderer currentHole = allHoles[index];
        Vector3 holePos = currentHole.transform.position;

        Transform rimTrans = currentHole.transform.Find("Hole_rim");
        currentHoleRim = (rimTrans != null) ? rimTrans.gameObject : null;
        if (currentHoleRim != null) currentHoleRim.SetActive(false);

        Transform crackTrans = currentHole.transform.Find("Hole_Crack");
        currentHoleCrack = (crackTrans != null) ? crackTrans.gameObject : null;

        Vector3 drillPos = drillRenderer.transform.position;
        drillPos.x = holePos.x;
        drillRenderer.transform.position = drillPos;

        Vector3 dustPos = dustParticles.transform.position;
        dustPos.x = holePos.x;
        dustPos.y = holePos.y;
        dustParticles.transform.position = dustPos;

        currentDrillStartY = holePos.y + startOffset;
        currentDrillEndY = holePos.y - depthOffset;

        UpdateDrillPosition();
    }

    void ProcessDrilling()
    {
        currentHeat += heatUpSpeed * Time.deltaTime;

        if (currentHeat < maxHeat)
        {
            currentDepth += drillSpeed * Time.deltaTime;
        }

        animTimer += Time.deltaTime;
        if (animTimer >= animationSpeed)
        {
            animTimer = 0;
            isFrame1 = !isFrame1;
            drillRenderer.sprite = isFrame1 ? drillFrame1 : drillFrame2;
        }

        if (!dustParticles.isPlaying) dustParticles.Play();
        if (currentHoleRim != null) currentHoleRim.SetActive(true);
    }

    void ProcessCooling()
    {
        currentHeat -= coolDownSpeed * Time.deltaTime;
        drillRenderer.sprite = drillFrame1;
        dustParticles.Stop();
        redZoneTimer = 0f;
    }

    void UpdateHeatUI()
    {
        currentHeat = Mathf.Clamp(currentHeat, 0, maxHeat);
        currentDepth = Mathf.Clamp(currentDepth, 0, targetDepth);

        float heatRatio = currentHeat / maxHeat;
        heatFillImage.fillAmount = heatRatio;

        if (heatRatio < 0.5f)
        {
            heatFillImage.color = Color.Lerp(Color.green, Color.yellow, heatRatio * 2);
            SetDustRate(10f);
            if (decorationIcon) decorationIcon.gameObject.SetActive(false);
            redZoneTimer = 0f;
        }
        else if (heatRatio < 0.85f)
        {
            heatFillImage.color = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), (heatRatio - 0.5f) * 3);
            SetDustRate(15f);
            if (decorationIcon) decorationIcon.gameObject.SetActive(false);
            redZoneTimer = 0f;
        }
        else
        {
            heatFillImage.color = Color.red;
            SetDustRate(25f);
            if (decorationIcon) decorationIcon.gameObject.SetActive(true);

            redZoneTimer += Time.deltaTime;

            if (redZoneTimer >= timeToCrack)
            {
                if (currentHoleCrack != null) currentHoleCrack.SetActive(true);
                TriggerGameOver();
            }
        }

        UpdateDrillPosition();

        if (currentDepth >= targetDepth)
        {
            FinishCurrentHole();
        }
    }

    void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        // [แทรก] หยุดเสียงเจาะเมื่อแพ้
        if (drillSource != null) drillSource.Stop();

        dustParticles.Stop();
        drillRenderer.sprite = drillFrame1;
        if (currentHoleRim != null) currentHoleRim.SetActive(false);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (restartButton != null) restartButton.SetActive(true);
    }

    void UpdateDrillPosition()
    {
        float depthRatio = currentDepth / targetDepth;
        Vector3 pos = drillRenderer.transform.position;
        pos.y = Mathf.Lerp(currentDrillStartY, currentDrillEndY, depthRatio);
        drillRenderer.transform.position = pos;

        SpriteRenderer currentHoleSprite = allHoles[currentHoleIndex];
        Color holeColor = currentHoleSprite.color;
        holeColor.a = depthRatio;
        currentHoleSprite.color = holeColor;
    }

    void FinishCurrentHole()
    {
        // [แทรก] หยุดเสียงเจาะเมื่อเจาะเสร็จ 1 รู
        if (drillSource != null) drillSource.Stop();

        dustParticles.Stop();
        drillRenderer.sprite = drillFrame1;
        if (currentHoleRim != null) currentHoleRim.SetActive(false);

        isMovingToNext = true;
        Invoke("GoToNextHole", 1.0f);
    }

    void GoToNextHole()
    {
        isMovingToNext = false;
        PrepareForHole(currentHoleIndex + 1);
    }

    void SetDustRate(float rate)
    {
        if (dustParticles.emission.rateOverTime.constant != rate)
        {
            var emission = dustParticles.emission;
            emission.rateOverTime = rate;
        }
    }

    IEnumerator WinGameRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        // [แทรก] หยุดเสียงทั้งหมดก่อนโหลดฉากใหม่
        StopAllSounds();

        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.PassLevel(provinceIndex);
            GameDataController.Instance.SaveCurrentScene(nextSceneName);
        }

        SceneManager.LoadScene(nextSceneName);
    }
}