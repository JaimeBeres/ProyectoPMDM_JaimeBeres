using UnityEngine;
using TMPro;

public class Cronometro : MonoBehaviour
{
    private TMP_Text texto;
    public static float tiempoTranscurrido = 0f;

    public static float tiempoFinal;

    void Start()
    {
        texto = GetComponent<TMP_Text>();
        tiempoFinal = 0f;
    }

    void Update()
    {
        tiempoTranscurrido += Time.deltaTime;

        int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60f);
        int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60f);

        texto.text = $"{minutos:00}:{segundos:00}";
    }

    void OnDestroy()
    {
        tiempoFinal = tiempoTranscurrido;
        
    }
}