using UnityEngine;

#nullable enable

public class CameraScript : MonoBehaviour
{
  float cameraPitch = 0f;

  float cameraZoom = 0f;

  Transform? pitchTransform;
  Transform? cameraTransform;
  [SerializeField]
  GameObject? turretPrefab;
  GameObject? turret;

  void HandleBuildOverlay()
  {
    RaycastHit hit;

    var camera = GameManagerScript.instance?.mainCamera;

    if (camera == null)
    {
      return;
    }

    var ray = camera.ScreenPointToRay(new Vector2
    {
      x = Screen.width / 2,
      y = Screen.height / 2,
    });

    Debug.DrawRay(camera.transform.position, camera.transform.forward * 20f, Color.green);

    if (Physics.Raycast(ray, out hit, 20f, 0b10000000))
    {
      turret?.SetActive(true);
      turret?.transform?.SetPositionAndRotation(hit.point, Quaternion.identity);
    }
    else
    {
      turret?.SetActive(false);
    }
  }

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

  void Start()
  {
    if (turretPrefab != null)
    {
      turret = Instantiate(turretPrefab);

      foreach (var meshRenderer in turret.GetComponentsInChildren<MeshRenderer>())
      {
        meshRenderer.material = GameManagerScript.instance?.semiTransparentMat;
      }

      turret.SetActive(false);
    }
  }

  void LateUpdate()
  {
    HandleMouseDeltas();

    HandleMouseScrollWheel();

    HandleBuildOverlay();
  }
}
