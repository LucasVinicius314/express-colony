using UnityEngine;

#nullable enable

public class UnitScript : MonoBehaviour
{
  [SerializeField]
  UnitType unitType;

  AirMovementScript? airMovementScript;

  void Awake()
  {
    airMovementScript = GetComponent<AirMovementScript>();
  }

  void Update()
  {
    var y = 0f;

    if (Input.GetKey(KeyCode.Space))
    {
      y += 1f;
    }

    if (Input.GetKey(KeyCode.C))
    {
      y -= 1f;
    }

    var input = new Vector3
    {
      x = Input.GetAxis("Horizontal"),
      y = y,
      z = Input.GetAxis("Vertical"),
    };

    switch (unitType)
    {
      case UnitType.air:
        if (airMovementScript != null)
        {
          airMovementScript.Move(Vector3.ClampMagnitude(input, 1f));
        }
        break;
    }
  }
}
