using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static bool HasShown = false;
    public static bool IsTutorialActive = false;

    [Header("UI Components")]
    public GameObject tutorialPanel;
    public TMP_Text dialogueText;

    [Header("Audio Settings")]
    public AudioSource sfxSource;       // สำหรับเสียงคลิก
    public AudioSource voiceSource;     // สำหรับเสียงแนะนำ (Voice Over)
    public AudioSource bgmSource;       // สำหรับเพลงพื้นหลัง
    public AudioClip clickSfx;          // เสียงกดข้าม
    public AudioClip backgroundMusic;   // เพลง BGM
    public AudioClip introductionVoice; // เสียงแนะนำที่มีเพียง 1 ไฟล์

    [Header("BGM Volume Control")]
    [Range(0f, 1f)] public float bgmLowVolume = 0.2f;    // ความดังเพลงตอนมีเสียงบรรยาย
    [Range(0f, 1f)] public float bgmNormalVolume = 0.6f;  // ความดังเพลงตอนเล่นเกมปกติ

    [Header("Content")]
    [TextArea(3, 5)]
    public string[] sentences;

    private int index = 0;

    void Start()
    {
        if (HasShown)
        {
            EndTutorial();
            return;
        }

        InitializeTutorial();
    }

    void InitializeTutorial()
    {
        sentences = new string[]
        {
            "ยินดีต้อนรับเข้าสู่จังหวัดร้อยเอ็ด วันนี้เราจะมาจับปูนากันนะ แต่ต้องระวังปูนะโดยมีวิธีการแยกรูปู รูงูดังนี้",
            "ลักษณะปากรูและบริเวณรอบ ๆ รู โดยรูปูนาจะมีขุยดินเป็นกองใหญ่ๆ \nมีรอยเท้าปูชัดเจน รูขรุขระ ในขณะที่รูงูมักจะเรียบเนียนเหมือนเข้าออกบ่อยๆ",
            "ขอให้หลานโชคดีกับการหาปูนะ \nโดยวันนี้จะจับปูให้ครบ 5 ตัว \nถ้าขุดโดนงูต้องเริ่มขุดใหม่นะจ้ะ"
        };

        IsTutorialActive = true;
        HasShown = true;
        tutorialPanel.SetActive(true);
        index = 0;

        // 1. เล่นเพลง BGM เบาๆ รอไว้
        if (bgmSource != null && backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.volume = bgmLowVolume;
            bgmSource.Play();
        }

        // 2. เล่นเสียงแนะนำไฟล์เดียวตอนเริ่ม
        if (voiceSource != null && introductionVoice != null)
        {
            voiceSource.clip = introductionVoice;
            voiceSource.loop = false;
            voiceSource.Play();
        }

        ShowSentence();
    }

    void Update()
    {
        if (IsTutorialActive && Input.GetMouseButtonDown(0))
        {
            NextSentence();
        }

        // (Option) ถ้าเสียงแนะนำจบก่อนที่คนจะกดจบ Tutorial ให้เร่งเพลงดังขึ้นอัตโนมัติ
        if (IsTutorialActive && voiceSource != null && !voiceSource.isPlaying && bgmSource.volume < bgmNormalVolume)
        {
            bgmSource.volume = Mathf.Lerp(bgmSource.volume, bgmNormalVolume, Time.deltaTime);
        }
    }

    void ShowSentence()
    {
        dialogueText.text = sentences[index];
    }

    public void NextSentence()
    {
        // เล่นเสียงคลิกปุ่มทุกครั้งที่กด
        if (sfxSource != null && clickSfx != null)
        {
            sfxSource.PlayOneShot(clickSfx);
        }

        index++;

        if (index < sentences.Length)
        {
            ShowSentence();
        }
        else
        {
            EndTutorial();
        }
    }

    void EndTutorial()
    {
        IsTutorialActive = false;
        tutorialPanel.SetActive(false);

        // ถ้ากดจบก่อนที่เสียงแนะนำจะพูดจบ ให้หยุดเสียงพูด
        if (voiceSource != null) voiceSource.Stop();

        // เร่งเสียงเพลงพื้นหลังให้ดังปกติ
        if (bgmSource != null)
        {
            bgmSource.volume = bgmNormalVolume;
            if (!bgmSource.isPlaying && backgroundMusic != null)
            {
                bgmSource.clip = backgroundMusic;
                bgmSource.Play();
            }
        }
    }
}