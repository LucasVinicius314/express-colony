using UnityEngine;

#nullable enable

public class GameManagerScript : MonoBehaviour
{
  static public GameManagerScript instance = default!;

  public Camera? mainCamera;
  public GameObject? currentUnit;

  void Awake()
  {
    instance = this;

    currentUnit = GameObject.Find("MainUnit");

    mainCamera = GameObject.Find("Camera").GetComponent<Camera>();
  }

  void Start()
  {
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
  }
}
