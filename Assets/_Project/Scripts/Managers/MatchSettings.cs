using UnityEngine;

public static class MatchSettings
{
    public const int MAX_PLAYERS = 6;
    
    public static int PlayerCount = 4;
    public static string[] PlayerNames = { "P1", "P2", "P3", "P4", "P5", "P6" };
    public static Color[] PlayerColors = { Color.cyan, Color.red, Color.yellow, Color.green, Color.magenta, Color.white };
    public static string[] PlayerBindings = { "<Keyboard>/a", "<Keyboard>/s", "<Keyboard>/d", "<Keyboard>/f", "<Keyboard>/g", "<Keyboard>/h" };

    private static bool _isLoaded = false;

    public static void Load()
    {
        if (_isLoaded) return;

        PlayerCount = PlayerPrefs.GetInt("MS_PlayerCount", 4);

        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            PlayerNames[i] = PlayerPrefs.GetString($"MS_Name_{i}", $"P{i + 1}");
            PlayerBindings[i] = PlayerPrefs.GetString($"MS_Bind_{i}", PlayerBindings[i]); // default from array

            string htmlColor = PlayerPrefs.GetString($"MS_Color_{i}", "");
            if (!string.IsNullOrEmpty(htmlColor) && ColorUtility.TryParseHtmlString(htmlColor, out Color parsedColor))
            {
                PlayerColors[i] = parsedColor;
            }
        }
        
        _isLoaded = true;
    }

    public static void Save()
    {
        PlayerPrefs.SetInt("MS_PlayerCount", PlayerCount);

        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            PlayerPrefs.SetString($"MS_Name_{i}", PlayerNames[i]);
            PlayerPrefs.SetString($"MS_Bind_{i}", PlayerBindings[i]);
            PlayerPrefs.SetString($"MS_Color_{i}", "#" + ColorUtility.ToHtmlStringRGBA(PlayerColors[i]));
        }

        PlayerPrefs.Save();
    }
}
