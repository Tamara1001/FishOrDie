using UnityEngine;
using UnityEditor;

public class EconomyRebalancer
{
    public static void Rebalance()
    {
        string[] guids = AssetDatabase.FindAssets("t:FishData", new[] { "Assets/_Project/Resources/Fish" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FishData fd = AssetDatabase.LoadAssetAtPath<FishData>(path);
            
            if (fd != null)
            {
                if (fd.fishName.ToLower().Contains("mojarra"))
                {
                    fd.valuePerKg = 80f;
                }
                else if (fd.fishName.ToLower().Contains("mocholo"))
                {
                    fd.valuePerKg = 30f;
                }
                else if (fd.fishName.ToLower().Contains("boga"))
                {
                    fd.valuePerKg = 25f;
                }
                else if (fd.fishName.ToLower().Contains("sabalo") || fd.fishName.ToLower().Contains("sbalo"))
                {
                    fd.valuePerKg = 15f;
                }
                else if (fd.fishName.ToLower().Contains("armado"))
                {
                    fd.valuePerKg = 18f;
                }
                else if (fd.fishName.ToLower().Contains("surubi") || fd.fishName.ToLower().Contains("surub"))
                {
                    fd.valuePerKg = 4f;
                }
                else if (fd.fishName.ToLower().Contains("dorado"))
                {
                    fd.valuePerKg = 18f;
                }
                else if (fd.name.ToLower().Contains("trash"))
                {
                    if (fd.name.Contains("Bota"))
                    {
                        fd.valuePerKg = -30f;
                    }
                    else
                    {
                        fd.valuePerKg = -20f;
                    }
                    
                    // Asegurar peso fijo para que el calculo basura de fijo
                    fd.minWeightKg = 1f;
                    fd.maxWeightKg = 1f;
                }
                
                EditorUtility.SetDirty(fd);
                Debug.Log($"[EconomyRebalancer] Actualizado {fd.fishName} -> /Kg");
            }
        }
        AssetDatabase.SaveAssets();
    }
}
