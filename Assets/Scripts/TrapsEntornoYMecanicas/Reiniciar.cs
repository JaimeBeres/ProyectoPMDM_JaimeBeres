using UnityEngine;
using UnityEngine.SceneManagement;

public class Reiniciar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void IrAlMenu()
    {
        SceneManager.LoadScene("Inicio");
        Cronometro.tiempoFinal= 0f;
        Cronometro.tiempoTranscurrido = 0f;
    }


}
