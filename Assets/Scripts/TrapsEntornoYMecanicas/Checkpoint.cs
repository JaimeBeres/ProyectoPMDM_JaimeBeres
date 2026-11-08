using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Transform respawn; //Por si se quisiera establecer un punto de reaparicion concreto asociado al checkpoint

    void Awake()
    {
        // En caso de no definir un punto de reaparicion, sera la posicion de la bandera
        if (respawn == null)
        {
            respawn = transform;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
                JugadorController jugador = collision.GetComponent<JugadorController>();

        if (jugador == null) return;//Por si sucediese algun error 
            

        // Guardamos el nuevo respawn
        Vector3 posicion = respawn.position;

        //Llamamos a la funcion checkpoint de jugadorcontroller
        jugador.Checkpoint(new Vector2(posicion.x, posicion.y)); 

        }
      
    }
}