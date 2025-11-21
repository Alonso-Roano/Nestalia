using System.Collections;
using UnityEngine;

public class DinoAttack_Movement : MonoBehaviour
{
    public enum AttackType { Top_FastFall, Top_SlowFall, Top_HomingFall, Side_Charge_LtoR, Side_Charge_RtoL }
    private enum MovementState { MovingToTarget, StoppingOnGround, ReturningToSpawn }

    [Header("Referencias")]
    public DinoBoss_Controller bossController;
    
    [Header("Parámetros de Movimiento")]
    public float speedFast = 20f;
    public float speedSlow = 8f;
    public float homingSpeed = 4f;
    public float returnSpeedMultiplier = 0.8f;
    public float stopDurationOnGround = 0.5f;

    [Header("Tags de Colisión")]
    public string groundTag = "Ground";

    
    private AttackType currentType;
    private MovementState currentState;
    private Vector3 spawnPosition;
    private GameObject player;
    
    private float currentSpeed;
    private Vector3 currentTargetPos_Top;
    private bool isHoming;

    
    public void Initialize(DinoBoss_Controller controller, AttackType type, float groundY)
    {
        bossController = controller;
        currentType = type;
        this.spawnPosition = transform.position;
        player = GameObject.FindGameObjectWithTag(bossController.playerTag);
        currentState = MovementState.MovingToTarget;

        if (IsTopAttack(currentType))
        {
            isHoming = (currentType == AttackType.Top_HomingFall);
            currentSpeed = (currentType == AttackType.Top_FastFall) ? speedFast : speedSlow;
            
            currentTargetPos_Top = new Vector3(transform.position.x, groundY - 20f, 0); 
        }
        else
        {
            StartCoroutine(MoveSideToSide_AndBack());
        }
    }

    bool IsTopAttack(AttackType type)
    {
        return type == AttackType.Top_FastFall || 
               type == AttackType.Top_SlowFall || 
               type == AttackType.Top_HomingFall;
    }

    void Update()
    {
        if (bossController == null || !IsTopAttack(currentType) || currentState == MovementState.StoppingOnGround)
        {
            return;
        }

        if (currentState == MovementState.MovingToTarget)
        {
            if (isHoming && player != null)
            {
                float targetX = Mathf.MoveTowards(transform.position.x, player.transform.position.x, homingSpeed * Time.deltaTime);
                currentTargetPos_Top.x = targetX;
            }
            
            transform.position = Vector3.MoveTowards(transform.position, 
                currentTargetPos_Top, 
                currentSpeed * Time.deltaTime);
        }
        else
        {
            float returnSpeed = currentSpeed * returnSpeedMultiplier;
            
            transform.position = Vector3.MoveTowards(transform.position, 
                spawnPosition, 
                returnSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, spawnPosition) < 0.1f)
            {
                bossController.ReportAttackFinished();
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (currentState != MovementState.MovingToTarget || !IsTopAttack(currentType))
        {
            return;
        }

        if (other.gameObject.CompareTag(groundTag))
        {
            StartCoroutine(StopAndReturnRoutine());
        }
    }

    IEnumerator StopAndReturnRoutine()
    {
        currentState = MovementState.StoppingOnGround;
        isHoming = false;
        
        yield return new WaitForSeconds(stopDurationOnGround);
        
        currentState = MovementState.ReturningToSpawn;
    }

    IEnumerator MoveSideToSide_AndBack()
    {
        float speed = speedFast;
        float returnSpeed = speed * returnSpeedMultiplier;
        
        float targetX;
        if (currentType == AttackType.Side_Charge_LtoR)
            targetX = bossController.rightLimit.position.x + 10f;
        else
            targetX = bossController.leftLimit.position.x - 10f;
        
        Vector3 targetPosition = new Vector3(targetX, transform.position.y, 0);

        while (Mathf.Abs(transform.position.x - targetX) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, 
                targetPosition, 
                speed * Time.deltaTime);
            yield return null;
        }

        while (Mathf.Abs(transform.position.x - spawnPosition.x) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, 
                spawnPosition, 
                returnSpeed * Time.deltaTime);
            yield return null;
        }

        bossController.ReportAttackFinished();
        Destroy(gameObject);
    }
}