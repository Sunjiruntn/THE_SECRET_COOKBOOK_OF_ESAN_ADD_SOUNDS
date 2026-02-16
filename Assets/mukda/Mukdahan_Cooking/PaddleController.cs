using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [Header("Paddle Parts")]
    public Transform paddleTip;       // ลาก PaddleTip (ลูก) มาใส่ช่องนี้
    public GameObject paddleVisual;   // ตัวภาพไม้พายทั้งหมด

    [Header("Boundary & Target")]
    public Transform currentContainer; // ลาก Bowl มาใส่
    public float stirRadius = 1.2f;    // รัศมีที่ยอมให้ "ปลายไม้" วิ่งได้
    public bool isAtPot = false;

    [Header("Batter Stages")]
    public GameObject ingredientGroup;
    public GameObject[] mixingStages;
    public IngredientManager ingManager;

    [Header("Stir Settings (Distance Based)")]
    public float stirDistanceThreshold1 = 30f;
    public float stirDistanceThreshold2 = 60f;
    public float stirDistanceFinish = 90f;

    private float totalStirDistance = 0f;
    private Vector3 lastTipPosition;
    public bool isMixed = false;
    private bool waitingForClick = false;

    void Start()
    {
        // เริ่มเกม: ซ่อนสเตจผสมทั้งหมด
        foreach (GameObject stage in mixingStages)
        {
            if (stage != null) stage.SetActive(false);
        }

        if (paddleTip != null) lastTipPosition = paddleTip.position;
    }

    void Update()
    {
        if (waitingForClick)
        {
            if (Input.GetMouseButtonDown(0)) CheckClickOnBowl();
            return;
        }

        if (!ingManager.isAllAdded || (isMixed && !isAtPot)) return;

        if (Input.GetMouseButton(0))
        {
            HandlePaddleMovement();
            if (!isAtPot) CalculateStirDistance();
        }
    }

    void HandlePaddleMovement()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // คำนวณหาตำแหน่งที่เหมาะสมเพื่อให้ "ปลายไม้ (Tip)" อยู่ในขอบเขต
        Vector3 tipOffset = paddleTip.position - transform.position;
        Vector3 targetTipPos = mousePos + tipOffset;

        Vector3 boundaryOffset = targetTipPos - currentContainer.position;
        if (boundaryOffset.magnitude > stirRadius)
        {
            boundaryOffset = boundaryOffset.normalized * stirRadius;
        }

        // ย้ายไม้พายโดยยึดตามขอบเขตของปลายไม้
        transform.position = (currentContainer.position + boundaryOffset) - tipOffset;
    }

    void CalculateStirDistance()
    {
        if (isMixed || paddleTip == null) return;

        // คำนวณระยะทางจากตำแหน่งของ "ปลายไม้" เท่านั้น
        float moveDistance = Vector3.Distance(paddleTip.position, lastTipPosition);

        if (moveDistance > 0.01f)
        {
            totalStirDistance += moveDistance;
            UpdateStirVisuals();
        }
        lastTipPosition = paddleTip.position;
    }

    void UpdateStirVisuals()
    {
        if (totalStirDistance >= stirDistanceFinish)
        {
            SetStage(2);
            FinishMixing();
        }
        else if (totalStirDistance >= stirDistanceThreshold2) SetStage(1);
        else if (totalStirDistance >= stirDistanceThreshold1)
        {
            ingredientGroup.SetActive(false);
            SetStage(0);
        }
    }

    void SetStage(int index)
    {
        for (int i = 0; i < mixingStages.Length; i++)
        {
            mixingStages[i].SetActive(i == index);
        }
    }

    void FinishMixing()
    {
        isMixed = true;
        waitingForClick = true;
        paddleVisual.SetActive(false); // ไม้พายหายไปเมื่อผสมเสร็จ
    }

    void CheckClickOnBowl()
    {
        // 1. แปลงตำแหน่งเมาส์เป็นพิกัดโลก
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        // 2. ยิง Raycast
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        // --- ส่วนเช็ก Error (ดูใน Console) ---
        if (hit.collider != null)
        {
            Debug.Log("คลิกโดนวัตถุชื่อ: " + hit.collider.gameObject.name);

            // เช็กว่าวัตถุที่คลิกโดน คืออันเดียวกับที่เราตั้งเป็น currentContainer ไหม
            if (hit.collider.gameObject == currentContainer.gameObject)
            {
                waitingForClick = false;
                FindObjectOfType<PouringSystem>().StartPour();
            }
            else
            {
                Debug.Log("คลิกโดนอย่างอื่นที่ไม่ใช่ชามผสมจ้ะ");
            }
        }
        else
        {
            Debug.Log("คลิกไม่โดน Collider อะไรเลยจ้ะ");
        }
    }
}