using UnityEngine;
using UnityEditor;

public class FishGenerator2
{
    [MenuItem("Tools/Balance Fishes")]
    public static void Generate()
    {
        string[] assetNames = { "Fish1_Mojarrita", "Fish2_ViejaDelAgua", "Fish3_Dorado", "Fish4_Sabalo", "Fish5_Boga", "Fish6_Armado", "Fish7_Mocholo" };
        string[] fishNames = { "Mojarra", "Surubi", "Dorado", "Sabalo", "Boga", "Armado", "Mocholo" };
        
        float[] diffs = { 0.15f, 0.9f, 0.85f, 0.4f, 0.35f, 0.5f, 0.3f };
        int[] minS = { 5, 80, 40, 30, 20, 30, 20 };
        int[] maxS = { 15, 150, 100, 60, 50, 70, 40 };
        float[] minW = { 0.03f, 10f, 2f, 2f, 1f, 2f, 1f };
        float[] maxW = { 0.5f, 40f, 15f, 6f, 3f, 6.5f, 2f };
        float[] vals = { 92f, 120f, 300f, 104f, 99f, 115f, 70f };

        for (int i=0; i<assetNames.Length; i++)
        {
            string path = "Assets/_Project/Resources/Fish/" + assetNames[i] + ".asset";
            FishData data = AssetDatabase.LoadAssetAtPath<FishData>(path);
            if (data == null) continue;
            
            data.fishName = fishNames[i];
            data.catchDifficulty = diffs[i];
            data.minSizeCm = minS[i];
            data.maxSizeCm = maxS[i];
            data.minWeightKg = minW[i];
            data.maxWeightKg = maxW[i];
            data.valuePerKg = vals[i];
            EditorUtility.SetDirty(data);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Peces balanceados.");
    }
}
