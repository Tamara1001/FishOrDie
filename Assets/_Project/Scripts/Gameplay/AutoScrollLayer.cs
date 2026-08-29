using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AutoScrollLayer : MonoBehaviour
{
    [Tooltip("Velocidad de movimiento. Negativo = mueve hacia la izquierda (Ej: Río, Cielo).")]
    public float speed = -1f;

    private float _width;
    private float _startX;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        _width = sr.bounds.size.x;
        _startX = transform.position.x;

        // Crear un clon exacto
        GameObject clone = new GameObject(gameObject.name + "_Clone");
        clone.transform.SetParent(transform); 
        
        // Colocar el clon justo donde termina la imagen original, dependiendo de la dirección
        float cloneOffset = (speed < 0) ? _width : -_width;
        clone.transform.position = new Vector3(transform.position.x + cloneOffset, transform.position.y, transform.position.z);
        
        // Copiar todas las propiedades visuales del SpriteRenderer original
        SpriteRenderer cloneSr = clone.AddComponent<SpriteRenderer>();
        cloneSr.sprite = sr.sprite;
        cloneSr.color = sr.color;
        cloneSr.flipX = sr.flipX;
        cloneSr.flipY = sr.flipY;
        cloneSr.drawMode = sr.drawMode;
        cloneSr.size = sr.size;
        cloneSr.sortingLayerID = sr.sortingLayerID;
        cloneSr.sortingOrder = sr.sortingOrder;
    }

    private void Update()
    {
        if (speed == 0) return;

        // Movemos la imagen en el mundo independientemente de su rotación
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);

        // Calculamos cuánto se alejó de su posición inicial
        float dist = transform.position.x - _startX;
        
        // Si ya se desplazó por completo un ancho de pantalla entero, lo "teletransportamos" hacia atrás
        // creando un bucle infinito invisible.
        if (speed < 0 && dist <= -_width)
        {
            transform.position = new Vector3(transform.position.x + _width, transform.position.y, transform.position.z);
        }
        else if (speed > 0 && dist >= _width)
        {
            transform.position = new Vector3(transform.position.x - _width, transform.position.y, transform.position.z);
        }
    }
}
