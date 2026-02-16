using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManagerUdon : MonoBehaviour
{
    [Header("Save Data Settings")]
    public static GameManagerUdon Instance;
    public GameObject ricePrefab;
    public GameObject winUI;
    public int totalRice = 15;
    private int currentRiceCount;

    [Header("Audio Settings")]
    public AudioSource sfxSource;       // ลาก AudioSource มาใส่ในช่องนี้
    public AudioClip clickRiceClip;     // ลากไฟล์เสียงกดเมล็ดข้าวมาใส่ในช่องนี้

    void Awake() { Instance = this; }

    void Start()
    {
        if (winUI != null) winUI.SetActive(false);
        SpawnRice();
    }

    void SpawnRice()
    {
        for (int i = 0; i < totalRice; i++)
        {
            Vector2 spawnPos = Random.insideUnitCircle * 3f;
            Instantiate(ricePrefab, spawnPos, Quaternion.identity);
        }
        currentRiceCount = totalRice;
    }

    public void RemoveRice()
    {
        // --- ส่วนที่เพิ่มเข้าไป: เล่นเสียงเมื่อกดเมล็ดสีดำ ---
        if (sfxSource != null && clickRiceClip != null)
        {
            sfxSource.PlayOneShot(clickRiceClip);
        }
        // -------------------------------------------

        currentRiceCount--;
        if (currentRiceCount <= 0)
        {
            StartCoroutine(WinAndReturn());
        }
    }

    IEnumerator WinAndReturn()
    {
        if (winUI != null) winUI.SetActive(true);
        yield return new WaitForSeconds(2f);

        // สำคัญ: บอก Script หลักว่าเก็บเสร็จแล้ว
        WashingGameManager.isBlackRiceDone = true;

        // แก้ชื่อซีนให้ตรงกับ Scene หลักของคุณ (เช่น "WashingScene")
        SceneManager.LoadScene("DryingRiceScene");
    }
}