using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IngredientManager : MonoBehaviour
{
    // สร้างสถานะ (Step) เพื่อให้จัดการลำดับง่ายขึ้น
    public enum CookingStep
    {
        Waiting,    // รออินโทรจบ
        AddDry,     // ขั้นตอนใส่ของแห้ง (แป้ง, น้ำตาล)
        AddWet,
        Mixing,
        Pouring,
        Done
    }

    [Header("Ingredient Sprites in Bowl")]
    public GameObject flourInBowl;
    public GameObject sugarInBowl;
    public GameObject coconutInBowl;
    public GameObject pandanInBowl;

    [Header("UI Feedback")]
    public TextMeshProUGUI statusText;
    public GameObject statusPanel;
    public GameObject ingredientButtons; // กลุ่มปุ่มกด แป้ง น้ำตาล...
    [Header("Next Phase (Pot/Pan)")]
    public PotPaddleController potController;
    private bool hasFlour, hasSugar;
    private bool hasCoconut, hasPandan;
    public bool isAllAdded { get; private set; }

    private CanvasGroup buttonsCanvasGroup;
    public CookingStep currentStep = CookingStep.Waiting; // เริ่มต้นที่สถานะรอ

    void Start()
    {
        // เริ่มต้น: ปิดกราฟิกของในชาม
        flourInBowl.SetActive(false);
        sugarInBowl.SetActive(false);
        coconutInBowl.SetActive(false);
        pandanInBowl.SetActive(false);

        // ดึง CanvasGroup มาใช้ และตั้งค่าให้ทะลุคลิกไปก่อน
        buttonsCanvasGroup = ingredientButtons.GetComponent<CanvasGroup>();
        if (buttonsCanvasGroup == null)
        {
            buttonsCanvasGroup = ingredientButtons.AddComponent<CanvasGroup>();
        }
        buttonsCanvasGroup.blocksRaycasts = false;

        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }
    }

    // ฟังก์ชันที่ IntroManager จะมาเรียกใช้
    public void StartMixingPhase()
    {
        if (buttonsCanvasGroup != null)
        {
            buttonsCanvasGroup.blocksRaycasts = true; // เปิดให้เมาส์ชนปุ่มได้ปกติ
        }
        ingredientButtons.SetActive(true);
        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = "ขั้นตอนที่ 1: ใส่แป้งข้าวเจ้าและน้ำตาลลงในชาม";
        }
        // ขยับ Step เป็น "ใส่ของแห้ง"
        currentStep = CookingStep.AddDry;
        statusText.text = "ขั้นตอนแรกให้ใส่แป้งข้าวเจ้าและน้ำตาลลงในชาม";

        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            if (statusText != null) statusText.gameObject.SetActive(false);
            if (statusPanel != null) statusPanel.SetActive(false);
            Debug.Log("[โหมดสอบ] ซ่อน Status เรียบร้อยแล้ว");
        }
        else
        {
            // 🟢 โหมดปกติ: เปิดข้อความสอน
            if (statusText != null)
            {
                statusText.gameObject.SetActive(true);
                statusText.text = "ขั้นตอนแรกให้ใส่แป้งข้าวเจ้าและน้ำตาลลงในชาม";
            }
            if (statusPanel != null) statusPanel.SetActive(true);
        }
    }

    public void AddIngredient(string name)
    {
        bool isCorrectClick = false;

        // ==========================================
        // 🛑 ตรวจสอบตามขั้นตอน (Step)
        // ==========================================
        if (currentStep == CookingStep.AddDry)
        {
            if (name == "Flour" && !hasFlour) { hasFlour = true; flourInBowl.SetActive(true); isCorrectClick = true; }
            else if (name == "Sugar" && !hasSugar) { hasSugar = true; sugarInBowl.SetActive(true); isCorrectClick = true; }
            else if (name == "Coconut" || name == "Pandan")
            {
                // เตือนเมื่อข้ามขั้นตอนไปกดของเหลวก่อน!
                statusText.text = "ผิดขั้นตอนจ้า ! ต้องใส่แป้งข้าวเจ้าและน้ำตาล(ของแห้ง) ให้ครบก่อนนะ";
            }
            else if ((name == "Flour" && hasFlour) || (name == "Sugar" && hasSugar))
            {
                statusText.text = "ใส่แป้งข้าวเจ้าและน้ำตาลแล้วจ้ะ";
            }
        }
        else if (currentStep == CookingStep.AddWet)
        {
            if (name == "Coconut" && !hasCoconut) { hasCoconut = true; coconutInBowl.SetActive(true); isCorrectClick = true; }
            else if (name == "Pandan" && !hasPandan) { hasPandan = true; pandanInBowl.SetActive(true); isCorrectClick = true; }
            else if (name == "Flour" || name == "Sugar")
            {
                statusText.text = "ของแห้งใส่ครบแล้วจ้า ตอนนี้ต้องใส่น้ำกะทิและน้ำใบเตยนะ";
            }
            else if ((name == "Coconut" && hasCoconut) || (name == "Pandan" && hasPandan))
            {
                statusText.text = "ใส่น้ำกะทิและใบเตยแล้วจ้ะ";
            }
        }

        // ==========================================
        // 📝 ระบบบันทึกคะแนน (โหมดสอบ)
        // ==========================================
        if (TestGameManager.Instance != null)
        {
            if (isCorrectClick) TestGameManager.Instance.RecordSuccess();
            else TestGameManager.Instance.RecordMistake(); // กดผิดขั้นตอน หรือกดซ้ำ หักคะแนน!
        }

        // ==========================================
        // 🔄 อัปเดตสถานะ Step และข้อความบนจอ
        // ==========================================
        if (isCorrectClick)
        {
            if (currentStep == CookingStep.AddDry)
            {
                if (hasFlour && hasSugar)
                {
                    currentStep = CookingStep.AddWet;
                    statusText.text = "ใส่ของแห้งครบแล้ว เติมน้ำกะทิและน้ำใบเตยต่อเลย";
                }
                else
                {
                    statusText.text = "ดีมาก ใส่ของแห้งที่เหลือให้ครบเลย";
                }
            }
            else if (currentStep == CookingStep.AddWet)
            {
                if (hasCoconut && hasPandan)
                {
                    currentStep = CookingStep.Mixing;
                    isAllAdded = true;
                    statusText.text = "ใส่ส่วนผสมครบแล้ว ใช้ไม้พายคนให้เข้ากันจนเป็นเนื้อเดียวกัน";

                    // ปิดปุ่มวัตถุดิบไปเลยก็ได้ เพราะใช้เสร็จแล้ว
                    if (buttonsCanvasGroup != null) buttonsCanvasGroup.blocksRaycasts = false;
                }
                else
                {
                    statusText.text = "เยี่ยม! ใส่ของเหลวที่เหลือให้ครบเลย";
                }
            }
        }


    }
    public void StirMixture()
    {
        if (currentStep == CookingStep.Mixing)
        {
            currentStep = CookingStep.Pouring;

            // เปลี่ยนข้อความเป็นประโยคที่คุณต้องการเป๊ะๆ
            statusText.text = "ผสมได้เนียนเข้ากันดีแล้วให้เทส่วนผสมลงหม้อ";

            Debug.Log("กำลังคนส่วนผสม...");
        }
        else if (currentStep < CookingStep.Mixing)
        {
            statusText.text = "เดี๋ยวก่อน! ยังใส่ส่วนผสมไม่ครบเลย จะรีบคนไปไหนจ๊ะ";
        }
    }

    // ==========================================
    // ✅ 4. ฟังก์ชันสำหรับ "เทลงหม้อ" (จุดเชื่อมต่อไปยัง PotPaddleController)
    // ==========================================
    public void PourIntoPot()
    {
        if (currentStep == CookingStep.Pouring)
        {
            currentStep = CookingStep.Done;

            // ✅ สั่งให้ระบบกวนในหม้อเริ่มทำงาน!
            if (potController != null)
            {
                potController.InitPot();
            }

            // ✅ ปิดกล่องข้อความของฝั่งชาม เพื่อให้ฝั่งหม้อโชว์ข้อความแทน
            if (statusText != null)
            {
                statusText.gameObject.SetActive(false);
            }

            Debug.Log("เทส่วนผสมลงหม้อแล้ว! ส่งไม้ต่อให้หม้อเรียบร้อย");
        }
        else if (currentStep < CookingStep.Pouring)
        {
            statusText.text = "ยังไม่ได้คนส่วนผสมให้เข้ากันเลยนะ! ใช้ไม้พายคนก่อนจ้า";
        }
    }
}