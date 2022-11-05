using UnityEngine;

#nullable enable

public class AirMovementScript : MonoBehaviour
{
  [SerializeField]
  [Min(0f)]
  float speed = 1f;

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
  }

  void Awake()
  {
    rb = GetComponent<Rigidbody>();
  }
}
