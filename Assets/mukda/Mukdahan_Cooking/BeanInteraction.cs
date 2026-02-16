using UnityEngine;

public class BeanInteraction : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log("คลิกถั่วแล้วจ้า!");
        // เมื่อคลิกถั่ว ให้ไปบอกไม้พายหม้อว่าใส่ถั่วแล้ว
        FindObjectOfType<PotPaddleController>().AddBeans();
    }
}