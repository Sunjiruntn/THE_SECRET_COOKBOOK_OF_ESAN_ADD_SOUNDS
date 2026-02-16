using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement; // เพิ่มสำหรับการโหลดซีน

public class CookingInstructorManager : MonoBehaviour
{
    [Header("Game Progression")]
    public string nextSceneName = "RiceHarvestScene";
    private int provinceIndex = 3;

    // =========================================================================
    // [NEW] AUDIO SETUP
    // =========================================================================
    [Header("Audio Settings")]
    public AudioSource voiceAudioSource;   // สำหรับเสียงบรรยาย
    public AudioSource musicAudioSource;   // สำหรับดนตรีพื้นหลัง
    public AudioSource sfxAudioSource;     // สำหรับเสียงคลิก
    public AudioClip introVoiceClip;       // ไฟล์เสียงแนะนำด่าน
    public AudioClip backgroundMusicClip;  // ไฟล์ดนตรีพื้นหลัง
    public AudioClip clickSFXClip;         // ไฟล์เสียงคลิก

    // =========================================================================
    // [1] ENUMS & STATES
    // =========================================================================
    public enum CookingStep
    {
        INTRO,
        STEP_1_PUMPKIN_DRAG,
        STEP_1_PUMPKIN_FLOAT,
        STEP_1_PUMPKIN_CLICK,

        STEP_2_DRAG_RICE,
        STEP_2_DRAG_PUMPKIN,
        STEP_2_MIX_BASE,

        STEP_3_ADD_LIQUIDS_1_COCONUT,
        STEP_3_ADD_LIQUIDS_2_SALT,
        STEP_3_ADD_LIQUIDS_3_SUGAR,
        STEP_3_MIX_LIQUIDS,

        STEP_4_ADD_COCONUT,
        STEP_4_ADD_SESAME,
        STEP_4_MIX_FINAL,

        GAME_OVER_SUCCESS
    }

    [Header("State")]
    public CookingStep currentStep = CookingStep.INTRO;
    private int mixingCount = 0;
    private bool isIntroFinished = false;
    private Coroutine dialogueCoroutine = null;
    private CookingStep stepBeforeError = CookingStep.INTRO;

    // =========================================================================
    // [2] INSPECTOR WIRING
    // =========================================================================

    [Header("UI & Character")]
    public GameObject grandmaObject;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI instructionText;
    public GameObject errorPanel;
    public TextMeshProUGUI errorText;

    [Header("Grandma Animation Setup")]
    public Transform grandmaTargetPosition;
    public float animationDuration = 2f;
    public float startOffsetX = -10f;

    [Header("🎬 Grandma Happy Animation")]
    public Sprite[] grandmaHappySprites;
    public SpriteRenderer grandmaSpriteRenderer;
    [Range(0.05f, 0.8f)]
    public float animationSpeed = 0.25f;
    [Range(1, 5)]
    public int animationLoops = 2;
    private Sprite grandmaOriginalSprite;

    [Header("Targets & Positions")]
    public Collider2D steamerTarget;
    public Collider2D mixingBowlTarget;
    public Transform pumpkinSteamedPos;
    public Transform pumpkinPreparedPos;
    public GameObject finalDish;

    [Header("Ingredients & Containers")]
    public Ingredient rawPumpkin;
    public Ingredient steamedPumpkinFloat;
    public Ingredient stickyRice;
    public Ingredient coconutMilk;
    public Ingredient sugarSweetener;
    public Ingredient salt;
    public Ingredient shreddedCoconut;
    public Ingredient sesameSeeds;
    public Ingredient mixingBowl;
    public SpriteRenderer mixingBowlRenderer;
    private List<Ingredient> allIngredients;

