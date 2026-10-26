using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class GrapplingHook2D : MonoBehaviour
{
    private enum EstadoGancho
    {
        Suelto,
        Balanceando,
        Retrayendo
    }

    [Header("Apuntado y disparo")]
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private Transform cursorMundo;
    [SerializeField] private float alcance = 12f;

    [Header("Cuerda")]
    [SerializeField] private float velocidadRetraccion = 8f;
    [SerializeField] private float longitudMinima = 0.75f;
    [SerializeField] private float grosorCuerda = 0.04f;

    private Rigidbody2D rb2d;
    private DistanceJoint2D jointGancho;
    private LineRenderer cuerda;
    private JugadorController jugadorController;
    private Camera camara;

    private EstadoGancho estado = EstadoGancho.Suelto;
    private Vector2 puntoAnclaje;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        cuerda = GetComponent<LineRenderer>();
        jugadorController = GetComponent<JugadorController>();
        camara = Camera.main;

        if (puntoDisparo == null)
            puntoDisparo = transform;

        // Crear o reutilizar la articulación física del gancho.
        jointGancho = GetComponent<DistanceJoint2D>();

        if (jointGancho == null)
        {
            jointGancho = gameObject.AddComponent<DistanceJoint2D>();
        }

        jointGancho.enabled = false;
        jointGancho.autoConfigureConnectedAnchor = false;
        jointGancho.autoConfigureDistance = false;
        jointGancho.maxDistanceOnly = true;
        jointGancho.connectedBody = null;

        // Preparar la cuerda visual.
        cuerda.positionCount = 2;
        cuerda.useWorldSpace = true;
        cuerda.startWidth = grosorCuerda;
        cuerda.endWidth = grosorCuerda;
        cuerda.enabled = false;

        // Ocultar el cursor del sistema solo si usamos uno personalizado.
        Cursor.visible = cursorMundo == null;
    }

    void OnDisable()
    {
        // Devolver el control si se desactiva el componente.
        if (jugadorController != null)
            jugadorController.EstablecerControlGancho(false);

        if (jointGancho != null)
            jointGancho.enabled = false;

        if (cuerda != null)
            cuerda.enabled = false;

        Cursor.visible = true;
    }

    void Update()
    {
        if (Mouse.current == null || camara == null)
            return;

        Vector2 ratonMundo = ObtenerPosicionRaton();

        // Actualizar el cursor personalizado.
        if (cursorMundo != null)
        {
            Vector3 posicionCursor = cursorMundo.position;
            posicionCursor.x = ratonMundo.x;
            posicionCursor.y = ratonMundo.y;
            cursorMundo.position = posicionCursor;
        }

        // Al pulsar el botón izquierdo, intentar engancharse.
        if (Mouse.current.leftButton.wasPressedThisFrame &&
            estado == EstadoGancho.Suelto)
        {
            IntentarEnganchar(ratonMundo);
        }

        // Al soltar el botón, comenzar la retracción.
        if (Mouse.current.leftButton.wasReleasedThisFrame &&
            estado == EstadoGancho.Balanceando)
        {
            estado = EstadoGancho.Retrayendo;
        }
    }

    Vector2 ObtenerPosicionRaton()
    {
        Vector2 posicionPantalla =
            Mouse.current.position.ReadValue();

        float distanciaZ =
            puntoDisparo.position.z - camara.transform.position.z;

        Vector3 posicionMundo = camara.ScreenToWorldPoint(
            new Vector3(
                posicionPantalla.x,
                posicionPantalla.y,
                distanciaZ
            )
        );

        return new Vector2(
            posicionMundo.x,
            posicionMundo.y
        );
    }

    void IntentarEnganchar(Vector2 objetivo)
    {
        Vector2 origen = puntoDisparo.position;
        Vector2 direccion = objetivo - origen;

        float distancia = direccion.magnitude;

        if (distancia <= 0.01f)
            return;

        // El rayo nunca supera el alcance máximo.
        float distanciaDisparo = Mathf.Min(distancia, alcance);

        // Detectar todos los colliders de la trayectoria,
        // sin utilizar una capa especial.
        RaycastHit2D[] impactos = Physics2D.RaycastAll(
            origen,
            direccion.normalized,
            distanciaDisparo
        );

        // Ordenar desde el objeto más cercano.
        System.Array.Sort(
            impactos,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit2D impacto in impactos)
        {
            if (impacto.collider == null)
                continue;

            // Ignorar triggers y colliders del propio jugador.
            if (impacto.collider.isTrigger)
                continue;

            if (impacto.collider.attachedRigidbody == rb2d ||
                impacto.collider.transform == transform ||
                impacto.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            // Ya tenemos un punto de anclaje válido.
            puntoAnclaje = impacto.point;

            // El anclaje del joint coincide con el punto desde
            // donde sale la cuerda en el personaje.
            Vector3 anclaLocal3D =
                transform.InverseTransformPoint(puntoDisparo.position);

            jointGancho.anchor = new Vector2(
                anclaLocal3D.x,
                anclaLocal3D.y
            );

            jointGancho.connectedBody = null;
            jointGancho.connectedAnchor = puntoAnclaje;

            jointGancho.distance = Mathf.Max(
                0.01f,
                Vector2.Distance(
                    (Vector2)puntoDisparo.position,
                    puntoAnclaje
                )
            );

            jointGancho.enabled = true;

            estado = EstadoGancho.Balanceando;

            // El gancho toma temporalmente el control del jugador.
            jugadorController?.EstablecerControlGancho(true);

            return;
        }
    }

    void FixedUpdate()
    {
        if (estado == EstadoGancho.Suelto)
            return;

        // Mientras retraemos, acortamos la longitud máxima de la cuerda.
        if (estado == EstadoGancho.Retrayendo)
        {
            jointGancho.distance = Mathf.MoveTowards(
                jointGancho.distance,
                longitudMinima,
                velocidadRetraccion * Time.fixedDeltaTime
            );

            // Al terminar de retraer, liberar al personaje.
            if (jointGancho.distance <= longitudMinima + 0.001f)
            {
                Desenganchar();
            }
        }
    }

    void LateUpdate()
    {
        ActualizarCuerda();
    }

    void ActualizarCuerda()
    {
        if (cuerda == null)
            return;

        if (estado == EstadoGancho.Suelto)
        {
            cuerda.enabled = false;
            return;
        }

        cuerda.enabled = true;

        cuerda.SetPosition(0, puntoDisparo.position);
        cuerda.SetPosition(1, puntoAnclaje);
    }

    void Desenganchar()
    {
        estado = EstadoGancho.Suelto;

        if (jointGancho != null)
            jointGancho.enabled = false;

        if (cuerda != null)
            cuerda.enabled = false;

        jugadorController?.EstablecerControlGancho(false);
    }
}