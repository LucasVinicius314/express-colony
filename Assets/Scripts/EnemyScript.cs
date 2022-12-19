using UnityEngine;

#nullable enable

public class EnemyScript : MonoBehaviour
{
  float hp = 100f;

  public bool TakeDamage(float damage)
  {
    var newHp = hp - damage;

    if (newHp <= 0)
    {
      Destroy(gameObject);

      return true;
    }
    else
    {
      hp = newHp;
    }

    GameManagerScript.instance.PlayHitMarkerSoundEffect();

    return false;
  }
}
