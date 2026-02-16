using UnityEngine;

public class BeanStatus : MonoBehaviour
{
    [Header("Sprite Settings")]
    public Sprite rawSprite;
    public Sprite cookedSprite;
    public Sprite burntSprite;

    [Header("Cooking Settings")]
    public float currentCookLevel = 0f;
    public float cookThreshold = 50f;
    public float burnThreshold = 100f;

    public bool isBurnt = false;
    public bool isCooked = false; // ตัวแปรจำว่า "ฉันสุกหรือยัง" (ของเม็ดนี้)

    private GameManager gameManager;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = rawSprite;
        gameManager = FindObjectOfType<GameManager>();
    }

    // ฟังก์ชันสั่งตาย (เรียกโดย HeatManager)
    public void ForceBurn()
    {
        if (isBurnt) return;

        // ถ้าเคยสุกมาก่อน ต้องแจ้งลดแต้มสุกคืนด้วย (เพราะตอนนี้เปลี่ยนเป็นไหม้แล้ว)
        if (isCooked)
        {
            isCooked = false;
            if (gameManager != null) gameManager.ReportCookedBean(false);
        }

        isBurnt = true;

        if (sr != null && burntSprite != null)
        {
            sr.sprite = burntSprite;
        }

        currentCookLevel = burnThreshold + 50f;

        // แจ้ง GameManager ว่าไหม้เพิ่ม 1 เม็ด
        if (gameManager != null)
        {
            gameManager.ReportBurntBean();
            // ไม่ต้องบวก gameManager.burntCount++ ที่นี่ เพราะในฟังก์ชัน ReportBurntBean ของ GameManager มันบวกให้อยู่แล้ว
        }
    }

    // ฟังก์ชันรับความร้อนปกติ
    public void AddHeat(float amount)
    {
        if (isBurnt) return;

        // เช็คว่าเตา Overheat ไหม?
        bool isOverheating = false;
        if (HeatManager.Instance != null)
        {
            isOverheating = HeatManager.Instance.currentHeat >= HeatManager.Instance.overheatThreshold;
        }

        currentCookLevel += amount;

        // --- โซนแก้ไข ---

        // 1. ถ้าเตาปกติ (ไม่แดง): ล็อคความร้อนไม่ให้เกิน 99.9 (เลี้ยงไฟได้เรื่อยๆ)
        if (!isOverheating)
        {
            if (currentCookLevel >= burnThreshold)
            {
                currentCookLevel = burnThreshold - 0.1f;
            }
        }
        // 2. ถ้าเตาแดง (Overheat): ไม่มีการปรานี!
        else
        {
            // ถ้าความร้อนทะลุเพดานไหม้ -> สั่งไหม้ทันที!
            if (currentCookLevel >= burnThreshold)
            {
                ForceBurn(); // 🔥 เรียกฟังก์ชันไหม้เลย!
                return; // จบการทำงานรอบนี้
            }
        }

        CheckStatus();
    }

    void CheckStatus()
    {
        if (isBurnt) return;

        // 1. เงื่อนไข: สุก
        if (currentCookLevel >= cookThreshold && currentCookLevel < burnThreshold)
        {
            sr.sprite = cookedSprite;

            if (!isCooked) // ถ้าเพิ่งสุกครั้งแรก
            {
                isCooked = true;
                if (gameManager != null) gameManager.ReportCookedBean(true);
            }
        }
        // 2. เงื่อนไข: ดิบ (เผื่อความร้อนลด)
        else if (currentCookLevel < cookThreshold)
        {
            sr.sprite = rawSprite;

            if (isCooked) // ถ้าเคยสุกแล้วกลับมาดิบ
            {
                isCooked = false;
                if (gameManager != null) gameManager.ReportCookedBean(false);
            }
        }
    }

    void FixedUpdate()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.velocity.magnitude > 10f)
        {
            rb.velocity = rb.velocity.normalized * 10f;
        }
    }
}