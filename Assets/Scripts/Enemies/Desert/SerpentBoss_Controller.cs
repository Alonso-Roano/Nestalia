using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Unity.Cinemachine;

public class SerpentBoss_Controller : MonoBehaviour
{
    [Header("Salud y UI")]
    public float maxHealth = 200f;
    public float currentHealth;
    public Slider healthBar;
    private bool isPhaseTwo = false;

    [Header("Triggers de Activación")]
    public Canvas bossCanvas;
    public GameObject objectToActivate;
    public GameObject secondaryObjectToActivate; // 🔹 NUEVO: Segundo objeto opcional
    public string playerTag = "Player";

    [Header("Configuración de Cinemachine")]
    public CinemachineCamera virtualCamera;
    public float zoomedOutOffsetZ = -20f;
    public float zoomDuration = 1.0f;

    private CinemachineFollow transposer;
    private float originalOffsetZ;
    private Coroutine zoomCoroutine;

    [Header("Referencias de Prefabs")]
    public GameObject serpentPrefab_A;
    public GameObject serpentPrefab_B;
    public GameObject warningIndicatorPrefab;

    [Header("Puntos de la Arena")]
    public Transform groundLevel;
    public Transform leftLimit;
    public Transform rightLimit;

    [Header("Tiempos de Ataque")]
    public float minHideTime = 1f;
    public float maxHideTime = 3f;
    public float warningTime = 1.5f;
    public float furyWarningTime = 0.3f;
    [Range(0, 100)]
    public int furyAttackChance = 25;

    public float blinkInterval = 0.1f;
    private bool isAttacking = false;
    public GameObject ability;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;

        if (bossCanvas != null) bossCanvas.gameObject.SetActive(false);
        if (objectToActivate != null) objectToActivate.SetActive(false);
        if (secondaryObjectToActivate != null) secondaryObjectToActivate.SetActive(false); // 🔹 NUEVO

        if (virtualCamera != null)
        {
            transposer = virtualCamera.GetComponent<CinemachineFollow>();
            if (transposer != null)
                originalOffsetZ = transposer.FollowOffset.z;
            else
                Debug.LogError("La cámara virtual no tiene CinemachineFollow.");
        }
        ability.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player ha entrado en la arena. Iniciando Jefe.");
            isAttacking = false;

            if (bossCanvas != null) bossCanvas.gameObject.SetActive(true);
            if (objectToActivate != null) objectToActivate.SetActive(true);
            if (secondaryObjectToActivate != null) secondaryObjectToActivate.SetActive(true); // 🔹 NUEVO

            StopAllCoroutines();
            if (transposer != null)
            {
                if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
                zoomCoroutine = StartCoroutine(AnimateZoom(zoomedOutOffsetZ));
            }

            StartCoroutine(AttackLoop());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player ha salido de la arena. Reiniciando Jefe.");

            if (bossCanvas != null) bossCanvas.gameObject.SetActive(false);
            if (objectToActivate != null) objectToActivate.SetActive(false);
            if (secondaryObjectToActivate != null) secondaryObjectToActivate.SetActive(false);

            currentHealth = maxHealth;
            healthBar.value = currentHealth;

            ClearWarnings();

            StopAllCoroutines();

            isPhaseTwo = false;

            zoomCoroutine = StartCoroutine(AnimateZoom(originalOffsetZ));

            Serpent_Attack[] activeSerpents = FindObjectsByType<Serpent_Attack>(FindObjectsSortMode.None);
            foreach (Serpent_Attack serpent in activeSerpents)
            {
                serpent.gameObject.SetActive(false); ;
            }
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth;

        if (currentHealth <= 0)
            Die();

