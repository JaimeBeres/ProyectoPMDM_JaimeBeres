using UnityEngine;

public class RecogerFruta : MonoBehaviour
{
    [SerializeField]
    public int valorFruta = 1;
    private void OnTriggerEnter2D(Collider2D choque) //Comienza cuando la fruta detecta colision
    {
        if (choque.CompareTag("Player")) //Solo ocurre si colisiona con el player
        {
            JugadorController jugador = choque.GetComponent<JugadorController>(); // Llamamos al script del player

                jugador.RecogerFruta(valorFruta); //Empleamos la funcion pasandole el valor de la fruta
                Destroy(gameObject); //Una vez recogida la fruta desaparece
            
        }
    }
}