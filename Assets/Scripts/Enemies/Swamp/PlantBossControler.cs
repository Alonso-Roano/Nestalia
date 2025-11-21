using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // Necesario para la lista
using Unity.Cinemachine; 

public class PlantBossController : MonoBehaviour
{
    [Header("Salud y UI")]
    public float maxHealth; 
    public float currentHealth;
    public Slider healthBar;
    public Canvas bossCanvas;
    public string playerTag = "Player";

    [Header("Objetos de Arena")]
    public GameObject objectToActivate; // Barreras de la arena
    public GameObject secondaryObjectToActivate; // 🔹 NUEVO: Segundo objeto

    [Header("Configuración de Cinemachine")]
    public CinemachineCamera virtualCamera;
    public float zoomedOutOffsetZ = -20f;
    public float zoomDuration = 1.0f;

    private CinemachineFollow transposer;
    private float originalOffsetZ;
    private Coroutine zoomCoroutine;

    [Header("Referencias de Combate (Planta)")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject mosquitoPrefab;
    [SerializeField] private GameObject bolaPrefab;
    [SerializeField] private Transform puntoDeDisparo;
    [SerializeField] private Transform[] puntosSpawnMosquito;
    [SerializeField] private BossPlanta_PuntoDebil[] puntosDebiles; 
    [SerializeField] private int vidaPorPuntoDebil = 100; 

    [Header("Configuración de Combate (Planta)")]
    [SerializeField] private float cooldownEntreAcciones = 4f;
    [SerializeField] private int dañoDelRayo = 10;
    [SerializeField] private int cantidadProyectilesRayo = 20;
    [SerializeField] private float tiempoEntreProyectiles = 0.1f;
    [SerializeField] private float velocidadDelRayo = 10f;
    public GameObject ability;

    // --- Variables de Estado ---
    private Transform jugador;
    private enum Estado { Dormido, AtacandoRayo, Invocando, Cooldown, Muerto }
    private Estado estadoActual;

    private int puntosDebilesRestantes;
    private const string ANIM_IDLE = "Planta_Idle";

    // --- 🔹 NUEVAS VARIABLES DE ESTADO ---
    private bool isPhaseTwo = false;
    private GameObject[] mosquitosActivos; // Array para rastrear mosquitos

    void Start()
    {
        // Configuración de Salud
        maxHealth = vidaPorPuntoDebil * puntosDebiles.Length;
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        puntosDebilesRestantes = puntosDebiles.Length;
        isPhaseTwo = false; // 🔹 Asegurarse de empezar en fase 1

        // 🔹 Inicializar array de mosquitos
        mosquitosActivos = new GameObject[puntosSpawnMosquito.Length];

        // Inicializar puntos débiles
        foreach (var punto in puntosDebiles)
        {
            punto.Inicializar(this, vidaPorPuntoDebil);
        }

        // Configuración de Arena y UI
        if (bossCanvas != null) bossCanvas.gameObject.SetActive(false);
        if (objectToActivate != null) objectToActivate.SetActive(false);
        if (secondaryObjectToActivate != null) secondaryObjectToActivate.SetActive(false); // 🔹 NUEVO

        // Configuración de Cámara
        if (virtualCamera != null)
        {
            transposer = virtualCamera.GetComponent<CinemachineFollow>();
            if (transposer != null)
                originalOffsetZ = transposer.FollowOffset.z;
        }

        // Estado inicial del Jefe
        estadoActual = Estado.Dormido;
        if (animator != null) animator.Play(ANIM_IDLE);
        ability.SetActive(false);
    }

    // --- Manejo de la Arena (Del sistema normalizado) ---

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && estadoActual == Estado.Dormido)
        {
            Debug.Log("Player ha entrado en la arena. Iniciando Jefe Planta.");

            if (bossCanvas != null) bossCanvas.gameObject.SetActive(true);
            if (objectToActivate != null) objectToActivate.SetActive(true);
            if (secondaryObjectToActivate != null) secondaryObjectToActivate.SetActive(true); // 🔹 NUEVO

            if (transposer != null)
            {
                if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
                zoomCoroutine = StartCoroutine(AnimateZoom(zoomedOutOffsetZ));
            }

            ActivarJefe(other.transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && estadoActual != Estado.Muerto)
        {
            Debug.Log("Player ha salido de la arena. Reiniciando Jefe Planta.");
            ResetJefe();
        }
    }

