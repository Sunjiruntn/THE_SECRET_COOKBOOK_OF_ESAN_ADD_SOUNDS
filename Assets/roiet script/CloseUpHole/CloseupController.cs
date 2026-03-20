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

            StopAllAudioAndCleanUp();  // หยุดเสียงทั้งหมดก่อนเปลี่ยนฉาก

            SceneManager.LoadScene("SelectHole");
        });

        // ปลด guard แล้วค่อยเปิดปุ่มเสียม
        StartCoroutine(SceneClickGuard());
    }

    // ────────────────────────────────────────────────
    // ฟังก์ชันใหม่ – หยุดและเคลียร์เสียงทั้งหมดก่อนเปลี่ยนฉาก
    // ────────────────────────────────────────────────
    private void StopAllAudioAndCleanUp()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }

        if (sfxSource != null)
        {
            sfxSource.Stop();
            sfxSource.clip = null;
        }

        if (sfxShovel != null) sfxShovel.Stop();
        if (sfxReveal != null) sfxReveal.Stop();

        if (dirtFX != null && dirtFX.isPlaying) dirtFX.Stop();
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

        StartCoroutine(ShovelDigRoutine()); // ใช้คอร์รุตีนเลื่อน
    }

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

        if (mask)
        {
            var img = maskGO.GetComponent<Image>();
            if (!img || !img.sprite)
                Debug.LogWarning("[Closeup] ใช้ Mask แต่ไม่มี Image/Sprite บน GO ของ Mask");
            mask.showMaskGraphic = false;
        }

        if (ShovelImage)
        {
            ShovelImage.maskable = true;

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

        if (!ShovelButton)
            ShovelButton = ShovelImage.GetComponent<Button>() ?? ShovelImage.gameObject.AddComponent<Button>();

        ShovelImage.raycastTarget = true;
        ShovelImage.maskable = true;
        ShovelImage.material = null;

        if (ShovelButton)
        {
            ShovelButton.onClick.RemoveAllListeners();
            ShovelButton.onClick.AddListener(OnClickShovel);
            ShovelButton.interactable = true;
            ShovelButton.transition = Selectable.Transition.None;
        }

        var cg = ShovelImage.GetComponentInParent<CanvasGroup>();
        if (cg)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.ignoreParentGroups = false;
        }

        ShovelImage.transform.SetAsLastSibling();

        Debug.Log("[Closeup] ActivateShovel: ready");
    }

    IEnumerator ShovelDigRoutine()
    {
        shovelBusy = true;

        var rt = ShovelImage.rectTransform;

        yield return LerpUI(rt, rt.anchoredPosition, shovelDownPos, pressDownTime);

        if (sfxShovel) sfxShovel.Play();
        if (dirtFX) dirtFX.Play();

        yield return ShovelPryRoutine(rt, shovelDownPos, 3);

        if (holdAtBottom > 0f) yield return new WaitForSeconds(holdAtBottom);

        yield return LerpUI(rt, rt.anchoredPosition, shovelUpPos, returnUpTime);

        currentDigCount++;

        if (currentDigCount >= shovelStrokes)
        {
            RevealResult();
        }

        shovelBusy = false;
    }

    IEnumerator ShovelPryRoutine(RectTransform rt, Vector2 basePos, int pryCount)
    {
        Vector2 topPos = basePos + new Vector2(0f, shovelStrokeAmp);
        float halfDur = shovelStrokeDur / 2f;

        for (int i = 0; i < pryCount; i++)
        {
            yield return LerpUI(rt, basePos, topPos, halfDur);
            yield return LerpUI(rt, topPos, basePos, halfDur);
        }

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

    public void OnClickDig_Legacy()
    {
        if (HoleManager.Instance == null || HoleManager.Instance.IsDug(holeId)) return;
        StartCoroutine(ShovelDigRoutine());
    }

    void RevealResult()
    {
        var mgr = HoleManager.Instance;
        if (mgr == null || mgr.IsDug(holeId)) return;

        mgr.MarkDug(holeId);

        bool isRough = (result == HoleManager.HoleResult.Rough);
        RectTransform target = isRough ? CrabImage : SnakeImage;

        if (sfxReveal) sfxReveal.Play();
        target.gameObject.SetActive(true);
        target.SetAsLastSibling();

        if (DigButton) DigButton.interactable = false;
        if (ShovelImage) ShovelImage.raycastTarget = false;

        if (isRough) // ปู
        {
            if (sfxSource && crabSfx) sfxSource.PlayOneShot(crabSfx);

            Vector2 startPos = new Vector2(61f, -32f);
            target.anchoredPosition = startPos;
            float moveLeftAmount = 50f;
            Vector2 endPos = new Vector2(startPos.x - moveLeftAmount, 0f);
            StartCoroutine(CrabWalkRoutine(target, startPos, endPos, 1.8f));

            if (mgr.crabCount >= 5)
            {
                StartCoroutine(GameWinRoutine());
            }
        }
        else // งู
        {
            if (sfxSource && snakeSfx) sfxSource.PlayOneShot(snakeSfx);

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
        rt.anchoredPosition = from;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.SmoothStep(0f, 1f, t);
            rt.anchoredPosition = Vector2.Lerp(from, to, k);
            yield return null;
        }
        rt.anchoredPosition = to;
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

            float xSlide = Mathf.Lerp(0f, xOffset, k);
            float xSway = Mathf.Sin((Time.timeSinceLevelLoad) * Mathf.PI * 2f * swayHz) * swayAmp;

            rt.anchoredPosition = new Vector2(baseX + xSlide + xSway, y);
            yield return null;
        }

        rt.anchoredPosition = new Vector2(baseX + xOffset, toY);
    }

    public void OnClickBack()
    {
        if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);

        StopAllAudioAndCleanUp();  // หยุดเสียงก่อนกลับ

        SceneManager.LoadScene("SelectHole");
    }

    void AttachShovelToActiveMask(bool isRough)
    {
        if (!ShovelImage) return;
        Transform parent = (isRough ? MaskRough : MaskSmooth)?.transform;
        if (!parent) return;

        var rt = ShovelImage.rectTransform;
        rt.SetParent(parent, true);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(2.0f);

        StopAllAudioAndCleanUp();  // หยุดเสียงก่อนเปิด ResultPanel และเปลี่ยนฉาก

        if (resultPanel) resultPanel.SetActive(true);

        if (resultText)
            resultText.text = "หลานขุดเจองู ต้องกลับไปเริ่มขุดใหม่นะจ๊ะ \nเดี๋ยวจะสอนใหม่อีกรอบ";

        if (actionButtonText)
            actionButtonText.text = "เริ่มใหม่";

        if (actionButton)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() =>
            {
                if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);

                StopAllAudioAndCleanUp();  // หยุดอีกครั้งก่อนรีเซ็ตและเปลี่ยนฉาก

                if (HoleManager.Instance != null) HoleManager.Instance.ResetAndRandomize();
                TutorialManager.HasShown = false;
                SceneManager.LoadScene("SelectHole");
            });
        }
    }

    IEnumerator GameWinRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        StopAllAudioAndCleanUp();  // หยุดก่อนเปิด ResultPanel

        if (resultPanel) resultPanel.SetActive(true);

        if (resultText)
            resultText.text = "หลานจับปูครบแล้ว ไปทำอาหารกันเลย";

        if (actionButtonText)
            actionButtonText.text = "เข้าครัว";

        if (actionButton)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() =>
            {
                if (sfxSource && clickSfx) sfxSource.PlayOneShot(clickSfx);

                StopAllAudioAndCleanUp();  // หยุดก่อนไปฉากครัว

                GoToKitchen();
            });
        }
    }

    void GoToKitchen()
    {
        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.PassLevel(0);
            GameDataController.Instance.SaveCurrentScene("CookingStage");
        }

        if (HoleManager.Instance != null)
        {
            Destroy(HoleManager.Instance.gameObject);
        }

        SceneManager.LoadScene("CookingStage");
    }
}