using UnityEngine;
using UnityEngine.SceneManagement;

public class PantallaInicio : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Comenzar()
    {
        SceneManager.LoadScene("Juego");
        Cronometro.tiempoFinal= 0f;
        Cronometro.tiempoTranscurrido = 0f;
    }

   
}
