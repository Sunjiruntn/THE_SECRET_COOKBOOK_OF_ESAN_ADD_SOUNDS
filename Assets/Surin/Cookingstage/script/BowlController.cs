using UnityEngine;

public class BowlController : MonoBehaviour
{
    [Header("Bowl Base (ตัวชาม)")]
    public SpriteRenderer bowlBaseRenderer;

    [Header("Ingredient Objects (ของในชาม)")]
    public GameObject flourObject;
    public GameObject sugarObject;
    public GameObject saltObject;
    public GameObject coconutObject;

    [Header("Mixing Stages")]
    public Sprite mixingStage1;
    public Sprite mixingStage2;
    public Sprite finishedState;

    [Header("Reset Settings")]
    public Sprite emptyBowlSprite; // ** อย่าลืมลากรูปชามเปล่ามาใส่ตรงนี้ **

    [Header("Status")]
    public bool isFinishedMixing = false;

    private bool canMix = false;
    private int mixCount = 0;

    // Config จำนวนครั้ง
    private int step1Threshold = 4;
    private int step2Threshold = 8;
    private int finishThreshold = 15;

    public void ResetBowl()
    {
        HideRawIngredients();
        canMix = false;
        isFinishedMixing = false;
        mixCount = 0;

        // รีเซ็ตกลับเป็นชามเปล่า
        if (bowlBaseRenderer != null && emptyBowlSprite != null)
        {
            bowlBaseRenderer.sprite = emptyBowlSprite;
        }
    }

    public void UpdateBowlVisual(IngredientType addedIngredient)
    {
        // (ส่วนนี้เหมือนเดิม... เปิดของตามที่เทใส่)
        switch (addedIngredient)
        {
            case IngredientType.StickyRiceFlour: if (flourObject) flourObject.SetActive(true); break;
            case IngredientType.CaneSugar: if (sugarObject) sugarObject.SetActive(true); break;
            case IngredientType.Salt: if (saltObject) saltObject.SetActive(true); break;
            case IngredientType.GratedCoconut: if (coconutObject) coconutObject.SetActive(true); break;
        }
    }

    public void EnableMixing()
    {
        isFinishedMixing = false;
        canMix = true;
        mixCount = 0;
    }

    void OnMouseDown()
    {
        // 1. กรณี: กำลังคนผสม
        if (canMix)
        {
            mixCount++;
            if (mixCount == step1Threshold) { HideRawIngredients(); if (mixingStage1) bowlBaseRenderer.sprite = mixingStage1; }
            else if (mixCount == step2Threshold) { if (mixingStage2) bowlBaseRenderer.sprite = mixingStage2; }
            else if (mixCount >= finishThreshold) { FinishMixing(); }
            return;
        }

        // 2. กรณี: ผสมเสร็จแล้ว (กดเพื่อเท)
        if (isFinishedMixing)
        {
            if (CookingGameManager.Instance.CheckIngredientOnly(IngredientType.MixedDough))
            {
                // ==========================================
                // ✅ แก้ไข: ไม่ต้องลอย! แค่เปลี่ยนเป็นชามเปล่า
                // ==========================================

                // 1. เปลี่ยนรูปเป็นชามเปล่าทันที
                if (bowlBaseRenderer != null && emptyBowlSprite != null)
                {
                    bowlBaseRenderer.sprite = emptyBowlSprite;
                }

                // 2. บอก Manager ว่า "เทแล้วนะ" (Manager จะไปสั่งหม้อให้เล่นอนิเมชั่นเอง)
                CookingGameManager.Instance.CompleteStep(IngredientType.MixedDough);
            }
        }
    }

    void HideRawIngredients()
    {
        if (flourObject) flourObject.SetActive(false);
        if (sugarObject) sugarObject.SetActive(false);
        if (saltObject) saltObject.SetActive(false);
        if (coconutObject) coconutObject.SetActive(false);
    }

    void FinishMixing()
    {
        canMix = false;
        isFinishedMixing = true;
        HideRawIngredients();
        if (finishedState != null && bowlBaseRenderer != null) bowlBaseRenderer.sprite = finishedState;
    }
}