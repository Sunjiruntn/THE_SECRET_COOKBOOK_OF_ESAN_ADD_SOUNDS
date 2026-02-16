using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SmartClickableText : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI myText;
    private RecipeBookController bookController;

    void Awake()
    {
        myText = GetComponent<TextMeshProUGUI>();
        // ค้นหา Controller ในฉาก
        bookController = FindObjectOfType<RecipeBookController>();
    }

    // ฟังก์ชันนี้ทำงานเมื่อ "คลิกที่ตัวหนังสือ"
    public void OnPointerClick(PointerEventData eventData)
    {
        if (myText == null) return;

        // เช็คว่าข้อความยาวจนโดนตัด (...) หรือเปล่า?
        if (myText.isTextTruncated)
        {
            Debug.Log("ข้อความยาวเกิน! ส่งเรื่องให้ Controller เปิด Popup");
            if (bookController != null)
            {
                bookController.OpenTextPopup(myText.text);
            }
        }
        else
        {
            // ถ้าข้อความสั้น กดแล้วจะไม่เกิดอะไรขึ้น (ตามที่ต้องการ)
            Debug.Log("ข้อความสั้น อ่านในหน้าได้เลย");
        }
    }
}