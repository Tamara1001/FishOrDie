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
