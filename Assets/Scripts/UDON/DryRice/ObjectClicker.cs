using UnityEngine;

public class ObjectClicker : MonoBehaviour {
    [Tooltip("พิมพ์ 'Rice' หรือ 'Bucket'")]
    public string objectType; 
    private WashingGameManager manager;

    void Start() {
        manager = Object.FindFirstObjectByType<WashingGameManager>();
    }

    void OnMouseDown() {
        // เมื่อคลิก จะแสดงข้อความใน Console ทันทีเพื่อทดสอบ
        Debug.Log("มีการคลิกเกิดขึ้นที่วัตถุ: " + gameObject.name + " (Type: " + objectType + ")");
        
        if (manager != null) {
            manager.OnObjectClicked(objectType, this.gameObject);
        } else {
            Debug.LogError("หา WashingGameManager ในฉากไม่เจอ!");
        }
    }
}