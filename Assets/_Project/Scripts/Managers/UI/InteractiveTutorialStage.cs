using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractiveTutorialStage : MonoBehaviour
{
    private GameObject _demoPlayer;
    private List<GameObject> _demoFishes = new List<GameObject>();
    private Coroutine _slideRoutine;

    private PlayerSpawner _playerSpawner;
    private RiverSpawner _riverSpawner;

    private GameObject _demoHUD;
    private List<GameObject> _hiddenLobbyPlayers = new List<GameObject>();

    // Posición visible: centro-arriba para que nada quede tapado por la barra inferior
    private const float DEMO_X = 0f;
    private const float DEMO_Y = 2.5f;

    public void Initialize()
    {
        _playerSpawner = Object.FindFirstObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
        _riverSpawner = Object.FindFirstObjectByType<RiverSpawner>(FindObjectsInactive.Include);
    }

    public void PlaySlide(int slideIndex)
    {
        ClearStage();
        if (_slideRoutine != null) StopCoroutine(_slideRoutine);
        
        switch (slideIndex)
        {
            case 0: _slideRoutine = StartCoroutine(Slide_Controls()); break;
            case 1: _slideRoutine = StartCoroutine(Slide_Fishes()); break;
            case 2: _slideRoutine = StartCoroutine(Slide_Bite()); break;
            case 3: _slideRoutine = StartCoroutine(Slide_SkillCheck()); break;
            case 4: _slideRoutine = StartCoroutine(Slide_Elimination()); break;
        }
    }

    private void OnDisable()
    {
        ClearStage();
    }

    // =========================================================================
    // SLIDE 0 — Controles: el jugador apreta su tecla y se mueve
    // =========================================================================
    private IEnumerator Slide_Controls()
    {
        SpawnDemoPlayer();

        while (true)
        {
            yield return new WaitForSeconds(1.2f);
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Player_InputPress");
            
            if (_demoPlayer != null)
            {
                Transform vr = _demoPlayer.transform.Find("VisualRoot");
                if (vr != null)
                {
                    Vector3 baseScale = vr.localScale;
                    vr.localScale = new Vector3(baseScale.x * 1.2f, baseScale.y * 0.8f, baseScale.z);
                    float t = 0;
                    while (t < 0.2f)
                    {
                        t += Time.deltaTime;
                        vr.localScale = Vector3.Lerp(vr.localScale, baseScale, t / 0.2f);
                        yield return null;
                    }
                    vr.localScale = baseScale;
                }
            }
        }
    }

    // =========================================================================
    // SLIDE 1 — Peces: muestra 3 peces nadando (verde, amarillo, rojo)
    // =========================================================================
    private IEnumerator Slide_Fishes()
    {
        HideLobbyPlayers();

        if (_riverSpawner == null || _riverSpawner.FishPrefab == null) yield break;

        FishData[] allFish = Resources.LoadAll<FishData>("Fish");
        if (allFish.Length == 0) yield break;

        FishData green = null, yellow = null, red = null, trash = null;
        foreach (var f in allFish)
        {
            if (f.valuePerKg <= 0 && trash == null) { trash = f; continue; }
            if (f.valuePerKg <= 0) continue;
            
            if (f.maxWeightKg <= 1.5f && green == null) green = f;
            else if (f.maxWeightKg > 1.5f && f.maxWeightKg < 9f && yellow == null) yellow = f;
            else if (f.maxWeightKg >= 9f && red == null) red = f;
        }

        FishData[] tiers = { green, yellow, red, trash };
        Camera cam = Camera.main;
        // Peces en distintas alturas de la zona inferior (45%, 32%, 19%, 6%)
        float[] viewY = { 0.45f, 0.32f, 0.19f, 0.06f }; 
        float[] worldY = new float[4];

        for (int i = 0; i < 4; i++)
        {
            if (cam != null)
                worldY[i] = cam.ViewportToWorldPoint(new Vector3(0.5f, viewY[i], cam.nearClipPlane)).y;
            else
                worldY[i] = 2.5f - (i * 1.5f); // Fallback
        }

        float rightEdge = cam != null ? cam.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, cam.nearClipPlane)).x : 6f;
        float leftEdge = cam != null ? cam.ViewportToWorldPoint(new Vector3(-0.2f, 0.5f, cam.nearClipPlane)).x : -6f;
        float speed = 2.5f;

        for (int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i] == null) continue;
            
            // Distribuir el spawn inicial a lo largo del recorrido para que no salgan todos juntos
            float startX = Mathf.Lerp(leftEdge, rightEdge, (float)i / tiers.Length);
            
            GameObject go = Instantiate(_riverSpawner.FishPrefab, new Vector3(startX, worldY[i], 0f), Quaternion.identity);
            if (go.TryGetComponent(out FishVisual visual))
            {
                visual.Initialize(tiers[i], 0f, Vector3.left, null);
                _demoFishes.Add(go);
            }
        }

        while (true)
        {
            float t = Time.time;
            for (int i = 0; i < _demoFishes.Count; i++)
            {
                if (_demoFishes[i] != null)
                {
                    Transform ft = _demoFishes[i].transform;
                    Vector3 pos = ft.position;
                    
                    // Nado constante hacia la izquierda
                    pos.x -= speed * Time.deltaTime;
                    
                    // Pequeño bamboleo vertical natural
                    pos.y = worldY[i] + Mathf.Sin(t * 1.5f + i) * 0.15f;

                    if (pos.x < leftEdge)
                    {
                        pos.x = rightEdge;
                    }
                    
                    ft.position = pos;
                }
            }
            yield return null;
        }
    }

    // =========================================================================
    // SLIDE 2 — Bite: el jugador espera el pique y reacciona
    // =========================================================================
    private IEnumerator Slide_Bite()
    {
        SpawnDemoPlayer();

        while (true)
        {
            yield return new WaitForSeconds(0.8f);
            
            if (_demoPlayer != null && _demoPlayer.TryGetComponent(out PlayerController ctrl))
            {
                ctrl.TriggerBobberAlert();
            }

            yield return new WaitForSeconds(0.3f);
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Player_InputPress");

            yield return new WaitForSeconds(0.6f);
        }
    }

    // =========================================================================
    // SLIDE 3 — Skill Check: primero falla, después acierta
    // =========================================================================
    private IEnumerator Slide_SkillCheck()
    {
        SpawnDemoPlayer();
        
        // Pez de tutorial con dificultad baja para que la barra sea grande y clara
        FishData testFish = ScriptableObject.CreateInstance<FishData>();
        testFish.fishName = "TutorialFish";
        testFish.catchDifficulty = 0.3f;

        PlayerController controller = _demoPlayer != null 
            ? _demoPlayer.GetComponent<PlayerController>() 
            : null;

        if (controller == null) yield break;

        while (true)
        {
            // Crear un SkillCheck "manual" que no dispare callbacks reales del juego
            SkillCheck sc = new SkillCheck(testFish, (_) => {}, () => {});
            // Asignar directamente al controller para que el SkillCheckPanel lo vea
            controller.CurrentSkillCheck = sc;

            // FASE 1 — Hacerlo MAL (3s sin presionar)
            float elapsed = 0f;
            while (elapsed < 3f && sc.IsActive)
            {
                elapsed += Time.deltaTime;
                sc.Tick(Time.deltaTime, false);
                yield return null;
            }

            // FASE 2 — Hacerlo BIEN (7s siguiendo al pez)
            elapsed = 0f;
            while (elapsed < 7f && sc.IsActive)
            {
                elapsed += Time.deltaTime;
                bool shouldPress = sc.BarPosition < sc.FishPosition + 0.05f;
                sc.Tick(Time.deltaTime, shouldPress);
                yield return null;
            }

            // Limpiar y repetir
            controller.CurrentSkillCheck = null;
            yield return new WaitForSeconds(1f);
        }
    }

    // =========================================================================
    // SLIDE 4 — Eliminación: el monstruo se lleva al jugador
    // =========================================================================
    private IEnumerator Slide_Elimination()
    {
        SpawnDemoPlayer();

        while (true)
        {
            yield return new WaitForSeconds(1.5f);
            
            if (_demoPlayer != null && _demoPlayer.TryGetComponent(out PlayerController ctrl))
            {
                ctrl.Eliminate();
            }

            yield return new WaitForSeconds(4f);
            
            if (_demoPlayer != null) Destroy(_demoPlayer);
            if (_demoHUD != null) Destroy(_demoHUD);
            SpawnDemoPlayer();
        }
    }

    // =========================================================================
    // SPAWN HELPERS
    // =========================================================================
    private void HideLobbyPlayers()
    {
        if (_hiddenLobbyPlayers.Count == 0)
        {
            foreach (PlayerController p in FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (p.PlayerID != 99 && p.gameObject.activeSelf)
                {
                    p.gameObject.SetActive(false);
                    _hiddenLobbyPlayers.Add(p.gameObject);
                }
            }
        }
    }

    private void SpawnDemoPlayer()
    {
        HideLobbyPlayers();

        if (_playerSpawner == null || _playerSpawner.PlayerPrefab == null) return;
        
        Camera cam = Camera.main;
        Vector3 spawnPos = Vector3.zero;
        if (cam != null)
        {
            // Spawnear jugador un poco más arriba (40% de la pantalla)
            spawnPos = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.40f, cam.nearClipPlane));
            spawnPos.z = 0f;
        }

        _demoPlayer = Instantiate(_playerSpawner.PlayerPrefab, spawnPos, Quaternion.identity);
        
        if (_demoPlayer.TryGetComponent(out PlayerController controller))
        {
            controller.Initialize(99, "", Color.cyan, false, "DEMO");

            // Instanciar HUD para que se vea el SkillCheck
            RoundHUD hud = Object.FindFirstObjectByType<RoundHUD>(FindObjectsInactive.Include);
            if (hud != null && hud.PlayerSlotPrefab != null)
            {
                // Engancharlo al Canvas principal para que el anclaje sea relativo a toda la pantalla
                Canvas parentCanvas = GetComponentInParent<Canvas>();
                Transform hudParent = parentCanvas != null ? parentCanvas.transform : this.transform;
                
                _demoHUD = Instantiate(hud.PlayerSlotPrefab, hudParent);
                _demoHUD.SetActive(true);
                
                // Centrado en la pantalla (parte media-inferior)
                if (_demoHUD.TryGetComponent(out RectTransform rt))
                {
                    rt.anchorMin = new Vector2(0.5f, 0.55f);
                    rt.anchorMax = new Vector2(0.5f, 0.55f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one * 1.5f; // Grande y legible
                }

                if (_demoHUD.TryGetComponent(out PlayerHUDSlot slot))
                {
                    slot.Initialize(controller);
                    
                    // Ocultar textos/fondos del HUD — solo SkillCheckPanel visible
                    SkillCheckPanel scPanel = _demoHUD.GetComponentInChildren<SkillCheckPanel>(true);
                    Debug.Log($"[InteractiveTutorialStage] scPanel found? {scPanel != null}");

                    foreach (var txt in _demoHUD.GetComponentsInChildren<TMPro.TMP_Text>(true))
                    {
                        if (scPanel != null && txt.transform.IsChildOf(scPanel.transform)) continue;
                        txt.enabled = false;
                    }
                    foreach (var img in _demoHUD.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                    {
                        if (scPanel != null && img.transform.IsChildOf(scPanel.transform)) continue;
                        img.enabled = false;
                    }
                }
            }
        }
    }

    public void ClearStage()
    {
        if (_slideRoutine != null) { StopCoroutine(_slideRoutine); _slideRoutine = null; }
        if (_demoPlayer != null) Destroy(_demoPlayer);
        if (_demoHUD != null) Destroy(_demoHUD);
        foreach (var f in _demoFishes) if (f != null) Destroy(f);
        _demoFishes.Clear();

        foreach (var go in _hiddenLobbyPlayers)
        {
            if (go != null) go.SetActive(true);
        }
        _hiddenLobbyPlayers.Clear();
    }
}