using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MapHole : MonoBehaviour
{
    public int holeId; // 0..7

    void OnMouseDown()
    {
        if (TutorialManager.IsTutorialActive) return;

        if (HoleManager.Instance.IsDug(holeId))
        {
            Debug.Log($"หลุม {holeId} ขุดไปแล้ว");
            return;
        }

        // **ล็อกแพทเทิร์น** (ถ้ายังไม่เคยสุ่ม)
        HoleManager.Instance.RandomizeIfNeeded(holeId);

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        PlayerPrefs.SetInt("CurrentHole", holeId);
        SceneManager.LoadScene("Closeup");
    }
}