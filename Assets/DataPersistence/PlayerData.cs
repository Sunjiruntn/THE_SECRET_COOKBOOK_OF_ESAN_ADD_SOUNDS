[System.Serializable]
public class PlayerData
{
    // จำชื่อฉากล่าสุดที่เล่นค้างไว้ เพื่อให้ปุ่ม Continue รู้ว่าจะไปไหน
    // ถ้าเพิ่งเริ่มเกมครั้งแรก ให้เป็นค่าว่าง "" หรือชื่อฉาก Map
    public string lastSceneName = "";

   
    public bool[] provinceUnlocked = new bool[5]; // 0=กาฬสินธุ์, 1=ขอนแก่น...
    public int[] currentLevelInProvince = new int[5]; // ด่านย่อยที่ผ่านแล้ว
    public bool[] cookbookCollected = new bool[5]; // สูตรอาหารที่ได้แล้ว

    // Constructor
    public PlayerData()
    {
        lastSceneName = "MapSelect"; // กำหนดให้ Default คือหน้าเลือกด่าน

        // จังหวัดแรกเปิดเสมอ
        provinceUnlocked[0] = true;
        for (int i = 1; i < 5; i++) provinceUnlocked[i] = false;

        // รีเซ็ตค่าอื่นๆ
        for (int i = 0; i < 5; i++)
        {
            currentLevelInProvince[i] = 0;
            cookbookCollected[i] = false;
        }
    }
}