using UnityEngine;

public class MuertePorTrampa : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     private void OnTriggerEnter2D(Collider2D choque)
    {
        if (choque.CompareTag("Player")) //Detectamos colision con el player
        {
            JugadorController jugador = choque.GetComponent<JugadorController>();

                jugador.Morir(); //Llamamos a la funcion morir de jugadorcontroller
                
        }
    }
}
