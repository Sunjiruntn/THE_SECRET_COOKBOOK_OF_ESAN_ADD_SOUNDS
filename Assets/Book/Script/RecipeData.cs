using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "ThaiGame/RecipeData")]
public class RecipeData : ScriptableObject
{
    [Header("--- หน้าซ้าย (Left Page) ---")]
    public string provinceName;         // เช่น "จังหวัดร้อยเอ็ด"
    public string menuName;             // เช่น "ลาบปูนา"
    public Sprite foodImage;            // รูปอาหารจานใหญ่
    [TextArea(3, 5)]
    public string storyText;            // คำบรรยายใต้ภาพ (สตอรี่ของกิน)

    [Header("--- หน้าขวา (Right Page) ---")]
    // เราสร้างเป็น Class ย่อย เพื่อให้ 1 วัตถุดิบ มีทั้ง "รูป" และ "ชื่อ"
    public List<IngredientData> ingredients;

    [TextArea(3, 5)]
    public string howToText;            // วิธีทำ

    [TextArea(2, 4)]
    public string tipsText;             // เคล็ดลับการปรุง
}

// ตัวช่วยสำหรับเก็บข้อมูลวัตถุดิบ (รูป + ชื่อ)
[System.Serializable]
public class IngredientData
{
    public string name;      // ชื่อวัตถุดิบ (เช่น ข้าวคั่ว)
    public Sprite icon;      // รูปไอคอน (ถ้วยใส่ผงขาวๆ)
}