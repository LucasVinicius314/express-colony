using UnityEngine;

#nullable enable

public class ProjectileScript : MonoBehaviour
{
  [SerializeField]
  float impulse = 10f;

  Rigidbody? rb;

  System.Collections.IEnumerator DestroyCoroutine()
  {
    yield return new WaitForSeconds(10f);

    Destroy(gameObject);
  }

  void Awake()
  {
    rb = GetComponent<Rigidbody>();
  }

  void OnCollisionEnter(Collision collision)
  {
    var target = collision.gameObject;

    // TODO: fix, extract tags
    if (target.tag == "Enemy")
    {
      target.transform.parent.GetComponent<EnemyScript>()?.TakeDamage(20f);

      Destroy(gameObject);
    }
  }

  void Start()
  {
    StartCoroutine(DestroyCoroutine());

    if (rb != null)
    {
      rb.AddForce(transform.forward * impulse, ForceMode.Impulse);
    }
  }
}
