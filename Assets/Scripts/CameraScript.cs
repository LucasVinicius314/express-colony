using UnityEngine;

#nullable enable

public class CameraScript : MonoBehaviour
{
  float cameraPitch = 0f;

  float cameraZoom = 0f;

  Transform? pitchTransform;
  Transform? cameraTransform;

  void HandleMouseDeltas()
  {
    var y = Input.GetAxis("Mouse X");
    var x = -Input.GetAxis("Mouse Y");

    var currentUnit = GameManagerScript.instance.currentUnit;

    if (currentUnit != null)
    {
      transform.position = currentUnit.transform.position;
    }

    transform.Rotate(Vector3.up * y);

    cameraPitch = Mathf.Clamp(cameraPitch + x, -90f, 90f);

    if (pitchTransform != null)
    {
      pitchTransform.localRotation = Quaternion.Euler(new Vector3 { x = cameraPitch });
    }
  }

  void HandleMouseScrollWheel()
  {
    var z = -Input.mouseScrollDelta.y / 10f;

    cameraZoom = Mathf.Clamp(cameraZoom + z, -1f, 1f);

    if (cameraTransform != null)
    {
      cameraTransform.localPosition = new Vector3 { y = 2.6f, z = -4.67f } + Vector3.back * cameraZoom * 4f;
    }
  }

  void Awake()
  {
    pitchTransform = transform.Find("CameraPitch");
    cameraTransform = transform.Find("CameraPitch/Camera");
  }

  void LateUpdate()
  {
    HandleMouseDeltas();

    HandleMouseScrollWheel();
  }
}
