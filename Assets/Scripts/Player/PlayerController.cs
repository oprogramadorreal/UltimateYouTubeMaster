using UnityEngine;

[RequireComponent(typeof(Planet))]
[RequireComponent(typeof(Player))]
public sealed class PlayerController : MonoBehaviour
{
    private Planet planet;
    private Player player;
    private Rigidbody playerRigidbody;

    private void Awake()
    {
        planet = GetComponent<Planet>();
        player = GetComponent<Player>();
        playerRigidbody = GetComponentInChildren<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!planet.CanUpdate())
        {
            return;
        }

        if (player.HasCurrentActiveCamera())
        {
            MoveWithKeyboard();
            MoveWithTouch();
        }
    }

    private void MoveWithTouch()
    {
        var camTransform = Camera.main.transform;

        for (var i = 0; i < Input.touchCount; ++i)
        {
            Vector3 touchPosition = Input.touches[i].position;
            touchPosition.z = Vector3.Dot(transform.position - camTransform.position, camTransform.forward);
            touchPosition = Camera.main.ScreenToWorldPoint(touchPosition);
            touchPosition = Vector3.ProjectOnPlane(touchPosition, Vector3.forward);
            MovePlayer(10 * Vector3.Normalize(touchPosition - transform.position));
        }
    }

    private void MoveWithKeyboard()
    {
        // TODO: use camera direction

        MovePlayer(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f));
    }

    private void MovePlayer(Vector3 direction)
    {
        playerRigidbody.drag = 0.0f;
        player.Move(direction);
    }
}
