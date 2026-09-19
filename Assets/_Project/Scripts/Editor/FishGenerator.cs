using UnityEngine;
using UnityEditor;

public class FishGenerator
{
    [MenuItem("Tools/Generate Fishes")]
    public static void Generate()
    {
        string[] names = { "Fish4_Sabalo", "Fish5_Boga", "Fish6_Armado", "Fish7_Mocholo", "Trash1_Bolsa", "Trash2_Bota", "Trash3_Botella" };
        string[] fishNames = { "Sábalo", "Boga", "Armado", "Mocholo", "Bolsa de Plástico", "Bota Vieja", "Botella de Plástico" };
        string[] guids = { "0c38cf843c39a3b458e040a16e30ca55", "978cd2727d99cfe42976b297074876eb", "1e4cb238ec1e8754e86088d6122c3833", "acbd3966ee814054da170cbe7693d0f1", "cf999033c162bd4439344b9abdd19278", "42616d5786fbe924b90e63d84373b6ec", "561887e0db70a5a42b7065502064dabd" };
        float[] diffs = { 0.3f, 0.6f, 0.7f, 0.85f, 0.1f, 0.2f, 0.15f };
        int[] minS = { 30, 40, 40, 50, 10, 20, 15 };
        int[] maxS = { 60, 80, 90, 120, 20, 40, 30 };
        int[] minW = { 2, 3, 4, 5, 1, 2, 1 };
        int[] maxW = { 6, 8, 10, 15, 1, 3, 1 };
        int[] vals = { 6, 15, 12, 20, -25, -40, -15 };

        for (int i=0; i<names.Length; i++)
        {
            string path = "Assets/_Project/Resources/Fish/" + names[i] + ".asset";
            FishData data = AssetDatabase.LoadAssetAtPath<FishData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<FishData>();
                AssetDatabase.CreateAsset(data, path);
            }
            data.fishName = fishNames[i];
            
            string spritePath = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (!string.IsNullOrEmpty(spritePath))
                data.fishSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            
            data.catchDifficulty = diffs[i];
            data.minSizeCm = minS[i];
            data.maxSizeCm = maxS[i];
            data.minWeightKg = minW[i];
            data.maxWeightKg = maxW[i];
            data.valuePerKg = vals[i];

            EditorUtility.SetDirty(data);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Peces y basura generados correctamente en Resources/Fish.");
    }
}
