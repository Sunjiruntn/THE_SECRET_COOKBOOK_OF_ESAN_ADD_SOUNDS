using UnityEngine;
using System.Collections;
using TMPro;

public class RiceTutorialManager : MonoBehaviour {

    [Header("References")]
    public GameObject grandmaObject;
    public TMP_Text dialogueText; 
    public GameObject dialogueUI; 
    private HarvestManager harvestManager;

    [Header("Movement")]
    public float movementDuration = 1.5f;
    public float stopPositionX = 0.0f;
    public float exitPositionX = 12.0f;

    [Header("Audio Settings")]
    public AudioSource voiceSource;      // ลาก AudioSource ที่จะใช้เล่นเสียงพูดมาใส่
    public AudioSource musicSource;      // ลาก AudioSource ที่จะใช้เล่นเพลงพื้นหลังมาใส่
    public AudioClip introVoiceClip;     // ไฟล์เสียงแนะนำตอนเริ่มด่าน
    public AudioClip clickRiceClip;      // ไฟล์เสียงตอนกดที่รวงข้าว

    private string[] tutorialLines = new string[] {
        "มื้อนี้ยายสิพามาเกี่ยวข้าวไปเฮ็ดข้าวเม่าบ้านผือเด้อ ข้าวเม่านี่เพิ่นบ่ได้มีให้กินเบิดปีเด้อ มันสิมีแค่ช่วงคราวเดียวสั้นๆ ",
        "ช่วง เดือนเก้า เดือนสิบ ฮอดเดือนสิบเอ็ด (กันยา-พฤศจิกา) นี่ล่ะ เพราะมันเป็นช่วงที่ข้าวเหนียวกำลังพอดีตำ เพิ่นเอิ้นว่า 'ข้าวระยะน้ำนม' (ประมาณ 15-20 มื้อหลังมันถ้อยดอก",
        "เฮาต้องฟ้าวเกี่ยวเอามาคั่ว มาตำ ตอนมันยังอ่อนๆ อยู่ มันจั่งสิหอมห่วยๆ เนื้อนุ่ม ลิ้นสิเขียวงามตา กินแซ่บแท้ได๋หล่า โดยเฉพาะช่วง ออกพรรษา นี่ล่ะ",
        "เป็นยามเพิ่นลงมือเฮ็ดข้าวเม่ากันคักที่สุด ไปทางได๋กะได้กลิ่นหอมข้าวใหม่เต็มทีละบ้านเลยล่ะลูก",
        "สังเกตสีดีๆ เลือก 'ข้าวระยะเม่า' สีเขียวอ่อนรวงแก่ด้อหลาน",
        "พยายามเกี่ยวมาให้ยายสัก 5 รวง... ไปลองเบิ่งหลาน!"
    };

    void Start() {
        harvestManager = FindObjectOfType<HarvestManager>();

        // ตรวจสอบสถานะการเล่นรอบที่แล้ว
        if (PlayerPrefs.GetInt("HasFailedRice", 0) == 1) {
            PlayerPrefs.DeleteKey("HasFailedRice"); 
            tutorialLines = new string[] { 
                "โอ๋ย! บ่เป็นหยังลูก ลองเกี่ยวเบิ่งให้อีกจักเทื่อ", 
                "ไปลองใหม่ ยายเชื่อว่าลูกเฮ็ดได้!" 
            };
        }

        // เซตคุณยายอยู่นอกจอซ้าย
        grandmaObject.transform.position = new Vector3(-12f, grandmaObject.transform.position.y, 0f);
        if(dialogueUI != null) dialogueUI.SetActive(false); 

        // เริ่มขั้นตอน Tutorial
        StartCoroutine(StartTutorialSequence());
    }

    IEnumerator StartTutorialSequence() {
        // 1. เล่นเสียงแนะนำตอนเริ่มด่าน
        if (introVoiceClip != null && voiceSource != null) {
            voiceSource.PlayOneShot(introVoiceClip);
            // รอจนกว่าเสียงแนะนำจะจบลง (อิงตามความยาวไฟล์เสียง)
            yield return new WaitForSeconds(introVoiceClip.length);
        }

        // 2. เมื่อเสียงแนะนำจบ เริ่มเล่นเพลงพื้นหลัง
        if (musicSource != null) {
            musicSource.loop = true;
            musicSource.Play();
        }

        // 3. เริ่มลำดับการเคลื่อนที่และไดอะล็อกเดิม
        yield return StartCoroutine(GrandmaMove(stopPositionX)); // ลอยเข้า
        if(dialogueUI != null) dialogueUI.SetActive(true);
        
        yield return StartCoroutine(RunDialogue()); // รอคลิกบทพูดจนจบ
        
        if(dialogueUI != null) dialogueUI.SetActive(false); // ปิด UI บทพูด
        yield return StartCoroutine(GrandmaMove(exitPositionX)); // ลอยออก
        
        if (harvestManager != null) harvestManager.StartGame(); // ปลดล็อกการเกี่ยวข้าว
    }
    
    IEnumerator GrandmaMove(float targetX) {
        float timeElapsed = 0f;
        Vector3 startPos = grandmaObject.transform.position;
        Vector3 endPos = new Vector3(targetX, startPos.y, startPos.z);
        
        while (timeElapsed < movementDuration) {
            grandmaObject.transform.position = Vector3.Lerp(startPos, endPos, timeElapsed / movementDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        grandmaObject.transform.position = endPos;
    }

    IEnumerator RunDialogue() {
        int index = 0;
        while (index < tutorialLines.Length) {
            dialogueText.text = tutorialLines[index];
            bool clicked = false;
            while (!clicked) {
                // เช็คการคลิกเพื่อเปลี่ยนบทพูด
                if (Input.GetMouseButtonDown(0)) {
                    clicked = true;
                }
                yield return null;
            }
            index++;
        }
    }

    // ฟังก์ชันสำหรับเรียกใช้เมื่อกดที่ข้าว (เรียกจากสคริปต์คลิกข้าวของคุณ)
    public void PlayRiceClickSound() {
        if (voiceSource != null && clickRiceClip != null) {
            voiceSource.PlayOneShot(clickRiceClip);
        }
    }

    void Update() {
        // เช็คการคลิกเมาส์ในระหว่างที่เกมเริ่มแล้ว เพื่อเล่นเสียงคลิกข้าว
        // (ตรวจสอบว่า dialogueUI ปิดอยู่ หรือ harvestManager เริ่มเกมแล้ว)
        if (Input.GetMouseButtonDown(0) && dialogueUI != null && !dialogueUI.activeSelf) {
             PlayRiceClickSound();
        }
    }
}