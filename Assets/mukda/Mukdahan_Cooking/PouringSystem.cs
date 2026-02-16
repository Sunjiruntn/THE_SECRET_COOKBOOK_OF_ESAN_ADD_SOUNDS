using UnityEngine;
using System.Collections;

public class PouringSystem : MonoBehaviour
{
    [Header("Phase 1: Start State")]
    public GameObject bowl;             // ชามผสมหลัก
    public GameObject pot;              // หม้อ (ตั้งให้ Active ไว้ แต่ข้างในต้องว่าง)

    [Header("Phase 2: Objects to Hide Forever")]
    public GameObject[] objectsToHide;  // ชาม, เนื้อขนมสเตจ 3, ไม้พายอันเก่า

    [Header("Phase 3: Pouring Animation")]
    public GameObject pourEffectObject;
    public SpriteRenderer pourRenderer;
    public Sprite[] pourSprites;
    public Transform pourPoint;

    [Header("Phase 4: Final Stage")]
    public GameObject batterInPot;      // ขนมสเตจแรกในหม้อ
    public GameObject potPaddleObject;  // ไม้พายอันใหม่
    void Awake()
    {
        // สั่งซ่อน BatterInPot และไม้พายหม้อ ตั้งแต่เสี้ยววินาทีแรกที่รันเกม
        if (batterInPot != null) batterInPot.SetActive(false);
        if (potPaddleObject != null) potPaddleObject.SetActive(false);
    }
    void Start()
    {
        if (batterInPot != null) batterInPot.SetActive(false);
        if (potPaddleObject != null) potPaddleObject.SetActive(false);
        if (pourEffectObject != null) pourEffectObject.SetActive(false);

        // หม้อกับชามผสมต้องโชว์ (แต่หม้อจะว่างเปล่าเพราะขนมโดนสั่งปิดไปแล้ว)
        if (bowl != null) bowl.SetActive(true);
        if (pot != null) pot.SetActive(true);
    }

    public void StartPour()
    {
        StartCoroutine(PourRoutine());
    }

    IEnumerator PourRoutine()
    {
        // 1. ซ่อนชามผสมเดิม
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null) obj.SetActive(false);
        }

        // 2. เล่นภาพเท (ห้ามมีรูปชามว่างวางเฉยๆ ในนี้)
        pourEffectObject.SetActive(true);
        pourEffectObject.transform.position = pourPoint.position;
        if (batterInPot != null) batterInPot.SetActive(true);
        pourRenderer.sprite = pourSprites[0];
        yield return new WaitForSeconds(0.7f);
        pourRenderer.sprite = pourSprites[1];
        yield return new WaitForSeconds(0.7f);

        // 3. ปิดภาพเท (ชามจะหายไปตอนนี้)
        pourEffectObject.SetActive(false);

        // 4. เปิดขนมในหม้อและไม้พายใหม่
        if (batterInPot != null) batterInPot.SetActive(true);
        if (potPaddleObject != null) potPaddleObject.SetActive(true);
        pourEffectObject.SetActive(false); // ปิดรูปเท

        // เปิดหม้อและขนม
        if (pot != null) pot.SetActive(true);
        if (batterInPot != null) batterInPot.SetActive(true);

        // สั่งให้ไม้พายเริ่มทำงานและตั้งค่าสเตจขนมตอนนี้!
        if (potPaddleObject != null)
        {
            potPaddleObject.SetActive(true);
            potPaddleObject.GetComponent<PotPaddleController>().InitPot();
        }
    }
}