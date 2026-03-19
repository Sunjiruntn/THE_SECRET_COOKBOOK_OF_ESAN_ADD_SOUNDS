using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.SceneManagement;

public class KhaoMaoManager : MonoBehaviour
{
    [Header("--- Cutscene Settings ---")]
    [Tooltip("ลาก Object Video Player ที่เตรียมวิดีโอจบเกมไว้มาใส่ตรงนี้")]
    public VideoPlayer cutscenePlayer;

    [Header("--- Animator & Objects ---")]
    [Tooltip("ลาก Object สากที่มี Animator มาใส่")]
    public Animator pestleAnimator;
    [Tooltip("ลากกลุ่ม UI (เกจ, แถบเขียว, ตัวชี้) มาใส่เพื่อสั่งเปิด/ปิด")]
    public GameObject skillCheckGroup;

    [Header("--- UI Skill Check Elements ---")]
    public RectTransform indicator;
    public RectTransform greenZone;
    public Image feedbackOverlay;        // Image เต็มจอสำหรับทำสีวาบ (Alpha 0)

    [Header("--- Rice Display & Sprites ---")]
    public Image miniRiceDisplay;
    public Sprite[] progressSprites;     // ภาพข้าว 5 ระยะ (มุมจอ)
    public SpriteRenderer mortarRiceRenderer;
    public Sprite[] mortarRiceSprites;   // ภาพข้าว 5 ระยะ (ในครก)
    public Transform riceTransform;      // สำหรับทำ Effect ยืดหด

    [Header("--- Game Settings ---")]
    public float moveSpeed = 400f;       // ความเร็วเริ่มต้น
    public float speedIncrement = 50f;   // ความเร็วที่จะเพิ่มขึ้นในแต่ละเซต
    public float gaugeLimit = 150f;
    public float appearanceInterval = 3f;
    public Color perfectColor = new Color(0, 1, 0, 0.3f);
    public Color missColor = new Color(1, 0, 0, 0.3f);

    [Header("--- New Tutorial & Audio Settings ---")]
    public GameObject tutorialTextUI;    // UI Text แนะนำ (แสดง 5 วินาทีแรก)
    public AudioSource voiceSource;      // สำหรับเสียงแนะนำด่านและเสียงตำ (SFX)
    public AudioSource musicSource;      // สำหรับเพลง BGM (Loop)
    public AudioClip introVoiceClip;     // ไฟล์เสียงยายนแนะนำด่าน
    public AudioClip poundSoundClip;     // ไฟล์เสียงตอนตำข้าว (Spacebar)
    public AudioClip backgroundMusic;    // ไฟล์เพลงประกอบด่าน

    // ----------------------------
    //   เพิ่มระบบ Success Panel
    // ----------------------------
    [Header("--- Success Panel Settings ---")]
    public GameObject successPanel;          // UI แสดงความยินดี
    public Button successNextButton;         // ปุ่มถัดไป
    public string nextSceneName = "MiniGame1Mukda";  // ซีนถัดไป
                                                     // ----------------------------
    [Header("--- Save System Settings ---")]
    public int provinceIndex = 0;
    private bool movingRight = true;
    private bool canHit = false;
    private int hitCounter = 0;
    private int currentSet = 0;
    private bool isGameOver = true;      // ล็อคไว้จนกว่า Tutorial จะจบ

    void Start()
    {
        // เริ่มต้น: ซ่อน UI และรีเซ็ตค่าต่างๆ
        if (skillCheckGroup != null) skillCheckGroup.SetActive(false);
        if (feedbackOverlay != null) feedbackOverlay.color = new Color(0, 0, 0, 0);
        if (cutscenePlayer != null) cutscenePlayer.gameObject.SetActive(false);

        UpdateVisuals();

        // เริ่มลำดับการเข้าด่าน (Tutorial -> Voice -> Music -> Game)
        StartCoroutine(StartSequenceRoutine());

        // ----------------------------
        //   ส่วนที่เพิ่มสำหรับ Success Panel
        // ----------------------------
        if (successPanel != null)
            successPanel.SetActive(false);

        if (successNextButton != null)
            successNextButton.onClick.AddListener(OnSuccessNextButtonClicked);

        if (cutscenePlayer != null)
            cutscenePlayer.loopPointReached += OnCutsceneFinished;
        // ----------------------------
    }

    IEnumerator StartSequenceRoutine()
    {
        if (tutorialTextUI != null)
        {
            tutorialTextUI.SetActive(true);
            yield return new WaitForSeconds(5f);
            tutorialTextUI.SetActive(false);
        }

        if (voiceSource != null && introVoiceClip != null)
        {
            voiceSource.PlayOneShot(introVoiceClip);
            yield return new WaitForSeconds(introVoiceClip.length);
        }

        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        isGameOver = false;
        StartCoroutine(SkillCheckRoutine());
    }

