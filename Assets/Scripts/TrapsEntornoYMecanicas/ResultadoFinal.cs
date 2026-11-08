using TMPro;
using UnityEngine;

public class ResultadoFinal : MonoBehaviour
{
    private TMP_Text tiempo;
    private TMP_Text frutas;
    int sumaFrutas = 0;
    private float temporizador = 0f;
    private float intervalo = 0.05f;
    void Start()
    {
        
        tiempo = transform.Find("Tiempo").GetComponent<TMP_Text>();
        frutas = transform.Find("Puntos").GetComponent<TMP_Text>();
        // Obtener el tiempo de la partida.
        float tiempoFin = Cronometro.tiempoFinal;

        int minutos = Mathf.FloorToInt(tiempoFin / 60f);
        int segundos = Mathf.FloorToInt(tiempoFin % 60f);

        tiempo.text = $"{minutos:00}:{segundos:00}";

    
    }
    void Update()
    {
        if (sumaFrutas < TrofeoFinal.frutasFinal)
        {
            temporizador += Time.deltaTime;

            if (temporizador >= intervalo)
            {
                sumaFrutas++;
                frutas.text = sumaFrutas.ToString();
                temporizador = 0f;
            }
        }
    }

}
