using UnityEngine;

#nullable enable

public class AirMovementScript : MonoBehaviour
{
  [SerializeField]
  [Min(0f)]
  float speed = 1f;

  [SerializeField]
  [Min(0f)]
  float turnRate = .5f;

  Rigidbody? rb;

  public void Move(Vector3 relativeVector)
  {
    var cameraTransform = GameManagerScript.instance.mainCamera?.transform;

    if (rb == null || cameraTransform == null)
    {
      return;
    }

    var newVector = cameraTransform.forward * relativeVector.z
      + cameraTransform.right * relativeVector.x
      + transform.up * relativeVector.y;

    rb.AddForce(newVector * speed);

    var rbRot = rb.rotation;

    var rbVel = new Vector3
    {
      x = rb.velocity.x,
      z = rb.velocity.z,
    };

    if (rbVel.magnitude > .1f)
    {
      var direction = Vector3.SignedAngle(-rbVel, transform.forward, Vector3.up);

      if (Mathf.Abs(direction) < 178f)
      {
        rb.MoveRotation(Quaternion.Euler(0f, rb.rotation.eulerAngles.y + Mathf.Clamp(direction, -turnRate, turnRate), 0f));
      }
    }
  }

  void Awake()
  {
    rb = GetComponent<Rigidbody>();
  }
}
