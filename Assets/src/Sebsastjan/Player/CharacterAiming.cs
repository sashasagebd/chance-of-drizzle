using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;

public class CharacterAiming : MonoBehaviour
{
    [SerializeField] private float turnSpeed = 15f;

    [Header("Rig Layers")]
    [SerializeField] private Rig weaponAimingRig;  // Reference to RigLayer_WeaponAiming

    [Header("Aiming Settings")]
    [SerializeField] private float aimTransitionSpeed = 5f;  // Speed of transition between pose and aim
    [SerializeField] private float returnToPoseDelay = 1f;   // Seconds after shooting before returning to pose

    private Camera mainCamera;
    private float targetWeight = 0f;  // Target weight for the aiming rig (0 = pose, 1 = aim)
    private Coroutine returnToPoseCoroutine;

    void Start()
    {
        mainCamera = Camera.main;

        // Initialize rig weight to 0 (pose position)
        if (weaponAimingRig != null)
        {
            weaponAimingRig.weight = 0f;
        }
    }

    void LateUpdate()
    {
        float yawCamera = mainCamera.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yawCamera, 0), turnSpeed * Time.deltaTime);

        // Smoothly interpolate rig weight
        if (weaponAimingRig != null)
        {
            weaponAimingRig.weight = Mathf.Lerp(weaponAimingRig.weight, targetWeight, Time.deltaTime * aimTransitionSpeed);
        }
    }

    /// <summary>
    /// Call this when the weapon is fired to transition to aiming pose
    /// </summary>
    public void OnWeaponFired()
    {
        // Instantly set weight to 1 when shooting starts
        targetWeight = 1f;
        if (weaponAimingRig != null)
        {
            weaponAimingRig.weight = 1f;
        }

        // Cancel any existing return to pose coroutine
        if (returnToPoseCoroutine != null)
        {
            StopCoroutine(returnToPoseCoroutine);
        }

        // Start new coroutine to return to pose after delay
        returnToPoseCoroutine = StartCoroutine(ReturnToPoseAfterDelay());
    }

    private IEnumerator ReturnToPoseAfterDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(returnToPoseDelay);

        // Transition back to pose position
        targetWeight = 0f;
        returnToPoseCoroutine = null;
    }
}
