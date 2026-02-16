using UnityEngine;
using UnityEngine.UI;

public class HeatManager : MonoBehaviour
{
    public static HeatManager Instance;

    [Header("Heat Settings")]
    public float currentHeat = 0f;
    public float maxHeat = 100f;
    public float heatRiseRate = 5f;   // แนะนำ 5-10 พอครับ (40 เร็วไป)
    public float coolDownPerStir = 5f;
    public float overheatThreshold = 80f;

    [Header("Overheat Penalty")]
    public float overheatTimer = 0f;
    public float timeToBurn = 2f; // เวลา 2 วิ ไหม้ 1 เม็ด

    [Header("UI")]
    public Image heatGaugeImg;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            if (!gm.isGameActive || gm.isGameOver) return;
        }
        // 1. เพิ่มความร้อน
        currentHeat += heatRiseRate * Time.deltaTime;
        currentHeat = Mathf.Clamp(currentHeat, 0, maxHeat);

        // 2. อัปเดต UI
        if (heatGaugeImg != null)
        {
            heatGaugeImg.fillAmount = currentHeat / maxHeat;
            if (currentHeat >= overheatThreshold) heatGaugeImg.color = Color.red;
            else heatGaugeImg.color = Color.green;
        }

        // 3. ระบบจับเวลาเผาถั่ว
        if (currentHeat >= overheatThreshold)
        {
            overheatTimer += Time.deltaTime;

            if (overheatTimer >= timeToBurn)
            {
                BurnRandomBean(); // สั่งเผา
                overheatTimer = 0f; // ✅ รีเซ็ตเวลาทันที (สำคัญมาก!)
            }
        }
        else
        {
            overheatTimer = 0f; // ถ้าร้อนไม่ถึงขีดแดง รีเซ็ตเวลาทิ้ง
        }
    }

    void BurnRandomBean()
    {
        BeanStatus[] allBeans = FindObjectsOfType<BeanStatus>();

        foreach (BeanStatus bean in allBeans)
        {
            // ✅ แก้เงื่อนไข: เช็คที่ Boolean โดยตรง (ชัวร์กว่าเช็คตัวเลข)
            // หาถั่วที่ "ยังไม่ไหม้" (!isBurnt)
            if (!bean.isBurnt)
            {
                bean.ForceBurn(); // สั่งตาย
                break; // เจอแล้วหยุดวนลูปทันที (เผาแค่ 1 เม็ด)
            }
        }
    }

    public void ReduceHeat(float amount = -1f)
    {
        float reduction = (amount == -1f) ? coolDownPerStir : amount;
        currentHeat -= reduction;
        if (currentHeat < 0) currentHeat = 0;
    }
}