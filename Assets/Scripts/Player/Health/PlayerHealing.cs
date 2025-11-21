using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerHealth), typeof(Animator))]
public class PlayerHealing : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private Animator animator;

    [Header("Curación")]
    [SerializeField] [Range(0f,1f)] private float healingCapPercentage = 0.75f;
    [SerializeField] public float initialHealRate = 5f;
    [SerializeField] public float healAcceleration = 2.5f;

    [Header("Efectos")] 
    [SerializeField] private ParticleSystem healingParticles;
    [SerializeField] private GameObject healingAura;

    private bool isHealing;
    private float healHoldTime;
    private float currentHealRate;
    private float healBuffer;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();

        if (healingParticles) healingParticles.Stop();
        if (healingAura) healingAura.SetActive(false);
    }

    // ---- ESTA ES LA ÚNICA ACCIÓN ----
    // Hookea en PlayerInput → Heal.started y Heal.canceled → este mismo método.
    public void OnHeal(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            StartHealing();
        }
        else if (ctx.canceled)
        {
            StopHealing();
        } 
    }

    private void StartHealing()
    {
        float cap = playerHealth.maxHealth * healingCapPercentage;
        if (playerHealth.GetCurrentHealth() >= cap) return;

        isHealing = true;
        healHoldTime = 0f;
        healBuffer = 0f;
        currentHealRate = initialHealRate;

        LockPlayer(true);

        if (healingParticles) healingParticles.Play();
        if (healingAura) healingAura.SetActive(true);
    }

    private void StopHealing()
    {
        if (!isHealing) return;

        isHealing = false;
        LockPlayer(false);

        if (healingParticles) healingParticles.Stop();
        if (healingAura) healingAura.SetActive(false);
    }

    private void Update()
    {
        if (!isHealing) return;

        float cap = playerHealth.maxHealth * healingCapPercentage;

        if (playerHealth.GetCurrentHealth() >= cap)
        {
            StopHealing();
            return;
        }

        healHoldTime += Time.deltaTime;
        currentHealRate = initialHealRate + healHoldTime * healAcceleration;

        healBuffer += currentHealRate * Time.deltaTime;

        if (healBuffer >= 1f)
        {
            int amount = Mathf.FloorToInt(healBuffer);

            float maxAllowed = cap - playerHealth.GetCurrentHealth();
            if (amount > maxAllowed) amount = Mathf.FloorToInt(maxAllowed);

            if (amount > 0)
                playerHealth.Heal(amount);

            healBuffer -= Mathf.FloorToInt(healBuffer);
        }
    }

    private void LockPlayer(bool lockState)
    {
        var controller = GetComponent<PlayerMovement>();
        if (controller) controller.enabled = !lockState;

        animator.SetBool("IsHealing", lockState);

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
