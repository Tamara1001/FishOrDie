using UnityEngine;

public static class MatchSettings
{
    public const int MAX_PLAYERS = 6;
    
    public static bool[] PlayerActive = new bool[MAX_PLAYERS];

    public static int PlayerCount
    {
        get => GetActivePlayerCount();
        set
        {
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                PlayerActive[i] = (i < value);
            }
        }
    }

    // Paleta de colores centralizada y ordenada (Arcoíris + Extras)
    public static readonly Color[] AvailableColors = { 
        Color.red, 
        new Color(1f, 0.5f, 0f), // Naranja
        Color.yellow, 
        Color.green, 
        Color.cyan, 
        Color.blue,
        new Color(0.29f, 0f, 0.51f), // Indigo
        new Color(0.5f, 0f, 1f), // Violeta
        Color.magenta,
        Color.white,
        Color.black,
        new Color(1f, 0.75f, 0.8f) // Rosa
    };

    public static string[] PlayerNames = { "P1", "P2", "P3", "P4", "P5", "P6" };
    
    // Valores por defecto (se cargarán desde AvailableColors en el orden predeterminado)
    public static Color[] PlayerColors = { 
        AvailableColors[0], AvailableColors[1], AvailableColors[2], 
        AvailableColors[3], AvailableColors[4], AvailableColors[5] 
    };
    
    public static string[] PlayerBindings = { "<Keyboard>/a", "<Keyboard>/s", "<Keyboard>/d", "<Keyboard>/f", "<Keyboard>/g", "<Keyboard>/h" };

    private static bool _isLoaded = false;

    public static void Load()
    {
        if (_isLoaded) return;

        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            // Por defecto, habilitar los primeros 4 si nunca se jugó
            int defaultActive = (i < 4) ? 1 : 0;
            PlayerActive[i] = PlayerPrefs.GetInt($"MS_Active_{i}", defaultActive) == 1;
            
            PlayerNames[i] = PlayerPrefs.GetString($"MS_Name_{i}", $"P{i + 1}");
            PlayerBindings[i] = PlayerPrefs.GetString($"MS_Bind_{i}", PlayerBindings[i]);

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
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            PlayerPrefs.SetInt($"MS_Active_{i}", PlayerActive[i] ? 1 : 0);
            PlayerPrefs.SetString($"MS_Name_{i}", PlayerNames[i]);
            PlayerPrefs.SetString($"MS_Bind_{i}", PlayerBindings[i]);
            PlayerPrefs.SetString($"MS_Color_{i}", "#" + ColorUtility.ToHtmlStringRGBA(PlayerColors[i]));
        }

        PlayerPrefs.Save();
    }

    public static int GetActivePlayerCount()
    {
        int count = 0;
        for (int i = 0; i < MAX_PLAYERS; i++)
            if (PlayerActive[i]) count++;
        return count;
    }

    public static void RemovePlayerAndShift(int indexToRemove)
    {
        // Mover todos los datos de la derecha hacia la izquierda para tapar el hueco
        for (int i = indexToRemove; i < MAX_PLAYERS - 1; i++)
        {
            PlayerActive[i] = PlayerActive[i + 1];
            PlayerNames[i] = PlayerNames[i + 1];
            PlayerColors[i] = PlayerColors[i + 1];
            PlayerBindings[i] = PlayerBindings[i + 1];
        }
        
        // El último slot siempre queda libre después de recorrer
        PlayerActive[MAX_PLAYERS - 1] = false;
        
        Save();
    }
}
