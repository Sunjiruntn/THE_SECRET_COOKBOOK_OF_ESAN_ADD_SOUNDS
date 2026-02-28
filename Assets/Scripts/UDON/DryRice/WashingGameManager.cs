using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class WashingGameManager : MonoBehaviour
{
    [Header("Save Data Settings")]
    public int provinceIndex = 2; // จังหวัดอุดรธานี Index = 4
    public string nextSceneName = "Kitchen";
    [Header("Video & UI Settings")]
    public VideoPlayer videoPlayer;
    public GameObject videoUI;
    public GameObject instructionPanel;
    public TMP_Text instructionText;
    public GameObject successUI;

    [Header("Sprites")]
    public SpriteRenderer bucketRenderer;
    public Sprite bucketWithWater, bucketWithRice, stir1, stir2, stir3, finalRice;

    private int currentStep = 0;
    private int stirCount = 0;

    // ตัวแปร static สำหรับจำสถานะข้ามซีน
    public static bool isBlackRiceDone = false;

    void Start()
    {
        if (successUI != null) successUI.SetActive(false);
        if (instructionPanel != null) instructionPanel.SetActive(false);

        // เช็กว่าถ้ากลับมาจากเก็บเมล็ดเสีย ให้เริ่มที่ขั้นตอนคนข้าวต่อเลย
        if (isBlackRiceDone)
        {
            if (videoUI != null) videoUI.SetActive(false);
            StartGameFromStirring();
        }
        else
        {
            if (bucketRenderer != null && bucketWithWater != null)
            {
                bucketRenderer.sprite = bucketWithWater;
            }

            if (videoPlayer != null)
            {
                if (videoUI != null) videoUI.SetActive(true);
                StartCoroutine(PlayVideoSequence());
            }
            else
            {
                StartGame();
            }
        }
    }

    void StartGame()
    {
        if (instructionPanel != null) instructionPanel.SetActive(true);
        currentStep = 1;
        instructionText.text = "ขั้นตอนที่ 1: คลิกที่เมล็ดข้าวเพื่อเติมลงในถังน้ำ";
    }

    void StartGameFromStirring()
    {
        if (instructionPanel != null) instructionPanel.SetActive(true);
        currentStep = 2;
        stirCount = 2; // เซตค่าว่าคนไปแล้ว 2 ครั้ง
        bucketRenderer.sprite = stir2;
        instructionText.text = "ล้างเมล็ดเสียแล้ว! ขั้นตอนที่ 2: คลิกที่ถังเพื่อคนครั้งสุดท้าย (2/3)";
    }

    IEnumerator PlayVideoSequence()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared) yield return null;
        videoPlayer.Play();
        while (videoPlayer.isPlaying || videoPlayer.frame < (long)videoPlayer.frameCount - 2) yield return null;

        videoPlayer.Stop();
        videoPlayer.targetCamera = null;
        videoPlayer.enabled = false;
        if (videoUI != null) videoUI.SetActive(false);

        StartGame();
    }

    public void OnObjectClicked(string objectType, GameObject clickedObject)
    {
        string cleanType = objectType.Trim();

        // ขั้นตอนที่ 1: คลิกข้าว
        if (currentStep == 1 && cleanType == "Rice")
        {
            bucketRenderer.sprite = bucketWithRice;
            if (clickedObject != null) clickedObject.SetActive(false);
            currentStep = 2;
            instructionText.text = "ขั้นตอนที่ 2: คลิกที่ถังเพื่อคนข้าว (0/3)";
        }
        // ขั้นตอนที่ 2: คลิกถัง (คนข้าว)
        else if (currentStep == 2 && cleanType == "Bucket")
        {
            stirCount++;

            if (stirCount == 1)
            {
                bucketRenderer.sprite = stir1;
                instructionText.text = "ขั้นตอนที่ 2: คนข้าว (1/3)";
            }
            else if (stirCount == 2)
            {
                bucketRenderer.sprite = stir2;
                // ตัดไปซีน BlackRiceScenes ทันทีที่กดครั้งที่ 2
                SceneManager.LoadScene("BlackRiceScenes");
            }
            else if (stirCount == 3)
            {
                bucketRenderer.sprite = stir3;
                currentStep = 3;
                instructionText.text = "ขั้นตอนที่ 3: คลิกที่ถังเพื่อเทน้ำออก";
            }
        }
        // ขั้นตอนที่ 3: คลิกถัง (เทน้ำ)
        else if (currentStep == 3 && cleanType == "Bucket")
        {
            bucketRenderer.sprite = finalRice;
            if (instructionPanel != null) instructionPanel.SetActive(false);
            if (successUI != null) successUI.SetActive(true);

            // รีเซ็ตค่า static เผื่อเล่นใหม่รอบหน้า
            isBlackRiceDone = false;
            if (GameDataController.Instance != null)
            {
                Debug.Log($"Saving Progress: Province {provinceIndex} - Washing Game Complete");

                // ผ่านด่านย่อย (Level) ของจังหวัดอุดรธานี
                GameDataController.Instance.PassLevel(provinceIndex);

                // จำฉากถัดไปไว้ (Kitchen)
                GameDataController.Instance.SaveCurrentScene(nextSceneName);

                // บันทึกลงไฟล์
                GameDataController.Instance.SaveGame();
            }
        }
    }

    public void NextScene()
    {
        SceneManager.LoadScene("Kitchen");
    }
}