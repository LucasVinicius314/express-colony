using UnityEngine;

#nullable enable

public class EnemyScript : MonoBehaviour
{
  float hp = 100f;

  public void TakeDamage(float damage)
  {
    var newHp = hp - damage;

    if (newHp <= 0)
    {
      Destroy(gameObject);
    }
    else
    {
      hp = newHp;
    }

    GameManagerScript.instance.PlayHitMarkerSoundEffect();
  }
}
