using UnityEngine;

// use static for now for simplicity. also consider using ScriptableObject
//[CreateAssetMenu(fileName = "GameConfig", menuName = "ScObjs/GameConfig")]
//public class GameConfig : ScriptableObject

public static class GameConfig {
    public static string playerName;
    public static bool showSubtitle;
}