        if (currentHealth <= maxHealth / 2 && !isPhaseTwo)
        {
            isPhaseTwo = true;
            Debug.Log("JEFE EN FASE 2!");
        }
    }

    void Die()
    {
        StopAllCoroutines();

        Serpent_Attack[] activeSerpents = FindObjectsByType<Serpent_Attack>(FindObjectsSortMode.None);
        foreach (Serpent_Attack serpent in activeSerpents)
            Destroy(serpent.gameObject);

        // 🔹 NUEVO: eliminar warnings si el jefe muere justo cuando aparecían
        ClearWarnings();

        if (bossCanvas != null) bossCanvas.gameObject.SetActive(false);
        if (objectToActivate != null) objectToActivate.SetActive(false);
        if (secondaryObjectToActivate != null) secondaryObjectToActivate.SetActive(false); // 🔹 NUEVO

        if (transposer != null && transposer.FollowOffset.z != originalOffsetZ)
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            StartCoroutine(AnimateZoom(originalOffsetZ));
        }

        Debug.Log("Jefe DERROTADO");
        ability.SetActive(true);
        Destroy(gameObject);
    }

    // 🔹 NUEVO: método auxiliar para limpiar alertas en pantalla
    void ClearWarnings()
    {
        GameObject[] warnings = GameObject.FindGameObjectsWithTag("Warning");
        foreach (GameObject w in warnings)
            Destroy(w);
    }

    IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(3f);

        Debug.Log("Empezando ataque");
        Debug.Log(currentHealth);
        Debug.Log("Esta atacando: " + isAttacking);
        while (currentHealth > 0)
        {
            if (isAttacking)
            {
                yield return null;
                continue;
            }

            isAttacking = true;
            float hideTime = Random.Range(minHideTime, maxHideTime);
            yield return new WaitForSeconds(hideTime);
            Debug.Log(isPhaseTwo && Random.Range(0, 100) < furyAttackChance);
            if (isPhaseTwo && Random.Range(0, 100) < furyAttackChance)

                yield return StartCoroutine(FuryAttack());
            else
                yield return StartCoroutine(NormalAttack());
        }
    }

    GameObject InstantiateSerpent(GameObject prefab, Vector3 position, Serpent_Attack.AttackType type)
    {
        Quaternion rotation = Quaternion.identity;

        if (prefab == serpentPrefab_B)
        {
            float yRotation = Random.Range(0, 2) * 180f;
            rotation = Quaternion.Euler(0, yRotation, 0);
        }

        GameObject serpentInstance = Instantiate(prefab, position, rotation);
        Serpent_Attack serpentScript = serpentInstance.GetComponent<Serpent_Attack>();

        if (serpentScript == null)
        {
            Debug.LogError($"¡El prefab instanciado ({prefab.name}) NO tiene el script Serpent_Attack!");
            Destroy(serpentInstance);
            ReportAttackFinished();
            return null;
        }

        serpentScript.Initialize(this, type, groundLevel.position.y);
        return serpentInstance;
    }

    IEnumerator BlinkIndicator(GameObject indicatorInstance, float duration)
    {
        if (indicatorInstance == null) yield break;

        SpriteRenderer sr = indicatorInstance.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("El Prefab 'warningIndicatorPrefab' NO tiene un SpriteRenderer!");
            Destroy(indicatorInstance);
            yield break;
        }

        // 🔹 NUEVO: marcar el objeto con tag "Warning" para poder eliminarlo luego
        indicatorInstance.tag = "Warning";

        float timer = 0f;
        bool isVisible = true;
        Color redColor = new Color(1f, 0f, 0f, 0.3f);
        Color transparentColor = new Color(1f, 0f, 0f, 0f);

        while (timer < duration)
        {
            sr.color = isVisible ? redColor : transparentColor;
            isVisible = !isVisible;

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        Destroy(indicatorInstance);
    }

    IEnumerator NormalAttack()
    {
        // 🔹 MODIFICACIÓN INICIA 🔹
        float emergeX;
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
        {
            // Obtener la posición X del jugador en este instante
            emergeX = player.transform.position.x;
            
            // Asegurarse de que la posición esté dentro de los límites de la arena
            emergeX = Mathf.Clamp(emergeX, leftLimit.position.x, rightLimit.position.x);
        }
        else
        {
            // Fallback si no se encuentra al jugador (comportamiento aleatorio original)
            Debug.LogWarning("SerpentBoss: No se encontró al jugador con tag '" + playerTag + "'. Usando X aleatorio.");
            emergeX = Random.Range(leftLimit.position.x, rightLimit.position.x);
        }
        // 🔹 MODIFICACIÓN TERMINA 🔹

        Vector3 emergePosition = new Vector3(emergeX, groundLevel.position.y, 0);
        GameObject prefabToInstantiate = (Random.Range(0, 2) == 0) ? serpentPrefab_A : serpentPrefab_B;
        Serpent_Attack.AttackType type = (Random.Range(0, 2) == 0)
            ? Serpent_Attack.AttackType.FastUpSlowDown
            : Serpent_Attack.AttackType.SlowUpFastDown;

        GameObject warningFX = Instantiate(warningIndicatorPrefab, emergePosition, Quaternion.identity);
        yield return StartCoroutine(BlinkIndicator(warningFX, warningTime));

        InstantiateSerpent(prefabToInstantiate, emergePosition, type);
    }

    IEnumerator FuryAttack()
    {
        Debug.Log("ATAQUE DE FURIA!");
        int attackCount = Random.Range(4, 7);

        for (int i = 0; i < attackCount; i++)
        {
            float emergeX = Random.Range(leftLimit.position.x, rightLimit.position.x);
            Vector3 emergePosition = new Vector3(emergeX, groundLevel.position.y, 0);
            GameObject prefabToInstantiate = (Random.Range(0, 2) == 0) ? serpentPrefab_A : serpentPrefab_B;

            GameObject warningFX = Instantiate(warningIndicatorPrefab, emergePosition, Quaternion.identity);
            yield return StartCoroutine(BlinkIndicator(warningFX, furyWarningTime));

            InstantiateSerpent(prefabToInstantiate, emergePosition, Serpent_Attack.AttackType.Fury);

            yield return new WaitForSeconds(0.8f);
        }

        ReportAttackFinished();
    }

    public void ReportAttackFinished()
    {
        isAttacking = false;
    }
}
