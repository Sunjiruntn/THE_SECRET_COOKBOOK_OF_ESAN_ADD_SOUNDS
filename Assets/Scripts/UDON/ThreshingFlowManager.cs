using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // สำหรับเปลี่ยนฉาก

public class ThreshingFlowController : MonoBehaviour {
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public GameObject videoUI;

    [Header("Grandma Settings")]
    public GameObject grandmaObject;
    public GameObject dialogueCanvas;
    public TMP_Text dialogueText;
    public float movementDuration = 1.5f;
    public float stopPositionX = 0.0f; 
    public string[] sentences; 

    [Header("Game Reference")]
    public ThreshingManager threshingManager;

    void Start() {
        // เตรียมพร้อมเริ่มต้น
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
        if (threshingManager != null) threshingManager.canPlay = false;

        // ตั้งยายไว้นอกจอทางซ้าย
        if (grandmaObject != null) {
            Vector3 startPos = grandmaObject.transform.position;
            grandmaObject.transform.position = new Vector3(-12f, startPos.y, startPos.z);
        }

        StartCoroutine(PlayVideoAndThenGrandma());
    }

    IEnumerator PlayVideoAndThenGrandma() {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared) {
            yield return null;
        }

        videoPlayer.Play();
        Debug.Log("วิดีโอเริ่มเล่น...");

        // รอจนกว่าวิดีโอจะจบ
        while (videoPlayer.isPlaying) {
            yield return null;
        }

        // ปิดจอวิดีโอและเริ่มขั้นตอนคุณยาย
        if (videoUI != null) videoUI.SetActive(false);
        yield return StartCoroutine(StartGrandmaSequence());
    }

    IEnumerator StartGrandmaSequence() {
        // 1. ยายลอยเข้ามา
        float timeElapsed = 0f;
        Vector3 startPos = grandmaObject.transform.position;
        Vector3 targetPos = new Vector3(stopPositionX, startPos.y, startPos.z);

        while (timeElapsed < movementDuration) {
            grandmaObject.transform.position = Vector3.Lerp(startPos, targetPos, timeElapsed / movementDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        grandmaObject.transform.position = targetPos;

        // 2. เริ่มบทพูด
        if (dialogueCanvas != null) dialogueCanvas.SetActive(true);
        int index = 0;
        while (index < sentences.Length) {
            dialogueText.text = sentences[index];
            
            if (Input.GetMouseButtonDown(0)) {
                index++;
                yield return new WaitForSeconds(0.2f);
            }
            yield return null;
        }

        // 3. พูดจบ ปิดกล่องข้อความและเริ่มตีข้าวได้
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
        if (threshingManager != null) threshingManager.canPlay = true;
    }

    // --- ฟังก์ชันสำหรับเปลี่ยนฉาก (ย้ายมาไว้ที่นี่เพื่อให้เรียกใช้จากปุ่มได้) ---
    public void NextScene() {
        // ชื่อ Scene ต้องตรงกับใน Build Settings
        SceneManager.LoadScene("DryingRiceScene");
    }
}

// หมายเหตุ: ลบ Class SceneController แยกออกไปได้เลย เพราะเรายุบฟังก์ชัน NextScene เข้ามาไว้ข้างบนแล้วครับ