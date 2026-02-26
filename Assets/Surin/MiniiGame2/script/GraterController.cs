using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections; // ✅ เพิ่มเพื่อใช้ Coroutine

[System.Serializable]
public class GraterController : MonoBehaviour
{
    // ==========================================
    // [แทรกใหม่] Audio System - ลากใส่ใน Inspector
    // ==========================================
    [Header("--- Audio Settings ---")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource;
    public AudioSource grateSource; // สำหรับเสียงขูด (ติ๊ก Loop ใน Inspector)

    public AudioClip bgmClip;
    public AudioClip clickSfx;
    public AudioClip introVoice;
    public AudioClip retryVoice;
    public AudioClip grateSfx;
    // ==========================================

    [Header("--- Win System & Level Settings (เพิ่มใหม่) ---")]
    public GameObject winPanel;
    public Button nextLevelButton;
    public string nextSceneName = "MiniGame3Surin";
    public int provinceIndex = 4;

    [Header("UI Status")]
    public TextMeshProUGUI statusText;

    [Header("Progress Bar (วงกลม)")]
    public Image circularProgressImage;

    [Header("Speed Bar (แนวตั้ง)")]
    public Image speedBarImage;
    public GameObject speedBarWholeObject;

    [Header("Warning UI")]
    public GameObject warningMessageObject;

    [Header("Game Over Popup")]
    public GameObject failPopupPanel;
    public TextMeshProUGUI failPopupText;
    public GameObject restartButtonObject;

    [Header("Dialogue System")]
    public GameObject dialoguePanel; 
    public TextMeshProUGUI dialogueText; 

    [TextArea(2, 3)]
    public string[] introSentences; 

    [TextArea(2, 3)]
    public string retrySentence = "ยายเอามะพร้าวใหม่มาเปลี่ยนให้แล้ว ขูดใหม่เด้อ"; 

    private static bool isRetryRound = false;

    private List<string> currentDialogueQueue = new List<string>();
    private bool isDialogueActive = false;

    [Header("References")]
    public Transform handTransform;
    public CoconutProgress coconutProgressScript;
    public Transform bladeTarget;

    [Header("Fail System")]
    public GameObject[] dirtyStrikeVisuals;
    public int maxDirtyStrikes = 3;

    [Header("Tray Settings")]
    public FlakeStage[] trayFlakes;

    [Header("Blade Visuals")]
    public GameObject smallFlakesOnBlade;
    public GameObject largeFlakesOnBlade;

    [Header("Zone Settings")]
    public float safeZoneHeight = 0.5f;
    public float safeZoneWidth = 0.3f;

    [Header("Grating Settings")]
    public float minSpeedThreshold = 5f;
    public float effortPerCoconut = 100f;
    private int totalCoconutsToWin = 3;

    private float globalEffort = 0f;
    private Vector3 lastMousePos;
    private float verticalSpeed;
    private bool isOnBlade = false;
    private bool isTooRough = false;

    private float smoothDisplaySpeed = 0f;

    private bool isGameFinished = false;
    private bool isGameFailed = false;
    private bool isPausedByWarning = false;
    private bool isDragging = false;

    private int currentDirtyStrikes = 0;
    private bool wasTooRoughLastFrame = false;

    private Vector3 dragOffset;
    private Camera mainCamera;
    private int lastUpdatedStageIndex = 0;
    private Vector3 initialHandPosition;

    void Start()
    {
        mainCamera = Camera.main;

        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.SaveCurrentScene(SceneManager.GetActiveScene().name);
        }

        if (handTransform != null)
        {
            initialHandPosition = handTransform.position;
            initialHandPosition.z = 0;
            Animator anim = handTransform.GetComponent<Animator>();
            if (anim != null) Destroy(anim);
        }

        if (trayFlakes != null)
            foreach (var flake in trayFlakes) if (flake.flakeObject != null) flake.flakeObject.SetActive(false);

        if (dirtyStrikeVisuals != null)
            foreach (var dirty in dirtyStrikeVisuals) if (dirty != null) dirty.SetActive(false);

        if (failPopupPanel != null) failPopupPanel.SetActive(false);
        if (restartButtonObject != null) restartButtonObject.SetActive(false);
        if (warningMessageObject != null) warningMessageObject.SetActive(false);
        if (circularProgressImage != null) circularProgressImage.fillAmount = 0f;
        if (speedBarImage != null) speedBarImage.fillAmount = 0f;

        if (speedBarWholeObject != null) speedBarWholeObject.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
        lastUpdatedStageIndex = 0;
        if (coconutProgressScript != null) lastUpdatedStageIndex = coconutProgressScript.currentStageIndex;

        StartDialogueSequence();
    }

