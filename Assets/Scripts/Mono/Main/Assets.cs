using UnityEngine;

public class Assets : MonoBehaviour
{
    public const int UNITS_LAYER = 6;
    public const int UI_LAYER = 5;

    public const int Neutral = 0;
    
    public static Assets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}