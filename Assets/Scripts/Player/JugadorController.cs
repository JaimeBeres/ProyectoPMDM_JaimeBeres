using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class JugadorController : MonoBehaviour
{
    private enum EstadoAnimacion //Enumerado para la transicion entre animaciones
    {
        Idle = 0,
        Run = 1,
        Jump = 2,
        Fall = 3,
        Walled = 4,
        died = 5,
        djump = 6
    }
    //Variables que empleara el player
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float salto = 8f;
    [SerializeField] private float dSalto = 4f;
    [SerializeField] private float frenadoAire = 10f;

    [Header("Mecanicas pared")]
    [SerializeField] private float velocidadDeslizamientoPared = 2f;
    [SerializeField] private float distanciaDeteccionPared = 0.12f;
    [SerializeField] private float velocidadSaltoParedX = 7f;
    [SerializeField] private float velocidadSaltoParedY = 8f;

    private InputAction moveAction;
    private InputAction jumpAction;

    private Rigidbody2D rb2d;
    private CapsuleCollider2D con;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 puntoReaparicion;
    private int layerSuelo;

    private Vector2 movimiento;

    // Resultados de los booleanos de detecccion 
    private bool saltoSolicitado;
    private bool haSaltadoPared;
    private bool dobleSalto = true;
    private bool muriendo = false;
    private bool enSuelo;
    private bool deslizandoPared;

    // Direccion del sprite del player.
    private bool giroSpritePendiente;
    private bool direccionSpriteIzquierda;

    //Frutas recogibles
    public int frutasRecogidas = 0;
    private int frutasCP;

    //Para el audio
    [SerializeField] private AudioClip sonidoSalto;
    private AudioSource audioSource;

    void Start() //Deteccion de componentes necesarios
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        rb2d = GetComponent<Rigidbody2D>();
        con = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        layerSuelo = LayerMask.GetMask("Suelo");
        puntoReaparicion = rb2d.position; //Sera el primer punto de reaparicion, el lugar donde coloquemos al player al comienzo
        audioSource = GetComponent<AudioSource>();
    }

    // ==========================================
    // UPDATE: CONTROLES Y ANIMACIONES
    // ==========================================

    void Update()
    {
        // No sobrescribir la animación de muerte.
        if (muriendo) return;
        // Leer los controles.
        if (moveAction != null)
        {
            movimiento = moveAction.ReadValue<Vector2>();
        }

        // Guardar la pulsación de salto.
        if (jumpAction != null && jumpAction.WasPressedThisFrame())
        {
            saltoSolicitado = true;
        }

        // Girar el sprite según la dirección de movimiento.
        if (spriteRenderer != null)
        {
            if (movimiento.x < -0.01f)
                spriteRenderer.flipX = true;
            else if (movimiento.x > 0.01f)
                spriteRenderer.flipX = false;

            // Aplicar el giro solicitado por un salto de pared.
            if (giroSpritePendiente)
            {
                spriteRenderer.flipX = direccionSpriteIzquierda;
                giroSpritePendiente = false;
            }
        }

        

        // Elegir un único estado de animación.
        EstadoAnimacion estadoActual = 0;

        if (enSuelo)
        {
            if (Mathf.Abs(movimiento.x) > 0.01f)
                estadoActual = EstadoAnimacion.Run;
            else
                estadoActual = EstadoAnimacion.Idle;
        }
        else if (deslizandoPared)
        {
            estadoActual = EstadoAnimacion.Walled;
        }
        else if (rb2d.linearVelocity.y > 0.01f && dobleSalto)
        {
            estadoActual = EstadoAnimacion.Jump;
        }
        else if (rb2d.linearVelocity.y > 0.01f && !dobleSalto)
        {
            estadoActual = EstadoAnimacion.djump;
        }
        else if (!enSuelo && !deslizandoPared && rb2d.linearVelocity.y < 0)
        {
            estadoActual = EstadoAnimacion.Fall;
        }
        if(saltoSolicitado) audioSource.PlayOneShot(sonidoSalto);
        // Enviar el estado al Animator.
        animator.SetInteger("estado", (int)estadoActual);
    }

    // ==========================================
    // FIXEDUPDATE: FÍSICAS Y MOVIMIENTO
    // ==========================================

    void FixedUpdate()
    {
        if (muriendo) return;

        enSuelo = TocaSuelo();
        bool hayPared = TocaPared(out float direccionSaltoPared);

        if (!dobleSalto)
        {
            haSaltadoPared = false;
        }

        if (enSuelo || hayPared)
        {
            dobleSalto = true;
            haSaltadoPared = false;
        }

        // Control del movimiento horizontal.
        if (enSuelo || Mathf.Abs(movimiento.x) > 0.01f && !haSaltadoPared)
        {
            rb2d.linearVelocityX = movimiento.x * velocidad;
        }
        else
        {
            // Frenar gradualmente cuando no hay entrada horizontal en el aire.
            rb2d.linearVelocityX = Mathf.MoveTowards(
                rb2d.linearVelocityX,
                0f,
                frenadoAire * Time.fixedDeltaTime
            );
        }

        // Saltos
        if (saltoSolicitado)
        {
            if (enSuelo)
            {
                // Salto normal.
                rb2d.linearVelocityY = salto;
                enSuelo = false;
            }
            else if (hayPared)
            {
                // Salto en dirección contraria a la pared.
                rb2d.linearVelocity = new Vector2(
                    direccionSaltoPared * velocidadSaltoParedX,
                    velocidadSaltoParedY
                );

                direccionSpriteIzquierda = direccionSaltoPared < 0f;
                giroSpritePendiente = true;

                haSaltadoPared = true;
                enSuelo = false;
            }
            else if (!enSuelo && dobleSalto) //En base a su velocidad vertical el impulso del doble salto se aplica de forma distinta
            {
                if (rb2d.linearVelocityY < 0)
                    rb2d.linearVelocityY = dSalto;
                else
                    rb2d.linearVelocityY += dSalto;

                dobleSalto = false;
            }
        }

        // Ya se ha saltado
        saltoSolicitado = false;

        // Detectar el deslizamiento en pared
        deslizandoPared =
            !enSuelo &&
            hayPared &&
            rb2d.linearVelocity.y <= 0f;

        // Alterar la velocidad vertical cuando nos deslizamos por una pared
        if (deslizandoPared && rb2d.linearVelocity.y < -velocidadDeslizamientoPared)
        {
            rb2d.linearVelocityY = -velocidadDeslizamientoPared;
        }
    }

    // ==========================================
    // DETECCIÓN DE SUELO Y PAREDES
    // ==========================================

    bool TocaSuelo() //Detectar si estamos en el suelo
    {
        Vector2 tamano = new Vector2(
            con.bounds.size.x * 0.8f,
            0.05f
        );

        Vector2 origen = new Vector2(
            con.bounds.center.x,
            con.bounds.min.y + 0.03f
        );

        RaycastHit2D hit = Physics2D.BoxCast(
            origen,
            tamano,
            0f,
            Vector2.down,
            0.32f,
            layerSuelo
        );

        return hit.collider != null &&
               hit.normal.y > 0.5f;
    }

    //Detectar si estamos colisionando con una pared
    bool TocaPared(out float direccionSalto) 
    {
        direccionSalto = 0f;

        float altura = con.bounds.center.y;

        // Detectar una pared a la izquierda.
        Vector2 origenIzquierdo = new Vector2(
            con.bounds.min.x + 0.02f,
            altura
        );

        RaycastHit2D hitIzquierdo = Physics2D.Raycast(
            origenIzquierdo,
            Vector2.left,
            distanciaDeteccionPared,
            layerSuelo
        );

        if (hitIzquierdo.collider != null && Mathf.Abs(hitIzquierdo.normal.x) > 0.5f)
        {
            direccionSalto = hitIzquierdo.normal.x;
            return true;
        }

        // Detectar una pared a la derecha.
        Vector2 origenDerecho = new Vector2(
            con.bounds.max.x - 0.02f,
            altura
        );

        RaycastHit2D hitDerecho = Physics2D.Raycast(
            origenDerecho,
            Vector2.right,
            distanciaDeteccionPared,
            layerSuelo
        );

        if (hitDerecho.collider != null && Mathf.Abs(hitDerecho.normal.x) > 0.5f)
        {
            direccionSalto = hitDerecho.normal.x;
            return true;
        }

        return false;
    }

    // ==========================================
    // CHECKPOINTS Y MUERTE
    // ==========================================

    public void Checkpoint(Vector2 posicion) //Funcion que llamara la funcion Checkpoint
    {
        if (muriendo) return; //Si me estoy muriendo no puedo establecer un nuevo checkpoint

        puntoReaparicion = posicion;
        frutasCP = frutasRecogidas; // Guardamos las frutas recogidas hasta ese punto

        Debug.Log("Checkpoint activado"); // Comprobacion por consola
    }

    public void Reaparecer()
    {
        saltoSolicitado = false; //Para evitar errores, no queremos reaparecer saltando
        rb2d.position = puntoReaparicion;
        frutasRecogidas = frutasCP; //Establecemos el numero de frutas como el que hemos guardado al llegar al checkpoint
        muriendo = false;
    }

    public void Morir()
    {
        if (muriendo) return; //No puedo morirme varias veces al mismo tiempo

        muriendo = true;

        //Cancelar el movimiento
        rb2d.linearVelocity = Vector2.zero; 
        rb2d.angularVelocity = 0f;

        //Indicarle el estado al animator
        EstadoAnimacion estadoActual = EstadoAnimacion.died;
        animator.SetInteger("estado", (int)estadoActual); 

        //LLamamos a la funcion reaparecer
        Invoke(nameof(Reaparecer), 2f);
    }

    // ==========================================
    // RECOGIBLES
    // ==========================================

    public void RecogerFruta(int valorFruta) //Funcion que sera llamada por RecogerFruta
    {
        frutasRecogidas += valorFruta;
    }
}