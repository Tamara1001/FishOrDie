using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerSpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject _playerPrefab;

    [Header("Spawn Layout")]
    [Tooltip("Posición Y del muelle donde aparecen los jugadores.")]
    [SerializeField] private float _spawnY = 0f;

    [Tooltip("Ancho total del área de spawn. Los jugadores se distribuyen equidistantemente.")]
    [SerializeField] private float _totalSpawnWidth = 6f;

    public IReadOnlyList<PlayerController> SpawnedPlayers => _spawnedPlayers;
    private readonly List<PlayerController> _spawnedPlayers = new();

    private void Awake()
    {
        // Seguro anti-clones (por si el GameSetup quedó dentro del DontDestroyOnLoad)
        if (FindObjectsByType<PlayerSpawner>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        MatchSettings.Load();
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        if (_playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] Player prefab is not assigned.");
            return;
        }

        int totalActive = MatchSettings.GetActivePlayerCount();
        
        // Failsafe matemático: Si alguien carga la escena directo sin pasar por el menú (0 jugadores),
        // forzamos al menos 2 jugadores para evitar que la división genere Coordenadas "NaN" (Invisibles).
        if (totalActive < 2)
        {
            Debug.LogWarning("[PlayerSpawner] Se detectaron menos de 2 jugadores. Forzando 2 jugadores por defecto.");
            MatchSettings.PlayerActive[0] = true;
            MatchSettings.PlayerActive[1] = true;
            totalActive = 2;
        }

        int activeIndex = 0; // Para el cálculo de layout

        for (int i = 0; i < MatchSettings.MAX_PLAYERS; i++)
        {
            if (!MatchSettings.PlayerActive[i]) continue;

            Vector3 spawnPosition = CalculateSpawnPosition(activeIndex, totalActive);
            Debug.Log($"[PlayerSpawner] Spawn {i} (ActiveIndex {activeIndex}) at Position: {spawnPosition}");
            GameObject instance   = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);

            if (instance.TryGetComponent(out PlayerController controller))
            {
                // Si está en la mitad derecha de la pantalla, mira hacia la izquierda
                bool faceLeft = activeIndex >= (totalActive / 2f);
                
                controller.Initialize(i, MatchSettings.PlayerBindings[i], MatchSettings.PlayerColors[i], faceLeft, MatchSettings.PlayerNames[i]);
                _spawnedPlayers.Add(controller);
            }

            activeIndex++;
        }

        Debug.Log($"[PlayerSpawner] {totalActive} player(s) spawned from MatchSettings.");
    }

    private Vector3 CalculateSpawnPosition(int index, int totalPlayers)
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector3.zero;

        // Calcula qué porcentaje de la pantalla ocupa este jugador (ej: 0.125, 0.375...)
        float fraction = (index + 0.5f) / totalPlayers;
        
        Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(fraction, 0.5f, cam.nearClipPlane));
        worldPos.y = _spawnY;
        worldPos.z = 0f;

        return worldPos;
    }
}
