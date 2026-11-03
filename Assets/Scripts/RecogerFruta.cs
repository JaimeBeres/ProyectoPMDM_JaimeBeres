using UnityEngine;

public class RecogerFruta : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D choque)
    {
        if (choque.CompareTag("Player"))
        {
            JugadorController jugador = choque.GetComponent<JugadorController>();

                jugador.RecogerFruta();
                Destroy(gameObject);
            
        }
    }
}