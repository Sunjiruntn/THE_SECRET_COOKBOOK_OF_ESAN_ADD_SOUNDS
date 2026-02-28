using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BlenderController : MonoBehaviour
{
    [Header("--- Win System & Level Settings (เพิ่มใหม่) ---")]
    public GameObject winPanel;       
    public Button nextLevelButton;    
    public string nextSceneName = "Mukdahan_MiniGame2"; 
    public int provinceIndex = 3;

    // ==========================================================
    // NEW: AUDIO SYSTEM (ส่วนที่เพิ่มเข้าไป)
    // ==========================================================
    [Header("--- Audio Settings ---")]
    public AudioSource bgmSource;        // สำหรับเพลงพื้นหลัง (Loop)
    public AudioSource sfxSource;        // สำหรับเสียงคลิก/เสียงปั่น
    public AudioSource voiceSource;      // สำหรับเสียงแนะนำ (Intro)
    
    [Space(10)]
    public AudioClip bgmClip;            // เพลงพื้นหลัง
    public AudioClip clickSfx;           // เสียงกดปุ่มทั่วไป
    public AudioClip blenderSfx;         // เสียงเครื่องปั่น (Loop)
    public AudioClip winSfx;             // เสียงตอนชนะ
    public AudioClip[] introVoices;      // เสียงพากย์แนะนำ (ใส่ให้ครบตามจำนวนประโยค Intro)

    // ==========================================================
    // 1. SETTINGS & VARIABLES
    // ==========================================================

    [Header("--- UI Intro (ระบบแนะนำแบบ Click Anywhere) ---")]
    public GameObject introBlockerPanel; 
    [TextArea(3, 5)]
    public string[] introSentences;      
    private int currentSentenceIndex = 0;
    private bool isIntroActive = false;  

    [Header("--- UI Progress (ระบบเปอร์เซ็นต์) ---")]
    public Slider progressSlider;      
    public float currentProgress = 0f; 
    public float blendingRate = 15f;   

    [Header("--- Blender Settings ---")]
    public int currentSpeed = 0;
    public Transform chunkSpawnPoint;  

    [Header("--- Heat System (ความร้อน) ---")]
    public Image heatGaugeImage;       
    public float currentHeat = 0f;
    public float maxHeat = 100f;
    private float overheatTimer = 0f;

    [Header("--- Lid Settings (ฝาปิด) ---")]
    public GameObject lidObject;
    public Transform lidOpenPos;       
    public Transform lidClosedPos;     
    public float lidMoveSpeed = 5.0f;
    public bool isLidClosed = false;
    private Coroutine currentLidCoroutine;

    [Header("--- Objects & Prefabs ---")]
    public GameObject pandanStaticObj;
    public GameObject anchanStaticObj;
    public GameObject pandanChunkPrefab;
    public GameObject anchanChunkPrefab;

    [Header("--- Liquid Stages (ภาพน้ำ) ---")]
    public SpriteRenderer liquidRenderer;
    public Sprite stage1_WaterOnly;
    public Sprite stage2_PandanMix;
    public Sprite stage3_PandanFine;
    public Sprite stage4_AnchanMix;

    [Header("--- UI Text ---")]
    public TextMeshProUGUI speedDisplayText;
    public TextMeshProUGUI warningText; 

    [Header("--- Shake & Anim ---")]
    public float shakeAmount = 0.05f;
    public Animator animator;
    private Vector3 initialLiquidPos;

    private bool hasPandan = false;
    private bool hasWater = false;
    private bool hasAnchan = false;
    private bool isPandanBlended = false;
    private bool isAnchanSpawned = false;
    private bool isAnchanFinished = false;
    private bool isProcessRunning = false;
    private List<GameObject> activeChunks = new List<GameObject>();

    // ==========================================================
    // 2. START & UPDATE
    // ==========================================================

    void Start()
    {
        if (liquidRenderer != null) initialLiquidPos = liquidRenderer.transform.localPosition;
        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.SaveCurrentScene(SceneManager.GetActiveScene().name);
        }
        
        ResetGame();
        StartIntro();
    }

    void Update()
    {
        if (isIntroActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // เล่นเสียงคลิกตอนกดข้าม Intro
                if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);
                AdvanceIntro(); 
            }
            return; 
        }

        if (currentHeat >= maxHeat * 0.8f)
        {
            overheatTimer += Time.deltaTime;
            if (overheatTimer > 2.0f)
            {
                ShowWarning("เครื่องร้อนเกินไป! เริ่มใหม่ค่ะ");
                Invoke("ResetGame", 1.5f);
                return;
            }
        }
        else
        {
            overheatTimer = 0f;
        }

        if (currentSpeed > 0)
        {
            currentHeat += (currentSpeed * 10f) * Time.deltaTime; 

            // จัดการเสียงปั่นตามระดับความเร็ว
            if (sfxSource && blenderSfx)
            {
                if (!sfxSource.isPlaying) { sfxSource.clip = blenderSfx; sfxSource.loop = true; sfxSource.Play(); }
                sfxSource.pitch = 0.8f + (currentSpeed * 0.2f); // ปั่นแรงขึ้น เสียงจะแหลมขึ้น
            }

            if (hasWater && liquidRenderer.gameObject.activeSelf)
            {
                float shakeY = Mathf.Sin(Time.time * (currentSpeed * 20)) * shakeAmount;
                liquidRenderer.transform.localPosition = initialLiquidPos + new Vector3(0, shakeY, 0);
                ShakeChunks();
            }
        }
        else
        {
            // หยุดเสียงปั่นเมื่อ Speed เป็น 0
            if (sfxSource && sfxSource.clip == blenderSfx) sfxSource.Stop();

            currentHeat -= 15f * Time.deltaTime;
            if (liquidRenderer != null) liquidRenderer.transform.localPosition = initialLiquidPos;
            if (animator != null) animator.SetBool("IsBlending", false);
        }

        currentHeat = Mathf.Clamp(currentHeat, 0, maxHeat);
        if (heatGaugeImage != null)
        {
            heatGaugeImage.fillAmount = currentHeat / maxHeat;
            heatGaugeImage.color = (heatGaugeImage.fillAmount > 0.8f) ? Color.red : Color.green;
        }

        if (progressSlider != null)
        {
            progressSlider.value = currentProgress / 100f;
        }
    }

    // ==========================================================
    // 3. INTRO SYSTEM (Click Anywhere)
    // ==========================================================

    void StartIntro()
    {
        if (introSentences.Length > 0)
        {
            isIntroActive = true;
            currentSentenceIndex = 0;
            if (introBlockerPanel != null) introBlockerPanel.SetActive(true);
            
            // หยุด BGM ไว้ก่อนจนกว่าจะพูดจบ
            if (bgmSource) bgmSource.Stop();

            ShowCurrentIntro();
        }
    }

    void ShowCurrentIntro()
    {
        if (warningText != null && currentSentenceIndex < introSentences.Length)
        {
            warningText.text = introSentences[currentSentenceIndex];
            
            // เล่นเสียงพากย์ตามลำดับ
            if (voiceSource && introVoices.Length > currentSentenceIndex)
            {
                voiceSource.Stop();
                voiceSource.PlayOneShot(introVoices[currentSentenceIndex]);
            }
        }
    }

    void AdvanceIntro()
    {
        currentSentenceIndex++;

        if (currentSentenceIndex >= introSentences.Length)
        {
            EndIntro();
        }
        else
        {
            ShowCurrentIntro();
        }
    }

    void EndIntro()
    {
        isIntroActive = false;
        if (introBlockerPanel != null) introBlockerPanel.SetActive(false);

        // เริ่มเล่น BGM เมื่อจบเสียงแนะนำ
        if (bgmSource && bgmClip)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        ShowWarning("เริ่มขั้นตอนแรก: ใส่ใบเตยลงไป");
    }

    // ==========================================================
    // 4. MASTER RESET (รีเซ็ตเกม)
    // ==========================================================
    public void ResetGame()
    {
        StopAllCoroutines();
        // หยุดเสียง SFX ทั้งหมดตอนรีเซ็ต
        if (sfxSource) sfxSource.Stop();

        if (winPanel != null) winPanel.SetActive(false);
        currentSpeed = 0;
        currentHeat = 0;
        overheatTimer = 0;
        currentProgress = 0f;
        if (progressSlider != null) progressSlider.value = 0f;

        hasPandan = false; hasWater = false; hasAnchan = false;
        isPandanBlended = false; isAnchanSpawned = false; isAnchanFinished = false; isProcessRunning = false;

        foreach (var chunk in activeChunks) { if (chunk != null) Destroy(chunk); }
        activeChunks.Clear();

        if (pandanStaticObj != null) pandanStaticObj.SetActive(false);
        if (anchanStaticObj != null) anchanStaticObj.SetActive(false);
        if (liquidRenderer != null) liquidRenderer.gameObject.SetActive(false);
        if (animator != null) animator.SetBool("IsBlending", false);

        isLidClosed = false;
        if (lidObject != null && lidOpenPos != null)
        {
            lidObject.SetActive(true);
            lidObject.transform.position = lidOpenPos.position;
            SpriteRenderer sr = lidObject.GetComponent<SpriteRenderer>();
            if (sr != null) { Color c = sr.color; c.a = 0.5f; sr.color = c; }
        }

        UpdateSpeedUI();
        if (!isIntroActive) ShowWarning("เริ่มขั้นตอนแรก: ใส่ใบเตยลงไป");
    }

    void WinGame()
    {
        if (winPanel != null) winPanel.SetActive(true);
        
        // เล่นเสียงชนะ และเบาเพลง BGM ลง
        if (bgmSource) bgmSource.volume = 0.3f;
        if (sfxSource && winSfx) sfxSource.PlayOneShot(winSfx);

        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.PassLevel(provinceIndex);
            GameDataController.Instance.SaveGame();
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
        if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);
        SceneManager.LoadScene(nextSceneName);
    }

    // ==========================================================
    // 5. BUTTON FUNCTIONS
    // ==========================================================

    public void OnClick_AddPandan()
    {
        if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);
        if (hasWater || isLidClosed) { ResetGame(); return; }
        hasPandan = true;
        if (pandanStaticObj != null) pandanStaticObj.SetActive(true);
        ShowWarning("ถัดไป: ใส่น้ำลงไป");
    }

    public void OnClick_AddWater()
    {
        if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);
        if (!hasPandan || isLidClosed) { ResetGame(); return; }
        hasWater = true;
        if (liquidRenderer != null)
        {
            liquidRenderer.gameObject.SetActive(true);
            liquidRenderer.sprite = stage1_WaterOnly;
        }
        ShowWarning("ถัดไป: ปิดฝาเครื่องปั่น");
    }

    public void OnClick_Lid()
    {
        if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);
        SetLidState(!isLidClosed);
    }

    public void OnClick_AddAnchan()
    {
        if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);
        if (!isPandanBlended) { ShowWarning("ต้องปั่นใบเตยให้เสร็จก่อน!"); return; }
        if (isLidClosed) { ResetGame(); return; }

        hasAnchan = true;
        if (anchanStaticObj != null) anchanStaticObj.SetActive(true);
        currentProgress = 0f;
        if (progressSlider != null) progressSlider.value = 0f;
        ShowWarning("ปิดฝา แล้วเริ่มปั่นได้เลย");
    }

    public void OnClick_SpeedButton()
    {
        if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);

        if (!isLidClosed) { ShowWarning("ปิดฝาเครื่องปั่นก่อน!"); return; }

        if (currentSpeed == 0)
        {
            if (!isPandanBlended)
            {
                if (!hasPandan) { ShowWarning("ใส่ใบเตยก่อนกดปั่น!"); return; }
                if (!hasWater) { ShowWarning("ใส่น้ำเปล่าก่อนกดปั่น!"); return; }
            }
            else if (!isAnchanFinished)
            {
                if (!hasAnchan) { ShowWarning("ใส่อัญชันก่อนกดปั่น!"); return; }
            }
        }

        currentSpeed++;
        if (currentSpeed > 3) currentSpeed = 0;
        UpdateSpeedUI();

        if (currentSpeed == 1 && !isProcessRunning)
        {
            if (hasPandan && hasWater && !isPandanBlended && !hasAnchan)
            {
                StartCoroutine(PandanBlendingSequence());
            }
            else if (hasAnchan && isPandanBlended && !isAnchanFinished)
            {
                StartCoroutine(AnchanBlendingSequence());
            }
        }
    }

    // ==========================================================
    // 6. BLENDING SEQUENCES & HELPERS (Logic เดิม ไม่แก้ไข)
    // ==========================================================

    IEnumerator PandanBlendingSequence()
    {
        isProcessRunning = true;
        if (animator != null) animator.SetBool("IsBlending", true);
        if (pandanStaticObj != null) pandanStaticObj.SetActive(false);
        if (activeChunks.Count == 0) SpawnChunks(pandanChunkPrefab, 10, "Pandan_Big");

        while (currentProgress < 100f)
        {
            if (currentSpeed > 0)
            {
                currentProgress += blendingRate * Time.deltaTime;
                if (currentProgress < 50f)
                {
                    if (liquidRenderer.sprite != stage1_WaterOnly) liquidRenderer.sprite = stage1_WaterOnly;
                }
                else if (currentProgress >= 50f && currentProgress < 90f)
                {
                    if (liquidRenderer.sprite != stage2_PandanMix)
                    {
                        liquidRenderer.sprite = stage2_PandanMix;
                        ResizeActiveChunks(0.6f);
                    }
                }
                else if (currentProgress >= 90f)
                {
                    if (liquidRenderer.sprite != stage3_PandanFine)
                    {
                        liquidRenderer.sprite = stage3_PandanFine;
                        ResizeActiveChunks(0.3f);
                    }
                }
            }
            yield return null;
        }

        currentProgress = 100f;
        if (progressSlider != null) progressSlider.value = 1f; 
        isPandanBlended = true;
        isProcessRunning = false;
        ShowWarning("ใบเตยละเอียดแล้ว! เปิดฝาเพื่อใส่อัญชัน");
    }

    IEnumerator AnchanBlendingSequence()
    {
        isProcessRunning = true;
        if (animator != null) animator.SetBool("IsBlending", true);
        currentProgress = 0f;
        if (progressSlider != null) progressSlider.value = 0f;

        if (!isAnchanSpawned)
        {
            if (anchanStaticObj != null) anchanStaticObj.SetActive(false);
            SpawnChunks(anchanChunkPrefab, 5, "Anchan");
            isAnchanSpawned = true;
        }

        while (currentProgress < 100f)
        {
            if (currentSpeed > 0)
            {
                currentProgress += blendingRate * Time.deltaTime;
            }
            yield return null;
        }

        currentProgress = 100f;
        if (progressSlider != null) progressSlider.value = 1f;
        if (liquidRenderer != null) liquidRenderer.sprite = stage4_AnchanMix;
        isAnchanFinished = true;
        isProcessRunning = false;
        ResizeActiveChunks(0f); 
        
        currentSpeed = 0;
        UpdateSpeedUI();
        if (animator != null) animator.SetBool("IsBlending", false);

        ShowWarning("ปั่นเสร็จเรียบร้อย! ได้น้ำใบเตยสีสวยงาม");
        yield return new WaitForSeconds(0.5f);
        WinGame();
    }

    void SetLidState(bool closed)
    {
        isLidClosed = closed;
        if (currentLidCoroutine != null) StopCoroutine(currentLidCoroutine);

        if (closed)
        {
            currentLidCoroutine = StartCoroutine(AnimateCloseLid());
            ShowWarning("พร้อมปั่นแล้ว!");
        }
        else
        {
            currentLidCoroutine = StartCoroutine(AnimateOpenLid());
            ShowWarning("ฝาเปิดอยู่");
        }
    }

    IEnumerator AnimateCloseLid()
    {
        if (lidObject != null)
        {
            lidObject.SetActive(true);
            SpriteRenderer sr = lidObject.GetComponent<SpriteRenderer>();
            if (sr != null) { Color c = sr.color; c.a = 1f; sr.color = c; } 

            while (Vector3.Distance(lidObject.transform.position, lidClosedPos.position) > 0.01f)
            {
                lidObject.transform.position = Vector3.MoveTowards(lidObject.transform.position, lidClosedPos.position, lidMoveSpeed * Time.deltaTime);
                yield return null;
            }
            lidObject.transform.position = lidClosedPos.position;
        }
    }

    IEnumerator AnimateOpenLid()
    {
        if (lidObject != null && lidOpenPos != null)
        {
            SpriteRenderer lidSprite = lidObject.GetComponent<SpriteRenderer>();
            Color startColor = lidSprite != null ? lidSprite.color : Color.white;
            float totalDist = Vector3.Distance(lidOpenPos.position, lidClosedPos.position);

            while (Vector3.Distance(lidObject.transform.position, lidOpenPos.position) > 0.01f)
            {
                lidObject.transform.position = Vector3.MoveTowards(lidObject.transform.position, lidOpenPos.position, lidMoveSpeed * Time.deltaTime);
                if (lidSprite != null)
                {
                    float currentDist = Vector3.Distance(lidObject.transform.position, lidClosedPos.position);
                    float alpha = 1 - (currentDist / totalDist);
                    lidSprite.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                }
                yield return null;
            }
            lidObject.transform.position = lidOpenPos.position;
        }
    }

    void SpawnChunks(GameObject prefab, int count, string nameID)
    {
        if (prefab == null) return;
        for (int i = 0; i < count; i++)
        {
            Vector3 centerPos = (chunkSpawnPoint != null) ? chunkSpawnPoint.position : transform.position;
            Vector3 randomPos = centerPos + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
            GameObject chunk = Instantiate(prefab, randomPos, Quaternion.identity);
            chunk.name = nameID + "_Chunk";
            if (chunk.GetComponent<Rigidbody2D>() == null)
            {
                Rigidbody2D rb = chunk.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1f;
            }
            if (chunk.GetComponent<CircleCollider2D>() == null) chunk.AddComponent<CircleCollider2D>();
            chunk.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            chunk.transform.SetParent(null);
            activeChunks.Add(chunk);
        }
    }

    void ShakeChunks()
    {
        foreach (var chunk in activeChunks)
        {
            if (chunk != null)
            {
                Rigidbody2D rb = chunk.GetComponent<Rigidbody2D>();
                if (rb != null) rb.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * (currentSpeed * 50f));
            }
        }
    }

    void ResizeActiveChunks(float targetScale)
    {
        foreach (GameObject chunk in activeChunks)
        {
            if (chunk != null) chunk.transform.localScale = Vector3.Lerp(chunk.transform.localScale, new Vector3(targetScale, targetScale, targetScale), 0.5f);
        }
    }

    void ShowWarning(string msg)
    {
        if (warningText != null) warningText.text = msg;
    }

    void UpdateSpeedUI()
    {
        if (speedDisplayText != null) speedDisplayText.text = "ระดับความเร็ว " + currentSpeed.ToString();
    }
}