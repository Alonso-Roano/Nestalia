using UnityEngine;

public class Worm : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("La velocidad con la que el objeto se mueve.")]
    public float velocidad = 2f;

    [Tooltip("La distancia total que el objeto recorrerá hacia arriba desde su punto inicial.")]
    public float distanciaSubida = 5f;

    private Vector3 posicionInicial;
    private int direccion = 1;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector3.up * velocidad * direccion * Time.deltaTime);
        if (direccion == 1 && transform.position.y >= posicionInicial.y + distanciaSubida)
        {
            direccion = -1;
        }
        else if (direccion == -1 && transform.position.y <= posicionInicial.y)
        {
            transform.position = new Vector3(posicionInicial.x, posicionInicial.y, posicionInicial.z);
            direccion = 1;
        }
    }
}