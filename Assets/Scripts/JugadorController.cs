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

    int layerSuelo;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        rb2d = GetComponent<Rigidbody2D>();
        con = GetComponent<CapsuleCollider2D>();

        layerSuelo = LayerMask.GetMask("Suelo");
    }

    void Update()
    {
        Vector2 movimiento = moveAction.ReadValue<Vector2>();

        rb2d.linearVelocityX = movimiento.x * velocidad;

        if (jumpAction.IsPressed() && TocaSuelo())
        {
            rb2d.AddForce(new Vector2(0, salto));
        }
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
    
}