    [Header("Mixing Bowl Visuals")]
    public Sprite bowl_Empty;
    public Sprite bowl_Rice;
    public Sprite bowl_Rice_Pumpkin;
    public Sprite bowl_Base_Mixed;
    public Sprite bowl_CoconutMilk_Added;
    public Sprite bowl_Salt_Added;
    public Sprite bowl_Sugar_Added;
    public Sprite bowl_Liquids_Mixed;
    public Sprite bowl_Coconut_Added_Final;
    public Sprite bowl_Sesame_Added_Final;
    public Sprite bowl_Final_Mixed;

    // =========================================================================
    // [3] INITIALIZATION & UPDATE
    // =========================================================================

    void Start()
    {
        allIngredients = new List<Ingredient>
        {
            rawPumpkin, steamedPumpkinFloat, stickyRice, coconutMilk, sugarSweetener, salt,
            shreddedCoconut, sesameSeeds, mixingBowl
        }.Where(i => i != null).ToList();

        errorPanel.SetActive(false);
        finalDish.SetActive(false);

        SetAllIngredientsActive(true);

        if (rawPumpkin != null) rawPumpkin.ResetPosition();
        if (steamedPumpkinFloat != null) steamedPumpkinFloat.gameObject.SetActive(false);

        if (grandmaSpriteRenderer != null)
        {
            grandmaOriginalSprite = grandmaSpriteRenderer.sprite;
        }

        // เริ่มต้นจัดการเสียง
        StartCoroutine(HandleIntroAudioAndMusic());
        StartCoroutine(StartIntroSequence());
    }

    // จัดการเสียงแนะนำจบแล้วต่อด้วยเพลงพื้นหลัง
    IEnumerator HandleIntroAudioAndMusic()
    {
        if (voiceAudioSource != null && introVoiceClip != null)
        {
            voiceAudioSource.clip = introVoiceClip;
            voiceAudioSource.Play();
            // รอจนกว่าเสียงแนะนำจะจบ
            yield return new WaitWhile(() => voiceAudioSource.isPlaying);
        }

        if (musicAudioSource != null && backgroundMusicClip != null)
        {
            musicAudioSource.clip = backgroundMusicClip;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }
    }

