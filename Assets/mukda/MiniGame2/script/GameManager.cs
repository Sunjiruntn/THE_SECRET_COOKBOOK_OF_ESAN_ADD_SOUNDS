using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // ✅ จำเป็นต้องมีเพื่อใช้ Coroutine

public class GameManager : MonoBehaviour
{
    public int provinceIndex = 1;
    public string nextSceneName = "Mukdahan_Cooking";
    public static bool isRestart = false;

    [Header("--- Audio Settings ---")]
    public AudioSource bgmSource;        
    public AudioSource sfxSource;        
    public AudioSource voiceSource;      
    
    [Space(10)]
    public AudioClip bgmClip;            
    public AudioClip clickSfx;           
    public AudioClip winSfx;             
    public AudioClip loseSfx;            
    public AudioClip introVoice;         

    [Header("Game State")]
    public bool isGameActive = false;
    public bool isGameOver = false;

    [Header("Game Rules")]
    public int burntCount = 0;
    public int maxBurntAllowed = 5;
    public int cookedCount = 0;
    public int totalBeans = 0;

    [Header("UI References")]
    public GameObject gameOverPanel; 
    public GameObject restartButton; 
    public GameObject nextLevelButton; 
    public TMP_Text panelTitleText;  

    [Header("Cooking Settings")]
    public float baseCookSpeed = 10f;
    public BeanSpawner beanSpawner;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false); 

        isGameActive = false;
        isGameOver = false;
        cookedCount = 0;
        burntCount = 0;
        Time.timeScale = 1;

        // ✅ เรียก Coroutine เพื่อรอให้เสียงพูดจบก่อนค่อยเปิด BGM
        StartCoroutine(PlayVoiceThenBGM());
    }

    // --- ฟังก์ชันใหม่: รอให้เสียงพากย์จบก่อนแล้วเพลงค่อยดัง ---
    IEnumerator PlayVoiceThenBGM()
    {
        if (voiceSource != null && introVoice != null)
        {
            voiceSource.clip = introVoice;
            voiceSource.Play();

            // รอจนกว่า voiceSource จะเล่นจบ (เช็คจากความยาวไฟล์เสียง)
            yield return new WaitForSeconds(introVoice.length);
        }

        // เมื่อเล่นเสียงจบ หรือถ้าไม่มีไฟล์เสียง ให้เริ่มเล่น BGM
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 0.5f; // ตั้งความดังเริ่มต้น
            bgmSource.Play();
        }
    }

    public void StartGameplay()
    {
        if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);

        isGameActive = true;
        if (beanSpawner != null) beanSpawner.SpawnBeans();

        BeanStatus[] allBeans = FindObjectsOfType<BeanStatus>();
        totalBeans = allBeans.Length;
    }

    void Update()
    {
        if (!isGameActive || isGameOver) return;

        float actualHeat = baseCookSpeed;
        if (HeatManager.Instance != null)
        {
            actualHeat = HeatManager.Instance.currentHeat * 0.05f;
        }

        BeanStatus[] allBeans = FindObjectsOfType<BeanStatus>();
        foreach (var bean in allBeans)
        {
            bean.AddHeat(actualHeat * Time.deltaTime);
        }
    }

    void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0;
        
        if (sfxSource != null && winSfx != null) sfxSource.PlayOneShot(winSfx);
        if (bgmSource != null) bgmSource.volume = 0.2f; // เบา BGM ลงตอนชนะ

        ShowEndGamePanel(true); 
    }

    void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0;
        
        if (sfxSource != null && loseSfx != null) sfxSource.PlayOneShot(loseSfx);
        if (bgmSource != null) bgmSource.Stop(); // หยุด BGM ตอนแพ้ (ถ้าต้องการ)

        ShowEndGamePanel(false); 
    }

    void ShowEndGamePanel(bool isWin)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (isWin)
        {
            if (GameDataController.Instance != null)
            {
                GameDataController.Instance.PassLevel(provinceIndex);
                GameDataController.Instance.SaveCurrentScene(nextSceneName);
            }
            if (restartButton != null) restartButton.SetActive(false);
            if (nextLevelButton != null) nextLevelButton.SetActive(true);
            if (panelTitleText != null) panelTitleText.text = "ภารกิจสำเร็จ ! คุณคั่วถั่วได้สีสวยน่าทานมากค่ะ";
        }
        else
        {
            if (restartButton != null) restartButton.SetActive(true);
            if (nextLevelButton != null) nextLevelButton.SetActive(false);
            if (panelTitleText != null) panelTitleText.text = "คุณคั่วถั่วไหม้ สีเข้มเกินไปเยอะเกินไปแล้ว กดปุ่มเพื่อเริ่มใหม่";
        }
    }

    public void RetryGame()
    {
        if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);

        isRestart = true;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToNextLevel()
    {
        if (sfxSource != null && clickSfx != null) sfxSource.PlayOneShot(clickSfx);

        Time.timeScale = 1; 
        SceneManager.LoadScene(nextSceneName);
    }

    void CheckWinCondition()
    {
        if (isGameOver) return;
        int finishedBeans = cookedCount + burntCount;

        if (finishedBeans >= totalBeans)
        {
            if (burntCount < maxBurntAllowed)
            {
                WinGame();
            }
            else
            {
                GameOver();
            }
        }
    }

    public void ReportCookedBean(bool increment)
    {
        if (isGameOver) return;
        if (increment) cookedCount++;
        else cookedCount--;
        CheckWinCondition();
    }

    public void ReportBurntBean()
    {
        if (isGameOver || !isGameActive) return;
        burntCount++;

        if (burntCount >= maxBurntAllowed)
        {
            GameOver();
        }
        else
        {
            CheckWinCondition();
        }
    }
}