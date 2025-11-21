using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTrap : MonoBehaviour
{
    // --- Configuración de la Trampa ---
    [Header("Configuración del Disparador")]
    [Tooltip("El objeto 'marcador' que parpadeará. Arrástralo aquí.")]
    [SerializeField] private GameObject attackMarker;
    
    [Tooltip("Duración total del parpadeo antes de atacar.")]
    [SerializeField] private float markerDuration = 1.5f;
    
    [Tooltip("Velocidad del parpadeo (ej. 0.2f = 5 parpadeos por segundo).")]
    [SerializeField] private float flashInterval = 0.2f;

    [Tooltip("Tiempo de espera antes de que la trampa pueda reactivarse.")]
    [SerializeField] private float trapCooldown = 5.0f;

    [Header("Configuración de Proyectiles")]
    [Tooltip("La lista de TODOS los prefabs de proyectiles (skins) que puede elegir.")]
    [SerializeField] private List<GameObject> projectilePrefabs;
    
    [Tooltip("Cuántos proyectiles disparar en cada activación.")]
    [SerializeField] private int numberOfProjectiles = 10;
    
    [Tooltip("Velocidad de los proyectiles.")]
    [SerializeField] private float projectileSpeed = 8.0f;
    
    [Tooltip("Un GameObject vacío cuyo 'position' y 'scale' definen el área de spawn.")]
    [SerializeField] private Transform spawnArea;

    // --- NUEVO: Retraso aleatorio entre proyectiles ---
    [Header("Configuración de Ráfaga")]
    [Tooltip("El retraso MÍNIMO entre cada proyectil (en segundos). 0.05 = 50ms")]
    [SerializeField] private float minDelayBetweenProjectiles = 0.05f; // --- NUEVO ---
    
    [Tooltip("El retraso MÁXIMO entre cada proyectil (en segundos). 0.2 = 200ms")]
    [SerializeField] private float maxDelayBetweenProjectiles = 0.2f; // --- NUEVO ---


    // --- Dirección del Ataque (Tu parámetro) ---
    public enum AttackDirection
    {
        TopToBottom,
        BottomToTop,
        LeftToRight,
        RightToLeft
    }

    [Header("Dirección del Ataque")]
    [Tooltip("Elige la dirección en la que se lanzarán los proyectiles.")]
    [SerializeField] private AttackDirection attackDirection;

    // --- Estado Interno ---
    private bool canActivate = true;
    private SpriteRenderer markerRenderer;

    private void Start()
    {
        if (attackMarker != null)
        {
            markerRenderer = attackMarker.GetComponent<SpriteRenderer>();
            attackMarker.SetActive(false); 
        }

        if (spawnArea == null)
        {
            Debug.LogError("¡No se ha asignado un Spawn Area a la trampa! Usando la posición de la trampa como fallback.");
            spawnArea = this.transform;
        }

        // --- NUEVO: Validar que el min no sea mayor que el max ---
        if (minDelayBetweenProjectiles > maxDelayBetweenProjectiles)
        {
            minDelayBetweenProjectiles = maxDelayBetweenProjectiles;
            Debug.LogWarning("MinDelay era mayor que MaxDelay. Se han igualado.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (canActivate && other.CompareTag("Player"))
        {
            StartCoroutine(ActivateTrapSequence());
        }
    }

    private IEnumerator ActivateTrapSequence()
    {
        // 1. Empezar Cooldown
        canActivate = false;

        // 2. Secuencia del Marcador (Parpadeo)
        if (attackMarker != null)
        {
            attackMarker.SetActive(true);
            float elapsedTime = 0f;
            bool isMarkerVisible = true;

            while (elapsedTime < markerDuration)
            {
                markerRenderer.enabled = isMarkerVisible;
                isMarkerVisible = !isMarkerVisible;
                
                yield return new WaitForSeconds(flashInterval);
                elapsedTime += flashInterval;
            }
            attackMarker.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(markerDuration);
        }

        // 3. ¡Lanzar el Ataque!
        // --- MODIFICADO: Ahora esperamos a que la corutina LaunchAttack termine ---
        yield return LaunchAttack(); 

        // 4. Esperar el Cooldown para reactivar
        yield return new WaitForSeconds(trapCooldown);
        canActivate = true;
    }

    // --- MODIFICADO: Esta función ahora es un IEnumerator ---
    private IEnumerator LaunchAttack()
    {
        Vector2 directionVector = GetDirectionVector();

        for (int i = 0; i < numberOfProjectiles; i++)
        {
            // Paso A: Elegir un prefab (skin) aleatorio
            GameObject prefabToSpawn = projectilePrefabs[Random.Range(0, projectilePrefabs.Count)];

            // Paso B: Elegir una posición aleatoria
            Vector2 spawnPos = GetRandomSpawnPosition();

            // Paso C: Instanciar
            GameObject projectileInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            // Paso D: Lanzarlo
            SimpleProjectile projectileScript = projectileInstance.GetComponent<SimpleProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Launch(directionVector, projectileSpeed);
            }
            else
            {
                Rigidbody2D rb = projectileInstance.GetComponent<Rigidbody2D>();
                if(rb != null) rb.linearVelocity = directionVector * projectileSpeed;
            }

            // --- NUEVO: Esperar un tiempo aleatorio antes de lanzar el siguiente ---
            float randomDelay = Random.Range(minDelayBetweenProjectiles, maxDelayBetweenProjectiles);
            yield return new WaitForSeconds(randomDelay);
        }
    }

    private Vector2 GetDirectionVector()
    {
        switch (attackDirection)
        {
            case AttackDirection.TopToBottom: return Vector2.down;
            case AttackDirection.BottomToTop: return Vector2.up;
            case AttackDirection.LeftToRight: return Vector2.right;
            case AttackDirection.RightToLeft: return Vector2.left;
            default: return Vector2.down;
        }
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 spawnCenter = spawnArea.position;
        Vector2 spawnSize = spawnArea.localScale;
        float spawnX = Random.Range(spawnCenter.x - spawnSize.x / 2, spawnCenter.x + spawnSize.x / 2);
        float spawnY = Random.Range(spawnCenter.y - spawnSize.y / 2, spawnCenter.y + spawnSize.y / 2);
        return new Vector2(spawnX, spawnY);
    }
}