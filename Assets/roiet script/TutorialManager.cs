using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // เพิ่มเผื่อไว้ใช้จัดการ Scene

public class TutorialManager : MonoBehaviour
{
    public static bool HasShown = false;
    public static bool IsTutorialActive = false;

    [Header("UI Components")]
    public GameObject tutorialPanel;
    public TMP_Text dialogueText;

    [Header("Audio Settings")]
    public AudioSource sfxSource;       
    public AudioSource voiceSource;     
    public AudioSource bgmSource;       
    public AudioClip clickSfx;          
    public AudioClip backgroundMusic;   
    public AudioClip introductionVoice; 

    [Header("BGM Volume Control")]
    [Range(0f, 1f)] public float bgmLowVolume = 0.2f;    
    [Range(0f, 1f)] public float bgmNormalVolume = 0.6f;  

    [Header("Content")]
    [TextArea(3, 5)]
    public string[] sentences;

    private int index = 0;

    // --- ส่วนที่เพิ่มเข้าไปใหม่ เพื่อแก้ปัญหาเสียงทับซ้อน ---
    void OnDisable()
    {
        StopAllSceneAudio();
    }

    void OnDestroy()
    {
        StopAllSceneAudio();
    }

    // ฟังก์ชันสำหรับสั่งหยุดเสียงทุกตัวใน Script นี้
    void StopAllSceneAudio()
    {
        if (sfxSource != null) sfxSource.Stop();
        if (voiceSource != null) voiceSource.Stop();
        if (bgmSource != null) bgmSource.Stop();
        
        // บังคับให้ AudioSource ปล่อย Clip ทิ้งเพื่อคืน Memory (Optional)
        if (bgmSource != null) bgmSource.clip = null; 
    }
    // --------------------------------------------------

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

        if (bgmSource != null && backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.volume = bgmLowVolume;
            bgmSource.Play();
        }

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

        if (IsTutorialActive && voiceSource != null && !voiceSource.isPlaying && bgmSource != null && bgmSource.volume < bgmNormalVolume)
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

        if (voiceSource != null) voiceSource.Stop();

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