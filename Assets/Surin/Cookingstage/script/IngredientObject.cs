using UnityEngine;
using System.Collections;

public class IngredientObject : MonoBehaviour
{
    public IngredientType myType;
    public Transform pourPoint;
    public float moveSpeed = 5f;

    // อนิเมชั่นต่างๆ
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite[] pourAnimation;
    public GameObject particleEffect;

    private Vector3 originalPosition;
    private bool isMoving = false;

    void Start()
    {
        originalPosition = transform.position;
        if (spriteRenderer && idleSprite) spriteRenderer.sprite = idleSprite;
    }

    void OnMouseDown()
    {
        if (CookingGameManager.Instance == null || isMoving) return;

        // 1. เช็คกับ Manager ว่าถูกไหม (ใช้ CheckIngredientOnly)
        // ** ขั้นตอนนี้ยังไม่ผ่านด่านนะ แค่เช็ค **
        bool isCorrect = CookingGameManager.Instance.CheckIngredientOnly(myType);

        if (isCorrect)
        {
            // ถ้าถูก -> เริ่มเล่นอนิเมชั่น
            StartCoroutine(MoveAndPourRoutine());
        }
    }

    IEnumerator MoveAndPourRoutine()
    {
        isMoving = true;

        // --- ช่วงเดินไปเท ---
        if (pourPoint != null)
            yield return StartCoroutine(MoveToPosition(pourPoint.position));

        // --- ช่วงเท (เปลี่ยนรูป) ---
        if (pourAnimation != null)
        {
            foreach (var frame in pourAnimation)
            {
                spriteRenderer.sprite = frame;
                yield return new WaitForSeconds(0.2f);
            }
        }

        if (particleEffect != null && pourPoint != null)
            Instantiate(particleEffect, pourPoint.position + Vector3.down * 0.5f, Quaternion.identity);

        yield return new WaitForSeconds(0.5f); // แช่ท่านั้นแป๊บนึง

        // 3. ยกขวดขึ้น (Animation 3 -> 2 -> 1)
        if (pourAnimation != null)
        {
            for (int i = pourAnimation.Length - 1; i >= 0; i--)
            {
                spriteRenderer.sprite = pourAnimation[i];
                yield return new WaitForSeconds(0.2f);
            }
        }
        // --- ช่วงเดินกลับ ---
        // (ใส่โค้ดเดินกลับ หรือเปลี่ยนรูปกลับตรงนี้ตามที่คุณเคยมี)
        CookingGameManager.Instance.CompleteStep(myType);

        // 4. เปลี่ยนกลับเป็นรูปปกติ (ตั้งขวดตรง)
        if (idleSprite) spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.3f);

        // 5. เดินกลับที่เดิม (ระหว่างนี้แป้งในชามจะโชว์แล้ว)
        yield return StartCoroutine(MoveToPosition(originalPosition));

        isMoving = false;
    }

    IEnumerator MoveToPosition(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }
}