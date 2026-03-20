using UnityEngine;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    public TextMeshProUGUI introText;
    public string[] introLines;
    private int currentIndex = 0;
    public GameObject introPanel;
    public IngredientManager ingredientManager;

    [Header("--- Audio Settings ---")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource;

    [Space(10)]
    public AudioClip bgmClip;            // เพลงพื้นหลัง
    public AudioClip clickSfx;           // เสียงปุ่มกด
    public AudioClip introVoice;         // เสียงแนะนำ (มีไฟล์เดียว)

    [Header("--- Test Mode (ลากคุณย่ามาใส่เพื่อซ่อน) ---")]
    public GameObject grandmaCharacter;  // เพิ่มตัวแปรคุณย่า เผื่ออยากให้ซ่อนในด่านนี้ด้วย

    // 🌟 [เพิ่มใหม่] เมื่อ Object นี้ถูกปิดใช้งาน หรือเปลี่ยน Scene ให้หยุดเสียงทันที
    void OnDisable()
    {
        StopAllSounds();
    }

    // 🌟 [เพิ่มใหม่] เพื่อความปลอดภัยยิ่งขึ้นเมื่อ Object ถูกทำลาย
    void OnDestroy()
    {
        StopAllSounds();
    }

    // ฟังก์ชันช่วยสำหรับหยุดเสียงทั้งหมดใน Manager นี้
    void StopAllSounds()
    {
        if (bgmSource != null) bgmSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
        if (voiceSource != null) voiceSource.Stop();
        
        // หยุด Coroutine ทั้งหมดที่อาจจะกำลังรอเล่น BGM อยู่
        StopAllCoroutines();
    }

    // 🌟 Awake ทำงานก่อน Start ปิด Panel ทันทีตั้งแต่เฟรมแรกที่เกมโหลด!
    void Awake()
    {
        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            if (introPanel != null) introPanel.SetActive(false);
            if (grandmaCharacter != null) grandmaCharacter.SetActive(false);
        }
    }

    void Start()
    {
        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            // โหมดสอบ ข้าม Intro ปิด Panel (ย้ำอีกรอบเพื่อความชัวร์)
            if (introPanel != null) introPanel.SetActive(false);
            if (grandmaCharacter != null) grandmaCharacter.SetActive(false);

            // ปิดเสียงพากย์ด้วยเผื่อมันแอบดัง
            if (voiceSource != null) voiceSource.Stop();

            PlayBGM();
            if (ingredientManager != null) ingredientManager.StartMixingPhase();
            return;
        }

        currentIndex = 0;
        if (introText != null && introLines.Length > 0)
            introText.text = introLines[currentIndex];

        // 1. เริ่มเล่นเสียงแนะนำทันทีที่เปิดเกม
        if (voiceSource != null && introVoice != null)
        {
            voiceSource.clip = introVoice;
            voiceSource.Play();

            // 2. เริ่มเปิดเพลงพื้นหลังหลังจากเสียงแนะนำจบ
            StartCoroutine(WaitAndPlayBGM());
        }
        else
        {
            // ถ้าไม่มีเสียงแนะนำ ให้เปิด BGM ทันที
            PlayBGM();
        }
    }

    IEnumerator WaitAndPlayBGM()
    {
        // รอจนกว่าเสียงพากย์จะจบ
        yield return new WaitForSeconds(introVoice.length);

        PlayBGM();
    }

    void PlayBGM()
    {
        if (bgmSource != null && bgmClip != null && !bgmSource.isPlaying)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void OnPanelClick()
    {
        // เสียงปุ่มกดทุกครั้งที่คลิก
        if (sfxSource != null && clickSfx != null)
        {
            sfxSource.PlayOneShot(clickSfx);
        }

        currentIndex++;

        if (currentIndex < introLines.Length)
        {
            introText.text = introLines[currentIndex];
        }
        else
        {
            // ปิด Panel และเริ่มเกม
            introPanel.SetActive(false);
            
            // เมื่อจบ Intro ถ้าคุณต้องการให้ BGM หยุดทันทีด้วย สามารถเรียกใช้ StopAllSounds() ตรงนี้ได้
            // แต่ถ้าอยากให้ BGM เล่นต่อไปจนจบด่าน ก็ไม่ต้องใส่ครับ
            
            if (ingredientManager != null)
            {
                ingredientManager.StartMixingPhase();
            }
        }
    }
}