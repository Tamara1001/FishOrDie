using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerSpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject _playerPrefab;

    [Range(2, 4)]
    [SerializeField] private int _numberOfPlayers = 2;



    [Header("Spawn Layout")]
    [Tooltip("Posición Y del muelle donde aparecen los jugadores.")]
    [SerializeField] private float _spawnY = 0f;

    [Tooltip("Ancho total del área de spawn. Los jugadores se distribuyen equidistantemente.")]
    [SerializeField] private float _totalSpawnWidth = 6f;

    [Header("Player Defaults")]
    [Tooltip("Rutas de binding del New Input System. Formato: <Keyboard>/letra\nEjemplos: <Keyboard>/a  <Keyboard>/space  <Keyboard>/g")]
    [SerializeField] private string[] _defaultBindings =
    {
        "<Keyboard>/a",
        "<Keyboard>/l",
        "<Keyboard>/g",
        "<Keyboard>/j"
    };

    [SerializeField] private Color[] _playerColors = { Color.cyan, Color.red, Color.yellow, Color.green };

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

        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        if (_playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] Player prefab is not assigned.");
            return;
        }

        for (int i = 0; i < _numberOfPlayers; i++)
        {
            Vector3 spawnPosition = CalculateSpawnPosition(i);
            GameObject instance   = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);

            if (!instance.TryGetComponent(out PlayerController controller))
            {
                Debug.LogError("[PlayerSpawner] Player prefab is missing a PlayerController component.");
                Destroy(instance);
                continue;
            }

            controller.Initialize(i, _defaultBindings[i], _playerColors[i]);
            _spawnedPlayers.Add(controller);
        }

        Debug.Log($"[PlayerSpawner] {_spawnedPlayers.Count} player(s) spawned.");
    }

    private Vector3 CalculateSpawnPosition(int index)
    {
        if (Camera.main == null) return new Vector3(0f, _spawnY, 0f);

        // Distribuimos los jugadores usando las fracciones de pantalla (0 a 1)
        // Ejemplo con 4: 0.125, 0.375, 0.625, 0.875 (igual que un HorizontalLayoutGroup con Force Expand)
        float fraction = (index + 0.5f) / _numberOfPlayers;
        
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(new Vector3(fraction, 0.5f, 10f));
        return new Vector3(worldPos.x, _spawnY, 0f);
    }
}
