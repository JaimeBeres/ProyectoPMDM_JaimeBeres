
using System;
using UnityEngine;

public class Movimientoplataforma : MonoBehaviour
{
    [SerializeField] float velocidad = 2f;
    [SerializeField] float distancia = 10f;
    Rigidbody2D rb2d;
    Vector2 izquierda;
    Vector2 derecha;
    [SerializeField] Boolean activarColision = false;
    [SerializeField] int direccion = 1;
    bool cayendo = false;
    public float VelocidadHorizontal;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        izquierda = rb2d.position + Vector2.left * distancia;
        derecha = rb2d.position + Vector2.right * distancia;
    }


    void FixedUpdate()
    {
        if (cayendo)
        {
            VelocidadHorizontal = rb2d.linearVelocityX;
            return;
        }

        Vector2 objetivo = direccion == 1 ? derecha : izquierda;

        Vector2 posicionAnterior = rb2d.position;

        Vector2 nuevaPosicion = Vector2.MoveTowards(
            posicionAnterior,
            objetivo,
            velocidad * Time.fixedDeltaTime
        );

        // Calcular la velocidad horizontal de la plataforma.
        VelocidadHorizontal =
            (nuevaPosicion.x - posicionAnterior.x) / Time.fixedDeltaTime;

        rb2d.MovePosition(nuevaPosicion);

        if (Vector2.Distance(nuevaPosicion, objetivo) < 0.01f)
        {
            direccion *= -1;
        }
    }
    void OnCollisionEnter2D(Collision2D collision) //Si quisieramos hacer que se caiga
    {
        if (activarColision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                cayendo = true;
                rb2d.bodyType = RigidbodyType2D.Dynamic;
                rb2d.gravityScale = 1f;

            }
        }
    }
}
