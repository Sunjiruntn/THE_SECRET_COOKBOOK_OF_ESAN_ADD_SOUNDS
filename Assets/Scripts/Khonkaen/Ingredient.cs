using UnityEngine;
using System.Collections;

// =========================================================================
// [1] ENUM: ID ของวัตถุดิบ
// =========================================================================
public enum IngredientID
{
    NONE,
    PUMPKIN_RAW,
    PUMPKIN_STEAMED_FLOAT,
    STICKY_RICE,
    COCONUT_MILK,
    SUGAR_SWEETENER,
    SALT,
    SHREDDED_COCONUT,
    SESAME_SEEDS,
    MIXING_BOWL
}

// =========================================================================
// [2] CLASS: Ingredient Component
// =========================================================================
public class Ingredient : MonoBehaviour
{
    [Header("Ingredient Setup")]
    public IngredientID ingredientId;
    public CookingInstructorManager manager;

    [Header("Interaction Flags")]
    public bool isDraggable = false;
    public bool isClickable = false;

    // ตำแหน่งเริ่มต้นสำหรับ Reset
    private Vector3 originalPosition;
    public int originalSortingOrder;

    void Start()
    {
        originalPosition = transform.position;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalSortingOrder = renderer.sortingOrder;
        }

        if (manager == null)
        {
            manager = FindObjectOfType<CookingInstructorManager>();
        }
    }

    public void ResetPosition()
    {
        transform.position = originalPosition;
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().sortingOrder = originalSortingOrder;
        }
    }

    // =========================================================================
    // [3] DRAG & DROP LOGIC
    // =========================================================================
    private void OnMouseDown()
    {
        if (manager == null || manager.IsGameOver() || !isDraggable) return;

        manager.SetAllIngredientsInteraction(false, false);
        isDraggable = true;

        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().sortingOrder = 10;
        }
    }

    private void OnMouseDrag()
    {
        if (manager == null || manager.IsGameOver() || !isDraggable) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;
        transform.position = mousePosition;
    }

    private void OnMouseUp()
    {
        if (manager == null || manager.IsGameOver() || !isDraggable) return;

        isDraggable = false;

        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().sortingOrder = originalSortingOrder;
        }

        Collider2D targetCollider = FindTargetCollider();

        if (targetCollider != null)
        {
            Ingredient targetIngredient = targetCollider.GetComponent<Ingredient>();
            manager.HandleDragDrop(this.ingredientId, targetIngredient != null ? targetIngredient.ingredientId : IngredientID.NONE);
        }
        else
        {
            ResetPosition();
            manager.ShowErrorPopup("❌ วางผิดเป้าหมาย! ลากวัตถุดิบใส่ชามคลุกเท่านั้นจ้ะ");
            // ⭐ แก้ไข: หมายเหตุว่า ShowErrorPopup จะจัดการ interaction เอง
            // ไม่ต้อง enable กลับที่นี่เพราะปุ่ม "ลองใหม่" จะเรียก ResetStepForRetry
        }
    }

    private Collider2D FindTargetCollider()
    {
        if (manager != null && manager.mixingBowlTarget != null && manager.mixingBowlTarget.OverlapPoint(transform.position))
        {
            return manager.mixingBowlTarget;
        }
        if (manager != null && manager.steamerTarget != null && manager.steamerTarget.OverlapPoint(transform.position))
        {
            return manager.steamerTarget;
        }
        return null;
    }

    // =========================================================================
    // [4] CLICK LOGIC (สำหรับการคลิก)
    // =========================================================================

    private void OnMouseUpAsButton()
    {
        if (manager == null || manager.IsGameOver() || !isClickable) return;

        // ⭐ แก้ไข: ตรวจสอบว่าไม่อยู่ในสถานะ error panel
        if (manager.errorPanel != null && manager.errorPanel.activeSelf) return;

        if (ingredientId == IngredientID.PUMPKIN_RAW && manager.currentStep == CookingInstructorManager.CookingStep.STEP_1_PUMPKIN_DRAG)
        {
            manager.HandleClick(this.ingredientId);
            return;
        }

        if (ingredientId == IngredientID.MIXING_BOWL ||
            ingredientId == IngredientID.PUMPKIN_STEAMED_FLOAT ||
            ingredientId == IngredientID.COCONUT_MILK ||
            ingredientId == IngredientID.SUGAR_SWEETENER ||
            ingredientId == IngredientID.SALT ||
            ingredientId == IngredientID.SHREDDED_COCONUT ||
            ingredientId == IngredientID.SESAME_SEEDS)
        {
            manager.HandleClick(this.ingredientId);
        }
    }
}