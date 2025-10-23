using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class GatilloDespeguePajaro : MonoBehaviour
{
    [Header("Configuración del Pájaro")]
    public GameObject pajaro;
    public Sprite[] spritesDespegue;

    [Header("Configuración de Animación")]
    public float duracionFrame = 0.2f;
    public string tagDelPlayer = "Player";
    public bool voltearLejosDelPlayer = true;

    [Header("Configuración de Movimiento")]
    public float velocidadVertical = 3f;
    public float velocidadVuelo = 5f;

    // Componentes cacheados
    private SpriteRenderer birdSpriteRenderer;
    private Transform birdTransform;
    private Animator birdAnimator; // <--- AÑADIDO 1: Variable para el Animator

    // Control de estado
    private bool yaActivado = false;
    private int estadoActual = 0; 
    private Vector2 direccionVuelo;

    void Start()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;

        if (pajaro == null)
        {
            Debug.LogError("¡No se ha asignado el GameObject 'pajaro' en el Inspector!");
            return;
        }

        if (spritesDespegue.Length != 6)
        {
            Debug.LogError("¡El array 'spritesDespegue' debe contener exactamente 6 sprites!");
            return;
        }

        // Cachear componentes del pájaro
        birdSpriteRenderer = pajaro.GetComponent<SpriteRenderer>();
        birdTransform = pajaro.transform;
        birdAnimator = pajaro.GetComponent<Animator>(); // <--- AÑADIDO 2: Buscar el Animator

        if (birdSpriteRenderer == null)
        {
            // Si el SpriteRenderer no está en el objeto principal,
            // prueba a buscarlo en los hijos:
            birdSpriteRenderer = pajaro.GetComponentInChildren<SpriteRenderer>();
            if (birdSpriteRenderer == null)
            {
                Debug.LogError("¡El GameObject 'pajaro' no tiene un componente SpriteRenderer (ni en sus hijos)!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaActivado || !other.CompareTag(tagDelPlayer))
        {
            return;
        }

        yaActivado = true;

        // --- ¡SOLUCIÓN! ---
        // Desactiva el Animator para que este script pueda controlar el sprite
        if (birdAnimator != null)
        {
            birdAnimator.enabled = false; // <--- AÑADIDO 3: Desactivar el Animator
        }
        // --------------------

        StartCoroutine(SecuenciaDespegue(other.transform));
    }

    private IEnumerator SecuenciaDespegue(Transform playerTransform)
    {
        // ... (El resto de tu corutina es idéntico y debería funcionar ahora) ...
        
        // --- 1. Voltear Sprite ---
        if (voltearLejosDelPlayer)
        {
            bool playerEstaIzquierda = playerTransform.position.x < birdTransform.position.x;
            birdSpriteRenderer.flipX = playerEstaIzquierda; 
        }

        direccionVuelo = birdSpriteRenderer.flipX ? Vector2.left : Vector2.right;


        // --- 2. Estados 1-3: Preparándose ---
        Debug.Log("sprite 1");
        estadoActual = 1;
        birdSpriteRenderer.sprite = spritesDespegue[0];
        yield return new WaitForSeconds(duracionFrame);

        Debug.Log("sprite 2");
        estadoActual = 2;
        birdSpriteRenderer.sprite = spritesDespegue[1];
        yield return new WaitForSeconds(duracionFrame);

        Debug.Log("sprite 3");
        estadoActual = 3;
        birdSpriteRenderer.sprite = spritesDespegue[2];
        yield return new WaitForSeconds(duracionFrame);

        // --- 3. Estado 4: Despegando ---
        Debug.Log("sprite 4");
        estadoActual = 4;
        birdSpriteRenderer.sprite = spritesDespegue[3];
        yield return new WaitForSeconds(duracionFrame * 1.5f); 

        // --- 4. Estado 5: Planeando ---
        estadoActual = 5;
        birdSpriteRenderer.sprite = spritesDespegue[4];
        yield return new WaitForSeconds(duracionFrame * 2f);

        // --- 5. Estado 6: Volando ---
        estadoActual = 6;
        birdSpriteRenderer.sprite = spritesDespegue[5];

        // Opcional: Desactivar el pájaro después de un tiempo
        yield return new WaitForSeconds(4f); 
        pajaro.SetActive(false); 
    }

    void Update()
    {
        // ... (Tu Update es correcto y no necesita cambios) ...
        if (!yaActivado)
        {
            return;
        }

        Vector2 movimiento = Vector2.zero;

        switch (estadoActual)
        {
            case 1: 
            case 2: 
            case 3: 
                break;
            
            case 4: 
                movimiento = (-(direccionVuelo/1.5f) + (Vector2.up/2)).normalized * velocidadVertical;
                break;

            case 5: 
                movimiento = (-direccionVuelo + Vector2.up).normalized * velocidadVuelo;
                break;

            case 6: 
                movimiento = (-direccionVuelo + Vector2.up * 0.5f).normalized * velocidadVuelo;
                break;
        }

        if (birdTransform != null)
        {
            birdTransform.Translate(movimiento * Time.deltaTime);
        }
    }
}