using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerProceduralAnimator : MonoBehaviour
{
    [Header("Referencias a las partes (Jerarquía Anidada)")]
    public Transform bote;
    public Transform torso;
    public Transform cabeza;
    public Transform brazo;
    public Transform mano;
    public Transform cania; // Caña
    public Transform soga;  // Soga
    public Transform piernas; // NUEVO: Piernas

    [Header("Ajustes de Idle (Respiración / Flote)")]
    public float velocidadFlote = 2f;
    public float fuerzaFlote = 0.05f;
    public float velocidadRespiracion = 1.5f;
    public float anguloRespiracion = 2f;

    [Header("Ajustes de Pesca (Acción)")]
    [Tooltip("Ángulo base del brazo al tirar. La caña y la soga usarán esto para contrarrestar.")]
    public float anguloTiron = 25f; 
    public float velocidadTiron = 20f;

    private PlayerController _player;
    
    // Posiciones y rotaciones base
    private Vector3 _boteStartPos;
    private Quaternion _torsoStartRot;
    private Quaternion _cabezaStartRot;
    private Quaternion _brazoStartRot;
    private Quaternion _manoStartRot;
    private Quaternion _caniaStartRot;
    private Quaternion _sogaStartRot;
    private Quaternion _piernasStartRot;

    private void Start()
    {
        _player = GetComponent<PlayerController>();

        // Guardamos las poses base
        if (bote) _boteStartPos = bote.localPosition;
        if (torso) _torsoStartRot = torso.localRotation;
        if (cabeza) _cabezaStartRot = cabeza.localRotation;
        if (brazo) _brazoStartRot = brazo.localRotation;
        if (mano) _manoStartRot = mano.localRotation;
        if (cania) _caniaStartRot = cania.localRotation;
        if (soga) _sogaStartRot = soga.localRotation;
        if (piernas) _piernasStartRot = piernas.localRotation;
    }

    private void Update()
    {
        if (_player == null) return;

        float time = Time.time;

        // --- 1. ANIMACIÓN IDLE ---
        if (bote) bote.localPosition = _boteStartPos + new Vector3(0f, Mathf.Sin(time * velocidadFlote) * fuerzaFlote, 0f);
        
        float torsoBreath = Mathf.Sin(time * velocidadRespiracion) * anguloRespiracion;
        float cabezaBreath = Mathf.Sin(time * velocidadRespiracion * 1.1f) * (anguloRespiracion * 1.5f);

        // --- 2. ANIMACIÓN DE PESCA ---
        bool isPulling = (_player.CurrentSkillCheck != null && _player.CurrentSkillCheck.IsActive && _player.IsPulling);
        
        // Exageramos más el cuerpo ya que la caña y soga hacen de ancla visual
        float anguloTorsoFinal = isPulling ? (anguloTiron * 0.45f) : 0f; 
        float anguloCabezaFinal = isPulling ? (anguloTiron * 0.55f) : 0f;
        float anguloBrazoFinal = isPulling ? anguloTiron : 0f;
        
        // La mano hereda del brazo, así que no le sumamos rotación extra
        float anguloManoFinal = 0f;
        
        // La caña se "dobla" hacia adelante (dirección contraria) por la tensión del pez
        float anguloCaniaFinal = isPulling ? (-anguloTiron * 0.7f) : 0f;

        // Aplicamos Lerp a las partes que rotan activamente
        if (torso) torso.localRotation = Quaternion.Lerp(torso.localRotation, _torsoStartRot * Quaternion.Euler(0, 0, anguloTorsoFinal + torsoBreath), Time.deltaTime * velocidadTiron);
        if (cabeza) cabeza.localRotation = Quaternion.Lerp(cabeza.localRotation, _cabezaStartRot * Quaternion.Euler(0, 0, anguloCabezaFinal + cabezaBreath), Time.deltaTime * velocidadTiron);
        
        if (brazo) brazo.localRotation = Quaternion.Lerp(brazo.localRotation, _brazoStartRot * Quaternion.Euler(0, 0, anguloBrazoFinal), Time.deltaTime * velocidadTiron);
        if (mano) mano.localRotation = Quaternion.Lerp(mano.localRotation, _manoStartRot * Quaternion.Euler(0, 0, anguloManoFinal), Time.deltaTime * velocidadTiron);
        if (cania) cania.localRotation = Quaternion.Lerp(cania.localRotation, _caniaStartRot * Quaternion.Euler(0, 0, anguloCaniaFinal), Time.deltaTime * velocidadTiron);

        // --- 3. CONTRAFUERZAS (Partes que se resisten a heredar rotación) ---
        
        // Las piernas son hijas del torso. Queremos que se queden apoyadas en el bote siempre,
        // así que anulan exactamente el giro del torso.
        if (piernas)
        {
            float torsoZ = GetZOffset(torso, _torsoStartRot);
            piernas.localRotation = _piernasStartRot * Quaternion.Euler(0, 0, -torsoZ);
        }

        // La soga anula TODO el giro que le heredaron sus 4 padres para apuntar siempre hacia abajo.
        if (soga)
        {
            float totalZ = GetZOffset(torso, _torsoStartRot) 
                         + GetZOffset(brazo, _brazoStartRot) 
                         + GetZOffset(mano, _manoStartRot) 
                         + GetZOffset(cania, _caniaStartRot);
            
            soga.localRotation = _sogaStartRot * Quaternion.Euler(0, 0, -totalZ);
        }
    }

    // Método auxiliar para saber cuántos grados reales se giró un objeto respecto a su pose inicial
    private float GetZOffset(Transform t, Quaternion startRot)
    {
        if (t == null) return 0f;
        return Mathf.DeltaAngle(startRot.eulerAngles.z, t.localEulerAngles.z);
    }
}
