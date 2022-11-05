using UnityEngine;

#nullable enable

public class GameManagerScript : MonoBehaviour
{
  static public GameManagerScript instance = default!;

  public Camera? mainCamera;

  void Awake()
  {
    instance = this;

    mainCamera = GameObject.Find("Camera").GetComponent<Camera>();
  }
}