    void Update()
    {
        if (canHit && !isGameOver)
        {
            MoveIndicator();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (voiceSource != null && poundSoundClip != null)
                {
                    voiceSource.PlayOneShot(poundSoundClip);
                }
                CheckPrecision();
            }
        }
    }

    void MoveIndicator()
    {
        float move = moveSpeed * Time.deltaTime;
        if (movingRight)
            indicator.anchoredPosition += new Vector2(move, 0);
        else
            indicator.anchoredPosition -= new Vector2(move, 0);

        if (indicator.anchoredPosition.x >= gaugeLimit) movingRight = false;
        if (indicator.anchoredPosition.x <= -gaugeLimit) movingRight = true;
    }

    IEnumerator SkillCheckRoutine()
    {
        while (currentSet < 5)
        {
            yield return new WaitForSeconds(appearanceInterval);

            if (isGameOver) yield break;

            indicator.anchoredPosition = new Vector2(-gaugeLimit, 0);
            movingRight = true;
            skillCheckGroup.SetActive(true);
            canHit = true;

            float timeout = 0f;
            while (canHit && timeout < 2.0f)
            {
                timeout += Time.deltaTime;
                yield return null;
            }

            skillCheckGroup.SetActive(false);
            canHit = false;
        }

        FinishGame();
    }

    void CheckPrecision()
    {
        if (pestleAnimator != null)
        {
            pestleAnimator.SetTrigger("PoundTrigger");
        }

        float distance = Mathf.Abs(indicator.anchoredPosition.x - greenZone.anchoredPosition.x);
        float zoneHalfWidth = greenZone.rect.width / 2f;

        if (distance <= zoneHalfWidth)
        {
            hitCounter++;
            StartCoroutine(FlashScreen(perfectColor));
            StartCoroutine(SquashAndStretchEffect());

            if (hitCounter >= 3)
            {
                currentSet++;
                hitCounter = 0;
                UpdateVisuals();
            }
        }
        else
        {
            hitCounter = 0;
            StartCoroutine(FlashScreen(missColor));
        }

        canHit = false;
        skillCheckGroup.SetActive(false);
    }

    void UpdateVisuals()
    {
        int index = Mathf.Clamp(currentSet, 0, 4);

        if (miniRiceDisplay != null && progressSprites.Length > index)
            miniRiceDisplay.sprite = progressSprites[index];

        if (mortarRiceRenderer != null && mortarRiceSprites.Length > index)
            mortarRiceRenderer.sprite = mortarRiceSprites[index];

        if (currentSet > 0 && currentSet < 5)
        {
            moveSpeed += speedIncrement;
        }
    }

    void FinishGame()
    {
        isGameOver = true;
        canHit = false;
        skillCheckGroup.SetActive(false);

        if (musicSource != null) musicSource.Stop();
        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.PassLevel(provinceIndex);        // ปลดล็อคระดับของจังหวัดนี้
            GameDataController.Instance.SaveCurrentScene(nextSceneName); // จำด่านต่อไป
            GameDataController.Instance.SaveGame();                      // บันทึกการเปลี่ยนแปลง
            Debug.Log("✅ บันทึกข้อมูลผ่านด่านข้าวเม่าเรียบร้อย!");
        }
        if (cutscenePlayer != null)
        {
            cutscenePlayer.gameObject.SetActive(true);
            cutscenePlayer.Play();
            Debug.Log("Playing Ending Cutscene...");
        }
        else
        {
            Debug.LogError("ไม่ได้ลาก Video Player ใส่ใน Inspector!");
        }
    }

    IEnumerator FlashScreen(Color targetColor)
    {
        if (feedbackOverlay == null) yield break;
        feedbackOverlay.color = targetColor;
        float duration = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(targetColor.a, 0, elapsed / duration);
            feedbackOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }
        feedbackOverlay.color = new Color(0, 0, 0, 0);
    }

    IEnumerator SquashAndStretchEffect()
    {
        if (riceTransform == null) yield break;
        Vector3 originalScale = Vector3.one;
        riceTransform.localScale = new Vector3(1.3f, 0.7f, 1f);
        yield return new WaitForSeconds(0.1f);
        riceTransform.localScale = originalScale;
    }

    // ----------------------------
    //        SUCCESS PANEL
    // ----------------------------

    void OnCutsceneFinished(VideoPlayer vp)
    {
        if (successPanel != null)
            successPanel.SetActive(true);
    }

    public void OnSuccessNextButtonClicked()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}