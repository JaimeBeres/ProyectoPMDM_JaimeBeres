using UnityEngine;
using UnityEngine.InputSystem;

public class JugadorController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float velocidad = 5f;
    [SerializeField] float salto = 8f;

    [Header("Wall Slide")]
    [SerializeField] float velocidadDeslizamientoPared = 2f;
    [SerializeField] float distanciaDeteccionPared = 0.12f;

    [Header("Wall Jump")]
    [SerializeField] float fuerzaSaltoParedX = 7f;
    [SerializeField] float fuerzaSaltoParedY = 8f;
    [SerializeField] float tiempoControlSaltoPared = 0.15f;

    InputAction moveAction;
    InputAction jumpAction;

    Rigidbody2D rb2d;
    CapsuleCollider2D con;
    Animator animator;
    SpriteRenderer spriteRenderer;

    int layerSuelo;

    Vector2 movimiento;
    bool saltoSolicitado;
    float tiempoControlHorizontal;

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
        movimiento = moveAction.ReadValue<Vector2>();

        // Guardamos la pulsación para procesar el salto en FixedUpdate.
        if (jumpAction.WasPressedThisFrame())
        {
            saltoSolicitado = true;
        }

        // No cambiamos la orientación durante el impulso del wall jump.
        if (tiempoControlHorizontal <= 0f && spriteRenderer != null)
        {
            if (movimiento.x < -0.01f)
                spriteRenderer.flipX = true;
            else if (movimiento.x > 0.01f)
                spriteRenderer.flipX = false;
        }
    }

    void FixedUpdate()
    {
        // Durante el impulso del wall jump respetamos su velocidad horizontal.
        if (tiempoControlHorizontal > 0f)
        {
            tiempoControlHorizontal -= Time.fixedDeltaTime;
        }
        else
        {
            rb2d.linearVelocityX = movimiento.x * velocidad;
        }

        bool enSuelo = TocaSuelo();
        bool hayPared = TocaPared(out float direccionSaltoPared);

        // Salto normal o wall jump.
        if (saltoSolicitado)
        {
            if (enSuelo)
            {
                rb2d.linearVelocityY = 0f;

                rb2d.AddForce(
                    Vector2.up * salto,
                    ForceMode2D.Impulse
                );

                enSuelo = false;
            }
            else if (hayPared)
            {
                // La normal apunta hacia fuera de la pared.
                rb2d.linearVelocity = new Vector2(
                    direccionSaltoPared * fuerzaSaltoParedX,
                    fuerzaSaltoParedY
                );

                tiempoControlHorizontal = tiempoControlSaltoPared;

                if (spriteRenderer != null)
                    spriteRenderer.flipX = direccionSaltoPared < 0f;

                animator.SetTrigger("wallJump");

                enSuelo = false;
            }

            saltoSolicitado = false;
        }

        // Wall Slide: solo mientras estamos en el aire y descendiendo.
        bool deslizandoPared =
            !enSuelo &&
            hayPared &&
            rb2d.linearVelocity.y < 0f;

        if (deslizandoPared &&
            rb2d.linearVelocity.y < -velocidadDeslizamientoPared)
        {
            rb2d.linearVelocityY = -velocidadDeslizamientoPared;
        }

        // Animaciones.
        animator.SetBool("isGrounded", enSuelo);

        animator.SetBool(
            "isRunning",
            Mathf.Abs(movimiento.x) > 0.01f && enSuelo
        );

        animator.SetBool(
            "isFalling",
            !enSuelo &&
            !deslizandoPared &&
            rb2d.linearVelocity.y < -0.01f
        );

        animator.SetBool("isWalled", deslizandoPared);
    }

    bool TocaSuelo()
    {
        Vector2 tamano = new Vector2(
            con.bounds.size.x * 0.8f,
            0.05f
        );

        Vector2 origen = new Vector2(
            con.bounds.center.x,
            con.bounds.min.y + 0.02f
        );

        RaycastHit2D hit = Physics2D.BoxCast(
            origen,
            tamano,
            0f,
            Vector2.down,
            0.12f,
            layerSuelo
        );

        return hit.collider != null && hit.normal.y > 0.5f;
    }

    bool TocaPared(out float direccionSalto)
    {
        direccionSalto = 0f;

        float altura = con.bounds.center.y;

        // Raycast horizontal hacia la izquierda.
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

        // Raycast horizontal hacia la derecha.
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

