using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleMapController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button btnProvince1;
    public Button btnProvince2;
    public Button btnProvince3;
    public Button btnProvince4;
    public Button btnProvince5;
    void Start()
    {
        // อ่านข้อมูลจาก GameData (ซึ่งจะเป็นค่า "ล่าสุด" เสมอ ไม่ว่าจะมาจาก Start หรือ Continue)
        if (GameDataController.Instance != null)
        {
            bool[] unlocked = GameDataController.Instance.playerData.provinceUnlocked;


            if (btnProvince1 != null) btnProvince1.interactable = unlocked[0];

            if (btnProvince2 != null) btnProvince2.interactable = unlocked[1];

            if (btnProvince3 != null) btnProvince3.interactable = unlocked[2];
            // ครั้งหน้ากด Continue จะได้กลับมาหน้านี้
            GameDataController.Instance.SaveCurrentScene("MapSelect");
        }
    }

    public void GoToProvince1()
    {
        SceneManager.LoadScene("SubMap_RoiEt");
    }

    public void GoToProvince2()
    {
        SceneManager.LoadScene("SubMap_Mukda");
    }
    public void GoToProvince3()
    {
        SceneManager.LoadScene("SubMap_Surin");
    }

    public void GoToProvince4()
    {
        SceneManager.LoadScene("SubMap_Khonkaen");
    }
    public void GoToProvince5()
    {
        SceneManager.LoadScene("SubMap_Udon");
    }

    public void GoToBackMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}