    void StartDialogueSequence()
    {
        currentDialogueQueue.Clear();
        AudioClip selectedVoice = null; // ไว้เก็บเสียงที่จะเล่น

        if (isRetryRound)
        {
            currentDialogueQueue.Add(retrySentence);
            selectedVoice = retryVoice; // เลือกเสียงพากย์ Retry
        }
        else
        {
            foreach (string sentence in introSentences)
            {
                currentDialogueQueue.Add(sentence);
            }
            selectedVoice = introVoice; // เลือกเสียงพากย์ Intro
        }

        // [แทรก] เล่นเสียงพากย์และรอเปิด BGM เมื่อเสียงจบ
        if (voiceSource != null && selectedVoice != null)
        {
            voiceSource.clip = selectedVoice;
            voiceSource.Play();
            StartCoroutine(WaitAndPlayBGM(selectedVoice.length));
        }
        else { PlayBGM(); }

        if (currentDialogueQueue.Count > 0)
        {
            isDialogueActive = true;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            ShowNextSentence();
        }
        else
        {
            EndDialogue(); 
        }
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
            if (dialogueText != null)
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
                // [แทรก] เสียงคลิกตอนเปลี่ยนบทพูด
                if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);
                ShowNextSentence();
            }
            return; 
        }

        if (isGameFinished || isGameFailed) 
        {
            // [แทรก] หยุดเสียงขูดถ้าเกมจบ
            if (grateSource != null && grateSource.isPlaying) grateSource.Stop();
            return;
        }

        int currentStage = coconutProgressScript != null ? coconutProgressScript.currentStageIndex : 0;
        if (currentStage > lastUpdatedStageIndex)
        {
            TriggerWarningState();
            lastUpdatedStageIndex = currentStage;
        }

        if (isPausedByWarning)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // [แทรก] เสียงคลิกปิด Warning
                if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);
                isPausedByWarning = false;
                if (warningMessageObject != null) warningMessageObject.SetActive(false);
                return;
            }
            else return;
        }

        CalculateSpeed();

        float realTimeSpeed = verticalSpeed / 20f;
        smoothDisplaySpeed = Mathf.Lerp(smoothDisplaySpeed, realTimeSpeed, Time.deltaTime * 10f);

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosWorld = GetMouseWorldPos();
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePosWorld);

            if (hitCollider != null && hitCollider.transform == handTransform)
            {
                isDragging = true;
                dragOffset = handTransform.position - mousePosWorld;
                dragOffset.z = 0;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            MoveHandToMouse();
            CheckGratingMechanic();

            if (isOnBlade && verticalSpeed > minSpeedThreshold && !isTooRough)
            {
                globalEffort += realTimeSpeed * Time.deltaTime;
                CheckProgression();
                UpdateTrayFlakes();

                // [แทรก] เล่นเสียงขูดมะพร้าว
                if (grateSource != null && !grateSource.isPlaying && grateSfx != null)
                {
                    grateSource.clip = grateSfx;
                    grateSource.Play();
                }
            }
            else 
            {
                // [แทรก] หยุดเสียงขูดถ้าไม่ได้ขูดจริง
                if (grateSource != null && grateSource.isPlaying) grateSource.Stop();
            }

            CheckPenaltyLogic();
        }
        else
        {
            ResetBladeVisuals();
            wasTooRoughLastFrame = false;
            verticalSpeed = 0f;
            // [แทรก] หยุดเสียงขูดเมื่อปล่อยมือ
            if (grateSource != null && grateSource.isPlaying) grateSource.Stop();
        }

        if (Input.GetMouseButtonUp(0)) isDragging = false;

        lastMousePos = Input.mousePosition;
        UpdateUI();
    }

    void TriggerWarningState()
    {
        isPausedByWarning = true;
        isDragging = false;
        if (warningMessageObject != null) warningMessageObject.SetActive(true);
        if (handTransform != null) handTransform.position = initialHandPosition;

        ResetBladeVisuals();
        verticalSpeed = 0;
        smoothDisplaySpeed = 0f;

        if (speedBarImage != null)
        {
            speedBarImage.fillAmount = 0f;
            speedBarImage.color = Color.green;
        }

        wasTooRoughLastFrame = false;
    }

    public void RestartGame()
    {
        // [แทรก] เสียงคลิกก่อน Restart
        if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);
        isRetryRound = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CheckPenaltyLogic()
    {
        if (isOnBlade && isTooRough)
        {
            if (!wasTooRoughLastFrame) AddDirtyStrike();
            wasTooRoughLastFrame = true;
        }
        else wasTooRoughLastFrame = false;
    }

    void AddDirtyStrike()
    {
        currentDirtyStrikes++;
        if (dirtyStrikeVisuals != null && currentDirtyStrikes <= dirtyStrikeVisuals.Length)
        {
            int index = currentDirtyStrikes - 1;
            if (dirtyStrikeVisuals[index] != null) dirtyStrikeVisuals[index].SetActive(true);
        }
        if (currentDirtyStrikes >= maxDirtyStrikes) GameOverFailed();
    }

    void GameOverFailed()
    {
        isGameFailed = true;
        isDragging = false;
        if (failPopupPanel != null)
        {
            failPopupPanel.SetActive(true);
            failPopupText.text = "คุณขูดมะพร้าวติดกะลาเยอะไปแล้ว\nให้ไปเริ่มขูดลูกใหม่";
        }
        if (restartButtonObject != null) restartButtonObject.SetActive(true);
        if (statusText != null) statusText.text = "";
    }

    void GameFinishedSuccess()
    {
        Debug.Log("🎉 ขูดมะพร้าวครบแล้ว! เย้!");
        if (winPanel != null) winPanel.SetActive(true);
        if (speedBarWholeObject != null) speedBarWholeObject.SetActive(false);
        if (handTransform != null) handTransform.gameObject.SetActive(false);

        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.PassLevel(provinceIndex);
            GameDataController.Instance.SaveCurrentScene(nextSceneName);
        }
    }

    public void GoToNextLevel()
    {
        // [แทรก] เสียงคลิกตอนไปด่านถัดไป
        if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);
        Debug.Log("กดปุ่มแล้ว");
        SceneManager.LoadScene(nextSceneName);
    }

    void CheckProgression()
    {
        float maxGlobalEffort = effortPerCoconut * totalCoconutsToWin;
        if (globalEffort >= maxGlobalEffort)
        {
            globalEffort = maxGlobalEffort;
            isGameFinished = true;
            if (coconutProgressScript != null && coconutProgressScript.currentStageIndex < totalCoconutsToWin)
                coconutProgressScript.AdvanceStage();
            GameFinishedSuccess();
            return;
        }

        int calculatedStage = Mathf.FloorToInt(globalEffort / effortPerCoconut);
        if (calculatedStage > coconutProgressScript.currentStageIndex)
        {
            if (coconutProgressScript != null) coconutProgressScript.AdvanceStage();
        }
    }

    void UpdateUI()
    {
        float totalMaxEffort = effortPerCoconut * totalCoconutsToWin;
        float progressFraction = Mathf.Clamp01(globalEffort / totalMaxEffort);
        if (circularProgressImage != null) circularProgressImage.fillAmount = progressFraction;

        if (speedBarWholeObject != null) speedBarWholeObject.SetActive(true);

        float currentSafeLimit = coconutProgressScript != null ? coconutProgressScript.GetCurrentSafeSpeed() : 100f;
        float smoothRatio = Mathf.Clamp01(smoothDisplaySpeed / currentSafeLimit);

        if (!isOnBlade) smoothRatio = 0f;

        if (speedBarImage != null)
        {
            speedBarImage.fillAmount = smoothRatio;
            if (smoothRatio >= 0.6f) speedBarImage.color = Color.red;
            else speedBarImage.color = Color.green;
        }

        if (statusText != null)
        {
            if (isGameFailed) return;
            if (isGameFinished) { statusText.text = "<size=150%><color=green><b>ภารกิจสำเร็จ!</b></color></size>\n100% Complete"; return; }

            string statusMsg = "";
            string warningMsg = "";
            int subStage = coconutProgressScript != null ? coconutProgressScript.currentStageIndex : 0;
            if (subStage > 0) warningMsg = "\n<color=orange>เนื้อมะพร้าวบางลง แล้วให้ระวัง อย่าขูดเร็ว!</color>";

            if (isOnBlade)
            {
                if (isTooRough) statusMsg = "<color=red>แรงเกินไป!</color>";
                else if (verticalSpeed > minSpeedThreshold) statusMsg = "<color=green>กำลังขูด...</color>";
                else statusMsg = "เร็วๆ หน่อย";
            }
            else statusMsg = "<color=yellow>พัก</color>";

            statusText.text = $"{warningMsg}\n";
        }
    }

    void UpdateTrayFlakes()
    {
        if (trayFlakes == null) return;
        float totalMaxEffort = effortPerCoconut * totalCoconutsToWin;
        float currentPercent = (globalEffort / totalMaxEffort) * 100f;

        foreach (var flake in trayFlakes)
        {
            if (flake.flakeObject != null && currentPercent >= flake.appearAtPercent)
                flake.flakeObject.SetActive(true);
        }
    }

    void CalculateSpeed()
    {
        float deltaY = Mathf.Abs(Input.mousePosition.y - lastMousePos.y);
        float deltaX = Mathf.Abs(Input.mousePosition.x - lastMousePos.x);
        if (deltaX > deltaY) verticalSpeed = 0;
        else verticalSpeed = deltaY / Time.deltaTime;
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePoint);
        worldPos.z = 0;
        return worldPos;
    }

    void MoveHandToMouse()
    {
        if (handTransform == null) return;
        Vector3 targetPos = GetMouseWorldPos() + dragOffset;
        targetPos.z = 0;
        handTransform.position = Vector3.Lerp(handTransform.position, targetPos, Time.deltaTime * 20f);
    }

    void CheckGratingMechanic()
    {
        if (bladeTarget == null || handTransform == null) return;
        float distY = Mathf.Abs(handTransform.position.y - bladeTarget.position.y);
        float distX = Mathf.Abs(handTransform.position.x - bladeTarget.position.x);
        isOnBlade = (distY <= safeZoneHeight && distX <= safeZoneWidth);

        float currentSafeLimit = coconutProgressScript != null ? coconutProgressScript.GetCurrentSafeSpeed() : 100f;
        isTooRough = (smoothDisplaySpeed > currentSafeLimit);
        UpdateBladeVisuals();
    }

    void UpdateBladeVisuals()
    {
        if (smallFlakesOnBlade) smallFlakesOnBlade.SetActive(false);
        if (largeFlakesOnBlade) largeFlakesOnBlade.SetActive(false);
        if (!isOnBlade || verticalSpeed <= minSpeedThreshold) return;
        if (!isTooRough)
        {
            if (verticalSpeed > minSpeedThreshold * 3) { if (largeFlakesOnBlade) largeFlakesOnBlade.SetActive(true); }
            else { if (smallFlakesOnBlade) smallFlakesOnBlade.SetActive(true); }
        }
    }

    void ResetBladeVisuals()
    {
        verticalSpeed = 0f;
        if (smallFlakesOnBlade) smallFlakesOnBlade.SetActive(false);
        if (largeFlakesOnBlade) largeFlakesOnBlade.SetActive(false);
    }

    void OnDrawGizmos()
    {
        if (bladeTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(bladeTarget.position, new Vector3(safeZoneWidth * 2, safeZoneHeight * 2, 0));
        }
    }
}