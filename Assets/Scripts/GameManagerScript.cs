using UnityEngine;

#nullable enable

public class GameManagerScript : MonoBehaviour
{
  static public GameManagerScript instance = default!;

  public Camera? mainCamera;
  public AudioSource? audioSource;
  public GameObject? currentUnit;

  [SerializeField]
  AudioClip? hitMarkerAudioClip;

  public void PlayHitMarkerSoundEffect()
  {
    if (audioSource != null && hitMarkerAudioClip != null)
    {
      audioSource.PlayOneShot(hitMarkerAudioClip, .05f);
    }
  }

  void Awake()
  {
    instance = this;

    audioSource = GetComponent<AudioSource>();

    currentUnit = GameObject.Find("MainUnit");

    mainCamera = GameObject.Find("Camera").GetComponent<Camera>();
  }

  void Start()
  {
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
  }
}
