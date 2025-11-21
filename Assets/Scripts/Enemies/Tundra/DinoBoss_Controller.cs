using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Unity.Cinemachine;
using Unity.Mathematics;

// 🔹 Renombrado de SerpentBoss_Controller a DinoBoss_Controller
public class DinoBoss_Controller : MonoBehaviour
{
    [Header("Salud y UI")]
    public float maxHealth = 200f;
    public float currentHealth;
    public Slider healthBar;
    private bool isPhaseTwo = false;

    [Header("Triggers de Activación")]
    public Canvas bossCanvas;
    public GameObject objectToActivate;
    public GameObject secondaryObjectToActivate;
    public GameObject powerSlowFall;
    public string playerTag = "Player";

    [Header("Configuración de Cinemachine")]
    public CinemachineCamera virtualCamera;
    public float zoomedOutOffsetZ = -20f;
    public float zoomDuration = 1.0f;

    private CinemachineFollow transposer;
    private float originalOffsetZ;
    private Coroutine zoomCoroutine;

    // 🔹 ----- MODIFICADO: Prefabs de Ataque ----- 🔹
    [Header("Referencias de Prefabs")]
    public GameObject attackPrefab_Above_A; // 3 ataques desde arriba
    public GameObject attackPrefab_Above_B;
    public GameObject attackPrefab_Above_C;
    public GameObject attackPrefab_Side;    // 1 ataque lateral para Fase 2
    public GameObject warningIndicatorPrefab; // El indicador de advertencia

    // 🔹 ----- MODIFICADO: Puntos de la Arena ----- 🔹
    [Header("Puntos de la Arena")]
    public Transform groundLevel; // Dónde aterrizan los ataques
    public Transform topLimit;    // Dónde aparecen los ataques de arriba
    public Transform leftLimit;   // Límite izquierdo de la arena
    public Transform rightLimit;  // Límite derecho de la arena
    public Transform sideSpawn_L; // Dónde aparece el ataque lateral izquierdo
    public Transform sideSpawn_R; // Dónde aparece el ataque lateral derecho

    // 🔹 ----- MODIFICADO: Tiempos de Ataque ----- 🔹
    [Header("Tiempos de Ataque")]
    public float minHideTime = 1f;
    public float maxHideTime = 3f;
    public float warningTime = 1.5f;
    public float sideAttackWarningTime = 1.0f; // Nuevo tiempo para el ataque lateral
    [Range(0, 100)]
    public int phase2SideAttackChance = 30; // Probabilidad del nuevo ataque en Fase 2

    public float blinkInterval = 0.1f;
    private bool isAttacking = false;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;

        if (bossCanvas != null) bossCanvas.gameObject.SetActive(false);
        if (objectToActivate != null) objectToActivate.SetActive(false);
        if (secondaryObjectToActivate != null) secondaryObjectToActivate.SetActive(false);

        if (virtualCamera != null)
        {
            transposer = virtualCamera.GetComponent<CinemachineFollow>();
            if (transposer != null)
                originalOffsetZ = transposer.FollowOffset.z;
            else
                Debug.LogError("La cámara virtual no tiene CinemachineFollow.");
        }
        powerSlowFall.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player ha entrado en la arena. Iniciando Jefe.");
            isAttacking = false;

            if (bossCanvas != null) bossCanvas.gameObject.SetActive(true);
            if (objectToActivate != null) objectToActivate.SetActive(true);
            if (secondaryObjectToActivate != null) secondaryObjectToActivate.SetActive(true);

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

