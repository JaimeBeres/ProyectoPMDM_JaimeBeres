using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class TrofeoFinal : MonoBehaviour
{
    public static int frutasFinal;
    public static float tiempoFin;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")){
            JugadorController jugador = collision.GetComponent<JugadorController>();
            frutasFinal = jugador.frutasRecogidas;
            SceneManager.LoadScene("Final");
        }
    }
}
