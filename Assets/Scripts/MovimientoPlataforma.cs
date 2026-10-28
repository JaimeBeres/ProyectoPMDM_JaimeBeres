
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

    void Start()
    {     
        rb2d = GetComponent<Rigidbody2D>();
        izquierda = rb2d.position + Vector2.left * distancia;
        derecha = rb2d.position + Vector2.right * distancia;
    }

    void Update()
    {     
        if (cayendo) return;
        Vector2 objetivo = direccion == 1 ? derecha : izquierda;
        rb2d.MovePosition(
            Vector2.MoveTowards(
                rb2d.position,
                objetivo,
                velocidad * Time.fixedDeltaTime
            )
        );
        if (Vector2.Distance(rb2d.position, objetivo) < 0.01f)
        {
            direccion *= -1;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(activarColision){
            if (collision.gameObject.CompareTag("Player"))
            {
                cayendo = true;
                rb2d.bodyType = RigidbodyType2D.Dynamic;
                rb2d.gravityScale = 1f;
                
            }
    }
    }
}