            // 🔹 Busca los nuevos ataques de dinosaurio
            DinoAttack_Movement[] activeAttacks = FindObjectsByType<DinoAttack_Movement>(FindObjectsSortMode.None);
            foreach (DinoAttack_Movement attack in activeAttacks)
            {
                attack.gameObject.SetActive(false);
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
            Debug.Log("¡JEFE EN FASE 2!");
        }
    }

    void Die()
    {
        StopAllCoroutines();

        // 🔹 Busca los nuevos ataques de dinosaurio
        DinoAttack_Movement[] activeAttacks = FindObjectsByType<DinoAttack_Movement>(FindObjectsSortMode.None);
        foreach (DinoAttack_Movement attack in activeAttacks)
            Destroy(attack.gameObject);

        ClearWarnings();

        if (bossCanvas != null) bossCanvas.gameObject.SetActive(false);
        if (objectToActivate != null) objectToActivate.SetActive(false);
        if (secondaryObjectToActivate != null) secondaryObjectToActivate.SetActive(false);

        if (transposer != null && transposer.FollowOffset.z != originalOffsetZ)
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            StartCoroutine(AnimateZoom(originalOffsetZ));
        }

        Debug.Log("Jefe DERROTADO");
        powerSlowFall.SetActive(true);
        Destroy(gameObject);
    }

    void ClearWarnings()
    {
        GameObject[] warnings = GameObject.FindGameObjectsWithTag("Warning");
        foreach (GameObject w in warnings)
            Destroy(w);
    }

    // 🔹 ----- MODIFICADO: Bucle de Ataque ----- 🔹
    IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(3f);

        Debug.Log("Empezando ataque");
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

            // Lógica de Fases
            if (isPhaseTwo)
            {
                // En Fase 2, decide entre ataque de arriba o ataque lateral
                if (Random.Range(0, 100) < phase2SideAttackChance)
                    yield return StartCoroutine(Phase2SideAttack());
                else
                    yield return StartCoroutine(NormalTopAttack());
            }
            else
            {
                // En Fase 1, solo ataques de arriba
                yield return StartCoroutine(NormalTopAttack());
            }
        }
    }

    // 🔹 ----- MODIFICADO: Instanciador de Ataque ----- 🔹
    GameObject InstantiateDinoAttack(GameObject prefab, Vector3 position, DinoAttack_Movement.AttackType type)
    {
        // La rotación aleatoria de la serpiente ya no es necesaria
        GameObject attackInstance = Instantiate(prefab, position, Quaternion.identity);
        DinoAttack_Movement attackScript = attackInstance.GetComponent<DinoAttack_Movement>();

        if (attackScript == null)
        {
            Debug.LogError($"¡El prefab instanciado ({prefab.name}) NO tiene el script DinoAttack_Movement!");
            Destroy(attackInstance);
            ReportAttackFinished();
            return null;
        }

        // Pasa la referencia del suelo para que el ataque sepa dónde detenerse
        attackScript.Initialize(this, type, groundLevel.position.y);
        return attackInstance;
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

    // 🔹 ----- MODIFICADO: Ataque Normal (Ahora desde Arriba) ----- 🔹
    IEnumerator NormalTopAttack()
    {
        float targetX;
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
        {
            targetX = player.transform.position.x;
            targetX = Mathf.Clamp(targetX, leftLimit.position.x, rightLimit.position.x);
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": No se encontró al jugador. Usando X aleatorio.");
            targetX = Random.Range(leftLimit.position.x, rightLimit.position.x);
        }

        // La advertencia aparece en el SUELO
        Vector3 warningPosition = new Vector3(targetX, groundLevel.position.y, 0);
        // El ataque aparece en el TECHO
        Vector3 spawnPosition = new Vector3(targetX, topLimit.position.y, 0);

        // Elige uno de los 3 ataques superiores
        int attackChoice = Random.Range(0, 3);
        GameObject prefabToInstantiate;
        DinoAttack_Movement.AttackType type;

        switch (attackChoice)
        {
            case 0:
                prefabToInstantiate = attackPrefab_Above_A;
                type = DinoAttack_Movement.AttackType.Top_FastFall; // (Asignación de ejemplo)
                break;
            case 1:
                prefabToInstantiate = attackPrefab_Above_B;
                type = DinoAttack_Movement.AttackType.Top_SlowFall; // (Asignación de ejemplo)
                break;
            default:
                prefabToInstantiate = attackPrefab_Above_C;
                type = DinoAttack_Movement.AttackType.Top_HomingFall; // (Asignación de ejemplo)
                break;
        }

        GameObject warningFX = Instantiate(warningIndicatorPrefab, warningPosition, Quaternion.identity);
        yield return StartCoroutine(BlinkIndicator(warningFX, warningTime));

        InstantiateDinoAttack(prefabToInstantiate, spawnPosition, type);
        // El ataque mismo llamará a ReportAttackFinished() cuando termine
    }

    // 🔹 ----- NUEVO: Ataque Lateral de Fase 2 ----- 🔹
    IEnumerator Phase2SideAttack()
    {
        Debug.Log("¡ATAQUE LATERAL FASE 2!");

        Vector3 spawnPosition;
        DinoAttack_Movement.AttackType type;

        // Elige un lado (izquierdo o derecho)
        if (Random.Range(0, 2) == 0)
        {
            spawnPosition = sideSpawn_L.position;
            type = DinoAttack_Movement.AttackType.Side_Charge_LtoR; // Carga de Izq a Der
        }
        else
        {
            spawnPosition = sideSpawn_R.position;
            type = DinoAttack_Movement.AttackType.Side_Charge_RtoL; // Carga de Der a Izq
        }
        
        // Pone la advertencia en el punto de spawn y lo bajamos para que se vea mejor
        GameObject warningFX = Instantiate(warningIndicatorPrefab, spawnPosition - Vector3.up * 200, Quaternion.Euler(0, 0, 90));
        yield return StartCoroutine(BlinkIndicator(warningFX, sideAttackWarningTime));

        InstantiateDinoAttack(attackPrefab_Side, spawnPosition, type);
        // El ataque mismo llamará a ReportAttackFinished() cuando termine
    }


    public void ReportAttackFinished()
    {
        isAttacking = false;
    }
}