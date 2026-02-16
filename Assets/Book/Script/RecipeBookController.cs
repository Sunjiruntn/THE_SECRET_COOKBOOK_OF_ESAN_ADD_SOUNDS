using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RecipeBookController : MonoBehaviour
{
    [Header("--- UI Control ---")]
    public GameObject bookPanel;
    public GameObject openButton;
    public GameObject lockOverlay;

    [Header("--- Buttons ---")]
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    [Header("--- Left Page UI ---")]
    public TextMeshProUGUI provinceNameText;
    public Image foodImage;
    public TextMeshProUGUI menuNameText;
    public TextMeshProUGUI storyText;

    [Header("--- Right Page UI ---")]
    public Transform ingredientsGrid;
    public GameObject ingredientSlotPrefab;
    public TextMeshProUGUI howToText;
    public TextMeshProUGUI tipsText;

    [Header("--- Headers to Hide (หัวข้อที่ต้องซ่อน) ---")]
    public GameObject headerIngredients; 
    public GameObject headerHowTo;       
    public GameObject headerTips;        

    [Header("--- Data ---")]
    public List<RecipeData> recipeList;

    [Header("--- Text Popup System ---")]
    [Tooltip("ลาก Popup_Overlay_Blocker สีดำๆ มาใส่ตรงนี้")]
    public GameObject textPopupBlocker;

    [Tooltip("ลากตัว FullText ใน Scroll View มาใส่ตรงนี้")]
    public TextMeshProUGUI popupFullText;

    [Tooltip("ลากปุ่มปิด Popup (X) มาใส่ตรงนี้")]
    public Button closePopupButton;

    [Header("--- Audio Settings ---")]
    public AudioSource sfxSource;      // ตัวเล่นเสียง
    public AudioClip clickSfx;        // ไฟล์เสียงกดปุ่มทั่วไป
    public AudioClip pageFlipSfx;     // ไฟล์เสียงพลิกหน้ากระดาษ (เพิ่มใหม่)

    private int currentPageIndex = 0;

    void Start()
    {
        CloseBook();

        if (openButton)
        {
            Button btn = openButton.GetComponent<Button>();
            if (btn) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OpenBook); }
        }
        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (prevButton) prevButton.onClick.AddListener(PrevPage);
        if (closeButton) closeButton.onClick.AddListener(CloseBook);
        
        if (closePopupButton)
            closePopupButton.onClick.AddListener(CloseTextPopup);

        if (textPopupBlocker) textPopupBlocker.SetActive(false);
    }

    // ฟังก์ชันช่วยเล่นเสียงคลิกปกติ
    private void PlayClickSound()
    {
        if (sfxSource != null && clickSfx != null)
        {
            sfxSource.PlayOneShot(clickSfx);
        }
    }

    // ฟังก์ชันช่วยเล่นเสียงพลิกหน้ากระดาษ (เพิ่มใหม่)
    private void PlayPageFlipSound()
    {
        if (sfxSource != null && pageFlipSfx != null)
        {
            sfxSource.PlayOneShot(pageFlipSfx);
        }
    }

    public void OpenBook()
    {
        PlayPageFlipSound(); // ใช้เสียงพลิกหน้าตอนเปิดหนังสือเพื่อให้ความรู้สึกเหมือนเปิดสมุด
        bookPanel.SetActive(true);
        openButton.SetActive(false);
        Time.timeScale = 0f;
        currentPageIndex = 0;
        UpdateUI();
    }

    public void CloseBook()
    {
        if (bookPanel.activeSelf) PlayClickSound(); 
        
        bookPanel.SetActive(false);
        openButton.SetActive(true);
        Time.timeScale = 1f;
    }

    public void NextPage()
    {
        if (currentPageIndex < recipeList.Count - 1)
        {
            PlayPageFlipSound(); // ✅ เปลี่ยนเป็นเสียงพลิกกระดาษ
            currentPageIndex++;
            UpdateUI();
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            PlayPageFlipSound(); // ✅ เปลี่ยนเป็นเสียงพลิกกระดาษ
            currentPageIndex--;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (recipeList == null || recipeList.Count == 0) return;

        RecipeData currentData = recipeList[currentPageIndex];

        bool isUnlocked = false;
        if (GameDataController.Instance != null)
        {
            if (currentPageIndex < GameDataController.Instance.playerData.cookbookCollected.Length)
                isUnlocked = GameDataController.Instance.playerData.cookbookCollected[currentPageIndex];
        }
        else isUnlocked = true;

        if (isUnlocked)
        {
            if (lockOverlay) lockOverlay.SetActive(false);
            if (headerIngredients) headerIngredients.SetActive(true);
            if (headerHowTo) headerHowTo.SetActive(true);
            if (headerTips) headerTips.SetActive(true);

            if (foodImage) { foodImage.gameObject.SetActive(true); foodImage.sprite = currentData.foodImage; }
            if (provinceNameText) provinceNameText.text = currentData.provinceName;
            if (menuNameText) menuNameText.text = currentData.menuName;
            if (storyText) storyText.text = currentData.storyText;
            if (howToText) howToText.text = currentData.howToText;
            if (tipsText) tipsText.text = currentData.tipsText;

            RefreshIngredientGrid(currentData);
        }
        else
        {
            if (lockOverlay) lockOverlay.SetActive(true);
            if (headerIngredients) headerIngredients.SetActive(false);
            if (headerHowTo) headerHowTo.SetActive(false);
            if (headerTips) headerTips.SetActive(false);

            if (provinceNameText) provinceNameText.text = "???";
            if (menuNameText) menuNameText.text = "???";
            if (storyText) storyText.text = "";
            if (howToText) howToText.text = "";
            if (tipsText) tipsText.text = "";
            if (foodImage) foodImage.gameObject.SetActive(false);

            if (ingredientsGrid != null)
                foreach (Transform child in ingredientsGrid) Destroy(child.gameObject);
        }

        if (prevButton) prevButton.interactable = (currentPageIndex > 0);
        if (nextButton) nextButton.interactable = (currentPageIndex < recipeList.Count - 1);
    }

    void RefreshIngredientGrid(RecipeData data)
    {
        if (ingredientsGrid == null || ingredientSlotPrefab == null) return;
        foreach (Transform child in ingredientsGrid) Destroy(child.gameObject);
        foreach (var ing in data.ingredients)
        {
            GameObject newSlot = Instantiate(ingredientSlotPrefab, ingredientsGrid);
            Image iconImg = newSlot.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI nameTxt = newSlot.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            if (iconImg != null) iconImg.sprite = ing.icon;
            if (nameTxt != null) nameTxt.text = ing.name;
        }
    }

    public void OpenTextPopup(string fullText)
    {
        if (textPopupBlocker != null)
        {
            PlayClickSound(); // ใช้เสียงคลิกปกติ
            textPopupBlocker.SetActive(true);
            if (popupFullText != null)
            {
                popupFullText.text = fullText;
                if (popupFullText.transform.parent != null)
                {
                    RectTransform contentRect = popupFullText.transform.parent.GetComponent<RectTransform>();
                    if (contentRect != null) contentRect.anchoredPosition = Vector2.zero;
                }
            }
        }
    }

    public void CloseTextPopup()
    {
        if (textPopupBlocker != null)
        {
            PlayClickSound(); // ใช้เสียงคลิกปกติ
            textPopupBlocker.SetActive(false);
        }
    }
}