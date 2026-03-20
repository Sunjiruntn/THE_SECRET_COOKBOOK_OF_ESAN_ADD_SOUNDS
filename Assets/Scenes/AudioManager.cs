using UnityEngine;
using UnityEngine.SceneManagement; // สำคัญมากสำหรับการดักจับการเปลี่ยนฉาก

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
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // ลงทะเบียนเหตุการณ์: เมื่อโหลดซีนใหม่ ให้รันฟังก์ชัน OnSceneLoaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
    }

    // ฟังก์ชันนี้จะทำงาน "ทุกครั้ง" ที่มีการเปลี่ยนฉากสำเร็จ
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded: " + scene.name + ". Cleaning up old audio...");
        StopAllAudio();
        
        // ถ้าต้องการให้เริ่มเล่นเสียงแนะนำด่านใหม่ทันทีที่โหลดฉากเสร็จ
        // สามารถเรียก PlayStageInstructionThenBGM() ตรงนี้ได้เลย
        // PlayStageInstructionThenBGM(); 
    }

    private void Start()
    {
        // เริ่มต้นครั้งแรกสุดเมื่อเข้าเกม
        PlayStageInstructionThenBGM();
    }

    /// <summary>
    /// หยุดเสียงทั้งหมดและยกเลิกคิว Invoke ที่ค้างมาจากซีนเก่า
    /// </summary>
    public void StopAllAudio()
    {
        // สำคัญที่สุด: ยกเลิก Invoke (nameof(PlayBackgroundMusic)) ที่ค้างอยู่
        CancelInvoke();

        if (bgmSource != null) bgmSource.Stop();
        if (voiceSource != null) voiceSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
    }

    public void PlayStageInstructionThenBGM()
    {
        StopAllAudio();

        if (stageInstructionClip != null)
        {
            voiceSource.clip = stageInstructionClip;
            voiceSource.Play();

            // สั่งเล่น BGM หลังจากคลิปเสียงพูดจบ
            Invoke(nameof(PlayBackgroundMusic), stageInstructionClip.length);
        }
        else
        {
            PlayBackgroundMusic();
        }
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlayButtonSFX()
    {
        if (buttonClickSFX != null)
        {
            sfxSource.PlayOneShot(buttonClickSFX);
        }
    }

    // เพื่อความปลอดภัย: เมื่อ Object ถูกทำลาย ให้ถอนการลงทะเบียน Event ด้วย
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}