using UnityEngine;

public class FishVisual : MonoBehaviour
{
    public FishData Data { get; private set; }
    public bool IsCatchable { get; private set; }
    
    private float _speed;
    private Vector3 _direction;
    private PlayerController _targetPlayer;
    private float _targetX;
    private float _catchRadius = 2.0f; // Qué tan cerca del target sube a la superficie
    
    private SpriteRenderer _sr;
    private SpriteRenderer _auraSr;
    private Color _baseEffectColor;
    private Vector3 _baseScale;

    private bool _hasPaused = false;
    private float _pauseTimer = 0f;

    private static Sprite _auraSprite;

    private static void EnsureAuraSprite()
    {
        if (_auraSprite != null) return;
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size/2f, size/2f);
        float radius = size/2f;
        for(int y=0; y<size; y++){
            for(int x=0; x<size; x++){
                float dist = Vector2.Distance(new Vector2(x, y), center);
                // Difuminado suave pero mucho más ajustado al centro
                float alpha = Mathf.Clamp01(1f - (dist / radius));
                alpha = alpha * alpha * alpha; // Curva pronunciada para que no parezca una nube gigante
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * 0.5f)); // 50% max alpha
            }
        }
        tex.Apply();
        _auraSprite = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f, 0.5f), 32f);
    }

    // Se llama desde el spawner al nacer
    public void Initialize(FishData data, float speed, Vector3 direction, PlayerController targetPlayer)
    {
        Data = data;
        _speed = speed;
        _direction = direction;
        _targetPlayer = targetPlayer;
        _targetX = targetPlayer != null ? targetPlayer.transform.position.x : 0f;

        _sr = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;

        EnsureAuraSprite();

        if (_sr != null && data != null)
        {
            _sr.sprite = data.fishSprite;
            _sr.flipX = direction.x < 0;
            
            _sr.sortingOrder += 5; // Empujamos el pez para que el aura no quede detras del agua

            // --- DEFINIR COLORES Y EFECTOS POR TIER ---
            if (data.valuePerKg < 0)
            {
                // Basura
                _baseEffectColor = new Color(0.2f, 0.2f, 0.2f, 0.7f); 
            }
            else if (data.maxWeightKg >= 9.0f)
            {
                // Gigante (Surubi, Dorado)
                _baseEffectColor = Color.red;
            }
            else if (data.maxWeightKg > 1.5f)
            {
                // Mediano (Sábalo, Armado, Boga, Mocholo)
                _baseEffectColor = Color.yellow;
            }
            else
            {
                // Chico (Mojarra)
                _baseEffectColor = Color.green;
            }

            // Crear Aura difusa
            GameObject auraObj = new GameObject("FishAura");
            auraObj.transform.SetParent(this.transform);
            auraObj.transform.localPosition = Vector3.zero;
            // Estirar el óvalo para que se ajuste más pegado al pez y no sea una nube gigante
            auraObj.transform.localScale = new Vector3(1.3f, 0.6f, 1f);

            _auraSr = auraObj.AddComponent<SpriteRenderer>();
            _auraSr.sprite = _auraSprite;
            _auraSr.color = _baseEffectColor;
            _auraSr.sortingLayerID = _sr.sortingLayerID;
            _auraSr.sortingLayerName = _sr.sortingLayerName;
            _auraSr.sortingOrder = _sr.sortingOrder - 1;

            // Iniciar rastro sutil para TODOS
            StartCoroutine(GhostTrailRoutine(_baseEffectColor));
        }
    }

    private System.Collections.IEnumerator GhostTrailRoutine(Color effectColor)
    {
        while (true)
        {
            yield return new WaitForSeconds(0.15f); // Menos frecuente para que sea sutil

            GameObject ghost = new GameObject("FishGhost");
            ghost.transform.position = transform.position;
            ghost.transform.localScale = transform.localScale;
            
            SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
            ghostSr.sprite = _sr.sprite;
            ghostSr.flipX = _sr.flipX;
            // Opacidad sutil de color
            ghostSr.color = new Color(effectColor.r, effectColor.g, effectColor.b, 0.25f);
            
            ghostSr.sortingLayerID = _sr.sortingLayerID;
            ghostSr.sortingLayerName = _sr.sortingLayerName;
            ghostSr.sortingOrder = _sr.sortingOrder - 2;

            ghost.AddComponent<FishGhostFader>();
        }
    }

    private void Update()
    {
        // Actualizar el target dinámicamente por si el jugador se mueve (aunque aquí no se mueva)
        if (_targetPlayer != null) _targetX = _targetPlayer.transform.position.x;

        float dist = Mathf.Abs(transform.position.x - _targetX);

        // --- LÓGICA DE PAUSA BAJO LA BOYA ---
        if (!_hasPaused && dist < 0.1f) // Justo en el centro del jugador
        {
            if (_pauseTimer == 0f && _targetPlayer != null)
            {
                // Primer frame de pausa: ¡Avisar a la boya! (si el jugador no fue eliminado)
                if (_targetPlayer.gameObject.activeInHierarchy)
                {
                    _targetPlayer.TriggerBobberAlert();
                }
            }

            _pauseTimer += Time.deltaTime;
            if (_pauseTimer < 0.5f)
            {
                // Mantenemos la lógica visual de "superficie" mientras está pausado
                IsCatchable = true;
                UpdateVisualDepth(0f); // 0f = Superficie total
                return; // Cortamos el update para que no avance de posición
            }
            else
            {
                _hasPaused = true;
            }
        }

        // Movimiento normal
        transform.position += _direction * _speed * Time.deltaTime;

        if (Mathf.Abs(transform.position.x) > 20f)
        {
            Destroy(gameObject);
            return;
        }

        // --- LÓGICA DE PROFUNDIDAD ---
        IsCatchable = dist <= _catchRadius;
        
        // depthFactor: 0 = Superficie, 1 = Profundidad
        float transitionRange = 1.5f; 
        float depthFactor = Mathf.Clamp01((dist - _catchRadius) / transitionRange); 

        UpdateVisualDepth(depthFactor);
    }

    private void UpdateVisualDepth(float depthFactor)
    {
        if (_sr != null)
        {
            Color deepColor = new Color(0.2f, 0.3f, 0.45f, 0.6f);
            _sr.color = Color.Lerp(Color.white, deepColor, depthFactor);

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
