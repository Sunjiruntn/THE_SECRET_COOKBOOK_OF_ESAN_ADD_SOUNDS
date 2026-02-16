using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // ✅ ต้องมีเพื่อใช้ Coroutine

public class IntroKnowledgeController : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI knowledgeText;

    [Header("Content")]
    [TextArea(3, 5)]
    public string[] knowledgeSentences;

    [Header("Navigation")]
    public string nextSceneName = "MINIGAME 1";

    [Header("--- Audio Settings ---")]
    public AudioSource bgmSource;        // สำหรับเพลงพื้นหลัง
    public AudioSource sfxSource;        // สำหรับเสียงกดปุ่ม
    public AudioSource voiceSource;      // สำหรับเสียงพากย์แนะนำ
    
    [Space(10)]
    public AudioClip bgmClip;            // เพลงพื้นหลัง
    public AudioClip clickSfx;           // เสียงเวลาคลิก/กดหน้าจอ
    public AudioClip introVoice;         // เสียงแนะนำตอนเริ่ม (มีไฟล์เดียว)

    private int currentIndex = 0;
    private bool isVoiceFinished = false;

    void Start()
    {
        ShowSentence();

        // 1. เริ่มเล่นเสียงแนะนำทันที
        if (voiceSource != null && introVoice != null)
        {
            voiceSource.clip = introVoice;
            voiceSource.Play();
            
            // 2. เริ่มเปิดเพลงพื้นหลังหลังจากเสียงแนะนำจบลง
            StartCoroutine(WaitAndPlayBGM());
        }
        else
        {
            // ถ้าไม่มีเสียงพากย์ ให้เปิด BGM ทันที
            PlayBGM();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            NextSentence();
        }
    }

    IEnumerator WaitAndPlayBGM()
    {
        // รอจนกว่าเสียงพากย์จะเล่นจบ (ตามความยาวไฟล์)
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

    void ShowSentence()
    {
        if (currentIndex < knowledgeSentences.Length)
        {
            if (knowledgeText != null)
            {
                knowledgeText.text = knowledgeSentences[currentIndex];
            }
        }
        else
        {
            GoToMiniGame();
        }
    }

    public void NextSentence()
    {
        // 3. เล่นเสียงคลิกทุกครั้งที่มีการกดเปลี่ยนประโยค
        if (sfxSource != null && clickSfx != null)
        {
            sfxSource.PlayOneShot(clickSfx);
        }

        currentIndex++;
        ShowSentence();
    }

    void GoToMiniGame()
    {
        Debug.Log("จบการให้ความรู้... กำลังไปที่ " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }
}