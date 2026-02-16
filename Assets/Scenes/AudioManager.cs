using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music")]
    public AudioSource bgmSource;
    public AudioClip backgroundMusic;

    [Header("Voice Instruction")]
    public AudioSource voiceSource;
    public AudioClip stageInstructionClip;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip buttonClickSFX;

    private void Awake()
    {
        // ทำให้เป็น Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ให้ข้ามฉากได้โดยไม่ถูกทำลาย
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayStageInstructionThenBGM();
    }

    /// <summary>
    /// เล่นเสียงแนะนำด่าน → เมื่อจบ → เปิดเพลงพื้นหลัง
    /// </summary>
    public void PlayStageInstructionThenBGM()
    {
        if (stageInstructionClip != null)
        {
            voiceSource.clip = stageInstructionClip;
            voiceSource.Play();

            // เริ่ม BGM หลังเสียงแนะนำจบ
            Invoke(nameof(PlayBackgroundMusic), stageInstructionClip.length);
        }
        else
        {
            PlayBackgroundMusic();
        }
    }

    /// <summary>
    /// เปิดเพลงพื้นหลังแบบลูป
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// เล่นเสียงเอฟเฟกต์ตอนกดปุ่ม
    /// </summary>
    public void PlayButtonSFX()
    {
        if (buttonClickSFX != null)
        {
            sfxSource.PlayOneShot(buttonClickSFX);
        }
    }
}