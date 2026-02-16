using UnityEngine;

public class PotPaddleController : MonoBehaviour
{
    private bool isDragging = false;
    [Header("Paddle Parts")]
    public Transform paddleTip;
    public SpriteRenderer paddleRenderer;
    public Sprite[] paddleVisuals; // 0: สะอาด, 1: เปื้อนแป้งดิบ, 2: เปื้อนแป้งสุก
    [Header("Phase 5: Ending (Outro)")]
    public GameObject outroPanel;
    [Header("Boundary & Pot Settings")]
    public Transform potCenter;
    public float potStirRadius = 1.0f;

    [Header("Batter Stages in Pot")]
    // ลากใส่ให้ครบ 4 ช่อง: 0: เริ่มต้น, 1: หนืดขึ้น, 2: มีถั่วแปะ, 3: สุกสุดท้าย
    public GameObject[] batterStages;
    public GameObject beanBowl;

    [Header("Stir Settings (Distance Based)")]
    public float distToStage2 = 40f;   // ระยะคนจาก 1 ไป 2
    public float distToStage4 = 60f;   // ระยะคนจาก 3 ไป 4

    private float currentDist = 0f;
    private int currentStep = 1;      // เริ่มต้นที่สเตจ 1
    private bool canAddBeans = false; // สถานะว่ากดถั่วได้หรือยัง
    private bool beansAdded = false;  // สถานะว่าใส่ถั่วไปแล้วหรือยัง
    private Vector3 lastTipPos;
    void Awake()
    {
        // สั่งปิดขนมทุกสเตจทันทีที่เกมโหลด! (ไม่สนว่าจะติ๊กอะไรไว้ใน Inspector)
        if (batterStages != null)
        {
            foreach (GameObject stage in batterStages)
            {
                if (stage != null) stage.SetActive(false);
            }
        }
        if (outroPanel != null)
        {
            outroPanel.SetActive(false);
        }
    }
    void Start()
    {
        // แก้ไข: ไม่ต้องสั่งปิดอะไรในนี้แล้ว เพราะเราจะให้ InitPot เป็นคนจัดการตอนเริ่มเททีเดียวจ้ะ
        // แค่เก็บตำแหน่งเริ่มต้นของปลายไม้พายก็พอ
        if (paddleTip != null) lastTipPos = paddleTip.position;
    }

    // ฟังก์ชันนี้จะถูกเรียกโดย PouringSystem เมื่อเทเสร็จ
    public void InitPot()
    {
        // 1. สั่งปิดขนมทุกสเตจก่อนเพื่อล้างกระดาน
        foreach (GameObject stage in batterStages)
        {
            if (stage != null) stage.SetActive(false);
        }

        // 2. บังคับเปิดสเตจแรก (BatterInPot) ทันที!
        if (batterStages.Length > 0 && batterStages[0] != null)
        {
            batterStages[0].SetActive(true);
        }

        // 3. รีเซ็ตค่าต่างๆ ให้พร้อมเริ่มเล่น
        UpdatePaddleVisual(0); // ไม้พายสะอาด
        currentStep = 1;
        currentDist = 0f;
        canAddBeans = false;
        beansAdded = false;
        // เปิดให้ Update ทำงาน
        this.enabled = true;
    }

    void Update()
    {
        // Safety Check
        if (potCenter == null || paddleTip == null) return;

        // 1. เช็กจังหวะ "กดเมาส์ลง" (Click Start)
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            // ยิงเลเซอร์เช็กว่าเมาส์ชี้โดนอะไร
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            // ถ้าโดนวัตถุ และวัตถุนั้นคือ "ไม้พายนี้" (gameObject นี้)
            if (hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
                isDragging = true; // เริ่มลากได้!
            }
        }

        // 2. เช็กจังหวะ "ปล่อยเมาส์" (Click Release)
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false; // หยุดลากทันที
        }

        // 3. ถ้ากำลังลากอยู่ ให้ขยับและคำนวณการคน
        if (isDragging)
        {
            HandleMovement();

            // คำนวณความสุก (เฉพาะตอนที่ยังไม่สุก และไม่ใช่ช่วงรอใส่ถั่ว)
            if (currentStep < 4 && !canAddBeans)
            {
                CalculateStir();
            }
        }
    }

    void HandleMovement()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector3 tipOffset = paddleTip.position - transform.position;
        Vector3 targetTipPos = mousePos + tipOffset;
        Vector3 offset = targetTipPos - potCenter.position;

        if (offset.magnitude > potStirRadius)
            offset = offset.normalized * potStirRadius;

        transform.position = (potCenter.position + offset) - tipOffset;
    }

    void CalculateStir()
    {
        float d = Vector3.Distance(paddleTip.position, lastTipPos);
        if (d > 0.01f)
        {
            currentDist += d;
            UpdateProgress();
        }
        lastTipPos = paddleTip.position;
    }

    void UpdateProgress()
    {
        // จากสเตจ 1 ไป 2: ขนมหนืดขึ้น
        if (currentStep == 1 && currentDist >= distToStage2)
        {
            currentStep = 2;
            SetBatterStage(1);      // แสดงสเตจ 2
            UpdatePaddleVisual(1);  // ไม้พายเริ่มเปื้อนแป้งดิบ
            canAddBeans = true;     // เปิดโหมดให้กดถั่วได้
            beanBowl.SetActive(true);
            Debug.Log("ขนมหนืดแล้ว ใส่ถั่วได้เลย!");
        }
        // จากสเตจ 3 ไป 4: คนต่อจนสุกสุดท้าย
        else if (currentStep == 3 && currentDist >= distToStage4)
        {
            currentStep = 4;
            SetBatterStage(3);      // แสดงสเตจ 4 (สุกแล้ว)
            UpdatePaddleVisual(2);  // ไม้พายเปื้อนแป้งสุก
            Debug.Log("ขนมสุกแล้วจ้า!");
            if (outroPanel != null) outroPanel.SetActive(true);

            // 2. ไม้พายหายตัวไป (ปิดตัวเอง)
            if (outroPanel != null)
            {
                // อนุญาตให้จบเกมได้แล้ว!
                outroPanel.GetComponent<EndingSystem>().canEndGame = true;
                outroPanel.SetActive(true);
            }
            this.gameObject.SetActive(false);
        }

    }

    public void AddBeans()
    {
        if (canAddBeans)
        {
            canAddBeans = false;
            beansAdded = true;
            currentStep = 3;        // ข้ามไปสเตจ 3
            currentDist = 0f;       // รีเซ็ตระยะคนเพื่อเริ่มนับไปสเตจ 4
            SetBatterStage(2);      // แสดงสเตจ 3 (มีถั่วแปะ)
            beanBowl.SetActive(false);
            Debug.Log("ใส่ถั่วแล้ว คนต่อให้สุกนะ!");
        }
    }

    void UpdatePaddleVisual(int index)
    {
        if (index < paddleVisuals.Length && paddleVisuals[index] != null)
        {
            paddleRenderer.sprite = paddleVisuals[index];
        }
    }

    void SetBatterStage(int index)
    {
        if (batterStages == null || batterStages.Length == 0) return;

        for (int i = 0; i < batterStages.Length; i++)
        {
            if (batterStages[i] != null)
            {
                // เปิดเฉพาะอันที่ตรงกับ index นอกนั้นปิดหมด
                batterStages[i].SetActive(i == index);
            }
        }
    }
}