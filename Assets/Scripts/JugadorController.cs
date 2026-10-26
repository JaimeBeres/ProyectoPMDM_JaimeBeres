using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
public class JugadorController : MonoBehaviour
{
    private enum EstadoAnimacion
    {
        Idle = 0,
        Run = 1,
        Jump = 2,
        Fall = 3,
        Walled = 4
    }

    [Header("Movimiento")]
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float salto = 8f;
    [SerializeField] private float frenadoAire = 10f;

    [Header("Wall Slide")]
    [SerializeField] private float velocidadDeslizamientoPared = 2f;
    [SerializeField] private float distanciaDeteccionPared = 0.12f;

    [Header("Wall Jump")]
    [SerializeField] private float velocidadSaltoParedX = 7f;
    [SerializeField] private float velocidadSaltoParedY = 8f;

    private InputAction moveAction;
    private InputAction jumpAction;

    private Rigidbody2D rb2d;
    private CapsuleCollider2D con;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private int layerSuelo;

    private Vector2 movimiento;
    private bool saltoSolicitado;
    private bool controlGanchoActivo;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        rb2d = GetComponent<Rigidbody2D>();
        con = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        layerSuelo = LayerMask.GetMask("Suelo");
    }

    void Update()
    {
        // Leer los controles.
        if (moveAction != null)
        {
            movimiento = moveAction.ReadValue<Vector2>();
        }

        // Guardar la pulsación de salto para procesarla en FixedUpdate.
        if (jumpAction != null && jumpAction.WasPressedThisFrame())
        {
            saltoSolicitado = true;
        }

        // Girar el sprite según la dirección de movimiento.
        if (!controlGanchoActivo && spriteRenderer != null)
        {
            if (movimiento.x < -0.01f)
                spriteRenderer.flipX = true;
            else if (movimiento.x > 0.01f)
                spriteRenderer.flipX = false;
        }
    }

    public void EstablecerControlGancho(bool activo)
    {
        controlGanchoActivo = activo;

        // Evitar que quede una pulsación de salto pendiente
        // mientras el gancho controla al personaje.
        if (activo)
            saltoSolicitado = false;
    }

    void FixedUpdate()
    {
        bool enSuelo = TocaSuelo();
        bool hayPared = TocaPared(out float direccionSaltoPared);

        // El movimiento normal solo actúa si el gancho
        // no está controlando al personaje.
        if (!controlGanchoActivo)
        {
            // Control horizontal normal.
            if (enSuelo || Mathf.Abs(movimiento.x) > 0.01f)
            {
                rb2d.linearVelocityX = movimiento.x * velocidad;
            }
            else
            {
                // Frenar gradualmente cuando no hay entrada horizontal
                // y el personaje está en el aire.
                rb2d.linearVelocityX = Mathf.MoveTowards(
                    rb2d.linearVelocityX,
                    0f,
                    frenadoAire * Time.fixedDeltaTime
                );
            }

            // Procesar la petición de salto.
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
                    // Salto que impulsa al personaje
                    // en dirección contraria a la pared.
                    rb2d.linearVelocity = new Vector2(
                        direccionSaltoPared * velocidadSaltoParedX,
                        velocidadSaltoParedY
                    );

                    if (spriteRenderer != null)
                    {
                        spriteRenderer.flipX =
                            direccionSaltoPared < 0f;
                    }

                    enSuelo = false;
                }
            }
        }

        // Consumir la petición para no repetir el salto.
        saltoSolicitado = false;

        // Detectar el deslizamiento por una pared.
        bool deslizandoPared =
            !enSuelo &&
            hayPared &&
            rb2d.linearVelocity.y < 0f;

        // Limitar la velocidad de caída mientras se desliza.
        if (deslizandoPared &&
            rb2d.linearVelocity.y < -velocidadDeslizamientoPared)
        {
            rb2d.linearVelocityY = -velocidadDeslizamientoPared;
        }

        // Elegir un único estado de animación.
        EstadoAnimacion estadoActual;

        if (enSuelo)
        {
            if (!controlGanchoActivo &&
                Mathf.Abs(movimiento.x) > 0.01f)
            {
                estadoActual = EstadoAnimacion.Run;
            }
            else
            {
                estadoActual = EstadoAnimacion.Idle;
            }
        }
        else if (deslizandoPared)
        {
            estadoActual = EstadoAnimacion.Walled;
        }
        else if (rb2d.linearVelocity.y > 0.01f)
        {
            estadoActual = EstadoAnimacion.Jump;
        }
        else
        {
            estadoActual = EstadoAnimacion.Fall;
        }

        // Comunicar el estado elegido al Animator.
        animator.SetInteger("estado", (int)estadoActual);
    }

    // Comprueba si existe una superficie válida debajo de los pies.
    bool TocaSuelo()
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
            0.12f,
            layerSuelo
        );

        return hit.collider != null &&
               hit.normal.y > 0.5f;
    }

    // Detecta una pared a izquierda o derecha y devuelve
    // la dirección horizontal que aleja al personaje de ella.
    bool TocaPared(out float direccionSalto)
    {
        direccionSalto = 0f;

        float altura = con.bounds.center.y;

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

        if (hitIzquierdo.collider != null &&
            Mathf.Abs(hitIzquierdo.normal.x) > 0.5f)
        {
            direccionSalto = hitIzquierdo.normal.x;
            return true;
        }

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

        if (hitDerecho.collider != null &&
            Mathf.Abs(hitDerecho.normal.x) > 0.5f)
        {
            direccionSalto = hitDerecho.normal.x;
            return true;
        }

        return false;
    }
}