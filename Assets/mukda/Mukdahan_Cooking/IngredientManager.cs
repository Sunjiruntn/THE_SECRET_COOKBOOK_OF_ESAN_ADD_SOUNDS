using UnityEngine;
using TMPro;

public class IngredientManager : MonoBehaviour
{
    [Header("Ingredient Sprites in Bowl")]
    public GameObject flourInBowl;
    public GameObject sugarInBowl;
    public GameObject coconutInBowl;
    public GameObject pandanInBowl;

    [Header("UI Feedback")]
    public TextMeshProUGUI statusText;
    public GameObject ingredientButtons; // กลุ่มปุ่มกด แป้ง น้ำตาล...

    private bool hasFlour, hasSugar;
    private bool hasCoconut, hasPandan;
    public bool isAllAdded { get; private set; }

    void Start()
    {
        // เริ่มต้น: ปิดทุกอย่าง
        flourInBowl.SetActive(false);
        sugarInBowl.SetActive(false);
        coconutInBowl.SetActive(false);
        pandanInBowl.SetActive(false);
        ingredientButtons.SetActive(false);
    }

    // ฟังก์ชันที่ IntroManager จะมาเรียกใช้
    public void StartMixingPhase()
    {
        ingredientButtons.SetActive(true);
        statusText.text = "เริ่มใส่ของแห้ง (แป้งหรือน้ำตาล) ลงในชาม";
    }

    public void AddIngredient(string name)
    {
        if (name == "Flour") { hasFlour = true; flourInBowl.SetActive(true); }
        if (name == "Sugar") { hasSugar = true; sugarInBowl.SetActive(true); }

        if (hasFlour && hasSugar)
        {
            statusText.text = "ของแห้งครบแล้ว ใส่น้ำมะพร้าวและใบเตยต่อเลย";
            if (name == "Coconut") { hasCoconut = true; coconutInBowl.SetActive(true); }
            if (name == "Pandan") { hasPandan = true; pandanInBowl.SetActive(true); }
        }
        else if (name == "Coconut" || name == "Pandan")
        {
            statusText.text = "ต้องใส่ของแห้งให้ครบก่อนนะ!";
        }

        if (hasFlour && hasSugar && hasCoconut && hasPandan)
        {
            isAllAdded = true;
            statusText.text = "ส่วนผสมครบแล้ว! ใช้ไม้พายคนให้เข้ากัน";
        }
    }
}