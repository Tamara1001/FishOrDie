using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEditor.Events;

public class MenuUIGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Wiki and Tutorial Panels")]
    public static void GeneratePanels()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene.");
            return;
        }

        // ============================================
        // 1. PANEL GALERIA
        // ============================================
        GameObject galeriaPanel = new GameObject("Panel_Galeria", typeof(RectTransform), typeof(Image), typeof(UI_WikiMenu));
        galeriaPanel.transform.SetParent(canvas.transform, false);
        RectTransform galRect = galeriaPanel.GetComponent<RectTransform>();
        galRect.anchorMin = Vector2.zero;
        galRect.anchorMax = Vector2.one;
        galRect.sizeDelta = Vector2.zero;
        galeriaPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.95f);
        
        // Title
        GameObject galTitle = CreateText(galeriaPanel.transform, "Title", "GALERÍA DE PECES", 50, true);
        RectTransform galTitleRect = galTitle.GetComponent<RectTransform>();
        galTitleRect.anchorMin = new Vector2(0, 1);
        galTitleRect.anchorMax = new Vector2(1, 1);
        galTitleRect.pivot = new Vector2(0.5f, 1);
        galTitleRect.anchoredPosition = new Vector2(0, -50);
        galTitleRect.sizeDelta = new Vector2(0, 80);

        // Close Button
        GameObject closeGal = CreateButton(galeriaPanel.transform, "Btn_Close", "X");
        RectTransform closeGalRect = closeGal.GetComponent<RectTransform>();
        closeGalRect.anchorMin = new Vector2(1, 1);
        closeGalRect.anchorMax = new Vector2(1, 1);
        closeGalRect.pivot = new Vector2(1, 1);
        closeGalRect.anchoredPosition = new Vector2(-40, -40);
        closeGalRect.sizeDelta = new Vector2(60, 60);

        UI_WikiMenu wikiMenu = galeriaPanel.GetComponent<UI_WikiMenu>();
        UnityEventTools.AddPersistentListener(closeGal.GetComponent<Button>().onClick, new UnityAction(wikiMenu.CloseGallery));

        // Scroll view
        GameObject scrollObj = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(galeriaPanel.transform, false);
        RectTransform scrollRect = scrollObj.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.1f, 0.1f);
        scrollRect.anchorMax = new Vector2(0.9f, 0.85f);
        scrollRect.sizeDelta = Vector2.zero;
        scrollObj.GetComponent<Image>().color = new Color(1, 1, 1, 0.05f);
        
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewRect = viewport.GetComponent<RectTransform>();
        viewRect.anchorMin = Vector2.zero;
        viewRect.anchorMax = Vector2.one;
        viewRect.sizeDelta = Vector2.zero;
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 500);

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(250, 300);
        grid.spacing = new Vector2(20, 20);
        grid.padding = new RectOffset(20, 20, 20, 20);
        
        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = contentRect;
        sr.viewport = viewRect;
        sr.horizontal = false;

        // Prefab for Slot
        GameObject slotObj = new GameObject("WikiSlot", typeof(RectTransform), typeof(Image), typeof(UI_WikiSlot));
        slotObj.GetComponent<Image>().color = new Color(0.15f, 0.25f, 0.35f, 1f);
        
        GameObject fishImg = new GameObject("FishImage", typeof(RectTransform), typeof(Image));
        fishImg.transform.SetParent(slotObj.transform, false);
        fishImg.GetComponent<Image>().preserveAspect = true;
        SetRectAnchor(fishImg, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.9f));

        GameObject nameTxt = CreateText(slotObj.transform, "Name", "Nombre Pez", 28, true);
        nameTxt.GetComponent<TextMeshProUGUI>().color = Color.white;
        SetRectAnchor(nameTxt, new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.35f));

        GameObject sizeTxt = CreateText(slotObj.transform, "Size", "10cm - 20cm", 20, false);
        sizeTxt.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);
        SetRectAnchor(sizeTxt, new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.25f));

        GameObject weightTxt = CreateText(slotObj.transform, "Weight", "1kg - 2kg", 20, false);
        weightTxt.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);
        SetRectAnchor(weightTxt, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.15f));

        GameObject valTxt = CreateText(slotObj.transform, "Value", "$100 x Kg", 22, true);
        valTxt.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        SetRectAnchor(valTxt, new Vector2(0.1f, -0.05f), new Vector2(0.9f, 0.05f));

        UI_WikiSlot wikiSlot = slotObj.GetComponent<UI_WikiSlot>();
        SerializedObject soWikiSlot = new SerializedObject(wikiSlot);
        soWikiSlot.FindProperty("_fishImage").objectReferenceValue = fishImg.GetComponent<Image>();
        soWikiSlot.FindProperty("_nameText").objectReferenceValue = nameTxt.GetComponent<TextMeshProUGUI>();
        soWikiSlot.FindProperty("_sizeText").objectReferenceValue = sizeTxt.GetComponent<TextMeshProUGUI>();
        soWikiSlot.FindProperty("_weightText").objectReferenceValue = weightTxt.GetComponent<TextMeshProUGUI>();
        soWikiSlot.FindProperty("_valueText").objectReferenceValue = valTxt.GetComponent<TextMeshProUGUI>();
        soWikiSlot.ApplyModifiedProperties();

        if (!System.IO.Directory.Exists("Assets/_Project/Prefabs/UI")) System.IO.Directory.CreateDirectory("Assets/_Project/Prefabs/UI");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(slotObj, "Assets/_Project/Prefabs/UI/UI_WikiSlot.prefab");
        DestroyImmediate(slotObj);

        SerializedObject soWikiMenu = new SerializedObject(wikiMenu);
        soWikiMenu.FindProperty("_contentContainer").objectReferenceValue = content.transform;
        soWikiMenu.FindProperty("_fishSlotPrefab").objectReferenceValue = prefab;
        soWikiMenu.ApplyModifiedProperties();

        galeriaPanel.SetActive(false);


        // ============================================
        // 2. PANEL TUTORIAL
        // ============================================
        GameObject tutRoot = new GameObject("Panel_TutorialRoot", typeof(RectTransform), typeof(UI_TutorialMenu));
        tutRoot.transform.SetParent(canvas.transform, false);
        SetRectAnchor(tutRoot, Vector2.zero, Vector2.one);

        GameObject tutPanel = new GameObject("TutorialWindow", typeof(RectTransform), typeof(Image));
        tutPanel.transform.SetParent(tutRoot.transform, false);
        SetRectAnchor(tutPanel, Vector2.zero, Vector2.one);
        tutPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);

        GameObject tutBox = new GameObject("Box", typeof(RectTransform), typeof(Image));
        tutBox.transform.SetParent(tutPanel.transform, false);
        tutBox.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);
        SetRectAnchor(tutBox, new Vector2(0.2f, 0.3f), new Vector2(0.8f, 0.7f));

        GameObject tutTitle = CreateText(tutBox.transform, "Title", "TUTORIAL", 40, true);
        SetRectAnchor(tutTitle, new Vector2(0, 0.8f), new Vector2(1, 1));

        GameObject tutText = CreateText(tutBox.transform, "Text", "Texto del tutorial...", 28, false);
        tutText.GetComponent<TextMeshProUGUI>().enableWordWrapping = true;
        SetRectAnchor(tutText, new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.8f));

        GameObject btnNext = CreateButton(tutBox.transform, "Btn_Next", "SIGUIENTE");
        SetRectAnchor(btnNext, new Vector2(0.65f, 0.05f), new Vector2(0.95f, 0.2f));

        GameObject btnSkip = CreateButton(tutBox.transform, "Btn_Skip", "SALTAR");
        SetRectAnchor(btnSkip, new Vector2(0.05f, 0.05f), new Vector2(0.35f, 0.2f));

        UI_TutorialMenu tutMenu = tutRoot.GetComponent<UI_TutorialMenu>();
        SerializedObject soTutMenu = new SerializedObject(tutMenu);
        soTutMenu.FindProperty("_tutorialPanel").objectReferenceValue = tutPanel;
        soTutMenu.FindProperty("_tutorialText").objectReferenceValue = tutText.GetComponent<TextMeshProUGUI>();
        soTutMenu.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(btnNext.GetComponent<Button>().onClick, new UnityAction(tutMenu.OnNextClicked));
        UnityEventTools.AddPersistentListener(btnSkip.GetComponent<Button>().onClick, new UnityAction(tutMenu.OnSkipClicked));

        // We leave it active so OnEnable/Start works, but it hides itself if already watched.
        
        Debug.Log("Paneles de Galería y Tutorial generados exitosamente en el Canvas!");
    }

    private static GameObject CreateText(Transform parent, string name, string text, int size, bool bold)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI txt = go.GetComponent<TextMeshProUGUI>();
        txt.text = text;
        txt.fontSize = size;
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string text)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        GameObject txt = CreateText(go.transform, "Text", text, 24, true);
        txt.GetComponent<TextMeshProUGUI>().color = Color.black;
        SetRectAnchor(txt, Vector2.zero, Vector2.one);
        return go;
    }

    private static void SetRectAnchor(GameObject go, Vector2 min, Vector2 max)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }
}
