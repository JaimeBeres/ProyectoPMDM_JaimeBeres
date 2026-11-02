using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Transform respawn;

    void Awake()
    {
        // Si no asignamos un punto específico,
        // utilizamos la posición de la propia bandera.
        if (respawn == null)
        {
            respawn = transform;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        JugadorController jugador =
            collision.GetComponentInParent<JugadorController>();

        if (jugador == null)
            return;

        // Guardamos la nueva posición de reaparición.
        Vector3 posicion = respawn.position;

        jugador.Checkpoint(
            new Vector2(posicion.x, posicion.y)
        );
    }
}