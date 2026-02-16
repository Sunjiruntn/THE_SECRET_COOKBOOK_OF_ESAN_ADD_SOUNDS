using UnityEngine;
using System.Collections;

public class SteamingController : MonoBehaviour
{
    [Header("Objects in Pot (ของในหม้อ - แบบนิ่ง)")]
    public GameObject pandanInPot;
    public GameObject doughRawInPot;    // แป้งดิบ (แบบนิ่งๆ กองก้นหม้อ)
    public GameObject doughCookedInPot; // แป้งสุก
    public GameObject lidObject;
    public ParticleSystem steamEffect;

    [Header("Animation Objects (ตัวเล่นอนิเมชั่น - แยกต่างหาก)")]
    // เปลี่ยนจาก Animator เป็น GameObject ตามที่คุณขอครับ
    public GameObject Anim_Drop_Dough;

    // (ถ้ามีอนิเมชั่นอื่นก็เพิ่มตรงนี้ได้ เช่น Anim_Drop_Pandan)

    [Header("Pandan Settings")]
    public Transform pandanStartPoint;
    public Transform pandanEndPoint;
    public float pandanDropSpeed = 5.0f;

    [Header("Lid Settings")]
    public Transform lidStartPoint;
    public Transform lidEndPoint;
    public float lidDropSpeed = 5.0f;

    void Start()
    {
        ResetSteamer();
    }

    public void ResetSteamer()
    {
        // 1. ซ่อนของนิ่งๆ ทั้งหมด
        if (pandanInPot) pandanInPot.SetActive(false);
        if (doughRawInPot) doughRawInPot.SetActive(false);
        if (doughCookedInPot) doughCookedInPot.SetActive(false);
        if (lidObject) lidObject.SetActive(false);

        // 2. ซ่อนตัวอนิเมชั่นด้วย (สำคัญ)
        if (Anim_Drop_Dough) Anim_Drop_Dough.SetActive(false);

        // 3. หยุดควัน
        if (steamEffect) steamEffect.Stop();

        // 4. รีเซ็ตสีฝาหม้อ
        if (lidObject != null)
        {
            SpriteRenderer sr = lidObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }
        }
    }

    // --- ฟังก์ชันช่วยเล่นอนิเมชั่น (เปิด -> รอ -> ปิด) ---
    // ใช้ Pattern เดียวกับที่คุณเคยทำปู/ผักเลยครับ
    IEnumerator CoPlayDropAnim(GameObject animObj, float duration)
    {
        if (animObj != null)
        {
            animObj.SetActive(true); // เปิดให้เล่น
            yield return new WaitForSeconds(duration); // รอจนจบ
            animObj.SetActive(false); // ซ่อนทิ้ง
        }
    }

    // 1. ใบเตย (ถ้าคุณอยากเปลี่ยนเป็น Anim ก็ทำแบบเดียวกันได้ครับ แต่ออันนี้คงของเดิมไว้ก่อน)
    public IEnumerator AnimateAddPandan()
    {
        yield return StartCoroutine(DropObjectRoutine(pandanInPot, pandanStartPoint, pandanEndPoint, pandanDropSpeed));
    }

    // 2. ขนม (ใช้ Anim_Drop_Dough ตามที่คุณต้องการ)
    public IEnumerator AnimateAddDough()
    {
        // เรียกใช้ฟังก์ชัน CoPlayDropAnim
        // เลข 2.0f คือเวลาที่เผื่อไว้ให้อนิเมชั่นเล่นจนจบ (ปรับได้ตามความยาวคลิป)
        yield return StartCoroutine(CoPlayDropAnim(Anim_Drop_Dough, 2.0f));

        // พออนิเมชั่นเล่นจบและหายไปแล้ว -> ให้เปิด "แป้งนิ่งๆ" ขึ้นมาแทน
        if (doughRawInPot) doughRawInPot.SetActive(true);
    }

    // 3. ฝาหม้อ
    public IEnumerator AnimateCloseLid()
    {
        if (lidObject != null)
        {
            SpriteRenderer sr = lidObject.GetComponent<SpriteRenderer>();
            if (sr != null) { Color c = sr.color; c.a = 1f; sr.color = c; }
        }
        yield return StartCoroutine(DropObjectRoutine(lidObject, lidStartPoint, lidEndPoint, lidDropSpeed));
    }

    public IEnumerator AnimateCookingProcess()
    {
        if (steamEffect) { steamEffect.gameObject.SetActive(true); steamEffect.Play(); }

        yield return new WaitForSeconds(3.0f);

        if (steamEffect) steamEffect.Stop();

        if (doughRawInPot) doughRawInPot.SetActive(false);
        if (doughCookedInPot) doughCookedInPot.SetActive(true);

        yield return StartCoroutine(OpenLidAnimation());
    }

    // ฟังก์ชันช่วยเคลื่อนที่ (สำหรับใบเตย/ฝาหม้อ)
    IEnumerator DropObjectRoutine(GameObject obj, Transform startPos, Transform endPos, float speed)
    {
        if (obj == null || startPos == null || endPos == null) yield break;
        obj.SetActive(true);
        obj.transform.position = startPos.position;
        while (Vector3.Distance(obj.transform.position, endPos.position) > 0.01f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, endPos.position, speed * Time.deltaTime);
            yield return null;
        }
        obj.transform.position = endPos.position;
    }

    IEnumerator OpenLidAnimation()
    {
        if (lidObject == null || lidStartPoint == null) yield break;
        SpriteRenderer lidSprite = lidObject.GetComponent<SpriteRenderer>();
        Color startColor = lidSprite != null ? lidSprite.color : Color.white;

        while (Vector3.Distance(lidObject.transform.position, lidStartPoint.position) > 0.01f)
        {
            lidObject.transform.position = Vector3.MoveTowards(lidObject.transform.position, lidStartPoint.position, lidDropSpeed * Time.deltaTime);
            if (lidSprite != null)
            {
                float alpha = Mathf.InverseLerp(0, Vector3.Distance(lidEndPoint.position, lidStartPoint.position), Vector3.Distance(lidObject.transform.position, lidEndPoint.position));
                lidSprite.color = new Color(startColor.r, startColor.g, startColor.b, 1 - alpha);
            }
            yield return null;
        }
        lidObject.SetActive(false);
        if (lidSprite != null) { Color c = startColor; c.a = 1f; lidSprite.color = c; }
    }
}