    void Update()
    {
        if (currentStep == CookingStep.INTRO && Input.GetMouseButtonDown(0) && isIntroFinished)
        {
            PlayClickSound(); // เล่นเสียงคลิก
            if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
            StartNextStep(isNext: true);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);

            if (hitCollider != null)
            {
                Ingredient hitIngredient = hitCollider.GetComponent<Ingredient>();
                if (hitIngredient != null && hitIngredient.ingredientId != IngredientID.MIXING_BOWL)
                {
                    return;
                }
            }

            if ((currentStep == CookingStep.STEP_2_MIX_BASE ||
                 currentStep == CookingStep.STEP_3_MIX_LIQUIDS ||
                 currentStep == CookingStep.STEP_4_MIX_FINAL) && !errorPanel.activeSelf)
            {
                PlayClickSound(); // เล่นเสียงคลิกเมื่อกดผสม
                mixingCount++;
                instructionText.text = $"Action: คลิกที่ชาม ( {mixingCount} / 5 ครั้ง)";

                if (mixingCount >= 5)
                {
                    if (currentStep == CookingStep.STEP_2_MIX_BASE)
                    {
                        mixingBowlRenderer.sprite = bowl_Base_Mixed;
                        StartNextStep();
                    }
                    else if (currentStep == CookingStep.STEP_3_MIX_LIQUIDS)
                    {
                        mixingBowlRenderer.sprite = bowl_Liquids_Mixed;
                        StartNextStep();
                    }
                    else if (currentStep == CookingStep.STEP_4_MIX_FINAL)
                    {
                        mixingBowlRenderer.sprite = bowl_Final_Mixed;
                        StartNextStep();
                    }
                }
            }
        }
    }

    private void PlayClickSound()
    {
        if (sfxAudioSource != null && clickSFXClip != null)
        {
            sfxAudioSource.PlayOneShot(clickSFXClip);
        }
    }

    // =========================================================================
    // [4] 🎬 GRANDMA HAPPY ANIMATION
    // =========================================================================

    IEnumerator PlayGrandmaHappyAnimation()
    {
        if (grandmaHappySprites == null || grandmaHappySprites.Length == 0 || grandmaSpriteRenderer == null)
            yield break;

        for (int loop = 0; loop < animationLoops; loop++)
        {
            foreach (Sprite sprite in grandmaHappySprites)
            {
                if (sprite != null)
                {
                    grandmaSpriteRenderer.sprite = sprite;
                    yield return new WaitForSeconds(animationSpeed);
                }
            }
        }

        if (grandmaHappySprites.Length > 0 && grandmaHappySprites[grandmaHappySprites.Length - 1] != null)
            grandmaSpriteRenderer.sprite = grandmaHappySprites[grandmaHappySprites.Length - 1];
        else if (grandmaOriginalSprite != null)
            grandmaSpriteRenderer.sprite = grandmaOriginalSprite;
    }

    IEnumerator SuccessSequence()
    {
        yield return StartCoroutine(TypeDialogue("พร้อมเสิร์ฟเด้อหล่า! ข้าวโจ้โรยงา เสร็จแล้ว ลูกเฮ็ดได้ดีหลาย!"));
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(TypeDialogue("ข้าวโจ้โรยงานี้เป็นขนมหวานพื้นบ้านอีสาน ที่คนขอนแก่นเฮานิยมเฮ็ดกินในงานบุญงานเทศกาลเด้อ"));
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(TypeDialogue("จำไว้บ่ว่า วัฒนธรรมและอาหารพื้นบ้านของบ้านเฮาคือมรดกที่มีค่า ต้องส่งต่อให้คนรุ่นหลังหล่า!"));

        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.CompleteProvince(provinceIndex);
            GameDataController.Instance.CollectCookbook(provinceIndex);
            GameDataController.Instance.SaveCurrentScene(nextSceneName);
            GameDataController.Instance.SaveGame();
        }

        // รออีกเล็กน้อยเพื่อให้ผู้เล่นได้เห็นความสำเร็จ แล้วโหลดซีนถัดไปทันที
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(nextSceneName);
    }

    // =========================================================================
    // [5] GAME FLOW & STATE MANAGEMENT
    // =========================================================================

    private IEnumerator DoType(string message)
    {
        dialogueText.text = "";
        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.02f);
        }
    }

    private IEnumerator TypeDialogue(string message)
    {
        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
        dialogueCoroutine = StartCoroutine(DoType(message));
        yield return dialogueCoroutine;
        dialogueCoroutine = null;
    }

    IEnumerator StartIntroSequence()
    {
        instructionText.text = "สถานะ: คุณยายกำลังเข้ามาเพื่อสอน...";
        yield return StartCoroutine(MoveGrandmaIntoScene());

        instructionText.text = "คลิกเพื่อฟังคุณยายเล่า";
        yield return StartCoroutine(TypeDialogue("มื้อนี้ยายสิสอนเฮ็ด 'ข้าวโจ้โรยงา' อาหารประจำจังหวัดขอนแก่นบ้านเฮาเด้อ วั่งหั่นเฮาได้วัตถุดิบฟักทองมาแล้ว"));

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        PlayClickSound();

        instructionText.text = "คลิกเพื่อเรียนรู้เกี่ยวกับจังหวัดขอนแก่น";
        yield return StartCoroutine(TypeDialogue("ลูกเห็นพระธาตุขามแก่นข้างหลังนี้บ่? นี่เป็นสัญลักษณ์สำคัญของจังหวัดบ้านเฮาเลยหล่า"));

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        PlayClickSound();

        yield return StartCoroutine(TypeDialogue("คำขวัญจังหวัดขอนแก่นว่า 'พระธาตุขามแก่น เสียงแคนดอกคูน ศูนย์รวมผ้าไหม ร่วมใจผูกเสี่ยว เที่ยวขอนแก่นนครใหญ่'"));

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        PlayClickSound();

        yield return StartCoroutine(TypeDialogue("'ไดโนเสาร์สิรินธรเน่ สุดเท่เหรียญทองแรกมวยโอลิมปิก' นี่คือเอกลักษณ์ของจังหวัดบ้านเฮาเด้อ"));

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        PlayClickSound();

        instructionText.text = "คลิกเพื่อเริ่มทำอาหาร";
        yield return StartCoroutine(TypeDialogue("เอาหล่า พร้อมที่สิเริ่มทำข้าวโจ้โรยงาแล้วบ่? ไปเฮาเริ่มเฮ็ดกันเด้อ!"));

        isIntroFinished = true;
    }

    public void StartNextStep(bool isNext = true)
    {
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }
        dialogueText.text = "";
        instructionText.text = "";

        if (isNext) currentStep = GetNextStep(currentStep);

        mixingCount = 0;
        SetAllIngredientsInteraction(false, false);
        ResetVisualsForStep(currentStep, isResetForRetry: false);

        switch (currentStep)
        {
            case CookingStep.STEP_1_PUMPKIN_DRAG:
                StartCoroutine(TypeDialogue("ขั้นตอนที่ 1: เฮามานึ่งฟักทองกันก่อนเด้อ คลิกที่ฟักทองดิบเพื่อนำไปใส่ในหวดเลย"));
                instructionText.text = "Action: คลิกที่ฟักทองดิบ";
                rawPumpkin.isClickable = true;
                break;

            case CookingStep.STEP_1_PUMPKIN_FLOAT:
                StartCoroutine(PumpkinSteamingSequence());
                break;

            case CookingStep.STEP_1_PUMPKIN_CLICK:
                StartCoroutine(TypeDialogue("ฟักทองสุกแล้ว! คลิกที่ฟักทองสุกเพื่อนำมาใช้ผสมได้เลยหล่า"));
                instructionText.text = "Action: คลิกที่ฟักทองสุก";
                steamedPumpkinFloat.isClickable = true;
                steamedPumpkinFloat.gameObject.SetActive(true);
                break;

            case CookingStep.STEP_2_DRAG_RICE:
                StartCoroutine(TypeDialogue("ขั้นตอนที่ 2: ลากข้าวเหนียวที่ยายนึ่งไว้แล้วในติบ ใส่ลงในชามใหญ่ก่อนเลย"));
                instructionText.text = "Action: ลากข้าวเหนียวในกระติบ วางบนชาม";
                stickyRice.isDraggable = true;
                break;

            case CookingStep.STEP_2_DRAG_PUMPKIN:
                StartCoroutine(TypeDialogue("เก่งหลายลุกหล่า! บัดนี่ลากฟักทองสุกที่เตรียมไว้เทิงโต๊ะใส่ตามลงไปเลย"));
                instructionText.text = "Action: ลากฟักทองสุก วางบนชาม";
                steamedPumpkinFloat.isDraggable = true;
                break;

            case CookingStep.STEP_2_MIX_BASE:
                StartCoroutine(TypeDialogue("คลุกให้ข้าวเหนียวกับฟักทองเข้ากัน"));
                instructionText.text = "Action: คลิกที่ชาม (0 / 5 ครั้ง)";
                break;

            case CookingStep.STEP_3_ADD_LIQUIDS_1_COCONUT:
                StartCoroutine(TypeDialogue("ขั้นตอนที่ 3: เติมความหอมมัน ใส่ 'กะทิ' ก่อนเลยหล่า"));
                instructionText.text = "Action: คลิกที่กะทิ";
                coconutMilk.isClickable = true;
                salt.isClickable = true;
                sugarSweetener.isClickable = true;
                break;

            case CookingStep.STEP_3_ADD_LIQUIDS_2_SALT:
                StartCoroutine(TypeDialogue("ต่อด้วยการตัดรส ใส่ 'เกลือ' จักหน่อยเพื่อรสชาติกลมกล่อม"));
                instructionText.text = "Action: คลิกที่เกลือ";
                coconutMilk.isClickable = true;
                salt.isClickable = true;
                sugarSweetener.isClickable = true;
                break;

            case CookingStep.STEP_3_ADD_LIQUIDS_3_SUGAR:
                StartCoroutine(TypeDialogue("สุดท้ายความหวาน ใส่ 'น้ำตาล' เติมความหวานให้ลงโต"));
                instructionText.text = "Action: คลิกที่น้ำตาล";
                coconutMilk.isClickable = true;
                salt.isClickable = true;
                sugarSweetener.isClickable = true;
                break;

            case CookingStep.STEP_3_MIX_LIQUIDS:
                StartCoroutine(TypeDialogue("คลุกเพื่อให้น้ำกะทิซึมเข้าเนื้อ"));
                instructionText.text = "Action: คลิกที่ชาม (0 / 5 ครั้ง)";
                break;

            case CookingStep.STEP_4_ADD_COCONUT:
                StartCoroutine(TypeDialogue("ขั้นตอนที่ 4: ใส่ 'บักพร้าวขูด' ลงไปเลยหล่า"));
                instructionText.text = "Action: คลิกที่มะพร้าวขูด";
                shreddedCoconut.isClickable = true;
                sesameSeeds.isClickable = true;
                break;

            case CookingStep.STEP_4_ADD_SESAME:
                StartCoroutine(TypeDialogue("ดีมากหล่า! สุดท้ายเด้อเพิ่มความหอมด้วย 'งาคั่ว' "));
                instructionText.text = "Action: คลิกที่งาขาวคั่ว";
                shreddedCoconut.isClickable = true;
                sesameSeeds.isClickable = true;
                break;

            case CookingStep.STEP_4_MIX_FINAL:
                StartCoroutine(TypeDialogue("คลุกให้ทุกอย่างเข้ากันดี"));
                instructionText.text = "Action: คลิกที่ชาม (0 / 5 ครั้ง)";
                break;

            case CookingStep.GAME_OVER_SUCCESS:
                if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
                dialogueCoroutine = null;
                StartCoroutine(PlayGrandmaHappyAnimation());
                StartCoroutine(SuccessSequence());
                instructionText.text = "";
                finalDish.SetActive(true);
                break;
        }
    }

    CookingStep GetNextStep(CookingStep step)
    {
        switch (step)
        {
            case CookingStep.INTRO: return CookingStep.STEP_1_PUMPKIN_DRAG;
            case CookingStep.STEP_1_PUMPKIN_DRAG: return CookingStep.STEP_1_PUMPKIN_FLOAT;
            case CookingStep.STEP_1_PUMPKIN_FLOAT: return CookingStep.STEP_1_PUMPKIN_CLICK;
            case CookingStep.STEP_1_PUMPKIN_CLICK: return CookingStep.STEP_2_DRAG_RICE;
            case CookingStep.STEP_2_DRAG_RICE: return CookingStep.STEP_2_DRAG_PUMPKIN;
            case CookingStep.STEP_2_DRAG_PUMPKIN: return CookingStep.STEP_2_MIX_BASE;
            case CookingStep.STEP_2_MIX_BASE: return CookingStep.STEP_3_ADD_LIQUIDS_1_COCONUT;
            case CookingStep.STEP_3_ADD_LIQUIDS_1_COCONUT: return CookingStep.STEP_3_ADD_LIQUIDS_2_SALT;
            case CookingStep.STEP_3_ADD_LIQUIDS_2_SALT: return CookingStep.STEP_3_ADD_LIQUIDS_3_SUGAR;
            case CookingStep.STEP_3_ADD_LIQUIDS_3_SUGAR: return CookingStep.STEP_3_MIX_LIQUIDS;
            case CookingStep.STEP_3_MIX_LIQUIDS: return CookingStep.STEP_4_ADD_COCONUT;
            case CookingStep.STEP_4_ADD_COCONUT: return CookingStep.STEP_4_ADD_SESAME;
            case CookingStep.STEP_4_ADD_SESAME: return CookingStep.STEP_4_MIX_FINAL;
            case CookingStep.STEP_4_MIX_FINAL: return CookingStep.GAME_OVER_SUCCESS;
            default: return step + 1;
        }
    }

    // =========================================================================
    // [6] INTERACTION HANDLERS
    // =========================================================================

    public void HandleDragDrop(IngredientID draggedID, IngredientID targetID)
    {
        bool isCorrect = false;
        if (targetID == IngredientID.MIXING_BOWL)
        {
            if (currentStep == CookingStep.STEP_2_DRAG_RICE && draggedID == IngredientID.STICKY_RICE)
            {
                mixingBowlRenderer.sprite = bowl_Rice;
                stickyRice.gameObject.SetActive(false);
                isCorrect = true;
            }
            else if (currentStep == CookingStep.STEP_2_DRAG_PUMPKIN && draggedID == IngredientID.PUMPKIN_STEAMED_FLOAT)
            {
                mixingBowlRenderer.sprite = bowl_Rice_Pumpkin;
                steamedPumpkinFloat.gameObject.SetActive(false);
                isCorrect = true;
            }
        }

        if (isCorrect)
        {
            PlayClickSound(); // เล่นเสียงเมื่อลากวางถูก
            StartNextStep();
        }
        else
        {
            if (targetID == IngredientID.MIXING_BOWL) FindIngredient(draggedID)?.ResetPosition();
            ShowErrorPopup("❌ เฮ้ดผิดเด้อ! ฟังใหม่อีกจักเทื่อ ");
        }
    }

    public void HandleClick(IngredientID clickedID)
    {
        bool isCorrectClick = false;
        if (clickedID == IngredientID.MIXING_BOWL) return;

        if (currentStep == CookingStep.STEP_1_PUMPKIN_DRAG && clickedID == IngredientID.PUMPKIN_RAW) isCorrectClick = true;
        else if (currentStep == CookingStep.STEP_1_PUMPKIN_CLICK && clickedID == IngredientID.PUMPKIN_STEAMED_FLOAT) isCorrectClick = true;
        else if (currentStep == CookingStep.STEP_3_ADD_LIQUIDS_1_COCONUT && clickedID == IngredientID.COCONUT_MILK) isCorrectClick = true;
        else if (currentStep == CookingStep.STEP_3_ADD_LIQUIDS_2_SALT && clickedID == IngredientID.SALT) isCorrectClick = true;
        else if (currentStep == CookingStep.STEP_3_ADD_LIQUIDS_3_SUGAR && clickedID == IngredientID.SUGAR_SWEETENER) isCorrectClick = true;
        else if (currentStep == CookingStep.STEP_4_ADD_COCONUT && clickedID == IngredientID.SHREDDED_COCONUT) isCorrectClick = true;
        else if (currentStep == CookingStep.STEP_4_ADD_SESAME && clickedID == IngredientID.SESAME_SEEDS) isCorrectClick = true;

        if (isCorrectClick)
        {
            PlayClickSound();
            // จัดการ Visual เฉพาะจุด
            if (currentStep == CookingStep.STEP_1_PUMPKIN_DRAG) { rawPumpkin.gameObject.SetActive(false); }
            else if (currentStep == CookingStep.STEP_3_ADD_LIQUIDS_1_COCONUT) { coconutMilk.gameObject.SetActive(false); mixingBowlRenderer.sprite = bowl_CoconutMilk_Added; }
            else if (currentStep == CookingStep.STEP_3_ADD_LIQUIDS_2_SALT) { salt.gameObject.SetActive(false); mixingBowlRenderer.sprite = bowl_Salt_Added; }
            else if (currentStep == CookingStep.STEP_3_ADD_LIQUIDS_3_SUGAR) { sugarSweetener.gameObject.SetActive(false); mixingBowlRenderer.sprite = bowl_Sugar_Added; }
            else if (currentStep == CookingStep.STEP_4_ADD_COCONUT) { shreddedCoconut.gameObject.SetActive(false); mixingBowlRenderer.sprite = bowl_Coconut_Added_Final; }
            else if (currentStep == CookingStep.STEP_4_ADD_SESAME) { sesameSeeds.gameObject.SetActive(false); mixingBowlRenderer.sprite = bowl_Sesame_Added_Final; }
            
            StartNextStep();
        }
        else if (!errorPanel.activeSelf)
        {
            ShowErrorPopup("❌ ผิดขั้นตอน! คุณยายบอกให้ทำขั้นตอน " + GetExpectedInstruction(currentStep) + " ก่อนจ้ะ");
        }
    }

    // =========================================================================
    // [7] UTILITIES & CORE LOGIC
    // =========================================================================

    IEnumerator PumpkinSteamingSequence()
    {
        instructionText.text = "สถานะ: กำลังนึ่ง (รอ 5 วินาที)...";
        yield return new WaitForSeconds(5f);
        if (steamedPumpkinFloat != null)
        {
            steamedPumpkinFloat.transform.position = pumpkinSteamedPos.position;
            steamedPumpkinFloat.gameObject.SetActive(true);
            Renderer rend = steamedPumpkinFloat.GetComponent<Renderer>();
            if (rend != null) rend.sortingOrder = 10;
        }
        StartNextStep();
    }

    IEnumerator MoveGrandmaIntoScene()
    {
        if (grandmaObject == null || grandmaTargetPosition == null) yield break;
        Vector3 endPos = grandmaTargetPosition.position;
        Vector3 startPos = endPos;
        startPos.x += startOffsetX;
        grandmaObject.transform.position = startPos;
        float startTime = Time.time;
        while (Time.time < startTime + animationDuration)
        {
            float t = (Time.time - startTime) / animationDuration;
            grandmaObject.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        grandmaObject.transform.position = endPos;
    }

    public void SetAllIngredientsActive(bool active)
    {
        if (rawPumpkin != null) rawPumpkin.gameObject.SetActive(active);
        if (steamedPumpkinFloat != null) steamedPumpkinFloat.gameObject.SetActive(active);
        if (stickyRice != null) stickyRice.gameObject.SetActive(active);
        if (coconutMilk != null) coconutMilk.gameObject.SetActive(active);
        if (sugarSweetener != null) sugarSweetener.gameObject.SetActive(active);
        if (salt != null) salt.gameObject.SetActive(active);
        if (shreddedCoconut != null) shreddedCoconut.gameObject.SetActive(active);
        if (sesameSeeds != null) sesameSeeds.gameObject.SetActive(active);
    }

    public void SetAllIngredientsInteraction(bool isDraggable, bool isClickable)
    {
        Ingredient[] ings = { rawPumpkin, steamedPumpkinFloat, stickyRice, coconutMilk, sugarSweetener, salt, shreddedCoconut, sesameSeeds };
        foreach (var i in ings) if (i != null) { i.isDraggable = isDraggable; i.isClickable = isClickable; }
        if (mixingBowl != null) { mixingBowl.isDraggable = isDraggable; mixingBowl.isClickable = false; }
    }

    public void ShowErrorPopup(string message)
    {
        stepBeforeError = currentStep;
        mixingCount = 0;
        errorText.text = message;
        errorPanel.SetActive(true);
        SetAllIngredientsInteraction(false, false);
    }

    public void ResetStepForRetry()
    {
        errorPanel.SetActive(false);
        SetAllIngredientsActive(true);
        ResetIngredientPositions();
        ResetVisualsForStep(stepBeforeError, isResetForRetry: true);
        currentStep = stepBeforeError;
        StartNextStep(isNext: false);
    }

    private void ResetIngredientPositions()
    {
        foreach (var ingredient in allIngredients)
        {
            if (ingredient == null) continue;
            if (ingredient.ingredientId == IngredientID.PUMPKIN_STEAMED_FLOAT && stepBeforeError >= CookingStep.STEP_1_PUMPKIN_CLICK && pumpkinPreparedPos != null)
                ingredient.transform.position = pumpkinPreparedPos.position;
            else
                ingredient.ResetPosition();
        }
    }

    void ResetVisualsForStep(CookingStep step, bool isResetForRetry)
    {
        if (mixingBowlRenderer == null) return;
        if (step <= CookingStep.STEP_2_DRAG_RICE) mixingBowlRenderer.sprite = bowl_Empty;
        else if (step == CookingStep.STEP_2_DRAG_PUMPKIN) mixingBowlRenderer.sprite = bowl_Rice;
        else if (step == CookingStep.STEP_2_MIX_BASE) mixingBowlRenderer.sprite = bowl_Rice_Pumpkin;
        else if (step == CookingStep.STEP_3_ADD_LIQUIDS_1_COCONUT) mixingBowlRenderer.sprite = bowl_Base_Mixed;
        else if (step == CookingStep.STEP_3_ADD_LIQUIDS_2_SALT) mixingBowlRenderer.sprite = bowl_CoconutMilk_Added;
        else if (step == CookingStep.STEP_3_ADD_LIQUIDS_3_SUGAR) mixingBowlRenderer.sprite = bowl_Salt_Added;
        else if (step == CookingStep.STEP_3_MIX_LIQUIDS) mixingBowlRenderer.sprite = bowl_Sugar_Added;
        else if (step == CookingStep.STEP_4_ADD_COCONUT) mixingBowlRenderer.sprite = bowl_Liquids_Mixed;
        else if (step == CookingStep.STEP_4_ADD_SESAME) mixingBowlRenderer.sprite = bowl_Coconut_Added_Final;
        else if (step == CookingStep.STEP_4_MIX_FINAL) mixingBowlRenderer.sprite = bowl_Sesame_Added_Final;
    }

    private Ingredient FindIngredient(IngredientID id) => allIngredients.FirstOrDefault(i => i.ingredientId == id);

    private string GetExpectedInstruction(CookingStep step)
    {
        switch (step)
        {
            case CookingStep.STEP_1_PUMPKIN_DRAG: return "คลิกที่ฟักทองดิบ";
            case CookingStep.STEP_1_PUMPKIN_CLICK: return "คลิกที่ฟักทองสุก";
            case CookingStep.STEP_2_DRAG_RICE: return "ลากข้าวเหนียวนึ่งใส่ชามคลุก";
            case CookingStep.STEP_2_DRAG_PUMPKIN: return "ลากฟักทองสุกใส่ชามคลุก";
            case CookingStep.STEP_2_MIX_BASE: return "คลิกที่ชามเพื่อผสม";
            case CookingStep.STEP_3_ADD_LIQUIDS_1_COCONUT: return "คลิกที่กะทิ";
            case CookingStep.STEP_3_ADD_LIQUIDS_2_SALT: return "คลิกที่เกลือ";
            case CookingStep.STEP_3_ADD_LIQUIDS_3_SUGAR: return "คลิกที่น้ำตาล";
            case CookingStep.STEP_4_ADD_COCONUT: return "คลิกที่มะพร้าวขูด";
            case CookingStep.STEP_4_ADD_SESAME: return "คลิกที่งาขาวคั่ว";
            default: return "ขั้นตอนที่ถูกต้อง";
        }
    }

    public bool IsGameOver() => currentStep == CookingStep.GAME_OVER_SUCCESS;
}