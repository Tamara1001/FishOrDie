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

        int count = MatchSettings.PlayerCount;
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = CalculateSpawnPosition(i, count);
            GameObject instance   = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);

            if (instance.TryGetComponent(out PlayerController controller))
            {
                // Si está en la mitad derecha de la pantalla (index >= mitad), mira hacia la izquierda
                bool faceLeft = i >= (count / 2f);
                
                controller.Initialize(i, MatchSettings.PlayerBindings[i], MatchSettings.PlayerColors[i], faceLeft, MatchSettings.PlayerNames[i]);
                _spawnedPlayers.Add(controller);
            }
        }

        Debug.Log($"[PlayerSpawner] {count} player(s) spawned from MatchSettings.");
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
