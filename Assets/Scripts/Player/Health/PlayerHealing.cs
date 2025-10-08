using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el nuevo Input System

// Asegúrate de que el objeto tenga los componentes necesarios
[RequireComponent(typeof(PlayerHealth), typeof(Animator))]
public class PlayerHealing : MonoBehaviour
{
    [Header("Referencias de Componentes")]
    // Arrastra aquí tu 'Input Action' de curación desde el editor
    [SerializeField] private InputActionReference healAction; 
    // El script de salud que ya tienes
    private PlayerHealth playerHealth; 
    // El Animator del jugador
    private Animator animator;

    [Header("Configuración de Curación")]
    [Tooltip("El porcentaje máximo de vida al que se puede curar (ej. 0.75 para 75%).")]
    [SerializeField] [Range(0f, 1f)] private float healingCapPercentage = 0.75f;
    [Tooltip("Puntos de vida curados por segundo al inicio.")]
    [SerializeField] public float initialHealRate = 5f;
    [Tooltip("Cuánto aumenta la velocidad de curación por cada segundo.")]
    [SerializeField] private float healAcceleration = 2.5f;

    [Header("Efectos Visuales")]
    [Tooltip("Arrastra aquí tu sistema de partículas de curación.")]
    [SerializeField] private ParticleSystem healingParticles;
    [Tooltip("Arrastra aquí el objeto del aura verde.")]
    [SerializeField] private GameObject healingAura;

    // Variables internas para controlar el estado
    private bool isHealing = false;
    private float currentHealRate;
    private float healHoldTime;
    private float healBuffer = 0f; // Acumulador para curar en números enteros

    // Al iniciar el juego, obtenemos las referencias
    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();

        // Desactivamos los efectos al empezar
        if (healingParticles != null) healingParticles.Stop();
        if (healingAura != null) healingAura.SetActive(false);
    }

    // Activamos los listeners del input
    private void OnEnable()
    {
        healAction.action.started += StartHealing;
        healAction.action.canceled += StopHealing;
    }

    // Desactivamos los listeners para evitar errores
    private void OnDisable()
    {
        healAction.action.started -= StartHealing;
        healAction.action.canceled -= StopHealing;
    }

    // Se llama cuando se presiona el botón de curar
    private void StartHealing(InputAction.CallbackContext context)
    {
        Debug.Log("Curando");
        float healingCap = playerHealth.maxHealth * healingCapPercentage;
        Debug.Log(healingCap);
        Debug.Log(playerHealth.GetCurrentHealth());
        // No hacer nada si ya tenemos suficiente vida
        if (playerHealth.GetCurrentHealth() >= healingCap) return;
        
        isHealing = true;
        
        // Reiniciamos contadores
        healHoldTime = 0f;
        healBuffer = 0f;
        currentHealRate = initialHealRate;

        // Bloqueamos al jugador y activamos la animación
        LockPlayer(true);

        // Activamos los efectos visuales
        if (healingParticles != null) healingParticles.Play();
        if (healingAura != null) healingAura.SetActive(true);
    }

    // Se llama cuando se suelta el botón de curar
    private void StopHealing(InputAction.CallbackContext context)
    {
        if (!isHealing) return;

        isHealing = false;

        // Desbloqueamos al jugador y desactivamos la animación
        LockPlayer(false);
        
        // Detenemos los efectos visuales
        if (healingParticles != null) healingParticles.Stop();
        if (healingAura != null) healingAura.SetActive(false);
    }

    private void Update()
    {
        if (!isHealing) return;

        float healingCap = playerHealth.maxHealth * healingCapPercentage;
        // Si alcanzamos el tope de vida, paramos
        if (playerHealth.GetCurrentHealth() >= healingCap)
        {
            StopHealing(new InputAction.CallbackContext()); // Llama a la función para detener todo
            return;
        }

        // Aceleramos la curación
        healHoldTime += Time.deltaTime;
        currentHealRate = initialHealRate + (healHoldTime * healAcceleration);

        // Usamos un buffer para no perder decimales y curar de forma fluida
        healBuffer += currentHealRate * Time.deltaTime;

        if (healBuffer >= 1f)
        {
            int amountToHeal = Mathf.FloorToInt(healBuffer);

            // Aseguramos no pasarnos del tope al curar
            if (playerHealth.GetCurrentHealth() + amountToHeal > healingCap)
            {
                amountToHeal = Mathf.FloorToInt(healingCap - playerHealth.GetCurrentHealth());
            }

            if(amountToHeal > 0)
            {
                playerHealth.Heal(amountToHeal);
            }
            
            // Restamos la cantidad curada del buffer
            healBuffer -= Mathf.FloorToInt(healBuffer);
        }
    }
    
    // Método clave para bloquear y desbloquear al jugador
    private void LockPlayer(bool lockState)
    {
        // La forma más sencilla de bloquear inputs es desactivar el script de control
        // Asegúrate de que tienes una referencia a tu script 'PlayerController'
        PlayerMovement controller = GetComponent<PlayerMovement>();
        if (controller != null)
        {
            controller.enabled = !lockState;
        }

        // Activamos o desactivamos la animación de curación
        animator.SetBool("IsHealing", lockState);

        // Si estamos bloqueando, reseteamos todas las demás animaciones
        if (lockState)
        {
            animator.SetFloat("Movement", 0);
            animator.SetBool("IsWallClinging", false);
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsPlanning", false);
            
            animator.ResetTrigger("Jump");
            animator.ResetTrigger("WallJump");
            animator.ResetTrigger("DoubleJump");
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("AttackDown");
            animator.ResetTrigger("AttackUp");
        }
    }
}