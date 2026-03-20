using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro; 

public class GrandmaTutorialManager : MonoBehaviour {

    [Header("Character and Scene References")]
    public GameObject grandmaObject;
    public TMP_Text dialogueText; 
    
    [Header("Movement Settings")]
    public float movementDuration = 1.5f; 
    public float stopPositionX = 0.0f;     
    public string nextSceneName = "PumpkinGardenScene";

    [Header("Audio Settings")]
    public AudioSource voiceSource;      // สำหรับเสียงแนะนำและเสียงคลิก (SFX)
    public AudioSource musicSource;      // สำหรับเพลงพื้นหลัง
    public AudioClip introVoiceClip;     // ไฟล์เสียงแนะนำด่าน
    public AudioClip clickSoundClip;     // ไฟล์เสียงตอนกดคลิกเปลี่ยนบทพูด

    [Header("Dialogue Content")]
    private string[] tutorialLines = new string[] {
        "สวัสดีจ้ะหลาน มื้อนี้ยายสิพามาเบิ่งวิธีเก็บบักอึที่แซ่บที่สุด",
        "ข้อแรกเลยที่ต้องเบิ่งกะคือ สีต้องสุกเต็มที่เด้อแล้วกะต้องสีสม่ำเสมอเบิ่ดทั้งลูก",
        "หนวดมันนี่กะต้องเหี่ยว แห้ง หรือเป็นสีน้ำตาลเด้อ ถ้ายังเขียวอยู่แปลว่าอ่อนเกินไป ใช้บ่ได้",
        "เอาล่ะ ไปลองเบิ่งกันเลยนะ! ถ้าเลือกผิดกะบ่เป็นหยัง กลับมาหายายใหม่กะได้",
    };
    
    private string[] failureLines = new string[] {
        "โอ๋ย! บ่เป็นหยังดอกหลาน จำบ่ได้กะมาเบิ่งใหม่อีกได้",
        "จำให้แม่นๆ เด้อ! **สีต้องเข้ม** แล้ว **หนวดต้องเหี่ยว** นั่นแหละของแท้!",
        "ไปลองใหม่เด้อ ยายเชื่อว่าหลานเฮ็ดได้!",
    };
    
    private int dialogueIndex = 0;
    private bool isReturningFromFailure = false;

    void Start() {
        // ตรวจสอบว่าเฟลมาจากด่านฟักทองหรือไม่
        if (PlayerPrefs.GetInt("HasFailedPumpkin", 0) == 1)
        {
            isReturningFromFailure = true; 
            PlayerPrefs.DeleteKey("HasFailedPumpkin"); 
            tutorialLines = failureLines; 
        }

        // ตั้งค่าตำแหน่งเริ่มต้นของยาย
        Vector3 startPosition = new Vector3(-10f, grandmaObject.transform.position.y, 0f);
        grandmaObject.transform.position = startPosition;
        dialogueText.text = ""; 
        
        StartCoroutine(StartTutorialSequence());
    }

    IEnumerator StartTutorialSequence() {
        // 1. จัดการเสียงแนะนำ (เฉพาะเมื่อไม่ใช่การกลับมาเริ่มใหม่)
        if (!isReturningFromFailure) {
            if (voiceSource != null && introVoiceClip != null) {
                voiceSource.PlayOneShot(introVoiceClip);
                yield return new WaitForSeconds(introVoiceClip.length);
            }
        }

        // 2. เริ่มเพลงพื้นหลัง
        if (musicSource != null) {
            musicSource.loop = true;
            musicSource.Play();
        }

        // 3. ยายลอยเข้าฉาก
        yield return StartCoroutine(GrandmaFloatIn());

        // 4. เริ่มบทพูด (รอจนจบทุกประโยค)
        yield return StartCoroutine(RunDialogue());

        // --- [ส่วนที่แก้ไข] หยุดเสียงทุกอย่างก่อนเปลี่ยนฉาก ---
        StopAllSounds();

        // 5. ไปฉากถัดไป
        SceneManager.LoadScene(nextSceneName);
    }
    
    IEnumerator GrandmaFloatIn() {
        float timeElapsed = 0f;
        Vector3 startPosition = grandmaObject.transform.position;
        Vector3 targetPosition = new Vector3(stopPositionX, startPosition.y, startPosition.z);
        
        while (timeElapsed < movementDuration) {
            grandmaObject.transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / movementDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        grandmaObject.transform.position = targetPosition;
    }

    IEnumerator RunDialogue() {
        while (dialogueIndex < tutorialLines.Length) {
            dialogueText.text = tutorialLines[dialogueIndex];
            
            bool clicked = false;
            while (!clicked) {
                if (Input.GetMouseButtonDown(0)) {
                    // เล่นเสียงคลิก
                    if (voiceSource != null && clickSoundClip != null) {
                        voiceSource.PlayOneShot(clickSoundClip);
                    }
                    clicked = true;
                }
                yield return null;
            }
            
            dialogueIndex++;
        }
        
        dialogueText.text = "";
    }

    // ฟังก์ชันสำหรับสั่งหยุดเสียงทั้งหมดใน Manager นี้
    private void StopAllSounds() {
        if (musicSource != null) {
            musicSource.Stop();
        }
        if (voiceSource != null) {
            voiceSource.Stop();
        }
    }
}