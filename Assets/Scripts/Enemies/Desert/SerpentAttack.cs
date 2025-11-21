using System.Collections;
using UnityEngine;

public class Serpent_Attack : MonoBehaviour
{
    // Tipos de ataque que define el controlador
    public enum AttackType { FastUpSlowDown, SlowUpFastDown, Fury }
    
    [Header("Referencias")]
    public SerpentBoss_Controller bossController; // Asignado por el Spawner
    
    [Header("Parámetros de Movimiento")]
    public float attackHeight = 6f; // Qué tan alto emerge sobre el suelo
    public float speedFast = 15f;
    public float speedSlow = 4f;
    public float hangTime = 2f; // Tiempo que se queda arriba (vulnerable)
    
    private AttackType currentType;
    private float startY; // La Y del suelo
    private float targetY; // La Y máxima a la que llega
    private float hideY; // La Y a la que se esconde

    // Inicializado por el Boss Controller
    public void Initialize(SerpentBoss_Controller controller, AttackType type, float groundY)
    {
        bossController = controller;
        currentType = type;
        startY = groundY;
        
        targetY = startY + attackHeight;
        hideY = startY - 10f; // Empezar muy abajo

        // Posicionarse inicialmente escondido
        transform.position = new Vector3(transform.position.x, hideY, 0);
        
        // Iniciar la corutina de ataque
        StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        float emergeSpeed;
        float descendSpeed;
        float waitTime;

        // --- 1. Definir parámetros según el tipo de ataque ---
        switch (currentType)
        {
            case AttackType.FastUpSlowDown:
                emergeSpeed = speedFast;
                descendSpeed = speedSlow;
                waitTime = hangTime;
                break;
            case AttackType.SlowUpFastDown:
                emergeSpeed = speedSlow;
                descendSpeed = speedFast;
                waitTime = hangTime;
                break;
            case AttackType.Fury:
                emergeSpeed = speedFast;
                descendSpeed = speedFast;
                waitTime = 0.2f; // Casi no espera arriba
                break;
            default:
                emergeSpeed = speedSlow;
                descendSpeed = speedSlow;
                waitTime = hangTime;
                break;
        }

        // --- 2. Emerger ---
        while (transform.position.y < targetY)
        {
            transform.position = Vector3.MoveTowards(transform.position, 
                new Vector3(transform.position.x, targetY, 0), 
                emergeSpeed * Time.deltaTime);
            yield return null;
        }

        // --- 3. Esperar Arriba (Vulnerable) ---
        yield return new WaitForSeconds(waitTime);

        // --- 4. Descender ---
        while (transform.position.y > hideY)
        {
            transform.position = Vector3.MoveTowards(transform.position, 
                new Vector3(transform.position.x, hideY, 0), 
                descendSpeed * Time.deltaTime);
            yield return null;
        }

        // --- 5. Limpieza ---
        // Si no es un ataque de Furia, avisa al manager que ha terminado
        if (currentType != AttackType.Fury)
        {
            bossController.ReportAttackFinished();
        }
        
        // Autodestruirse
        Destroy(gameObject);
    }
}