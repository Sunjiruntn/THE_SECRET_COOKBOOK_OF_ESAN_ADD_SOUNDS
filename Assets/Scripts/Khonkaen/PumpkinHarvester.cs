using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
// ลบ using TMPro; ออกไป เพราะเราจะไม่ใช้ TextMeshPro แล้ว

public class PumpkinHarvester : MonoBehaviour
{
    private int provinceIndex = 3;
    public string nextSceneName = "KitchenScene";
    // ต้องลาก GameObject ของ UI Image ที่สร้างไว้มาใส่ใน Inspector ของฟักทองทุกชิ้น
    public GameObject resultImageSuccess; // ลาก ResultImage_Success มาใส่
    public GameObject resultImageFailure; // ลาก ResultImage_Failure มาใส่

    [Header("Pumpkin Type")]
    public bool isRipeAndReady = false;

    // ชื่อ Scene อ้างอิง
    private const string TUTORIAL_SCENE = "TutorialScene";
    private const string SUCCESS_SCENE = "KitchenScene";

    void Start()
    {
        // ตรวจสอบให้แน่ใจว่ารูปภาพผลลัพธ์ถูกปิดไว้ตั้งแต่แรก
        if (resultImageSuccess != null) resultImageSuccess.SetActive(false);
        if (resultImageFailure != null) resultImageFailure.SetActive(false);
    }

    void OnMouseDown()
    {

        // ปิด Collider ทันทีเพื่อป้องกันการคลิกซ้ำ
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider != null)
        {
            myCollider.enabled = false;
        }

        if (isRipeAndReady)
        {
            // *** 1. สำเร็จ: จัดการความสำเร็จ (แสดงรูปภาพ -> โหลด Scene) ***
            StartCoroutine(HandleSuccess());

        }
        else
        {
            // *** 2. ล้มเหลว: จัดการความล้มเหลว (แสดงรูปภาพ -> โหลด Scene) ***
            StartCoroutine(HandleFailure());
        }
    }

    // Coroutine จัดการความสำเร็จ (แสดงรูปภาพ -> หน่วงเวลา -> โหลด Scene)
    IEnumerator HandleSuccess()
    {
        Debug.Log("SUCCESS: ได้ฟักทองแก่จัดแล้ว!");

        // 1. แสดงรูปภาพสำเร็จ
        if (resultImageSuccess != null)
        {
            resultImageSuccess.SetActive(true);
        }

        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.PassLevel(provinceIndex);

            GameDataController.Instance.SaveCurrentScene(nextSceneName);

            GameDataController.Instance.SaveGame();
        }
        // 2. หน่วงเวลา 2 วินาที เพื่อให้ผู้เล่นได้เห็นรูปภาพ
        yield return new WaitForSeconds(2.0f);

        // 3. ปิดรูปภาพ (ถ้ายังอยู่ใน Scene นี้)
        if (resultImageSuccess != null)
        {
            resultImageSuccess.SetActive(false);
        }

        // 4. ทำลายวัตถุและโหลด Scene
        Destroy(gameObject);
        SceneManager.LoadScene(SUCCESS_SCENE);
    }

    // Coroutine จัดการความล้มเหลว (แสดงรูปภาพ -> หน่วงเวลา -> โหลด Scene)
    IEnumerator HandleFailure()
    {
        Debug.Log("FAILURE: ฟักทองอ่อนเกินไป! กลับไปทบทวนบทเรียน...");

        // 1. แสดงรูปภาพความล้มเหลว
        if (resultImageFailure != null)
        {
            resultImageFailure.SetActive(true);
        }

        // 2. หน่วงเวลา 2 วินาที เพื่อให้ผู้เล่นได้เห็นรูปภาพ
        yield return new WaitForSeconds(2.0f);

        // 3. ปิดรูปภาพ (ถ้ายังอยู่ใน Scene นี้)
        if (resultImageFailure != null)
        {
            resultImageFailure.SetActive(false);
        }

        // 4. ตั้งค่าสัญญาณความล้มเหลว (สำหรับบทพูดพิเศษของคุณยาย)
        PlayerPrefs.SetInt("HasFailedPumpkin", 1);

        // 5. ทำลายวัตถุและโหลด Scene
        Destroy(gameObject);
        SceneManager.LoadScene(TUTORIAL_SCENE);
    }
}