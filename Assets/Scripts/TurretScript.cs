using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class TurretScript : MonoBehaviour
{
  [Header("References")]

  [SerializeField]
  GameObject? trailPrefab;

  [Header("Rotations")]

  [Header("Elevation")]
  [Tooltip("Speed at which the turret's guns elevate up and down.")]
  public float ElevationSpeed = 30f;

  [Tooltip("Highest upwards elevation the turret's barrels can aim.")]
  public float MaxElevation = 60f;

  [Tooltip("Lowest downwards elevation the turret's barrels can aim.")]
  public float MaxDepression = 5f;

  [Header("Traverse")]

  [Tooltip("Speed at which the turret can rotate left/right.")]
  public float TraverseSpeed = 60f;

  [Tooltip("When true, the turret can only rotate horizontally with the given limits.")]
  [SerializeField] private bool hasLimitedTraverse = false;
  [Range(0f, 179f)] public float LeftLimit = 120f;
  [Range(0f, 179f)] public float RightLimit = 120f;

  [Header("Behavior")]

  [Tooltip("When idle, the turret does not aim at anything and simply points forwards.")]
  public bool IsIdle = false;

  // [Tooltip("Position the turret will aim at when not idle. Set this to whatever you want" +
  //     "the turret to actively aim at.")]
  // public Vector3 AimPosition = Vector3.zero;

  [Tooltip("When the turret is within this many degrees of the target, it is considered aimed.")]
  [SerializeField] private float aimedThreshold = 1f;
  private float limitedTraverseAngle = 0f;

  [Header("Debug")]
  public bool DrawDebugRay = true;
  public bool DrawDebugArcs = false;

  private float angleToTarget = 0f;
  private float elevation = 0f;

  private bool isAimed = false;
  private bool isBaseAtRest = false;
  private bool isBarrelAtRest = false;

  /// <summary>
  /// True when the turret cannot rotate freely in the horizontal axis.
  /// </summary>
  public bool HasLimitedTraverse { get { return hasLimitedTraverse; } }

  /// <summary>
  /// True when the turret is idle and at its resting position.
  /// </summary>
  public bool IsTurretAtRest { get { return isBarrelAtRest && isBaseAtRest; } }

  /// <summary>
  /// True when the turret is aimed at the given <see cref="AimPosition"/>. When the turret
  /// is idle, this is never true.
  /// </summary>
  public bool IsAimed { get { return isAimed; } }

  /// <summary>
  /// Angle in degress to the given <see cref="AimPosition"/>. When the turret is idle,
  /// the angle reports 999.
  /// </summary>
  public float AngleToTarget { get { return IsIdle ? 999f : angleToTarget; } }

  [SerializeField]
  [Min(0f)]
  float viewRange;

  List<GameObject> targets = new List<GameObject>();
  GameObject? target;
  Transform? headTransform;
  Transform? barrelTransform;

  void DebugDrawLineToTargets()
  {
    // TODO: Remove or improve method

    foreach (var target in targets)
    {
      if (target == null) continue;

      Debug.DrawLine(transform.position, target.transform.position);
    }
  }

  IEnumerator AutoUpdateTargetsCoroutine()
  {
    while (true)
    {
      UpdateTargets();

      // TODO: Variable retargetting delay
      yield return new WaitForSeconds(.2f);
    }
  }

  IEnumerator AutoShootingCoroutine()
  {
    while (true)
    {
      if (ShouldShoot())
      {
        Shoot(target!);

        // TODO: Variable firing rate
        yield return new WaitForSeconds(.2f);
      }
      else
      {
        yield return new WaitForEndOfFrame();
      }
    }
  }

  IEnumerator ShowProjectileTrailCoroutine(GameObject currentTarget)
  {
    if (trailPrefab != null && headTransform != null)
    {
      var trail = Instantiate(trailPrefab);

      var lineRenderer = trail.GetComponent<LineRenderer>();

      lineRenderer.SetPositions(new Vector3[] { headTransform.transform.position, currentTarget.transform.position });
      lineRenderer.enabled = true;
      lineRenderer.widthCurve = AnimationCurve.Constant(0f, 1f, .1f);

      // TODO: Variable trail despawn rate
      yield return new WaitForSeconds(.2f);

      // TODO: Pool trail
      Destroy(trail);
    }
  }

  void OnTargetDeath()
  {
    UpdateTargets();
  }

  bool ShouldShoot()
  {
    return IsAimed && target != null;
  }

  void UpdateTargets()
  {
    var allTargets = GameObject.FindGameObjectsWithTag("Enemy");

    var potentialTargets = allTargets.Where(v => (v.transform.position - transform.position).magnitude < viewRange);

    targets = potentialTargets.ToList();

    target = targets.Count == 0 ? null : targets.First();
  }

  void Shoot(GameObject currentTarget)
  {
    // TODO: Variable damage
    // currentTarget.SendMessage(DamageTakenMessage.message, new DamageTakenMessage { damage = 2f, source = gameObject });

    currentTarget.GetComponent<EnemyScript>()?.TakeDamage(20f);

    if (trailPrefab != null)
    {
      StartCoroutine(ShowProjectileTrailCoroutine(currentTarget));
    }
  }

  void RotateTurretToIdle(Transform currentHead)
  {
    // Rotate the base to its default position.
    if (hasLimitedTraverse)
    {
      limitedTraverseAngle = Mathf.MoveTowards(
          limitedTraverseAngle,
          0f,
          TraverseSpeed * Time.deltaTime
        );

      if (Mathf.Abs(limitedTraverseAngle) > Mathf.Epsilon)
      {
        currentHead.localEulerAngles = Vector3.up * limitedTraverseAngle;
      }
      else
      {
        isBaseAtRest = true;
      }
    }
    else
    {
      currentHead.rotation = Quaternion.RotateTowards(
          currentHead.rotation,
          transform.rotation,
          TraverseSpeed * Time.deltaTime
        );

      isBaseAtRest = Mathf.Abs(currentHead.localEulerAngles.y) < Mathf.Epsilon;
    }

    if (barrelTransform != null)
    {
      elevation = Mathf.MoveTowards(elevation, 0f, ElevationSpeed * Time.deltaTime);

      if (Mathf.Abs(elevation) > Mathf.Epsilon)
      {
        barrelTransform.localEulerAngles = Vector3.right * -elevation;
      }
      else
      {
        isBarrelAtRest = true;
      }
    }
    else
    {
      // Barrels automatically at rest if there are no barrels.
      isBarrelAtRest = true;
    }
  }

  float GetTurretAngleToTarget(Vector3 targetPosition, Transform currentHead)
  {
    var angle = 999f;

    if (barrelTransform != null)
    {
      angle = Vector3.Angle(targetPosition - barrelTransform.position, barrelTransform.forward);
    }
    else
    {
      var flattenedTarget = Vector3.ProjectOnPlane(
          targetPosition - currentHead.position,
          currentHead.up
        );

      angle = Vector3.Angle(
          flattenedTarget - currentHead.position,
          currentHead.forward
        );
    }

    return angle;
  }

  void RotateBarrelsToFaceTarget(Vector3 targetPosition, Transform currentHead, Transform currentBarrel)
  {
    var localTargetPos = currentHead.InverseTransformDirection(targetPosition - currentBarrel.position);
    var flattenedVecForBarrels = Vector3.ProjectOnPlane(localTargetPos, Vector3.up);

    var targetElevation = Vector3.Angle(flattenedVecForBarrels, localTargetPos);
    targetElevation *= Mathf.Sign(localTargetPos.y);

    targetElevation = Mathf.Clamp(targetElevation, -MaxDepression, MaxElevation);
    elevation = Mathf.MoveTowards(elevation, targetElevation, ElevationSpeed * Time.deltaTime);

    if (Mathf.Abs(elevation) > Mathf.Epsilon)
    {
      currentBarrel.localEulerAngles = Vector3.right * -elevation;
    }

#if UNITY_EDITOR
    if (DrawDebugRay)
    {
      Debug.DrawRay(currentBarrel.position, currentBarrel.forward * localTargetPos.magnitude, Color.red);
    }
#endif
  }

  void RotateBaseToFaceTarget(Vector3 targetPosition, Transform currentHead)
  {
    var turretUp = transform.up;

    var vecToTarget = targetPosition - currentHead.position;
    var flattenedVecForBase = Vector3.ProjectOnPlane(vecToTarget, turretUp);

    if (hasLimitedTraverse)
    {
      var turretForward = transform.forward;
      var targetTraverse = Vector3.SignedAngle(turretForward, flattenedVecForBase, turretUp);

      targetTraverse = Mathf.Clamp(targetTraverse, -LeftLimit, RightLimit);
      limitedTraverseAngle = Mathf.MoveTowards(
          limitedTraverseAngle,
          targetTraverse,
          TraverseSpeed * Time.deltaTime
        );

      if (Mathf.Abs(limitedTraverseAngle) > Mathf.Epsilon)
      {
        currentHead.localEulerAngles = Vector3.up * limitedTraverseAngle;
      }
    }
    else
    {
      if (currentHead.forward != Vector3.zero && flattenedVecForBase != Vector3.zero)
      {
        currentHead.rotation = Quaternion.RotateTowards(
            Quaternion.LookRotation(currentHead.forward, turretUp),
            Quaternion.LookRotation(flattenedVecForBase, turretUp),
            TraverseSpeed * Time.deltaTime
          );
      }
    }

#if UNITY_EDITOR
    if (DrawDebugRay && barrelTransform == null)
    {
      Debug.DrawRay(currentHead.position,
          currentHead.forward * flattenedVecForBase.magnitude,
          Color.red
        );
    }
#endif
  }


#if UNITY_EDITOR
  // This should probably go in an Editor script, but dealing with Editor scripts
  // is a pain in the butt so I'd rather not.
  void OnDrawGizmosSelected()
  {
    if (!DrawDebugArcs) return;

    if (headTransform != null)
    {
      const float kArcSize = 10f;
      var colorTraverse = new Color(1f, .5f, .5f, .1f);
      var colorElevation = new Color(.5f, 1f, .5f, .1f);
      var colorDepression = new Color(.5f, .5f, 1f, .1f);

      var arcRoot = barrelTransform != null ? barrelTransform : headTransform;

      // Red traverse arc
      UnityEditor.Handles.color = colorTraverse;
      if (hasLimitedTraverse)
      {
        UnityEditor.Handles.DrawSolidArc(
            arcRoot.position, headTransform.up,
            transform.forward, RightLimit,
            kArcSize
          );
        UnityEditor.Handles.DrawSolidArc(
            arcRoot.position, headTransform.up,
            transform.forward, -LeftLimit,
            kArcSize
          );
      }
      else
      {
        UnityEditor.Handles.DrawSolidArc(
            arcRoot.position, headTransform.up,
            transform.forward, 360f,
            kArcSize
          );
      }

      if (barrelTransform != null)
      {
        // Green elevation arc
        UnityEditor.Handles.color = colorElevation;
        UnityEditor.Handles.DrawSolidArc(
            barrelTransform.position, barrelTransform.right,
            headTransform.forward, -MaxElevation,
            kArcSize
          );

        // Blue depression arc
        UnityEditor.Handles.color = colorDepression;
        UnityEditor.Handles.DrawSolidArc(
            barrelTransform.position, barrelTransform.right,
            headTransform.forward, MaxDepression,
            kArcSize
          );
      }
    }
  }
#endif

  void Aim()
  {
    if (headTransform == null) return;

    if (IsIdle)
    {
      if (!IsTurretAtRest)
      {
        RotateTurretToIdle(headTransform);
      }

      isAimed = false;
    }
    else
    {
      if (target == null) return;

      var aimPosition = target.transform.position;

      RotateBaseToFaceTarget(aimPosition, headTransform);

      if (barrelTransform != null)
      {
        RotateBarrelsToFaceTarget(aimPosition, headTransform, barrelTransform);
      }

      // Turret is considered "aimed" when it's pointed at the target.
      angleToTarget = GetTurretAngleToTarget(aimPosition, headTransform);

      // Turret is considered "aimed" when it's pointed at the target.
      isAimed = angleToTarget < aimedThreshold;

      isBarrelAtRest = false;
      isBaseAtRest = false;
    }
  }

  private void Awake()
  {
    headTransform = transform.Find("Head");
    barrelTransform = headTransform?.Find("Barrel");
  }

  void Start()
  {
    StartCoroutine(AutoUpdateTargetsCoroutine());
    StartCoroutine(AutoShootingCoroutine());
  }

  void Update()
  {
    DebugDrawLineToTargets();

    Aim();
  }
}
