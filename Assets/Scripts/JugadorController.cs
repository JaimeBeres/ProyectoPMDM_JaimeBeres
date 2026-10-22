using UnityEngine;
using UnityEngine.InputSystem;

public class JugadorController : MonoBehaviour
{
    [SerializeField]
    int velocidad;
    [SerializeField]
    int salto;
    InputAction moveAction;
    InputAction jumpAction;
    Rigidbody2D rb2d;
    CapsuleCollider2D con;
    Animator animator;
    int layerSuelo;
    SpriteRenderer spriteRenderer;
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        rb2d = GetComponent<Rigidbody2D>();
        con = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        layerSuelo = LayerMask.GetMask("Suelo");
        animator = GetComponent<Animator>();
    }
     bool TocaSuelo()
    {
        return Physics2D.BoxCast(
            con.bounds.center,
            con.bounds.size,
            0f,
            Vector2.down,
            0.1f,
            layerSuelo
        );
    }
    void Update()
    {
        Vector2 movimiento = moveAction.ReadValue<Vector2>();
        if (movimiento.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (movimiento.x > 0)
        {
            spriteRenderer.flipX = false;
        }

        rb2d.linearVelocityX = movimiento.x * velocidad;

        if (jumpAction.IsPressed() && TocaSuelo())
        {
            rb2d.AddForce(new Vector2(0, salto));
        }
        animator.SetBool("isGrounded", TocaSuelo());
    }

   
    
}

