using UnityEngine;

public class MuertePorTrampa : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     private void OnTriggerEnter2D(Collider2D choque)
    {
        if (choque.CompareTag("Player"))
        {
            JugadorController jugador = choque.GetComponent<JugadorController>();

                jugador.Morir();
                
            
        }
    }
}
