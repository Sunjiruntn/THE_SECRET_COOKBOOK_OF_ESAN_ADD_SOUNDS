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

    void Start()
    {
        if (TestGameManager.Instance != null && TestGameManager.Instance.isTestMode)
        {
            // โหมดสอบ ข้าม Intro ปิด Panel 
            introPanel.SetActive(false);
            PlayBGM(); 
            if (ingredientManager != null) ingredientManager.StartMixingPhase();
            return; 
        }
        currentIndex = 0;
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
            if (ingredientManager != null)
            {
                ingredientManager.StartMixingPhase();
            }
        }
    }
}