using TMPro;
using UnityEngine;

public class TextoFrutas : MonoBehaviour

{
    TMP_Text texto;
    JugadorController jugador;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        texto = GetComponent<TMP_Text>();
        jugador = FindFirstObjectByType<JugadorController>();
        texto.text = "hola";
    }

    // Update is called once per frame
    void Update()
    {
        texto.text = jugador.frutasRecogidas.ToString();
        
       
    }
}
