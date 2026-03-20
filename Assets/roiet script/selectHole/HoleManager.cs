using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HoleManager : MonoBehaviour
{
    public static HoleManager Instance;

    public enum HoleResult { Unchecked, Smooth, Rough } // Smooth=งู, Rough=ปู

    [Header("Config")]
    [Min(1)] public int totalHoles = 8;
    [Min(0)] public int crabRequired = 5;
    [Min(0)] public int snakeRequired = 3;

    [Header("Runtime (readonly)")]
    public HoleResult[] holeResults;
    public bool[] dug;
    public int crabCount = 0;

    bool patternReady = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            AllocateArrays();
            // **ไม่สุ่มทันที** → รอเรียก BuildPatternIfNeeded เมื่อต้องการจริง
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // เมื่อ Scene นี้ถูกโหลดหรือถูกเปิดใช้งานอีกครั้ง (สำคัญมาก!)
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ถ้ากลับมาที่ Scene Map → รีเซ็ตให้เริ่มใหม่ทั้งด่าน
        if (scene.name == "Map" || scene.name == "MainMap" || scene.name.Contains("Map"))
        {
            ResetAndRandomize();
            Debug.Log("[HoleManager] Map scene loaded → Reset pattern & dug status");
        }
    }

    void AllocateArrays()
    {
        totalHoles = 8;
        crabRequired = 5;
        snakeRequired = 3;

        totalHoles = Mathf.Max(1, totalHoles);
        holeResults = new HoleResult[totalHoles];
        dug = new bool[totalHoles];

        ResetRuntimeData();
        
        Debug.Log($"[Forced Config] Crabs: {crabRequired}, Snakes: {snakeRequired}");
    }

    void ResetRuntimeData()
    {
        for (int i = 0; i < totalHoles; i++)
        {
            holeResults[i] = HoleResult.Unchecked;
            dug[i] = false;
        }
        crabCount = 0;
        patternReady = false;
    }

    void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void RandomizeIfNeeded(int id)
    {
        BuildPatternIfNeeded();
    }

    public HoleResult GetHoleResult(int id)
    {
        if (!IsValid(id)) { Debug.LogWarning($"[HoleManager] GetHoleResult: invalid id {id}"); return HoleResult.Smooth; }
        return holeResults[id];
    }

    public bool IsDug(int id)
    {
        if (!IsValid(id)) return false;
        return dug[id];
    }

    public void MarkDug(int id)
    {
        if (!IsValid(id)) return;
        if (dug[id]) return;

        dug[id] = true;
        if (holeResults[id] == HoleResult.Rough) crabCount++;
    }

    public bool AllCrabsFound() => crabCount >= Mathf.Clamp(crabRequired, 0, totalHoles);

    bool IsValid(int id) => id >= 0 && id < totalHoles;

    public void ResetAndRandomize()
    {
        AllocateArrays();           // สร้าง array ใหม่ + reset ค่า
        BuildPatternIfNeeded();     // สุ่มแพทเทิร์นใหม่ทันที
    }

    void BuildPatternIfNeeded()
    {
        if (patternReady) return;

        List<HoleResult> tempList = new List<HoleResult>();

        for (int i = 0; i < crabRequired; i++)
            tempList.Add(HoleResult.Rough);

        for (int i = 0; i < snakeRequired; i++)
            tempList.Add(HoleResult.Smooth);

        while (tempList.Count < totalHoles)
            tempList.Add(HoleResult.Smooth);

        ShuffleList(tempList);

        for (int i = 0; i < totalHoles; i++)
        {
            holeResults[i] = (i < tempList.Count) ? tempList[i] : HoleResult.Smooth;
        }

        patternReady = true;

        int c = 0, s = 0;
        foreach (var r in holeResults) { if (r == HoleResult.Rough) c++; else s++; }
        Debug.Log($"[HoleManager] FINAL FORCE: Crabs={c}, Snakes={s}");
    }

#if UNITY_EDITOR
    [ContextMenu("Debug/Reset & Randomize Pattern")]
    void _EditorReset() { ResetAndRandomize(); }
#endif
}