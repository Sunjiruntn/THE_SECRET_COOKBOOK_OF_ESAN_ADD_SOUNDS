using UnityEngine;
using System.Collections.Generic;

public class HoleManager : MonoBehaviour
{
    public static HoleManager Instance;

    public enum HoleResult { Unchecked, Smooth, Rough } // Smooth=งู, Rough=ปู

    [Header("Config")]
    [Min(1)] public int totalHoles = 8;     // จำนวนหลุมในด่านนี้
    [Min(0)] public int crabRequired = 5;   // ต้องมีปูกี่หลุม
    [Min(0)] public int snakeRequired = 3;  // ต้องมีงูกี่หลุม

    [Header("Runtime (readonly)")]
    public HoleResult[] holeResults;        // ขนาด = totalHoles
    public bool[] dug;                      // ขุดไปแล้วหรือยัง
    public int crabCount = 0;               // นับปูที่ขุดเจอแล้ว

    bool patternReady = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            AllocateArrays();
            BuildPatternIfNeeded();         // สุ่มแพทเทิร์นครั้งแรก
        }
        else Destroy(gameObject);
    }

    void AllocateArrays()
    {
        // บังคับค่าให้เป็น 8 เสมอ ไม่สน Inspector
        totalHoles = 8;
        crabRequired = 5;
        snakeRequired = 3;
        
        totalHoles = Mathf.Max(1, totalHoles);
        holeResults = new HoleResult[totalHoles];
        dug = new bool[totalHoles];
        for (int i = 0; i < totalHoles; i++)
            holeResults[i] = HoleResult.Unchecked;
        crabCount = 0;
        patternReady = false;

        // เช็คให้ชัวร์ว่าค่าถูกไหม (ดูได้ที่ Console)
        Debug.Log($"[Forced Config] Crabs: {crabRequired}, Snakes: {snakeRequired}");
    }

    // ===================== PATTERN =====================



    void Shuffle(List<int> list)
    {
        System.Random rng = new System.Random();
        for (int n = list.Count - 1; n > 0; n--)
        {
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    // ===================== PUBLIC API (เข้ากับโค้ดเดิม) =====================

    // โค้ดเดิมเรียกเมธอดนี้—เราจะให้มัน “รับประกันว่ามีแพทเทิร์นแล้ว” แทนการสุ่มแบบ 50/50
    public void RandomizeIfNeeded(int id)
    {
        BuildPatternIfNeeded();
        // ไม่ทำอะไรเพิ่ม เพราะแพทเทิร์นทั้งด่านถูก set แล้ว
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

    // ============= Utilities =============

    // เรียกตอนเริ่มรอบใหม่/รีสตาร์ทด่าน
    public void ResetAndRandomize()
    {
        AllocateArrays();
        BuildPatternIfNeeded();
    }
    void BuildPatternIfNeeded()
    {
        // ถ้ามีแพทเทิร์นแล้วก็ออกไปเลย (ป้องกันการสุ่มซ้ำระหว่างเล่น)
        if (patternReady) return;

        // 1. สร้าง List ชั่วคราวขึ้นมา
        List<HoleResult> tempList = new List<HoleResult>();

        // 2. สั่งยัด "ปู (Rough)" ลงไป 5 ตัว (แก้เลข 5 ตรงนี้ได้ถ้าอยากเปลี่ยน)
        for (int i = 0; i < 5; i++)
        {
            tempList.Add(HoleResult.Rough);
        }

        // 3. สั่งยัด "งู (Smooth)" ลงไป 3 ตัว (แก้เลข 3 ตรงนี้ได้)
        for (int i = 0; i < 3; i++)
        {
            tempList.Add(HoleResult.Smooth);
        }

        // 4. (กันเหนียว) ถ้าใน List ยังไม่ครบตามจำนวนรูจริง ให้เติมงูจนเต็ม
        while (tempList.Count < totalHoles)
        {
            tempList.Add(HoleResult.Smooth);
        }

        // 5. สับไพ่ (Shuffle) ให้ตำแหน่งมั่ว
        ShuffleList(tempList);

        // 6. เอาลง Array จริง
        // ต้องแน่ใจว่า Array ถูกสร้างขนาดไว้พอดี
        if (holeResults == null || holeResults.Length != totalHoles)
            holeResults = new HoleResult[totalHoles];

        for (int i = 0; i < totalHoles; i++)
        {
            // กัน Error กรณี List สั้นกว่า Array (แต่ข้อ 4 กันไว้แล้ว)
            if (i < tempList.Count)
                holeResults[i] = tempList[i];
            else
                holeResults[i] = HoleResult.Smooth; // ถ้าขาด ให้เป็นงู
        }

        patternReady = true;

        // เช็คผลลัพธ์ใน Console
        int c = 0, s = 0;
        foreach (var r in holeResults) { if (r == HoleResult.Rough) c++; else s++; }
        Debug.Log($"[HoleManager] FINAL FORCE: Crabs={c}, Snakes={s}");
    }

    // ฟังก์ชันสำหรับสับ List โดยเฉพาะ (เพิ่มฟังก์ชันนี้ต่อท้ายลงไปใน Class ด้วยนะคะ)
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
#if UNITY_EDITOR
    [ContextMenu("Debug/Reset & Randomize Pattern")]
    void _EditorReset() { ResetAndRandomize(); }
#endif
}
