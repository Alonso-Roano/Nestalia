using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMovement : MonoBehaviour
{
    Transform cam; //Main Camera
    Vector3 camStartPos;
    // float distance; // No necesitamos una sola distancia, usaremos 'offset'
    Vector2 travelOffset; // Distancia de desplazamiento (X, Y)

    GameObject[] backgrounds;
    Material[] mat;
    float[] backSpeed;

    float farthestBack;

    [Range(0.00001f, 1f)]
    public float parallaxSpeedX = 1f; // Renombrado para claridad en el eje X
    
    // ⭐ Nuevo: Multiplicador para reducir el efecto en el eje Y
    [Range(0.00001f, 1f)]
    public float parallaxSpeedY = 0.2f; 

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
        }

        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++) //find the farthest background
        {
            // Solo considerar el eje Z para la profundidad de paralaje
            if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBack)
            {
                farthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backCount; i++) //set the speed of bacground
        {
            // La velocidad se calcula en base a la distancia Z
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    private void LateUpdate()
    {
        // ⭐ Cambio 1: Calcular el desplazamiento total de la cámara (X e Y)
        travelOffset.x = cam.position.x - camStartPos.x;
        travelOffset.y = cam.position.y - camStartPos.y;

        // Esta línea parece mover el contenedor completo y podría interferir con el paralaje.
        // Si no es estrictamente necesaria para tu configuración, considera comentarla.
        // La mantendré como estaba, pero ten en cuenta que mueve el objeto ParallaxMovement.
        transform.position = new Vector3(cam.position.x - 1, transform.position.y, 9.92f);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            // La velocidad se basa en la profundidad Z
            float speed = backSpeed[i];
            
            // ⭐ Cambio 2: Aplicar las diferentes velocidades para X e Y
            // Desplazamiento X: travelOffset.x * speed * parallaxSpeedX
            // Desplazamiento Y: travelOffset.y * speed * parallaxSpeedY (reducido)
            
            float offsetX = travelOffset.x * speed * parallaxSpeedX;
            float offsetY = travelOffset.y * speed * parallaxSpeedY;
            
            // Aplicar el desplazamiento a la textura
            mat[i].SetTextureOffset("_MainTex", new Vector2(offsetX, offsetY));
        }
    }
}