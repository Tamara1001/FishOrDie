using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class GalleryPolisher
{
    [MenuItem("Tools/Polish Gallery UI")]
    public static void Polish()
    {
        // 1. Modify the Prefab
        string prefabPath = "Assets/_Project/Prefabs/UI/UI_WikiSlot.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            // Change background
            Image bg = prefab.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = new Color(0f, 0f, 0f, 0.4f);
            }
            
            // Re-anchor elements so fish is bigger and texts are smaller but spaced nicely
            Transform fishImg = prefab.transform.Find("FishImage");
            if (fishImg != null)
            {
                RectTransform rt = fishImg.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.45f);
                rt.anchorMax = new Vector2(0.95f, 0.95f);
            }
            
            Transform nameTxt = prefab.transform.Find("Name");
            if (nameTxt != null)
            {
                RectTransform rt = nameTxt.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.30f);
                rt.anchorMax = new Vector2(0.95f, 0.45f);
                nameTxt.GetComponent<TextMeshProUGUI>().fontSize = 26;
            }

            Transform sizeTxt = prefab.transform.Find("Size");
            if (sizeTxt != null)
            {
                RectTransform rt = sizeTxt.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.20f);
                rt.anchorMax = new Vector2(0.95f, 0.30f);
                sizeTxt.GetComponent<TextMeshProUGUI>().fontSize = 18;
            }

            Transform weightTxt = prefab.transform.Find("Weight");
            if (weightTxt != null)
            {
                RectTransform rt = weightTxt.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.10f);
                rt.anchorMax = new Vector2(0.95f, 0.20f);
                weightTxt.GetComponent<TextMeshProUGUI>().fontSize = 18;
            }

            Transform valTxt = prefab.transform.Find("Value");
            if (valTxt != null)
            {
                RectTransform rt = valTxt.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.0f);
                rt.anchorMax = new Vector2(0.95f, 0.10f);
                TextMeshProUGUI valTmp = valTxt.GetComponent<TextMeshProUGUI>();
                valTmp.fontSize = 20;
                valTmp.color = Color.white; // Handled in code formatting now
            }

            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log("[GalleryPolisher] Prefab pulido.");
        }

        // 2. Modify Scene Elements
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            Transform galeria = canvas.transform.Find("Panel_Galeria");
            if (galeria != null)
            {
                Transform scrollView = galeria.Find("Scroll View");
                if (scrollView != null)
                {
                    Transform viewport = scrollView.Find("Viewport");
                    if (viewport != null)
                    {
                        Transform content = viewport.Find("Content");
                        if (content != null)
                        {
                            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
                            if (grid != null)
                            {
                                grid.childAlignment = TextAnchor.MiddleCenter;
                                grid.spacing = new Vector2(30, 40);
                                grid.cellSize = new Vector2(240, 320);
                                grid.padding = new RectOffset(30, 30, 30, 30);
                                EditorUtility.SetDirty(grid);
                                Debug.Log("[GalleryPolisher] GridLayout actualizado en la escena.");
                            }
                        }
                    }
                }
            }
        }
    }
}
