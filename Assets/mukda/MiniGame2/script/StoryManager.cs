using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class StoryManager : MonoBehaviour
{
    [Header("Story Settings")]
    [TextArea(3, 10)]
    public List<string> sentences;
    public TMP_Text storyText;
    public GameObject storyPanel;

    private int currentIndex = 0;
    private GameManager gameManager;

    // 1. ✅ เพิ่มตัวแปรนี้เพื่อกันไม่ให้มันทำงานซ้ำ
    private bool isStoryFinished = false;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (GameManager.isRestart)
        {
            currentIndex = sentences.Count - 1;
        }
        else
        {
            currentIndex = 0;
        }

        UpdateText();
    }

    void Update()
    {
        // 2. ✅ เพิ่มบรรทัดนี้: ถ้าเล่าจบแล้ว ให้หยุดทำงานทันที (ไม่ต้องรับคลิกอีก)
        if (isStoryFinished) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (currentIndex < sentences.Count - 1)
            {
                currentIndex++;
                UpdateText();
            }
            else
            {
                StartGame();
            }
        }
    }

    void UpdateText()
    {
        if (sentences.Count > 0)
        {
            storyText.text = sentences[currentIndex];
        }
    }

    void StartGame()
    {
        // 3. ✅ ติ๊กถูกว่าจบแล้วนะ (ห้ามใครมากดเล่นอีก)
        isStoryFinished = true;

        storyPanel.SetActive(false);

        if (gameManager != null)
        {
            gameManager.StartGameplay();
        }

        // แถม: ปิดสคริปต์ตัวเองไปเลยเพื่อความชัวร์และประหยัดเครื่อง
        this.enabled = false;
    }
}