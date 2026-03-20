using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CookingGameManager : MonoBehaviour
{
    // ==========================================
    // [แทรกใหม่] Audio System Settings
    // ==========================================
    [Header("--- Audio Settings (เพิ่มใหม่) ---")]
    public AudioSource bgmSource;        // สำหรับเพลงพื้นหลัง
    public AudioSource sfxSource;        // สำหรับเสียงปุ่ม/คลิก
    public AudioSource voiceSource;      // สำหรับเสียงพากย์แนะนำ

    public AudioClip bgmClip;
    public AudioClip clickSfx;
    public AudioClip introVoice;         // เสียงพากย์รวมทั้งหมด (หรือประโยคแรก)
    // ==========================================

    [Header("--- Win System & Level Settings (เพิ่มใหม่) ---")]
    public GameObject winPanel;
    public Button backToMapButton;
    public string nextSceneName = "MapSelect";
    public int provinceIndex = 4;

    public static CookingGameManager Instance;
    public GameObject clickableLidOnTable;

    [Header("UI & Objects")]
    public TextMeshProUGUI instructionText;
    public BowlController bowl;
    public SteamingController steamer;

    [Header("Game Logic")]
    public IngredientType currentRequiredIngredient = IngredientType.None;
    public bool canPlayerClick = false;

    private bool isStepComplete = false;

    void Awake() { Instance = this; }

    void Start()
    {
        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.SaveCurrentScene(SceneManager.GetActiveScene().name);
        }

        if (winPanel != null) winPanel.SetActive(false);
        // ==========================================
        // [เพิ่มใหม่] เช็คว่าเป็นโหมดสอบหรือไม่ (ถ้าสอบให้ข้าม Intro)
        // ==========================================
        bool isExam = false;
        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            isExam = true;

            // สั่งปิดข้อความคำสอน/คำใบ้ทั้งหมด ทันทีที่รู้ว่าเป็นการสอบ!
            if (instructionText != null)
            {
                instructionText.gameObject.SetActive(false);
            }
        }

        StartGame(isExam);
    }

    public void StartGame(bool skipIntro)
    {
        if (clickableLidOnTable) clickableLidOnTable.SetActive(false);
        StopAllCoroutines();
        if (bowl != null) bowl.ResetBowl();
        if (steamer != null) steamer.ResetSteamer();

        // [แทรก] จัดการเรื่องเสียงก่อนเริ่ม Flow
        if (!skipIntro)
        {
            StartCoroutine(PlayIntroAndThenBGM());
        }
        else
        {
            PlayBGM();
        }

        StartCoroutine(CoPlayKhanomNielFlow(skipIntro));
    }

    // [แทรกใหม่] Coroutine เล่นเสียงแนะนำจนจบแล้วค่อยเปิดเพลงคลอ
    IEnumerator PlayIntroAndThenBGM()
    {
        if (voiceSource != null && introVoice != null)
        {
            voiceSource.clip = introVoice;
            voiceSource.Play();
            yield return new WaitForSeconds(introVoice.length);
        }
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

    // --- ส่วนที่เพิ่มเพื่อแก้ปัญหาเสียงซ้อน ---
    public void StopAllAudio()
    {
        if (bgmSource != null) bgmSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
        if (voiceSource != null) voiceSource.Stop();
    }

    // หยุดเสียงเมื่อ Script ถูกทำลาย (เช่น ตอนเปลี่ยนฉาก)
    void OnDestroy()
    {
        StopAllAudio();
    }
    // ---------------------------------------

    public bool CheckIngredientOnly(IngredientType typeToCheck)
    {
        if (!canPlayerClick || currentRequiredIngredient == IngredientType.None) return false;

        if (typeToCheck == currentRequiredIngredient)
        {
            return true;
        }
        else
        {
            if (TestGameManager.Instance != null)
            {
                TestGameManager.Instance.RecordMistake();
            }

            StartCoroutine(WrongAndRestartRoutine());
            return false;
        }
    }

    public void CompleteStep(IngredientType finishedIngredient)
    {
        if (bowl != null) bowl.UpdateBowlVisual(finishedIngredient);
        isStepComplete = true;
        currentRequiredIngredient = IngredientType.None;
        
        if (TestGameManager.Instance != null)
        {
            TestGameManager.Instance.RecordSuccess();
        }
    }

    IEnumerator WrongAndRestartRoutine()
    {
        canPlayerClick = false;
        instructionText.text = "ผิดอันแล้วนาง! เอาใหม่ๆ";
        yield return new WaitForSeconds(2.0f);
        StartGame(true);
    }

    IEnumerator CoPlayKhanomNielFlow(bool skipIntro)
    {
        if (!skipIntro)
        {
            canPlayerClick = false;
            instructionText.text = "กลับมาที่ครัวของเฮา เฮาสิมาเริ่มทำนมเนียลกันเด้อ";
            yield return null;
            yield return new WaitUntil(() =>
            {
                bool clicked = Input.GetMouseButtonDown(0);
                if (clicked && sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);
                return clicked;
            });

            instructionText.text = "โดยวัตถุดิบจะมีแป้งข้าวเหนียว น้ำตาลทรายแดงหรือน้ำตาลอ้อยบดผง เกลือ มะพร้าวแก่ขูดขุย\nและมะพร้าวทึกทึนขูดหยาบที่ลูกขูดมานั่นเอง";
            yield return null;
            yield return new WaitUntil(() =>
            {
                bool clicked = Input.GetMouseButtonDown(0);
                if (clicked && sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);
                return clicked;
            });

            instructionText.text = "แม่เตรียมหม้อไว้แล้วโดยน้ำในหม้อเป็นน้ำมะพร้าว\nและใส่ใบเตยลงใบเพื่อเพิ่มความหอมให้กับขนม";
            yield return null;
            yield return new WaitUntil(() =>
            {
                bool clicked = Input.GetMouseButtonDown(0);
                if (clicked && sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);
                return clicked;
            });

            instructionText.text = "แล้วก็เอาเนียลหรือกะลามะพร้าวที่ลูกเจาะรู\nมาวางไว้ปากหม้อให้แล้วเด้อ";
            yield return null;
            yield return new WaitUntil(() =>
            {
                bool clicked = Input.GetMouseButtonDown(0);
                if (clicked && sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);
                return clicked;
            });
        }

        // STEP 1: แป้ง
        instructionText.text = "นำแป้งข้าวเหนียวเทลงไปที่ถ้วยผสม";
        canPlayerClick = true;
        currentRequiredIngredient = IngredientType.StickyRiceFlour;

        yield return new WaitUntil(() => isStepComplete);
        isStepComplete = false;

        // STEP 2: น้ำตาล
        instructionText.text = "จากนั้นใส่น้ำตาลทรายอดงหรือน้ำตาลอ้อยบดผงลงไป";
        currentRequiredIngredient = IngredientType.CaneSugar;
        yield return new WaitUntil(() => isStepComplete);
        isStepComplete = false;

        // STEP 3: เกลือ
        instructionText.text = "ใส่เกลือลงเล็กน้อย";
        currentRequiredIngredient = IngredientType.Salt;
        yield return new WaitUntil(() => isStepComplete);
        isStepComplete = false;

        // STEP 4: มะพร้าว
        instructionText.text = "จากนั้นใส่เนื้อมะพร้าสแก่ขูดขุยและมะพร้าวทึกทึนขูดหยาบ";
        currentRequiredIngredient = IngredientType.GratedCoconut;
        yield return new WaitUntil(() => isStepComplete);
        isStepComplete = false;

        // --- คนผสม ---
        canPlayerClick = false;
        instructionText.text = "กดที่ชามรัวๆ เพื่อคลุกเคล้าให้เข้ากัน";
        bowl.EnableMixing();
        yield return new WaitUntil(() => bowl.isFinishedMixing);

        instructionText.text = "เมื่อเข้ากันแล้วถัดไปจะเอาขนมที่ผสมเสร็จแล้วไปนึ่ง";
        yield return new WaitForSeconds(3.0f);

        // --- PHASE 2: นึ่ง ---

        // STEP 5: ใบเตย
        instructionText.text = "ใส่ใบเตยเพื่อรองขนมเพื่อจะเอาขนมออกได้ง่าย";
        canPlayerClick = true;
        currentRequiredIngredient = IngredientType.PandanLeaf;

        yield return new WaitUntil(() => isStepComplete);
        isStepComplete = false;

        canPlayerClick = false;
        yield return StartCoroutine(steamer.AnimateAddPandan());

        // STEP 6: ใส่เนื้อขนม
        instructionText.text = "กดที่ชามผสม เพื่อเทแป้งข้าวเนียวที่ผสมใส่ลงเนียล";
        canPlayerClick = true;
        currentRequiredIngredient = IngredientType.MixedDough;
        yield return new WaitUntil(() => isStepComplete);
        isStepComplete = false;

        canPlayerClick = false;
        yield return StartCoroutine(steamer.AnimateAddDough());
        yield return new WaitForSeconds(1.0f);

        // STEP 7: ปิดฝา
        instructionText.text = "ปิดฝาหม้อรอให้สุกราว 3-5 นาที";

        if (clickableLidOnTable) clickableLidOnTable.SetActive(true);

        canPlayerClick = true;
        currentRequiredIngredient = IngredientType.PotLid;

        yield return new WaitUntil(() => isStepComplete);
        isStepComplete = false;

        if (clickableLidOnTable) clickableLidOnTable.SetActive(false);

        yield return StartCoroutine(steamer.AnimateCloseLid());

        // รอสุก
        instructionText.text = "ความร้อนจากไอน้ำพุ่งผ่านกะลาที่เจาะไว้จนทำให้ขนมสุก";
        yield return StartCoroutine(steamer.AnimateCookingProcess());

        instructionText.text = "แป้งขนมสุกดีแล้ว จะได้เนื้อขนมสีน้ำตาลอ่อนหอมกลิ่นมะพร้าว \nเนื้อเหนียวหนึบรสหวาน มัน กลมกล่อม";

        yield return new WaitForSeconds(2.0f);

        WinGame();
    }

    void WinGame()
    {
        Debug.Log("🎉 ทำขนมเนียลสำเร็จแล้ว!");
        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            StopAllAudio(); // หยุดเสียงก่อนจบโหมดสอบ
            TestGameManager.Instance.FinishExam();
            return; 
        }

        if (winPanel != null) winPanel.SetActive(true);

        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.CompleteProvince(provinceIndex);
            GameDataController.Instance.CollectCookbook(provinceIndex);
            GameDataController.Instance.SaveCurrentScene(nextSceneName);
        }

        if (backToMapButton != null)
        {
            backToMapButton.onClick.RemoveAllListeners();
            backToMapButton.onClick.AddListener(GoToMap);
        }
    }

    public void GoToMap()
    {
        // [แทรก] เสียงคลิกก่อนเปลี่ยนฉาก
        if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);
        
        // สั่งหยุดทุกเสียงก่อนเปลี่ยนฉาก
        StopAllAudio();
        
        SceneManager.LoadScene(nextSceneName);
    }
}