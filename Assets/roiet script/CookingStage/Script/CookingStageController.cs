using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
public class CookingStageController : MonoBehaviour
{

    public string nextSceneName = "MiniGame1Mukda";
    public int provinceIndex = 0;
    // ===== Helpers: Raycast control =====
    void SetBlockRaycast(GameObject go, bool block)
    {
        if (!go) return;
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = block; // true = บังคลิก, false = คลิกทะลุ
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LogRaycastUnderPointer();
        }

    
        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            if (Grandma_FullBody != null && Grandma_FullBody.activeInHierarchy)
                Grandma_FullBody.SetActive(false);

            if (Grandma_BehindTable != null && Grandma_BehindTable.activeInHierarchy)
                Grandma_BehindTable.SetActive(false);

            // ดักปิดกรอบข้อความคำใบ้ด้วย เผื่อมันเด้งมาพร้อมคุณย่า
            if (instructionText != null && instructionText.gameObject.activeInHierarchy)
                instructionText.gameObject.SetActive(false);
        }
    }

    [Header("--- Intro Character & Transition ---")]
    public Button actionButton;
    public GameObject Grandma_FullBody;      // ตัวละคร: ย่าตัวใหญ่ (Scene 1)
    public Animator Animator_FullBody;       // Animator ของตัวใหญ่

    public GameObject Grandma_BehindTable;   // ตัวละคร: ย่าหลังโต๊ะ (Scene 2)
    public Animator Animator_BehindTable;    // Animator ของตัวหลังโต๊ะ

    [Header("--- Drop Animations ---")]
    public GameObject Anim_Group_CrabFat;
    public GameObject Anim_Drop_Veg;
    public GameObject Anim_Pour_num;
    public GameObject Anim_Drop_ch;
    // ===== UI =====
    [Header("UI")]
    public TextMeshProUGUI instructionText;
    public GameObject Pot_Table;
    public GameObject BG;

    public GameObject Table;
    // ===== Buttons (คลิกสิ่งของ) =====
    [Header("Buttons")]
    public Button BowlMeatButton;        // ใส่อกปูลงครก
    public Button PestleButton;          // โขลก (สาก) - เผื่อใช้ภายหลัง
    public Button BottleWaterButton;     // เติมน้ำ
    public Button BowlFilterButton;      // กรอง
    public Button BowlFatButton;         // ใส่มันปู
    public Button BowlVegButton;         // ใส่ผัก
    public Button BowlSeasoningButton;   // ใส่เครื่องปรุง
    public Button MortarButton;          // ปุ่มที่ครอบ "ครก" เพื่อกดเริ่มโขลก

    public Button MortarWithWaterButton;  // ปุ่มที่วางบน Mortar_WithWater เพื่อสั่ง "กรอง"

    // ===== Mortar Group =====
    [Header("Mortar Group")]
    public GameObject Mortar_Empty;
    public GameObject Mortar_WithCrab;
    public GameObject Mortar_Pounding;   // เฟรมขณะโขลก
    public GameObject sak_ani;           // สาก 2 เฟรม (เปิดตอนโขลก)
    public GameObject Mortar_Crushed;    // หลังโขลกละเอียด
    public GameObject Mortar_WithWater;

    // ===== Bowls =====
    [Header("Bowls")]
    public GameObject CoveredBowl;       // ถ้วยที่มีผ้าคลุม (หลังเทอกปูลงครก)
    public GameObject FilteredBowl;
    public Button FilteredBowlButton;
    public GameObject Bowl_Filtering;     // ถ้วยน้ำปู (หลังกรอง)

    // ===== Pot Group =====
    [Header("Pot Group")]
    public GameObject Pot_Empty;
    public GameObject Pot_WithWater;
    public GameObject Pot_Boiling;       // ต้มน้ำปูเดือด (2 เฟรม)
    public GameObject Pot_WithCrabFat;
    public GameObject Pot_BoilingFat;
    public GameObject Pot_Reducing;      // งวดลง
    public GameObject Pot_AddVet;        // หลังใส่ผัก
    public GameObject Pot_Addseason;     // หลังใส่เครื่องปรุง
    public GameObject Pot_Final;         // เสร็จ

    // ===== Settings =====
    [Header("Timings (sec)")]
    public float poundDuration = 1.2f;   // โชว์อนิเมชั่นโขลก
    public float boilDuration = 1.5f;   // เดือดน้ำปู
    public float fatBoilDuration = 1.5f; // เดือดพร้อมมันปู
    public float reduceDuration = 1.5f; // เคี่ยวให้งวด

    // ===== Filtering bowls (animation states) =====
    [Header("UI Popups")]
    public Button PotFinalButton;
    public GameObject FinalFoodCard; // การ์ดรูปอาหาร
    enum Step
    {
        Intro = 0,
        PutMeatToMortar,     // กด BowlMeatButton
        PoundCrabMeat,       // กดครก (MortarButton)
        AddWater,            // กด BottleWaterButton
        FilterCrabWater,     // กด BowlFilterButton
        PourToPotAndBoil,    // เทลงหม้อ + เดือด
        AddCrabFatAndReduce, // ใส่มันปู + เดือด + เคี่ยว
        AddVegAndSeason,     // ใส่ผัก + เครื่องปรุง
        Done
    }

    Step step;
    private bool isPoundingLocked = false;

    // ========== Lifecycle ==========
    void Awake()
    {
        // ซ่อนของที่อาจบังคลิกไว้ก่อน
        if (CoveredBowl) CoveredBowl.SetActive(false);
        if (FilteredBowl) FilteredBowl.SetActive(false);

        if (CoveredBowl)
        {
            var img = CoveredBowl.GetComponent<Image>();
            if (img) img.raycastTarget = false; // ไม่บังคลิกตอนเริ่ม
            SetBlockRaycast(CoveredBowl, false);
        }
    }

    void Start()
    {
        Anim_Pour_num.SetActive(false);
        Anim_Drop_ch.SetActive(false);
        Anim_Drop_Veg.SetActive(false);
        Anim_Group_CrabFat.SetActive(false);
        FinalFoodCard.SetActive(false);
        var tableImage = Table.GetComponent<Image>();
        if (tableImage != null)
        {
            tableImage.raycastTarget = false;  // 🔥 สำคัญ!
        }
        DisableRaycastOnImages(Pot_Boiling);
        DisableRaycastOnImages(Pot_Table);
        DisableRaycastOnImages(BG);

        // --- ภาพเริ่มต้น ---
        // ครก
        Mortar_Empty.SetActive(true);
        Mortar_WithCrab.SetActive(false);
        Mortar_Pounding.SetActive(false);
        sak_ani.SetActive(false);
        Mortar_Crushed.SetActive(false);
        Mortar_WithWater.SetActive(false);
        Bowl_Filtering.SetActive(false);

        // ถ้วย
        CoveredBowl.SetActive(false);     // ต้องซ่อนตั้งแต่แรก
        FilteredBowl.SetActive(false);

        // หม้อ
        ShowOnly(Pot_Empty, Pot_WithWater, Pot_Boiling, Pot_WithCrabFat, Pot_BoilingFat, Pot_Reducing, Pot_AddVet, Pot_Addseason, Pot_Final);
        if (Pot_Empty) Pot_Empty.SetActive(true);



        // เปิดเฉพาะปุ่มที่ใช้ในขั้นแรก
        SetButtons(meat: true, pestle: true, water: true, filter: true, fat: true, veg: true, season: true);
        MortarButton.interactable = false; // ยังห้ามกดครกจนกว่าจะใส่อกปู

        // ให้แน่ใจว่าระบบ UI พร้อมรับคลิก
        ValidateUI();
        BringClickableOnTop(BowlMeatButton.gameObject);



        if (BowlMeatButton) BowlMeatButton.onClick.AddListener(OnPutMeatToMortar);
        if (MortarButton) MortarButton.onClick.AddListener(OnMortarClicked);
        if (PestleButton) PestleButton.onClick.AddListener(OnPound);
        if (BottleWaterButton)
        {
            BottleWaterButton.onClick.AddListener(() =>
            {
                Debug.Log("[CLICK] BottleWaterButton");
                OnAddWater();
            });
        }
        if (BowlFilterButton) BowlFilterButton.onClick.AddListener(OnFilter);
        if (BowlFatButton) BowlFatButton.onClick.AddListener(OnAddFat);
        if (BowlVegButton)
            BowlVegButton.onClick.AddListener(() => StartCoroutine(OnAddVeg()));

        if (BowlSeasoningButton) BowlSeasoningButton.onClick.AddListener(OnSeasoning);

        if (FilteredBowlButton)
        {
            FilteredBowlButton.onClick.RemoveAllListeners();
            FilteredBowlButton.onClick.AddListener(OnFilteredBowlClicked);
            FilteredBowlButton.gameObject.SetActive(false); // เริ่มต้นซ่อนไว้ก่อน
        }



        GoTo(Step.Intro);
        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            // โหมดสอบ: ปิดคำใบ้ ปิดคุณย่า และเริ่มเกมทันที
            if (instructionText) instructionText.gameObject.SetActive(false);
            if (Grandma_FullBody) Grandma_FullBody.SetActive(false);
            if (Grandma_BehindTable) Grandma_BehindTable.SetActive(false);

            SetButtons(true, true, true, true, true, true, true); // เปิดปุ่มรอไว้
            step = Step.PutMeatToMortar; // ข้ามไปขั้นทำอาหารเลย
        }
        else
        {
            // โหมดปกติ: ให้คุณย่าเล่าเรื่องตามเดิม
            GoTo(Step.Intro);
        }


        AssertRefs();
    }

    void ForceShow(GameObject go)
    {
        if (!go) return;
        // เปิดพาเรนต์ทั้งหมด
        Transform t = go.transform;
        while (t != null) { t.gameObject.SetActive(true); t = t.parent; }
        go.SetActive(true);

        // สีไม่โปร่งใส
        var g = go.GetComponent<Graphic>();
        if (g) g.color = new Color(g.color.r, g.color.g, g.color.b, 1f);

        // ถ้ามี CanvasGroup
        var cg = go.GetComponent<CanvasGroup>();
        if (cg) { cg.alpha = 1f; cg.blocksRaycasts = true; }

        // ดันขึ้นบนสุด
        BringClickableOnTop(go);
    }

    void OnAddWater()
    {
        if (step != Step.AddWater)
        {
            Debug.LogWarning($"กดปุ่มใส่น้ำผิดจังหวะ! ตอนนี้อยู่ที่ Step: {step}");
            return; // <--- สำคัญมาก: สั่งหยุดตรงนี้ ไม่ให้รันโค้ดเปลี่ยนรูปข้างล่าง
        }
        if (!Mortar_WithWater)
        {
            Debug.LogError("Mortar_WithWater is NULL. Drag the 'Mortar_WithWater' GameObject to the inspector slot.");
            return;
        }

        Debug.Log("[CLICK] BottleWaterButton -> show Mortar_WithWater");

        // ปิดเฟรมอื่นที่อาจทับ
        if (Mortar_Empty) Mortar_Empty.SetActive(false);
        if (Mortar_WithCrab) Mortar_WithCrab.SetActive(false);
        if (Mortar_Pounding) Mortar_Pounding.SetActive(false);
        if (sak_ani) sak_ani.SetActive(false);
        if (Mortar_Crushed) Mortar_Crushed.SetActive(false);

        // ถ้าต้องการให้ตำแหน่งตรงกับครกเดิม
        var rtWater = Mortar_WithWater.GetComponent<RectTransform>();
        var rtCrushed = Mortar_Crushed ? Mortar_Crushed.GetComponent<RectTransform>() : null;
        if (rtWater && rtCrushed) { rtWater.anchoredPosition = rtCrushed.anchoredPosition; rtWater.sizeDelta = rtCrushed.sizeDelta; }

        // บังคับโชว์และอยู่ข้างบน
        ForceShow(Mortar_WithWater);

        // DEBUG: สถานะจริงหลังสั่งโชว์
        Debug.Log($"[DBG] Mortar_WithWater activeSelf={Mortar_WithWater.activeSelf}, activeInHierarchy={Mortar_WithWater.activeInHierarchy}, parent={Mortar_WithWater.transform.parent?.name}");

        GoTo(Step.FilterCrabWater);
    }
    void AssertRefs()
    {
        if (!instructionText) Debug.LogError("Assign 'instructionText'.");
        if (!BowlMeatButton) Debug.LogError("Assign 'BowlMeatButton'.");
        if (!MortarButton) Debug.LogError("Assign 'MortarButton'.");
        if (!BottleWaterButton) Debug.LogError("Assign 'BottleWaterButton'.");
        if (!MortarWithWaterButton) Debug.LogWarning("Assign 'MortarWithWaterButton' (for step 5).");
    }


    // ========== Validation / Utilities ==========
    void BringClickableOnTop(GameObject go)
    {
        if (!go) return;
        go.transform.SetAsLastSibling();
        var parentCanvas = go.GetComponentInParent<Canvas>();
        if (parentCanvas != null && go.transform.parent != null)
            go.transform.parent.SetAsLastSibling();
    }
    void EnableAlphaHitTest(Button btn)
    {
        if (!btn) return;

        var image = btn.targetGraphic as Image;
        if (image != null)
        {
            // ✅ ใช้ Try-Catch ดัก Error ไว้ เพื่อไม่ให้เกมค้าง
            try
            {
                // ถ้าทำได้ให้ทำ (คลิกทะลุส่วนใส)
                image.alphaHitTestMinimumThreshold = 0.01f;
            }
            catch (System.Exception)
            {
                // ⚠️ ถ้าทำไม่ได้ (เช่น เป็นรูป Sliced) ก็ช่างมัน ปล่อยผ่านไป
                // เกมจะได้ไม่แดง และปุ่มจะยังกดได้แบบสี่เหลี่ยมเต็มๆ
            }
        }
    }


    void ValidateUI()
    {

        EnableAlphaHitTest(BowlMeatButton);
        EnableAlphaHitTest(PestleButton);
        // EnableAlphaHitTest(BottleWaterButton);
        EnableAlphaHitTest(BowlFilterButton);
        EnableAlphaHitTest(BowlFatButton);
        EnableAlphaHitTest(BowlVegButton);
        EnableAlphaHitTest(BowlSeasoningButton);
        EnableAlphaHitTest(MortarButton);

        // 1) Canvas ต้องมี GraphicRaycaster
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            var gr = canvas.GetComponent<GraphicRaycaster>();
            if (!gr) canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        // 2) ปุ่มต้องมี Graphic ที่ raycastTarget = true
        EnsureButtonGraphicRaycastable(BowlMeatButton);
        EnsureButtonGraphicRaycastable(PestleButton);
        EnsureButtonGraphicRaycastable(BottleWaterButton);
        EnsureButtonGraphicRaycastable(BowlFilterButton);
        EnsureButtonGraphicRaycastable(BowlFatButton);
        EnsureButtonGraphicRaycastable(BowlVegButton);
        EnsureButtonGraphicRaycastable(BowlSeasoningButton);
        EnsureButtonGraphicRaycastable(MortarButton);

        // 3) มี EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
        // บังคับให้กราฟิกของปุ่มครกรับเรย์แคสต์แน่นอน
        if (MortarButton)
        {
            var g = MortarButton.targetGraphic;
            if (g != null) g.raycastTarget = true;
        }

        // ดีบัก: แจ้งว่าปุ่มครกพร้อมหรือยัง
        Debug.Log($"[Cooking] MortarButton active={MortarButton && MortarButton.gameObject.activeInHierarchy}, interactable={MortarButton && MortarButton.interactable}");

    }

    void SetInstruction(string msg)
    {
        if (instructionText) instructionText.text = msg;
        else Debug.LogError("instructionText is NULL. Drag your TMP text to 'instructionText' in Inspector.");
    }

    // ========== State Machine ==========
    void GoTo(Step s)
    {
        step = s;
        switch (step)
        {
            case Step.Intro:
                // เรียกฟังก์ชันเล่าเรื่องของคุณย่า (แทนโค้ดเดิม)
                StartCoroutine(CoPlayIntroSequence());
                break;

            case Step.PoundCrabMeat:
                // ปิดปุ่มอื่นทั้งหมด
                SetButtons(false, false, false, false, false, false, false);

                // โชว์ครกมีเนื้อปูแน่ ๆ (กันหลงไปเฟรมอื่น)
                if (Mortar_Empty) Mortar_Empty.SetActive(false);
                if (Mortar_Crushed) Mortar_Crushed.SetActive(false);
                if (Mortar_Pounding) Mortar_Pounding.SetActive(false);
                if (sak_ani) sak_ani.SetActive(false);
                if (Mortar_WithCrab) Mortar_WithCrab.SetActive(true);

                // ดันครกขึ้นบนสุดของ Canvas เพื่อรับคลิกก่อน
                if (Mortar_WithCrab) BringClickableOnTop(Mortar_WithCrab);

                // เปิดปุ่มครก และแน่ใจว่ามีกราฟิกรับเรย์แคสต์
                if (MortarButton)
                {
                    MortarButton.gameObject.SetActive(true); // ✅ ให้แน่ใจว่าเปิดอยู่
                    MortarButton.interactable = true;
                    EnsureButtonGraphicRaycastable(MortarButton);
                }
                // ข้อความสอน
                SetInstruction("โขลกในส่วนของอกปูที่แยกใส่ครกไว้ให้ละเอียด (กดที่ครก)");
                break;


            case Step.AddWater:
                // ปิดปุ่มอื่น เปิดเฉพาะขวดน้ำ
                SetButtons(true, true, true, true, true, true, true);

                // โชว์ครกหลังโขลก แต่ไม่ให้บังคลิก
                if (Mortar_Empty) Mortar_Empty.SetActive(false);
                if (Mortar_WithCrab) Mortar_WithCrab.SetActive(false);
                if (Mortar_Pounding) Mortar_Pounding.SetActive(false);
                if (sak_ani) sak_ani.SetActive(false);
                if (Mortar_Crushed)
                {
                    Mortar_Crushed.SetActive(true);
                    var g = Mortar_Crushed.GetComponent<Graphic>();
                    if (g) g.raycastTarget = false;           // คลิกทะลุ
                    SetBlockRaycast(Mortar_Crushed, false);
                }
                if (MortarButton) MortarButton.interactable = false;
                // เปิดปุ่มขวดน้ำให้คลิกได้แน่ ๆ
                if (BottleWaterButton)
                {
                    BottleWaterButton.gameObject.SetActive(true);
                    BottleWaterButton.interactable = true;
                    EnsureButtonGraphicRaycastable(BottleWaterButton);
                    BringClickableOnTop(BottleWaterButton.gameObject);
                }

                // กันถ้วยคลุมบัง
                if (CoveredBowl)
                {
                    var cb = CoveredBowl.GetComponent<Graphic>();
                    if (cb) cb.raycastTarget = false;
                    SetBlockRaycast(CoveredBowl, false);
                }

                SetInstruction("พอโขลกอกปูละเอียดแล้ว ให้เติมน้ำลงไปเพื่อที่จะกรองเอาน้ำปู (กดขวดน้ำ)");
                break;


            case Step.FilterCrabWater:
                // เปิดเฉพาะปุ่มกรอง
                SetButtons(true, true, true, true, true, true, true);

                // โชว์ภาพครกที่มีน้ำแน่นอน
                if (Mortar_WithWater) Mortar_WithWater.SetActive(true);

                // ซ่อนปุ่มคลุมบนครก (ถ้ามี)
                if (MortarWithWaterButton)
                {
                    MortarWithWaterButton.interactable = false;
                    MortarWithWaterButton.gameObject.SetActive(false);
                }

                // ทำให้ปุ่มกรองคลิกได้แน่นอน และอยู่บนสุด
                if (BowlFilterButton)
                {
                    BowlFilterButton.gameObject.SetActive(true);
                    BowlFilterButton.interactable = true;
                    EnsureButtonGraphicRaycastable(BowlFilterButton);
                    BringClickableOnTop(BowlFilterButton.gameObject);
                }
                else
                {
                    Debug.LogWarning("BowlFilterButton is NULL. Assign it in Inspector.");
                }

                // กันของอื่นบังคลิก
                if (CoveredBowl)
                {
                    var g = CoveredBowl.GetComponent<Graphic>();
                    if (g) g.raycastTarget = false;
                    SetBlockRaycast(CoveredBowl, false);
                }

                SetInstruction("ยายเตรียมผ้าขาวไว้สำหรับกรองน้ำปู หลานเอาน้ำปูในครกเทใส่บนผ้า (กดที่ครก)");
                break;


            case Step.PourToPotAndBoil:
                SetButtons(true, true, true, true, true, true, true);
                instructionText.text = "แล้วนำน้ำปูที่กรองเสร็จมาเทใส่หม้อ";
                StartCoroutine(CoBoilOnly());
                break;

            case Step.AddCrabFatAndReduce:
                SetButtons(true, true, true, true, true, true, true);
                instructionText.text = "จากนั้นใส่มันปูจากกระดองลงหม้อ ต้มให้เดือดและเคี่ยวจนงวด";
                break;

            // case Step.AddVegAndSeason:
            //     SetButtons(false, false, false, false, false, true, false);
            //     instructionText.text = "8) ใส่ผัก แล้วใส่เครื่องปรุง คนให้เข้ากัน จากนั้นพักให้เย็น";
            //     break;

            case Step.Done:
                // ❌ ของเดิม (สาเหตุที่ทำให้กดได้อยู่):
                // SetButtons(true, true, true, true, true, true, true);

                // ✅ แก้เป็นแบบนี้ (ปิดทุกปุ่ม):
                SetButtons(false, false, false, false, false, false, false);

                //instructionText.text = "เสร็จแล้ว! ลาบปูนาหอมๆ พร้อมรับประทานจ้า";

                if (Pot_Final) Pot_Final.SetActive(true);
                break;
        }
    }


    // ========== Button Handlers ==========
    void OnPutMeatToMortar()
    {
        if (step != Step.PutMeatToMortar)
        {
            HandleWrongStep();
            return;
        }
        if (TestGameManager.Instance != null) TestGameManager.Instance.RecordSuccess(); // [เพิ่ม] บวกคะแนน!

        StartCoroutine(CoPlayDropAnim(Anim_Drop_ch, 1.5f));
        // ครก: จากว่าง -> มีอกปู
        Mortar_Empty.SetActive(true);
        Mortar_WithCrab.SetActive(true);

        // ถ้วย: โชว์ถ้วยคลุมแทน และซ่อนถ้วยอกปู
        if (CoveredBowl)
        {
            CoveredBowl.SetActive(true);
            var coverImg = CoveredBowl.GetComponent<Image>();
            if (coverImg) coverImg.raycastTarget = true; // บังคลิกเพื่อกันกดย้ำ
            SetBlockRaycast(CoveredBowl, true);
        }
        if (BowlMeatButton) BowlMeatButton.gameObject.SetActive(false);

        // เปิดให้ "กดครก" เพื่อเริ่มโขลก
        MortarButton.interactable = true;
        SetButtons(false, false, false, false, false, false, false);

        GoTo(Step.PoundCrabMeat);
    }

    void OnMortarClicked()
    {
        if (isPoundingLocked) return;
        if (step != Step.PoundCrabMeat)
        {
            HandleWrongStep();
            return;
        }
        if (MortarButton) MortarButton.interactable = false;
        if (PestleButton) PestleButton.interactable = false;
        if (TestGameManager.Instance != null) TestGameManager.Instance.RecordSuccess(); // [เพิ่ม] บวกคะแนน!
        StartCoroutine(CoPound());
    }

    void OnPound()
    {
        if (step != Step.PoundCrabMeat)
        {
            HandleWrongStep();
            return;
        }
        if (TestGameManager.Instance != null) TestGameManager.Instance.RecordSuccess(); // [เพิ่ม] บวกคะแนน!
        StartCoroutine(CoPound());
    }
    bool waterAdded = false;
    void OnFilter()
    {
        if (step != Step.FilterCrabWater)
        {
            HandleWrongStep();
            return;
        }
        if (BowlFilterButton) BowlFilterButton.interactable = false;
        if (TestGameManager.Instance != null) TestGameManager.Instance.RecordSuccess(); // [เพิ่ม] บวกคะแนน!
        StartCoroutine(CoFilter());
    }
    // ฟังก์ชันเล่าเรื่องของคุณย่า (Intro)
    // ฟังก์ชันเล่าเรื่องของคุณย่า (Intro) - แบบกดเพื่อไปต่อ
    IEnumerator CoPlayIntroSequence()
    {
        // 1. ปิดปุ่มทุกอย่างก่อน ระหว่างย่าพูด
        SetButtons(false, false, false, false, false, false, false);

        // --- ฉากที่ 1: โชว์ย่าตัวใหญ่ ---
        if (Grandma_BehindTable) Grandma_BehindTable.SetActive(false); // ซ่อนตัวหลังโต๊ะ
        if (Grandma_FullBody)
        {
            Grandma_FullBody.SetActive(true); // โชว์ตัวใหญ่
            // ดันมาหน้าสุด (ถ้าจำเป็น)
            Grandma_FullBody.transform.SetAsLastSibling();

        }

        // --- ประโยคที่ 1 ---
        SetInstruction("ย่าสิเป็นผู้สอนสอนหลานทำลาบปูนา มื้อนี้ย่าใส่ผ้าไหมลายสาเกตุมานำเด้อลูก (แตะเพื่อไปต่อ)");

        // รอ 0.1 วิ กันการกดเบิ้ล แล้วรอจนกว่าจะคลิก
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        SetInstruction("โดยผ้าไหมสาเกต ประกอบด้วยลายมัดหมี่พื้นบ้าน 5 ลาย");
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // --- ประโยคที่ 2 ---
        SetInstruction("ได้แก่ ลายโคมเจ็ด ลายนาคน้อย ลายคองเอี้ย ลายหมากจับ ลายค้ำเพา ");
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        SetInstruction("ลวดลายทั้ง 5 ลายนี้ ได้นำมาประยุกต์ไว้ในผ้าผืนเดียวกัน เป็นผ้าเอกลักษณ์ประจำจังหวัดร้อยเอ็ด");
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // --- ประโยคที่ 3 ---
        SetInstruction("3. เอาล่ะเราไปเริ่มทำเลยดีกว่า");

        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        SetInstruction("");
        // สลับตัวละคร!
        if (Grandma_FullBody) Grandma_FullBody.SetActive(false);
        if (Grandma_BehindTable) Grandma_BehindTable.SetActive(true);  // โชว์ตัวหลังโต๊ะ (เล่นท่า Idle เอง).

        // --- ประโยคที่ 4 (ข้อมูลการล้างปู) ---
        SetInstruction("เริ่มจากนำปูนามาล้างด้วยน้ำสะอาด และนำมาแกะ แยกส่วนกระดองปูออก ส่วนขาปูติดมาด้วยคือส่วนอกปูนะลูก");

        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // --- เข้าสู่เกม (เปิดปุ่ม) ---
        SetInstruction("จากนั้นใส่อกปูที่แยกไว้ลงครก (กดถ้วยอกปู)");

        // เปิดเฉพาะปุ่มอกปู
        SetButtons(true, true, true, true, true, true, true);

        // เปลี่ยน Step เพื่อเริ่มเกม
        step = Step.PutMeatToMortar;
    }

    IEnumerator CoFilter()
    {

        // เปิดปุ่มกรองให้เห็นได้ชัดระหว่างกำลังกรอง
        if (BowlFilterButton)
        {
            BowlFilterButton.interactable = false;     // ห้ามกดซ้ำ
            BowlFilterButton.gameObject.SetActive(true); // ✅ ให้โชว์ระหว่างกรอง
            BringClickableOnTop(BowlFilterButton.gameObject);
        }


        // แสดงข้อความสอนระหว่างกรอง
        SetInstruction("กำลังกรองน้ำปู...");

        // แอนิเมชันกรอง (เช่นแสดงถ้วยกรองเฉพาะกิจ)
        if (Bowl_Filtering)
        {

            Bowl_Filtering.SetActive(true);  // ถ้ามีถ้วยแอนิเมชัน “กำลังกรอง”
            BringClickableOnTop(Bowl_Filtering);

        }

        // รอสักครู่ (จำลองเวลาที่กรองจริง)
        yield return new WaitForSeconds(1.5f); // ปรับเวลาได้ตามใจ

        // ปิดแอนิเมชัน "กำลังกรอง"
        if (Bowl_Filtering)
        {
            Bowl_Filtering.SetActive(false);
        }

        // แสดงผลลัพธ์หลังกรองเสร็จ
        if (FilteredBowl)
        {
            FilteredBowl.SetActive(true); // ✅ โชว์ถ้วยน้ำปูหลังกรองเสร็จ
            BringClickableOnTop(FilteredBowl);
        }

        // ปิดถ้วยที่มีผ้าคลุม
        if (CoveredBowl)
        {
            CoveredBowl.SetActive(false); // ✅ หายไปตอนกรองเสร็จ
        }

        // ปิดปุ่มกรองหลังเสร็จ
        if (BowlFilterButton)
        {
            BowlFilterButton.gameObject.SetActive(false); // ✅ ปิดเมื่อเสร็จแล้ว
        }

        // ไปขั้นต่อไป (เทลงหม้อ)
        SetInstruction("กดที่ถ้วยน้ำปู เพื่อเทลงหม้อ");
        if (FilteredBowlButton)
        {
            FilteredBowlButton.gameObject.SetActive(true);
            FilteredBowlButton.interactable = true;
            BringClickableOnTop(FilteredBowlButton.gameObject);
        }
    }

    void OnAddFat()
    {
        // ต้องอยู่ขั้น PourToPotAndBoil ถึงจะใส่มันปูได้
        if (step != Step.PourToPotAndBoil)
        {
            HandleWrongStep();
            return;
        }
        if (TestGameManager.Instance != null) TestGameManager.Instance.RecordSuccess(); // [เพิ่ม] บวกคะแนน!
        // เปลี่ยนขั้นไป AddCrabFatAndReduce
        GoTo(Step.AddCrabFatAndReduce);

        // เริ่ม Coroutine ใส่มันปู + เคี่ยว
        StartCoroutine(CoAddFatAndReduce());
    }

    IEnumerator OnAddVeg()
    {
        // 1. เช็คว่ากดถูกจังหวะไหม
        if (step != Step.AddCrabFatAndReduce)
        {
            HandleWrongStep();
            yield break;
        }
        if (TestGameManager.Instance != null) TestGameManager.Instance.RecordSuccess(); // [เพิ่ม] บวกคะแนน!
        Debug.Log("[CLICK] BowlVegButton -> AddVegAndSeason");
        StartCoroutine(CoPlayDropAnim(Anim_Drop_Veg, 1.0f));
        // 2. แสดงผลหม้อใส่ผัก
        ShowOnly(Pot_Empty, Pot_WithWater, Pot_Boiling, Pot_WithCrabFat,
                 Pot_BoilingFat, Pot_Reducing, Pot_Addseason, Pot_Final);

        if (Pot_AddVet) Pot_AddVet.SetActive(true);

        SetInstruction("ปรุงรสใส่พริกป่น ข้าวคั่ว น้ำปลา น้ำปลาร้า ตามรสชาติที่ชอบ...");

        // ✅✅✅ 3. เปลี่ยน Step ทันที! (สำคัญมาก) 
        // ต้องเปลี่ยนตรงนี้ เพื่อให้ปุ่มเครื่องปรุง (Seasoning) รู้ว่าถึงคิวของมันแล้ว
        GoTo(Step.AddVegAndSeason);

        // 4. เปิดปุ่มเครื่องปรุง
        if (BowlSeasoningButton)
        {
            BowlSeasoningButton.gameObject.SetActive(true);
            BowlSeasoningButton.interactable = true;
            if (BowlSeasoningButton.targetGraphic != null)
                BowlSeasoningButton.targetGraphic.raycastTarget = true;

            BringClickableOnTop(BowlSeasoningButton.gameObject);
        }

        // ❌❌❌ ลบ Loop while เดิมทิ้งไปเลยครับ ห้ามใส่กลับมา
        /* while (step == Step.AddCrabFatAndReduce)
            yield return null;
        */
    }


    // 2. แก้ไขฟังก์ชัน OnSeasoning (ปรุงรส)
    void OnSeasoning()
    {
        if (step != Step.AddVegAndSeason) { HandleWrongStep(); return; }

        Debug.Log("[CLICK] BowlSeasoningButton -> Start waiting...");

        if (TestGameManager.Instance != null) TestGameManager.Instance.RecordSuccess(); // [เพิ่ม] บวกคะแนน!
        // 🔒 ปิดปุ่มวัตถุดิบทุกอย่างทันที! (ผู้เล่นจะได้กดอย่างอื่นไม่ได้)
        SetButtons(false, false, false, false, false, false, false);

        // 🔒 ปิดปุ่มหม้อไว้ก่อน (กันคนกดก่อนเวลา 3 วิ)
        if (PotFinalButton) PotFinalButton.interactable = false;

        // ซ่อนการ์ดไว้ก่อน (กันเหนียว)
        if (FinalFoodCard) FinalFoodCard.SetActive(false);

        // เริ่มนับเวลาถอยหลัง
        StartCoroutine(CoWaitAndEnablePot());
    }
    public void OnClickFinalPot()
    {
        // ป้องกันการกดปุ่มซ้ำ
        if (PotFinalButton) PotFinalButton.interactable = false;

        // สั่งเริ่ม Coroutine เพื่อนับเวลา
        StartCoroutine(CoShowCardSequence());
    }
    // ========== Coroutines ==========
    IEnumerator CoShowCardSequence()
    {
        Debug.Log("[CLICK] Final Pot -> Show Card");
        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            TestGameManager.Instance.FinishExam();
            yield break; // หยุดการทำงานตรงนี้เลย ไม่ต้องโชว์การ์ด ไม่ต้องเปลี่ยนฉากเอง
        }
        // ✅ 1. แสดงข้อความก่อนทันที
        SetInstruction("ลาบปูหอม ๆ พร้อมรับประทานจ้า! (จบเกม)");

        // ✅ 2. สั่งรอ 2 วินาที (ต้องใช้ yield return)
        yield return new WaitForSeconds(1.0f);
        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.CompleteProvince(0);
            GameDataController.Instance.CollectCookbook(provinceIndex);
            GameDataController.Instance.SaveCurrentScene(nextSceneName);

        }
        // ✅ 3. ค่อยโชว์การ์ดอาหารทีหลัง
        if (FinalFoodCard)
        {
            FinalFoodCard.SetActive(true);
            BringClickableOnTop(FinalFoodCard);

            if (actionButton)
            {
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() =>
                {
                    Debug.Log("🔘 กดปุ่ม Action Button แล้ว!");
                    // โหลดซีน Cookingstage
                    SceneManager.LoadScene("TutorialScene");
                });
            }
            else
            {
                Debug.LogError("❌ หา actionButton ไม่เจอ! ");
            }
        }

        // จบเกม
        GoTo(Step.Done);
    }
    IEnumerator CoWaitAndEnablePot()
    {
        // รอ 3 วินาที (เวลาปรุงรส)
        yield return new WaitForSeconds(2.0f);

        // โชว์หม้อใบสุดท้าย
        if (Pot_Final) Pot_Final.SetActive(true);
        if (Pot_Addseason) Pot_Addseason.SetActive(false); // ซ่อนหม้อปรุงรส

        // ✅ เปลี่ยนข้อความสั่งให้กดหม้อ
        SetInstruction("เสร็จแล้ว! กดที่หม้อเพื่อตักเสิร์ฟ");

        // ✅ เปิดปุ่มที่หม้อให้กดได้!
        if (PotFinalButton)
        {
            PotFinalButton.interactable = true;
            PotFinalButton.gameObject.SetActive(true);

            // ทำให้แน่ใจว่ากดติด + อยู่บนสุด
            EnsureButtonGraphicRaycastable(PotFinalButton);
            BringClickableOnTop(PotFinalButton.gameObject);
        }
    }
    IEnumerator CoPound()
    {
        // ปิดภาพอื่น เปิดเฟรมโขลก + สาก
        Mortar_WithCrab.SetActive(false);
        Mortar_Pounding.SetActive(true);
        sak_ani.SetActive(true);

        yield return new WaitForSeconds(poundDuration);

        // จบเป็นเนื้อละเอียด
        Mortar_Pounding.SetActive(false);
        sak_ani.SetActive(false);
        Mortar_Crushed.SetActive(true);

        // ห้ามกดครกซ้ำ
        MortarButton.interactable = false;

        GoTo(Step.AddWater); // ไปขั้นเติมน้ำ
    }

    IEnumerator CoBoilOnly()
    {
        // เทลงหม้อ
        ShowOnly(Pot_Empty, Pot_WithCrabFat, Pot_BoilingFat, Pot_Reducing, Pot_AddVet, Pot_Addseason, Pot_Final);
        if (Pot_WithWater) Pot_WithWater.SetActive(true);


        // เดือด
        yield return new WaitForSeconds(1.5f);
        // แสดงหม้อเดือด
        ShowOnly(Pot_Empty, Pot_WithWater, Pot_WithCrabFat, Pot_BoilingFat,
                 Pot_Reducing, Pot_AddVet, Pot_Addseason, Pot_Final);

        if (Pot_Boiling) Pot_Boiling.SetActive(true);

        // ตั้งข้อความสอนให้ผู้เล่นรู้ว่าต้องกดถ้วยมันปูต่อ
        SetInstruction("พอหม้อกำลังเดือด ให้ใส่มันปูที่แยกจากจากกระดองลงหม้อ (กดถ้วยกระดองปู)");
        // เปิดปุ่มมันปูให้กดได้

        // ✅ เปิดปุ่มมันปูให้กดได้
        // ✅ เปิดปุ่มมันปูให้กดได้
        if (BowlFatButton)
        {
            BowlFatButton.gameObject.SetActive(true);
            BowlFatButton.interactable = true;
            if (BowlFatButton.targetGraphic != null)
                BowlFatButton.targetGraphic.raycastTarget = true;

            BringClickableOnTop(BowlFatButton.gameObject);
        }

        // ✅ รอจนกว่าจะกดปุ่มมันปู
        while (step == Step.PourToPotAndBoil)
        {
            yield return null; // รอ frame ต่อ frame จนเปลี่ยน step
        }
    }
    void DisableRaycastOnImages(GameObject obj)
    {
        foreach (var img in obj.GetComponentsInChildren<Image>(true))
        {
            img.raycastTarget = false;
        }
    }
    IEnumerator CoPlayDropAnim(GameObject dropObj, float duration)
    {
        if (dropObj == null) yield break;

        // เปิดตัว -> อนิเมชั่นจะเล่นเองอัตโนมัติเพราะมี Animator อยู่
        dropObj.SetActive(true);

        // รอจนอนิเมชั่นเล่นจบ
        yield return new WaitForSeconds(duration);

        // ซ่อนตัวกลับไปเหมือนเดิม
        dropObj.SetActive(false);
    }

    // ฟังก์ชันช่วยรอเวลาก่อนเปลี่ยน Step (ใช้แทน GoTo แบบเดิมในบางจุด)
    IEnumerator CoWaitNextStep(Step nextStep, string instruction, float delay)
    {
        // รอให้ของตกลงไปในหม้อก่อน
        yield return new WaitForSeconds(delay);

        // ค่อยเปลี่ยน Step และข้อความ
        instructionText.text = instruction;
        GoTo(nextStep);
    }
    IEnumerator CoAddFatAndReduce()
    {
        SetInstruction("ใส่มันปูจากกระดองลงหม้อ...");

        // 1. ปิดปุ่มทั้งหมดก่อน ระหว่างรออนิเมชัน (กันคนกดมั่วตอนหม้อกำลังเดือด)
        SetButtons(false, false, false, false, false, false, false);

        // สั่งให้มันปูร่วงลงมา (ใช้ Anim_Group_CrabFat หรือ Anim_Drop_CrabFat ตามที่คุณตั้งชื่อ)
        // เลข 1.0f คือเวลาที่เผื่อไว้ให้อนิเมชั่นเล่นจนจบ
        StartCoroutine(CoPlayDropAnim(Anim_Group_CrabFat, 1.7f));

        // แสดงหม้อมีมันปู
        ShowOnly(Pot_Empty, Pot_WithWater, Pot_Boiling, Pot_Reducing, Pot_AddVet, Pot_Addseason, Pot_Final);
        if (Pot_WithCrabFat) Pot_WithCrabFat.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // เดือดกับมันปู
        if (Pot_BoilingFat) Pot_BoilingFat.SetActive(true);
        yield return new WaitForSeconds(2.0f);

        SetInstruction("ต้มทิ้งไว้ให้เดือด.. เคี่ยวให้งวดลง");

        // เคี่ยวให้งวด
        ShowOnly(Pot_Empty, Pot_WithWater, Pot_WithCrabFat, Pot_BoilingFat,
                 Pot_AddVet, Pot_Addseason, Pot_Final);
        if (Pot_Reducing) Pot_Reducing.SetActive(true);

        yield return new WaitForSeconds(3.0f);

        // --- สิ้นสุดอนิเมชั่น ---

        Pot_Final.SetActive(true);

        SetInstruction("หลังจากที่ต้มทิ้งไว้จนน้ำเหลือนิดหน่อย ให้ทำการใส่ต้นหอม ใส่ผักที่เตรียมไว้ (กดถ้วยผัก)");

        // ✅✅✅ แก้ไขตรงนี้: เปิดทุกปุ่มให้กดได้ (เพื่อวัดใจผู้เล่น)
        // เรียงตาม: meat, pestle, water, filter, fat, veg, season
        SetButtons(true, true, true, true, true, true, true);

        // เสริม: ดันปุ่มผักขึ้นมาบนสุดเพื่อให้แน่ใจว่ากดง่าย (แต่ปุ่มอื่นก็กดได้นะ)
        if (BowlVegButton) BringClickableOnTop(BowlVegButton.gameObject);

        // รอจนกว่าผู้เล่นจะกด "ปุ่มผัก" (ซึ่งจะไปเปลี่ยน step ในฟังก์ชัน OnAddVeg)
        // แต่ถ้าผู้เล่นกดปุ่มอื่น -> มันจะเข้าฟังก์ชันของปุ่มนั้น -> เช็ค Step ไม่ตรง -> HandleWrongStep
        while (step == Step.AddCrabFatAndReduce)
            yield return null;

        // เมื่อหลุด Loop นี้แสดงว่ากดถูกแล้ว
        GoTo(Step.AddVegAndSeason);
    }

    IEnumerator CoFinish()
    {
        yield return new WaitForSeconds(0.3f);
        ShowOnly(Pot_Empty, Pot_WithWater, Pot_Boiling, Pot_WithCrabFat, Pot_BoilingFat, Pot_Reducing, Pot_AddVet, Pot_Addseason);
        if (Pot_Final) Pot_Final.SetActive(true);
        GoTo(Step.Done);
    }

    // ========== Common Helpers ==========
    void SetButtons(bool meat, bool pestle, bool water, bool filter, bool fat, bool veg, bool season)
    {
        SetButtonState(BowlMeatButton, meat);
        SetButtonState(PestleButton, pestle);
        SetButtonState(BottleWaterButton, water);
        SetButtonState(BowlFilterButton, filter);
        SetButtonState(BowlFatButton, fat);
        SetButtonState(BowlVegButton, veg);
        SetButtonState(BowlSeasoningButton, season);
    }

    void SetButtonState(Button btn, bool shouldEnable)
    {
        if (btn == null) return;

        // ให้เปิด/ปิดการคลิก
        btn.interactable = shouldEnable;

        if (btn.targetGraphic != null)
        {
            // ✅ ปรับตรงนี้: เปิด raycast เสมอถ้าปุ่มควรคลิกได้
            btn.targetGraphic.raycastTarget = shouldEnable;
        }

        // ปรับสีปุ่มให้ไม่ซีดแม้จะ disabled
        var colors = btn.colors;
        colors.disabledColor = colors.normalColor;
        btn.colors = colors;
    }



    void ShowOnly(params GameObject[] group)
    {
        foreach (var go in group) if (go) go.SetActive(false);
    }

    // ให้ปุ่มมีกราฟิกที่รับ raycast ได้ (กันเคส targetGraphic ว่าง)
    void EnsureButtonGraphicRaycastable(Button btn)
    {
        if (!btn) return;
        var g = btn.targetGraphic;
        if (g != null) g.raycastTarget = true;
        else
        {
            var img = btn.GetComponentInChildren<Graphic>();
            if (img != null) img.raycastTarget = true;
        }
    }
    void LogRaycastUnderPointer()
    {
        if (EventSystem.current == null) { Debug.LogWarning("No EventSystem."); return; }
        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);
        Debug.Log("[Raycast] " + string.Join(" > ", results.Select(r => r.gameObject.name)));
    }
    void OnFilteredBowlClicked()
    {
        if (step != Step.FilterCrabWater)
        {
            Debug.LogWarning("Clicked FilteredBowlButton but not in FilterCrabWater step");
            return;
        }

        Debug.Log("[CLICK] FilteredBowlButton -> Pour to pot");

        // ปิดปุ่มไม่ให้กดซ้ำ
        FilteredBowlButton.interactable = false;
        FilteredBowlButton.gameObject.SetActive(false);

        // ปิดชามน้ำปู (เทเสร็จ)
        if (FilteredBowl) FilteredBowl.SetActive(false);

        StartCoroutine(CoPlayDropAnim(Anim_Pour_num, 1.0f));
        // แสดงข้อความ
        SetInstruction("กำลังเทน้ำปูลงหม้อ...");

        // ไปขั้นต้ม
        GoTo(Step.PourToPotAndBoil);
    }

    // =========================================================
    // 🟥 ส่วนจัดการเมื่อกดผิด (Wrong Step Handler)
    // =========================================================

    /// <summary>
    /// เรียกใช้ฟังก์ชันนี้เมื่อผู้เล่นกดผิดปุ่ม
    /// </summary>
    void HandleWrongStep()
    {
        Debug.Log("🚨 WRONG STEP! Resetting...");
        if (TestGameManager.Instance.isTestMode)
        {
            Debug.Log("[โหมดสอบ] กดผิด! จดคะแนนแล้ว และบังคับซ่อนคุณย่า");

            // ✅ บังคับปิดคุณย่าและตัวหนังสืออีกรอบ! (กันมันแอบเด้งขึ้นมาเอง)
            if (Grandma_FullBody != null) Grandma_FullBody.SetActive(false);
            if (Grandma_BehindTable != null) Grandma_BehindTable.SetActive(false);
            if (instructionText != null) instructionText.gameObject.SetActive(false);

            // ออกจากฟังก์ชันไปเลย ปล่อยโต๊ะทำอาหารไว้เหมือนเดิม
            return;
        }
        // 1. ปิดการคลิกทุกปุ่มชั่วคราว กันผู้เล่นกดรัว
        SetButtonsAll(false);

        // 2. ปิดปุ่มพิเศษอื่น ๆ ด้วย (ถ้าเปิดอยู่)
        if (MortarButton) MortarButton.interactable = false;
        if (FilteredBowlButton) FilteredBowlButton.interactable = false;

        // 3. แสดงข้อความแจ้งเตือน
        SetInstruction("หลานทำผิดขั้นตอนแล้ว! ต้องกลับไปทำใหม่ตั้งแต่ต้นนะจ๊ะ");

        // 4. เริ่มนับถอยหลังเพื่อรีเซ็ตเกม
        StartCoroutine(CoResetToStart(2.0f));
    }

    /// <summary>
    /// รอเวลา แล้วสั่งรีเซ็ตทุกอย่างกลับไป Step 1
    /// </summary>
    IEnumerator CoResetToStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPoundingLocked = false;
        // --- 1. ล้างกระดาน (เหมือนเดิม) ---
        ShowOnly(Pot_Empty, Pot_WithWater, Pot_Boiling, Pot_WithCrabFat,
                 Pot_BoilingFat, Pot_Reducing, Pot_AddVet, Pot_Addseason, Pot_Final);

        if (Mortar_WithCrab) Mortar_WithCrab.SetActive(false);
        if (Mortar_Pounding) Mortar_Pounding.SetActive(false);
        if (sak_ani) sak_ani.SetActive(false);
        if (Mortar_Crushed) Mortar_Crushed.SetActive(false);
        if (Mortar_WithWater) Mortar_WithWater.SetActive(false);

        if (CoveredBowl) CoveredBowl.SetActive(false);
        if (FilteredBowl) FilteredBowl.SetActive(false);
        if (Bowl_Filtering) Bowl_Filtering.SetActive(false);

        if (MortarButton) MortarButton.gameObject.SetActive(false);
        if (FilteredBowlButton) FilteredBowlButton.gameObject.SetActive(false);

        // --- 2. จัดฉากใหม่ (แก้ตรงนี้) ---

        // เปิดภาพเริ่มต้น
        if (Mortar_Empty) Mortar_Empty.SetActive(true);
        if (Pot_Empty) Pot_Empty.SetActive(true);

        // เปิดปุ่มถ้วยอกปู
        if (BowlMeatButton) BowlMeatButton.gameObject.SetActive(true);

        // ✅✅✅ เพิ่มบรรทัดนี้ครับ! สำคัญมาก!
        // สั่งให้ปุ่มทุกอันที่โชว์อยู่ สามารถคลิกได้
        SetButtonsAll(true);

        // ตั้งค่า Step
        step = Step.PutMeatToMortar; // หรือ Step.Intro แล้วแต่คุณตั้ง

        // ข้อความเริ่มเกม
        SetInstruction("กลับมาเริ่มใหม่นะจ๊ะ! นำอกปูใส่ลงครก (กดที่ถ้วยอกปู)");
    }

    /// <summary>
    /// Helper: สั่งเปิด/ปิด ปุ่มวัตถุดิบหลักทั้งหมดในทีเดียว
    /// </summary>
    void SetButtonsAll(bool interactable)
    {
        // รายชื่อปุ่มวัตถุดิบทั้งหมด
        SetButtonState(BowlMeatButton, interactable);
        SetButtonState(BottleWaterButton, interactable);
        SetButtonState(BowlFilterButton, interactable);
        SetButtonState(BowlFatButton, interactable);
        SetButtonState(BowlVegButton, interactable);
        SetButtonState(BowlSeasoningButton, interactable);

        // ปุ่มสาก (ถ้ามี)
        SetButtonState(PestleButton, interactable);
    }

}
