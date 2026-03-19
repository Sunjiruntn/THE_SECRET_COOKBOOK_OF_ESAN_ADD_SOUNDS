using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public class CloseupController : MonoBehaviour
{
    [Header("End Game UI")]
    public GameObject resultPanel;      // ลาก ResultPanel มาใส่
    public TMP_Text resultText;         // ลาก ResultText มาใส่
    public Button actionButton;         // ลาก ActionButton มาใส่
    public TMP_Text actionButtonText;   // ลาก Text ในปุ่มมาใส่ (เผื่ออยากเปลี่ยนคำ)
    [SerializeField] RectTransform shovelRT;   // = ShovelImage.rectTransform
    [SerializeField] Vector2 shovelStartPos = new Vector2(278.99882f, 81.99969f); // ค่าที่เห็นใน Inspector

    [Header("Static UI")]
    public GameObject BG;
    public GameObject HoleSmooth;
    public GameObject HoleRough;

    [Header("Rims (top covers)")]
    public GameObject RimSmooth;   // ใช้กับ Smooth เท่านั้น
    public GameObject RimRough;    // ใช้กับ Rough เท่านั้น

    [Header("Masks (UI Mask)")]
    public GameObject MaskSmooth;  // Image + Mask (Show Mask Graphic = OFF)
    public GameObject MaskRough;   // Image + Mask (Show Mask Graphic = OFF)

    [Header("Animals (children of each mask)")]
    public RectTransform CrabImage;   // ลูกของ MaskRough
    public RectTransform SnakeImage;  // ลูกของ MaskSmooth

    [Header("Buttons (Legacy)")]
    public Button DigButton;          // ไม่ใช้แล้ว แต่คงไว้เผื่อ
    public Button BackButton;

    [Header("Rise Animation")]
    public float riseDistance = 120f;
    public float riseTime = 0.35f;

    private int holeId;
    private HoleManager.HoleResult result;
    private bool alreadyDug;

    [Header("Snake Slither")]
    public float snakeLeftOffset = 40f;
    public float snakeExtraTime = 0.15f;
    public float snakeSwayAmp = 6f;
    public float snakeSwayHz = 2.5f;

    // ------------------ NEW: Shovel UI ------------------
    [Header("Shovel UI")]
    public Image ShovelImage;          // รูปเสียมบน UI (ใส่ Button component บน GameObject เดียวกันได้)
    public Sprite shovelFrame1;        // เฟรมที่ 1
    public Sprite shovelFrame2;        // เฟรมที่ 2
    public float shovelFPS = 10f;      // ความเร็วสลับเฟรม (เฟรม/วินาที)
    public int shovelStrokes = 3;      // จิ้ม-งัด กี่ครั้งก่อนเฉลย
    public float shovelStrokeAmp = 10f;// ระยะสั่นแกว่ง (พิกเซล)
    public float shovelStrokeDur = 0.12f; // เวลาต่อ 1 จังหวะ (ลง-ขึ้น)

    [Tooltip("ตำแหน่งออฟเซ็ตตอนขุด (เช่น ก้มลงใกล้ปากหลุม)")]
    public Vector2 shovelDigOffset = new Vector2(18f, -12f);

    [Header("FX (optional)")]
    public ParticleSystem dirtFX;      // ฝุ่นดิน (วางใกล้ปากหลุม)
    public AudioSource sfxShovel;      // เสียงปักเสียม
    public AudioSource sfxReveal;      // เสียงเฉลย/โผล่

    [Header("Audio Settings (เพิ่มเติม)")]
    public AudioSource bgmSource;      // ลาก AudioSource ที่เปิด Loop เพลงพื้นหลังมาใส่
    public AudioSource sfxSource;      // ลาก AudioSource สำหรับเล่นคลิก/เสียงสัตว์มาใส่
    public AudioClip clickSfx;        // เสียงกดปุ่ม
    public AudioClip crabSfx;         // เสียงปู
    public AudioClip snakeSfx;        // เสียงงู

    private bool shovelBusy = false;   // กันคลิกรัวระหว่างอนิเมชัน
    private Vector2 shovelBasePos;     // เก็บตำแหน่งเดิมไว้คืนค่า

    // --- Up/Down press config ---
    [Header("Shovel Press Motion")]
    public RectTransform ShovelDownMarker;     // ตัวบอกตำแหน่งตอนกด (วางใน Scene แล้วลากมาใส่)
    public float pressDownTime = 0.15f;        // เวลากดลง
    public float holdAtBottom = 0.05f;        // ค้างก้นหลุมกี่วิ
    public float returnUpTime = 0.18f;        // เวลากลับขึ้น
    public bool returnUpAfterReveal = true;   // โผล่ผลแล้วให้เสียมถอยขึ้นไหม

    private Vector2 shovelUpPos;
    private Vector2 shovelDownPos;
    // 2) พิกัดแยกสำหรับ Smooth/Rough (ใส่ไว้ใกล้ๆ Shovel UI)
    [Header("Shovel Positions (per hole type)")]
    public Vector2 shovelUpPosSmooth = new Vector2(279f, 81f);   // ตั้งต้นของ Smooth
    public Vector2 shovelDownPosSmooth = new Vector2(225f, 0f);   // ตอนกดของ Smooth
    public Vector2 shovelUpPosRough = new Vector2(279f, 81f);   // ตั้งต้นของ Rough (ใส่เลขที่เหมาะ)
    public Vector2 shovelDownPosRough = new Vector2(225f, 0f);   // ตอนกดของ Rough (ใส่เลขที่เหมาะ)

    [SerializeField] private Button ShovelButton;
    private bool inputGuard = true;

    private int currentDigCount = 0;
    void Start()
    {
        // เล่นเพลงพื้นหลังทันที
        if (bgmSource != null && !bgmSource.isPlaying) bgmSource.Play();

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        // สุ่มว่าหลุมนี้ต้องขุด 4, 5 หรือ 6 ครั้ง (Random.Range int ตัวท้ายจะไม่ถูกนับ ต้องใช้ 7 ถึงจะได้ 6)
        shovelStrokes = Random.Range(4, 7);
        currentDigCount = 0;
        holeId = PlayerPrefs.GetInt("CurrentHole", -1);
        var mgr = HoleManager.Instance;
        if (holeId < 0 || mgr == null) { Debug.LogError("Closeup init error"); return; }

        mgr.RandomizeIfNeeded(holeId);
        result = mgr.GetHoleResult(holeId);
        alreadyDug = mgr.IsDug(holeId);
        bool isRough = (result == HoleManager.HoleResult.Rough);

        // เปิด/ปิดชุดหลุม/มาสก์/ริม
        if (BG) BG.SetActive(true);
        if (HoleSmooth) HoleSmooth.SetActive(!isRough);
        if (HoleRough) HoleRough.SetActive(isRough);
        if (MaskSmooth) MaskSmooth.SetActive(!isRough);
        if (MaskRough) MaskRough.SetActive(isRough);
        if (RimSmooth) RimSmooth.SetActive(!isRough);
        if (RimRough) RimRough.SetActive(isRough);

        // ย้ายเข้า Mask แบบ keep screen pos
        AttachShovelToActiveMask(isRough);

        // เลือกชุดพิกัดตามชนิดหลุม
        shovelUpPos = isRough ? shovelUpPosRough : shovelUpPosSmooth;
        shovelDownPos = isRough ? shovelDownPosRough : shovelDownPosSmooth;

        // ตั้งตำแหน่งเริ่มต้น (ครั้งเดียว)
        if (ShovelImage)
        {
            var rt = ShovelImage.rectTransform;
            rt.anchoredPosition = shovelUpPos;
        }

        // ปิด Animator ถ้าไม่ใช้
        var anim = ShovelImage ? ShovelImage.GetComponent<Animator>() : null;
        if (anim) anim.enabled = false;
        EnsureMaskSetup(isRough);
        ActivateShovel();          // ทำให้พร้อมตั้งแต่แรก
        if (ShovelButton) ShovelButton.interactable = false;

        // ดัน Rim อยู่บนสุด
        if (isRough && RimRough) RimRough.transform.SetAsLastSibling();
        if (!isRough && RimSmooth) RimSmooth.transform.SetAsLastSibling();

        // ซ่อนสัตว์ (ยังไม่ขุด)
        SetupStart(CrabImage);
        SetupStart(SnakeImage);

        // ตั้งพิกัดขึ้น/ลง ตามชนิดหลุม (ครั้งเดียว)
        if (isRough)
        {
            shovelUpPos = shovelUpPosRough;
            shovelDownPos = shovelDownPosRough;
        }
        else
        {
            shovelUpPos = shovelUpPosSmooth;
            shovelDownPos = shovelDownPosSmooth;
        }
        if (ShovelImage)
            ShovelImage.rectTransform.anchoredPosition = shovelUpPos;

        // ถ้าหลุมนี้ขุดแล้ว → โชว์ผลและปิดอินพุต
        if (alreadyDug)
        {
            if (isRough) ShowAtTop(CrabImage, false);
            else ShowAtTop(SnakeImage, true);
            if (DigButton) DigButton.interactable = false;
            if (ShovelImage) ShovelImage.raycastTarget = false;
        }

        // ปิดเส้นทางปุ่ม Dig เดิม (อย่าผูก OnClick ใหม่)
        if (DigButton != null)
        {
            DigButton.onClick.RemoveAllListeners();
            DigButton.interactable = false;
            // หรือ DigButton.gameObject.SetActive(false);
        }

        // ผูกปุ่มเสียม (ถ้าลากมาแล้ว)
        if (ShovelButton)
        {
            ShovelButton.onClick.RemoveAllListeners();
            ShovelButton.onClick.AddListener(OnClickShovel);
            ShovelButton.interactable = false; // ปิดไว้ก่อน กันคลิกค้างข้ามซีน
        }

        if (BackButton) BackButton.onClick.AddListener(() =>
        {
            if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx); // เสียงกดกลับ
            SceneManager.LoadScene("SelectHole");
        });

        // ปลด guard แล้วค่อยเปิดปุ่มเสียม
        StartCoroutine(SceneClickGuard());
    }


    IEnumerator SceneClickGuard()
    {
        float t = 0f;
        while (Input.GetMouseButton(0) && t < 1f) { t += Time.unscaledDeltaTime; yield return null; }
        yield return new WaitForSecondsRealtime(0.05f);

        inputGuard = false;
        ActivateShovel();   // << เปิดปุ่ม/รับคลิกอีกครั้งหลังปลดการ์ด
    }


    // เริ่มต้น: ปิด และวางไว้ใต้ปากหลุม
    void SetupStart(RectTransform rt)
    {
        if (!rt) return;
        rt.gameObject.SetActive(false);
        var p = rt.anchoredPosition;
        p.y = -Mathf.Abs(riseDistance);
        rt.anchoredPosition = p;
    }

    // โชว์ที่ปลายทางบนปากหลุม (isSnake=true จะเลื่อนไปซ้าย)
    void ShowAtTop(RectTransform rt, bool isSnake)
    {
        if (!rt) return;
        rt.gameObject.SetActive(true);
        var p = rt.anchoredPosition;
        p.y = 0f;
        if (isSnake) p.x -= Mathf.Abs(snakeLeftOffset);
        rt.anchoredPosition = p;
    }

    // ------------------ NEW: Handler คลิกที่รูปเสียม ------------------
    // ผูกใน Inspector: เพิ่ม Button ที่ GameObject ของ ShovelImage แล้วลาก OnClick ไปที่ฟังก์ชันนี้
    void FixShovelPosition()
    {
        if (!ShovelImage) return;

        // รีเซ็ต anchor/pivot ให้อยู่ตรงกลาง
        var rt = ShovelImage.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // รีเซ็ต local position ให้ตรงกลาง Mask
        rt.anchoredPosition = Vector2.zero;
    }

    public void OnClickShovel()
    {
        // [แทรกเพิ่ม] เล่นเสียงคลิกทันทีที่กดเสียม
        if (sfxSource != null && clickSfx != null && !shovelBusy && !inputGuard)
        {
            sfxSource.PlayOneShot(clickSfx);
        }

        RectTransform rt = ShovelImage != null ? ShovelImage.rectTransform : null;
        Debug.Log($"from={rt.anchoredPosition} to={shovelDownPos} (isRough={result})");


        if (inputGuard) return;
        if (shovelBusy) return;

        var mgr = HoleManager.Instance;
        if (mgr == null) return;
        if (mgr.IsDug(holeId)) return;

        // อย่าเช็ค IsPointerOverGameObject() สำหรับปุ่ม UI
        StartCoroutine(ShovelDigRoutine()); // ใช้คอร์รุตีนเลื่อน
    }

    // ---------- ตั้งค่า/ตรวจ Mask ให้ถูก ----------
    void EnsureMaskSetup(bool isRough)
    {
        var maskGO = isRough ? MaskRough : MaskSmooth;
        if (!maskGO)
        {
            Debug.LogWarning("[Closeup] Mask GameObject is null.");
            return;
        }

        var mask = maskGO.GetComponent<Mask>();
        var rectMask = maskGO.GetComponent<RectMask2D>();
        if (!mask && !rectMask)
            Debug.LogWarning("[Closeup] MaskRough/MaskSmooth ไม่มี Mask/RectMask2D");

        // ถ้าเป็น Mask ปกติ → ต้องมี Image+Sprite และปิด showMaskGraphic ที่ 'Mask component'
        if (mask)
        {
            var img = maskGO.GetComponent<Image>();
            if (!img || !img.sprite)
                Debug.LogWarning("[Closeup] ใช้ Mask แต่ไม่มี Image/Sprite บน GO ของ Mask");
            mask.showMaskGraphic = false;
        }

        // ลูกที่จะถูกบังต้อง maskable และต้องไม่มี Canvas คั่น
        if (ShovelImage)
        {
            ShovelImage.maskable = true;

            // เตือนถ้ามี Canvas ซ้อนระหว่าง Mask กับ Shovel (มาสก์จะไม่ทำงาน)
            var t = ShovelImage.transform.parent;
            while (t != null && t.gameObject != maskGO)
            {
                if (t.GetComponent<Canvas>() != null)
                    Debug.LogWarning("[Closeup] พบ Canvas คั่นระหว่าง Mask กับ ShovelImage → มาสก์จะไม่ทำงาน");
                t = t.parent;
            }
        }
    }
    void ActivateShovel()
    {
        if (!ShovelImage) return;

        // ต่อสายปุ่มอัตโนมัติ ถ้ายังไม่ได้ลาก
        if (!ShovelButton)
            ShovelButton = ShovelImage.GetComponent<Button>() ?? ShovelImage.gameObject.AddComponent<Button>();

        ShovelImage.raycastTarget = true;
        ShovelImage.maskable = true;
        ShovelImage.material = null;          // กัน Shader แปลก ๆ

        // เปิดปุ่ม + ผูก onClick ใหม่ให้ชัวร์
        if (ShovelButton)
        {
            ShovelButton.onClick.RemoveAllListeners();
            ShovelButton.onClick.AddListener(OnClickShovel);
            ShovelButton.interactable = true; // << บังคับเปิด
                                              // ปิด Transition ชั่วคราวกันสีเทาหลอกตา
            ShovelButton.transition = Selectable.Transition.None;
        }

        // ถ้ามีพาเรนต์ที่เป็น CanvasGroup ให้เปิด
        var cg = ShovelImage.GetComponentInParent<CanvasGroup>();
        if (cg)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.ignoreParentGroups = false;
        }

        // ดันเสียมขึ้นบนสุดกันโดน UI บัง
        ShovelImage.transform.SetAsLastSibling();

        Debug.Log("[Closeup] ActivateShovel: ready");
    }

    // อนิเมชัน “ขุด”: สลับเฟรม 1↔2 + สั่น/แกว่ง + ออฟเซ็ตเข้าใกล้ปากหลุม แล้วค่อยเฉลยผล
    // อนิเมชันหลัก: กดลง -> งัดๆๆ -> ยกขึ้น -> เช็คว่าครบจำนวนหรือยัง
    IEnumerator ShovelDigRoutine()
    {
        shovelBusy = true; // ล็อกปุ่ม

        var rt = ShovelImage.rectTransform;

        // 1. จิ้มเสียมลงไปที่ก้นหลุม
        // Debug.Log($"[Down] Lerp {rt.anchoredPosition} -> {shovelDownPos}");
        yield return LerpUI(rt, rt.anchoredPosition, shovelDownPos, pressDownTime);


        // --- [ส่วนที่แก้ไข] : ทำท่างัดขึ้นลงตามจังหวะ ---
        if (sfxShovel) sfxShovel.Play(); // เล่นเสียงขุด
        if (dirtFX) dirtFX.Play();       // เล่นฝุ่น

        // **เรียกใช้ Routine ใหม่ที่นี่** (แทนการ Shake แบบสุ่ม)
        // เลข 3 คือจำนวนครั้งที่งัดต่อการกด 1 ที (ปรับได้)
        yield return ShovelPryRoutine(rt, shovelDownPos, 3);

        if (holdAtBottom > 0f) yield return new WaitForSeconds(holdAtBottom);
        // -------------------------------------------


        // 2. ยกเสียมขึ้นกลับไปท่าเตรียม
        // Debug.Log($"[Up] Lerp {rt.anchoredPosition} -> {shovelUpPos}");
        yield return LerpUI(rt, rt.anchoredPosition, shovelUpPos, returnUpTime);

        // 3. เช็คจำนวนครั้งรวม
        currentDigCount++;
        // Debug.Log($"Dug Total: {currentDigCount}/{shovelStrokes}");

        if (currentDigCount >= shovelStrokes)
        {
            // ถ้าครบโควต้า 4-6 ครั้งแล้ว -> เฉลย!
            RevealResult();
        }

        shovelBusy = false; // ปลดล็อกปุ่ม
    }
    // ฟังก์ชันใหม่: ทำท่างัดขึ้นลงเป็นจังหวะ (Pry motion)
    // basePos คือตำแหน่งก้นหลุม, pryCount คือจำนวนครั้งที่จะงัดต่อการกด 1 ที
    IEnumerator ShovelPryRoutine(RectTransform rt, Vector2 basePos, int pryCount)
    {
        // คำนวณตำแหน่ง "งัดขึ้น" โดยใช้ค่า Amp ที่คุณตั้งไว้
        Vector2 topPos = basePos + new Vector2(0f, shovelStrokeAmp);

        // เวลาในการขยับขึ้น (ครึ่งหนึ่งของ Dur) และลง (อีกครึ่ง)
        float halfDur = shovelStrokeDur / 2f;

        for (int i = 0; i < pryCount; i++)
        {
            // จังหวะงัดขึ้น (ใช้ LerpUI ตัวเดิมช่วย)
            yield return LerpUI(rt, basePos, topPos, halfDur);
            // จังหวะกดลง
            yield return LerpUI(rt, topPos, basePos, halfDur);
        }

        // จบแล้วบังคับให้อยู่ที่ก้นหลุมเป๊ะๆ
        rt.anchoredPosition = basePos;
    }
    IEnumerator LerpUI(RectTransform rt, Vector2 from, Vector2 to, float time)
    {
        time = Mathf.Max(0.0001f, time);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            rt.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    // ------------------ เดิม: ถูกเรียกตอนกดปุ่ม (คงไว้เผื่อ) ------------------
    public void OnClickDig_Legacy()
    {
        if (HoleManager.Instance == null || HoleManager.Instance.IsDug(holeId)) return;
        // ถ้ายังอยากใช้ปุ่มเดิม ให้เรียกอนิเมชันเสียมก่อนเฉลย:
        StartCoroutine(ShovelDigRoutine());
    }

    void RevealResult()
    {
        var mgr = HoleManager.Instance;
        if (mgr == null || mgr.IsDug(holeId)) return;

        mgr.MarkDug(holeId); // บันทึกและนับจำนวนปู

        bool isRough = (result == HoleManager.HoleResult.Rough);
        RectTransform target = isRough ? CrabImage : SnakeImage;

        if (sfxReveal) sfxReveal.Play();
        target.gameObject.SetActive(true);
        target.SetAsLastSibling();

        // ปิดอินพุตทั้งหมดทันที
        if (DigButton) DigButton.interactable = false;
        if (ShovelImage) ShovelImage.raycastTarget = false;

        if (isRough) // --- เจอปู ---
        {
            if (sfxSource && crabSfx) sfxSource.PlayOneShot(crabSfx); // เสียงปู

            // อนิเมชันปูเดิน
            Vector2 startPos = new Vector2(61f, -32f);
            target.anchoredPosition = startPos;
            float moveLeftAmount = 50f;
            Vector2 endPos = new Vector2(startPos.x - moveLeftAmount, 0f);
            StartCoroutine(CrabWalkRoutine(target, startPos, endPos, 1.8f));

            // เช็คว่าชนะหรือยัง?
            if (mgr.crabCount >= 5) // หรือใช้ mgr.crabRequired
            {
                StartCoroutine(GameWinRoutine());
            }
        }
        else // --- เจองู (แพ้) ---
        {
            if (sfxSource && snakeSfx) sfxSource.PlayOneShot(snakeSfx); // เสียงงู

            // อนิเมชันงู
            float snakeMoveX = 20f;
            StartCoroutine(RiseAndSlither(
                target, -Mathf.Abs(riseDistance), 0f, snakeMoveX,
                riseTime + snakeExtraTime, snakeSwayAmp, snakeSwayHz
            ));

            StartCoroutine(GameOverRoutine());
        }
    }

    IEnumerator CrabWalkRoutine(RectTransform rt, Vector2 from, Vector2 to, float duration)
    {
        float t = 0f;
        rt.anchoredPosition = from; // เริ่มที่ (61, -32) ชัวร์ๆ

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            // ใช้ SmoothStep เพื่อให้การเคลื่อนที่ดูนุ่มนวล (เริ่มช้า-จบช้า)
            float k = Mathf.SmoothStep(0f, 1f, t);

            // เลื่อนทั้ง X และ Y พร้อมกัน
            rt.anchoredPosition = Vector2.Lerp(from, to, k);

            yield return null;
        }
        rt.anchoredPosition = to; // จบที่ปลายทางเป๊ะๆ
    }
    IEnumerator RiseUI(RectTransform rt, float fromY, float toY, float duration)
    {
        float t = 0f;
        var pos = rt.anchoredPosition;
        pos.y = fromY;
        rt.anchoredPosition = pos;

        duration = Mathf.Max(0.0001f, duration);
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            pos.y = Mathf.Lerp(fromY, toY, k);
            rt.anchoredPosition = pos;
            yield return null;
        }
        pos.y = toY;
        rt.anchoredPosition = pos;
    }

    // เปลี่ยนชื่อตัวแปรจาก leftOffset เป็น xOffset เพื่อให้เข้าใจง่ายขึ้น
    IEnumerator RiseAndSlither(RectTransform rt, float fromY, float toY, float xOffset, float duration,
                               float swayAmp, float swayHz)
    {
        float t = 0f;
        var start = rt.anchoredPosition;
        start.y = fromY;
        rt.anchoredPosition = start;

        float baseX = start.x;

        duration = Mathf.Max(0.0001f, duration);
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            float y = Mathf.Lerp(fromY, toY, k);

            // [จุดที่แก้]: ลบ -Mathf.Abs ออก! ให้มันขยับตามค่า xOffset ที่ส่งมาตรงๆ
            // ถ้าส่งค่าบวก = ขวา, ค่าลบ = ซ้าย
            float xSlide = Mathf.Lerp(0f, xOffset, k);

            float xSway = Mathf.Sin((Time.timeSinceLevelLoad) * Mathf.PI * 2f * swayHz) * swayAmp;

            rt.anchoredPosition = new Vector2(baseX + xSlide + xSway, y);
            yield return null;
        }

        // จบที่ปลายทาง (บวก xOffset เข้าไปตรงๆ)
        rt.anchoredPosition = new Vector2(baseX + xOffset, toY);
    }

    public void OnClickBack()
    {
        if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx); // เสียงกดกลับ
        SceneManager.LoadScene("SelectHole");
    }
    void AttachShovelToActiveMask(bool isRough)
    {
        if (!ShovelImage) return;
        Transform parent = (isRough ? MaskRough : MaskSmooth)?.transform;
        if (!parent) return;

        var rt = ShovelImage.rectTransform;
        rt.SetParent(parent, true); // << true = คง world/screen pos
                                    // จัด anchor/pivot ให้กลาง (กันเพี้ยนเวลา scale)
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        // ไม่ต้อง rt.anchoredPosition = Vector2.zero ที่นี่!
    }

    // ------------------ Game Over / Win Logic ------------------

    // ฟังก์ชันเมื่อเจองู (แพ้)
    IEnumerator GameOverRoutine()
    {
        // 1. รอให้ตกใจงูแป๊บนึง
        yield return new WaitForSeconds(2.0f);

        // 2. เปิดหน้าต่างแจ้งเตือนแพ้ (UI ResultPanel)
        if (resultPanel) resultPanel.SetActive(true);

        if (resultText)
            resultText.text = "หลานขุดเจองู ต้องกลับไปเริ่มขุดใหม่นะจ๊ะ \nเดี๋ยวจะสอนใหม่อีกรอบ";

        if (actionButtonText)
            actionButtonText.text = "เริ่มใหม่";

        // 3. ตั้งค่าปุ่มกดเริ่มใหม่
        if (actionButton)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() =>
            {
                if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx); // เสียงกดเริ่มใหม่

                // A. รีเซ็ตข้อมูลปู/หลุม ทั้งหมด
                if (HoleManager.Instance != null) HoleManager.Instance.ResetAndRandomize();

                // B. สำคัญมาก: สั่งให้ Tutorial ลืมว่าเคยสอนแล้ว (เพื่อให้สอนใหม่)
                TutorialManager.HasShown = false;

                // C. กลับไปหน้าเลือกหลุม
                SceneManager.LoadScene("SelectHole");
            });
        }
    }
    // ฟังก์ชันเมื่อครบ 5 ตัว (ชนะ)
    IEnumerator GameWinRoutine()
    {
        // รอให้ปูเดินเสร็จ (2 วินาที)
        yield return new WaitForSeconds(0.5f);

        // เปิดหน้าต่าง UI
        if (resultPanel) resultPanel.SetActive(true);

        // ตั้งข้อความ
        if (resultText)
            resultText.text = "หลานจับปูครบแล้ว ไปทำอาหารกันเลย";

        if (actionButtonText)
            actionButtonText.text = "เข้าครัว";

        // ตั้งค่าปุ่ม ให้ไปซีน Cookingstage
        if (actionButton)
        {
            // 1. ล้างคำสั่งเก่าที่อาจค้างอยู่ออกก่อน (กันกดเบิ้ล)
            actionButton.onClick.RemoveAllListeners();

            // 2. สั่งว่า "ถ้าโดนกด ให้ไปเรียกฟังก์ชัน GoToKitchen นะ"
            actionButton.onClick.AddListener(() =>
            {
                if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx); // เสียงกดเข้าครัว
                GoToKitchen();
            });
        }
    }
    void GoToKitchen()
    {
        // ย้าย logic การเซฟและการเปลี่ยนฉากมาไว้ตรงนี้
        if (GameDataController.Instance != null)
        {
            // บันทึกว่าผ่านด่านย่อยนี้แล้ว
            GameDataController.Instance.PassLevel(0); // หรือ index ของจังหวัดตามที่คุณตั้ง

            // เซฟชื่อฉากล่าสุด
            GameDataController.Instance.SaveCurrentScene("CookingStage");
        }

        if (HoleManager.Instance != null)
        {
            Destroy(HoleManager.Instance.gameObject);
        }

        // ไปฉากทำอาหาร
        SceneManager.LoadScene("CookingStage");
    }
}