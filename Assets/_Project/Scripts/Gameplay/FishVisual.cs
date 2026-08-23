using UnityEngine;

public class FishVisual : MonoBehaviour
{
    public FishData Data { get; private set; }
    public bool IsCatchable { get; private set; }
    
    private float _speed;
    private Vector3 _direction;
    private float _targetX;
    private float _catchRadius = 2.0f; // Qué tan cerca del target sube a la superficie
    
    private SpriteRenderer _sr;
    private Vector3 _baseScale;

    // Se llama desde el spawner al nacer
    public void Initialize(FishData data, float speed, Vector3 direction, float targetX)
    {
        Data = data;
        _speed = speed;
        _direction = direction;
        _targetX = targetX;

        _sr = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;

        if (_sr != null && data != null)
        {
            _sr.sprite = data.fishSprite;
            _sr.flipX = direction.x < 0;

            if (data.catchDifficulty >= 0.7f)
            {
                StartCoroutine(GhostTrailRoutine());
            }
        }
    }

    private System.Collections.IEnumerator GhostTrailRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            GameObject ghost = new GameObject("FishGhost");
            ghost.transform.position = transform.position;
            ghost.transform.localScale = transform.localScale;
            
            SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
            ghostSr.sprite = _sr.sprite;
            ghostSr.flipX = _sr.flipX;
            // Opacidad inicial más alta (70%) para que se note más
            ghostSr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, 0.7f);
            
            // ¡CRUCIAL! Copiar la capa de renderizado para que no quede detrás del fondo
            ghostSr.sortingLayerID = _sr.sortingLayerID;
            ghostSr.sortingLayerName = _sr.sortingLayerName;
            
            // Usamos EXACTAMENTE el mismo Order in Layer. Si le restamos 1, 
            // corre el riesgo de irse a -1 y quedar detrás del río.
            ghostSr.sortingOrder = _sr.sortingOrder;

            // Le agregamos el script de auto-desvanecimiento para que se maneje solo
            ghost.AddComponent<FishGhostFader>();
        }
    }

    private void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;

        if (Mathf.Abs(transform.position.x) > 20f)
        {
            Destroy(gameObject);
            return;
        }

        // --- LÓGICA DE PROFUNDIDAD ---
        float dist = Mathf.Abs(transform.position.x - _targetX);
        IsCatchable = dist <= _catchRadius;

        if (_sr != null)
        {
            // depthFactor: 0 = Superficie, 1 = Profundidad
            float transitionRange = 1.5f; 
            float depthFactor = Mathf.Clamp01((dist - _catchRadius) / transitionRange); 

            // Efecto Visual: Color (Oscuro/Azulado en el fondo, Normal en la superficie)
            Color deepColor = new Color(0.2f, 0.3f, 0.45f, 0.6f);
            _sr.color = Color.Lerp(Color.white, deepColor, depthFactor);
            
            // Efecto Visual: Escala (Más chico en el fondo)
            float scaleMult = Mathf.Lerp(1f, 0.6f, depthFactor);
            transform.localScale = _baseScale * scaleMult;
        }
    }

    public void Hook()
    {
        Destroy(gameObject);
    }
}

public class FishGhostFader : MonoBehaviour
{
    private float _duration = 0.4f;
    private SpriteRenderer _sr;
    private float _startAlpha;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _startAlpha = _sr.color.a;
    }

    private void Update()
    {
        if (_sr == null) return;
        
        Color c = _sr.color;
        // Restamos opacidad basados en el alpha inicial y la duración
        c.a -= (_startAlpha / _duration) * Time.deltaTime;
        _sr.color = c;

        if (c.a <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
