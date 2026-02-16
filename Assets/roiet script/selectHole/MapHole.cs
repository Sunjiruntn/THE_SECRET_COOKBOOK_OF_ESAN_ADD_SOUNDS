using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class MapHole : MonoBehaviour
{

    public int holeId; // ตั้ง 0..8 ใน Inspector

    void OnMouseDown()
    {
        // ถ้า Tutorial ยังเปิดอยู่ ห้ามกดหลุม
        if (TutorialManager.IsTutorialActive) return;
        // -----------------

        if (HoleManager.Instance.IsDug(holeId))
        {
            Debug.Log($"หลุม {holeId} ขุดไปแล้ว");
            return;
        }
        if (HoleManager.Instance.IsDug(holeId))
        {
            Debug.Log($"หลุม {holeId} ขุดไปแล้ว");
            return; // ไม่ให้เข้าไปอีก
        }

        // ล็อกผลครั้งแรก
        HoleManager.Instance.RandomizeIfNeeded(holeId);
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        PlayerPrefs.SetInt("CurrentHole", holeId);
        SceneManager.LoadScene("Closeup"); // ชื่อซีนขุดของคุณ
    }
}