    private void ResetJefe()
    {
        estadoActual = Estado.Dormido;
        if (animator != null) animator.Play(ANIM_IDLE);

        StopAllCoroutines();
        jugador = null;

        // Ocultar UI y resetear cámara
        if (bossCanvas != null) bossCanvas.gameObject.SetActive(false);
        if (objectToActivate != null) objectToActivate.SetActive(false);
        if (secondaryObjectToActivate != null) secondaryObjectToActivate.SetActive(false); // 🔹 NUEVO

        if (transposer != null && transposer.FollowOffset.z != originalOffsetZ)
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(AnimateZoom(originalOffsetZ));
        }

        // 🔹 Limpiar mosquitos vivos
        LimpiarMosquitos();

        // Resetear salud
        currentHealth = maxHealth;
        healthBar.value = currentHealth;
        puntosDebilesRestantes = puntosDebiles.Length;
        isPhaseTwo = false; // 🔹 Resetear fase

        // Resetear los puntos débiles
        foreach (var punto in puntosDebiles)
        {
            punto.ResetPuntoDebil();
        }
    }

    IEnumerator AnimateZoom(float targetZ)
    {
        float timer = 0f;
        float startZ = transposer.FollowOffset.z;
        Vector3 baseOffset = transposer.FollowOffset;

        while (timer < zoomDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / zoomDuration);
            float smoothT = t * t * (3f - 2f * t);
            float newZ = Mathf.Lerp(startZ, targetZ, smoothT);
            transposer.FollowOffset = new Vector3(baseOffset.x, baseOffset.y, newZ);
            yield return null;
        }
        transposer.FollowOffset = new Vector3(baseOffset.x, baseOffset.y, targetZ);
        zoomCoroutine = null;
    }

    // --- Métodos de Salud (Nuevos y Modificados) ---

    public void ReportDamageTaken(int damage)
    {
        if (estadoActual == Estado.Dormido || estadoActual == Estado.Muerto) return;

        currentHealth -= damage;
        healthBar.value = currentHealth;

        // 🔹 Comprobar si entra en FASE 2
        if (currentHealth <= maxHealth / 2 && !isPhaseTwo)
        {
            isPhaseTwo = true;
            Debug.Log("¡JEFE ENTRA EN FASE 2!");
            // Aquí podrías aumentar la velocidad, cambiar música, etc.
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }
    }

    public void OnPuntoDebilDestruido()
    {
        if (estadoActual == Estado.Muerto) return;

        puntosDebilesRestantes--;
        Debug.Log("¡Punto débil destruido! Restantes: " + puntosDebilesRestantes);

        if (puntosDebilesRestantes <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (estadoActual == Estado.Muerto) return;

        estadoActual = Estado.Muerto;
        StopAllCoroutines();

        Debug.Log("Jefe PLANTA DERROTADO");

        // Resetea UI y Cámara
        if (bossCanvas != null) bossCanvas.gameObject.SetActive(false);
        if (objectToActivate != null) objectToActivate.SetActive(false);
        if (secondaryObjectToActivate != null) secondaryObjectToActivate.SetActive(false); // 🔹 NUEVO

        if (transposer != null && transposer.FollowOffset.z != originalOffsetZ)
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            StartCoroutine(AnimateZoom(originalOffsetZ));
        }

        // 🔹 Limpiar mosquitos vivos
        LimpiarMosquitos();

        // Lógica de muerte original
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        if (animator != null)
            Destroy(animator.gameObject, 0.5f);
        else
            Debug.LogWarning("No se encontró animator para destruir el objeto del jefe");

        ability.SetActive(true);
        Destroy(gameObject);
        this.enabled = false;
    }


    // --- Lógica de Combate (Modificada) ---

    public void ActivarJefe(Transform targetJugador)
    {
        if (estadoActual == Estado.Dormido)
        {
            Debug.Log("¡JEFE PLANTA ACTIVADO!");
            jugador = targetJugador;
            estadoActual = Estado.Cooldown;
            StartCoroutine(RutinaDeCombate());
        }
    }

    private IEnumerator RutinaDeCombate()
    {
        yield return new WaitForSeconds(1.5f); 

        while (estadoActual != Estado.Muerto)
        {
            // 1. ATACAR CON RAYO (Siempre)
            estadoActual = Estado.AtacandoRayo;
            yield return StartCoroutine(RutinaAtaqueRayo());
            
            estadoActual = Estado.Cooldown;
            if (animator != null) animator.Play(ANIM_IDLE);
            yield return new WaitForSeconds(cooldownEntreAcciones);

            // 2. INVOCAR MOSQUITOS (🔹 SOLO EN FASE 2)
            if (isPhaseTwo)
            {
                estadoActual = Estado.Invocando;
                yield return StartCoroutine(RutinaInvocacion());
                
                estadoActual = Estado.Cooldown;
                if (animator != null) animator.Play(ANIM_IDLE);
                yield return new WaitForSeconds(cooldownEntreAcciones);
            }
        }
    }

    private IEnumerator RutinaAtaqueRayo()
    {
        if (jugador == null) yield break;
        Vector2 direccion = (jugador.position - puntoDeDisparo.position).normalized;

        for (int i = 0; i < cantidadProyectilesRayo; i++)
        {
            if (estadoActual == Estado.Muerto) yield break; 
            GameObject bola = Instantiate(bolaPrefab, puntoDeDisparo.position, Quaternion.identity);
            var proyectil = bola.GetComponent<Proyectil_Rayo>();
            if (proyectil != null)
            {
                proyectil.Inicializar(direccion, dañoDelRayo, velocidadDelRayo);
            }
            yield return new WaitForSeconds(tiempoEntreProyectiles);
        }
    }

    /// <summary>
    /// 🔹 RUTINA DE INVOCACIÓN MODIFICADA
    /// </summary>
    private IEnumerator RutinaInvocacion()
    {
        if (mosquitoPrefab == null) yield break;

        // Usar un FOR para tener el índice
        for (int i = 0; i < puntosSpawnMosquito.Length; i++)
        {
            if (estadoActual == Estado.Muerto) yield break; 

            // 🔹 Comprobar si el mosquito de este spawner ya existe
            // (Si el mosquito murió, la referencia será 'null')
            if (mosquitosActivos[i] == null)
            {
                // No hay un mosquito vivo para este spawner, crear uno nuevo
                GameObject nuevoMosquito = Instantiate(mosquitoPrefab, puntosSpawnMosquito[i].position, puntosSpawnMosquito[i].rotation);
                
                // 🔹 Guardar la referencia al mosquito creado
                mosquitosActivos[i] = nuevoMosquito; 
                
                yield return new WaitForSeconds(0.5f); // Pausa entre cada spawn
            }
            // Si mosquitosActivos[i] no es null, significa que el mosquito
            // de ese punto de spawn sigue vivo, así que no se crea uno nuevo.
        }
    }

    /// <summary>
    /// 🔹 NUEVO: Limpia los mosquitos activos
    /// </summary>
    private void LimpiarMosquitos()
    {
        for (int i = 0; i < mosquitosActivos.Length; i++)
        {
            if (mosquitosActivos[i] != null)
            {
                Destroy(mosquitosActivos[i]);
                mosquitosActivos[i] = null;
            }
        }
    }
}