using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class TestGameManager : MonoBehaviour
{
    public static TestGameManager Instance;
    [Header("ข้อมูลด่านฝึกสอน (Tutorial)")]
    private float tutorialStartTime;
    public float tutorialTimeUsed;
    public int tutorialMistakes;
    public int tutorialAccuracy;

    [Header("สถานะการสอบ")]
    public bool isTestMode = false;
    private bool isRecording = false;

    [Header("ข้อมูลสถิติ")]
    private int correctCount = 0;
    private int mistakeCount = 0;
    private float startTime;
    private string currentMenuName;

    [Header("รายชื่อ Scene ด่านทำอาหาร")]
    public string[] cookingScenes = { "KhonKaenScene", "SurinScene", "MukdahanScene", "RoiEtScene", "UbonScene" };
    public string resultSceneName = "ResultScene";

    [Header("Google Form Config")]
    public string googleFormUrl = "https://docs.google.com/forms/d/e/1FAIpQLScLSjPMYK3GpiRFtEDAJXFLC5HX6QY_rv5I4ZlJsSB-5O9VgQ/formResponse";
    public string entryId_MenuName = "entry.123456";
    public string entryId_Mode = "entry.1266604681";
    public string entryId_Accuracy = "entry.1592310758";
    public string entryId_Time = "entry.345678";
    public string entryId_FailCount = "entry.456789";
    public string entryId_Grade = "entry.1829486465";


    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void StartRandomExam()
    {
        isTestMode = true;
        isRecording = true;
        correctCount = 0;
        mistakeCount = 0;
        startTime = Time.time;

        int randomIndex = UnityEngine.Random.Range(0, cookingScenes.Length);
        string selectedScene = cookingScenes[randomIndex];
        currentMenuName = selectedScene;

        Debug.Log("สุ่มได้ด่าน: " + selectedScene);
        SceneManager.LoadScene(selectedScene);
    }

    public void RecordSuccess()
    {
        if (!isRecording) return;

        correctCount++;
        // เปลี่ยนข้อความตรงนี้ ให้ดูเป็นกลางๆ
        Debug.Log($"🟢 [บันทึกคะแนน] ทำถูก! (คะแนนสะสม: {correctCount})");
    }

    public void RecordMistake(int penaltyScore = 50)
    {
        if (!isTestMode) return;
        mistakeCount++;
        Debug.Log($"ทำผิด! (ถูก: {correctCount} | ผิด: {mistakeCount})");
    }

    public void FinishExam()
    {
        if (!isTestMode) return;

        isRecording = false; // หยุดนับ
        isTestMode = false;
        float timeUsed = Time.time - startTime;

        int total = correctCount + mistakeCount;
        int accuracyScore = 0;
        if (total > 0)
        {
            float acc = ((float)correctCount / total) * 100f;
            accuracyScore = Mathf.RoundToInt(acc);
        }

        string grade = "F";
        if (accuracyScore >= 90) grade = "S";
        else if (accuracyScore >= 75) grade = "A";
        else if (accuracyScore >= 60) grade = "B";
        else grade = "C";

        Debug.Log($"จบสอบ! คะแนน: {accuracyScore} Grade: {grade}");

        PlayerPrefs.SetInt("FinalScore", accuracyScore);
        PlayerPrefs.SetInt("Mistakes", mistakeCount);
        PlayerPrefs.SetFloat("TimeUsed", timeUsed);
        PlayerPrefs.SetString("FinalGrade", grade);

        StartCoroutine(SendToGoogleForm(currentMenuName, "Test", accuracyScore, timeUsed, mistakeCount, grade));
        SceneManager.LoadScene(resultSceneName);
    }

    IEnumerator SendToGoogleForm(string menu, string mode, int accuracy, float time, int fails, string grade)
    {
        Debug.Log($"📦 เช็กก่อนส่งฟอร์ม! แม่นยำ: {accuracy}% | เวลา: {time} | ผิด: {fails}"); WWWForm form = new WWWForm();

        form.AddField(entryId_MenuName, menu);
        form.AddField(entryId_Mode, mode); // ตัวบอกว่าแถวนี้เป็น Tutorial หรือ Test
        form.AddField(entryId_Accuracy, accuracy.ToString());
        form.AddField(entryId_Time, time.ToString("F2"));
        form.AddField(entryId_FailCount, fails.ToString());
        form.AddField(entryId_Grade, grade);

        using (UnityWebRequest www = UnityWebRequest.Post(googleFormUrl, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
                Debug.Log($"✅ ส่งฟอร์มสำเร็จ! (เพิ่มแถวใหม่เป็นโหมด {mode})");
            else
                Debug.LogError("❌ Form Error: " + www.error);
        }
    }

}