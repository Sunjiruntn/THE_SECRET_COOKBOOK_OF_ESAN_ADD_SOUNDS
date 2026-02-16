using UnityEngine;

public class GameState : MonoBehaviour
{
    public static int selectedHole;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
