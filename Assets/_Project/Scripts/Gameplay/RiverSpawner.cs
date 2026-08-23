using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _fishPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float _minSpawnDelay = 1f;
    [SerializeField] private float _maxSpawnDelay = 3f;
    
    [Tooltip("Límites en Y donde pueden aparecer los peces")]
    [SerializeField] private float _minY = -2.5f;
    [SerializeField] private float _maxY = 1.5f;
    
    [Tooltip("Distancia en X (fuera de cámara) donde nacen")]
    [SerializeField] private float _spawnX = 12f; 

    // Lista pública para que el RoundManager pueda buscar qué peces hay en pantalla
    public List<FishVisual> ActiveFishes { get; private set; } = new List<FishVisual>();
    
    private Coroutine _spawnRoutine;
    private FishData[] _currentPool;
    private List<PlayerController> _activePlayers;

    // Se llama desde RoundManager al arrancar la ronda
    public void StartSpawning(FishData[] pool, List<PlayerController> players)
    {
        _currentPool = pool;
        _activePlayers = players;
        StopSpawning();
        _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    // Se llama al terminar la ronda o pausar
    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    // Limpia el río de golpe (útil para la transición entre rondas)
    public void ClearRiver()
    {
        foreach (var fish in ActiveFishes)
        {
            if (fish != null) Destroy(fish.gameObject);
        }
        ActiveFishes.Clear();
    }

    private void Update()
    {
        // Limpiamos los nulos (peces que cruzaron toda la pantalla y se autodestruyeron)
        ActiveFishes.RemoveAll(f => f == null);
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minSpawnDelay, _maxSpawnDelay));

            if (_fishPrefab == null || _currentPool == null || _currentPool.Length == 0) 
                continue;

            // Pickeamos tipo de pez al azar del pool de este nivel
            FishData randomFish = _currentPool[Random.Range(0, _currentPool.Length)];
            
            // Elegir un jugador objetivo al azar para que el pez salga a la superficie frente a él
            float targetX = 0f;
            if (_activePlayers != null && _activePlayers.Count > 0)
            {
                int rndIndex = Random.Range(0, _activePlayers.Count);
                targetX = _activePlayers[rndIndex].transform.position.x;
            }
            
            // 50% chance de salir de izquierda a derecha, 50% de derecha a izquierda
            bool fromLeft = Random.value > 0.5f;
            
            float startX = fromLeft ? -_spawnX : _spawnX;
            float startY = Random.Range(_minY, _maxY);
            Vector3 spawnPos = new Vector3(startX, startY, 0f);

            GameObject go = Instantiate(_fishPrefab, spawnPos, Quaternion.identity, transform);
            
            if (go.TryGetComponent(out FishVisual visual))
            {
                float speed = Random.Range(2f, 5f);
                Vector3 dir = fromLeft ? Vector3.right : Vector3.left;
                
                visual.Initialize(randomFish, speed, dir, targetX);
                ActiveFishes.Add(visual);
            }
        }
    }
}
