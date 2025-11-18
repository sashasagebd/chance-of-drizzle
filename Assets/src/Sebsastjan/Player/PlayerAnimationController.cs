using UnityEngine;

/// <summary>
/// Advanced player animation controller with 8-way locomotion blend tree support
/// Handles idle, walk, run, sprint, crouch, jump, and shooting animations
/// Works with PlayerController3D to receive movement state
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    public PlayerController3D playerController;
    public CharacterController characterController;

    [Header("Animation Settings")]
    [Tooltip("Smoothing speed for velocity parameters")]
    public float velocityDampTime = 0.1f;

    [Tooltip("Speed threshold to trigger idle animation")]
    public float idleSpeedThreshold = 0.1f;

    [Tooltip("Duration of shoot animation overlay")]
    public float shootAnimationDuration = 0.3f;

    [Tooltip("Use aiming pose when shooting (true) or just trigger shoot animation (false)")]
    public bool aimWhenShooting = true;

    [Tooltip("Animation speed multiplier for running")]
    public float runAnimationSpeed = 1.25f;

    [Tooltip("Animation speed multiplier for sprinting")]
    public float sprintAnimationSpeed = 1.0f;

    private Animator _animator;
    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _isShooting;
    private float _shootTimer;
    private float _aimTimer;
    private float _aimDuration = 0.5f; // How long to stay in aim pose after shooting

    // Animator parameter hashes (for performance)
    private int _velocityXHash;
    private int _velocityZHash;
    private int _speedHash;
    private int _isGroundedHash;
    private int _isCrouchingHash;
    private int _isSprintingHash;
    private int _isAimingHash;
    private int _shootTriggerHash;

    // Smoothed velocity values for blend trees
    private float _smoothVelocityX;
    private float _smoothVelocityZ;
    private Vector3 _lastPosition;

    void Awake()
    {
        _animator = GetComponent<Animator>();

        // Auto-find references if not set
        if (playerController == null)
        {
            playerController = GetComponentInParent<PlayerController3D>();
        }

        if (characterController == null)
        {
            characterController = GetComponentInParent<CharacterController>();
        }

        // Cache animator parameter hashes
        _velocityXHash = Animator.StringToHash("VelocityX");
        _velocityZHash = Animator.StringToHash("VelocityZ");
        _speedHash = Animator.StringToHash("Speed");
        _isGroundedHash = Animator.StringToHash("isGrounded");
        _isCrouchingHash = Animator.StringToHash("isCrouching");
        _isSprintingHash = Animator.StringToHash("isSprinting");
        _isAimingHash = Animator.StringToHash("isAiming");
        _shootTriggerHash = Animator.StringToHash("Shoot");

        _lastPosition = transform.position;
    }

    void Update()
    {
        if (_animator == null || characterController == null || playerController == null)
            return;

        // Get movement state from CharacterController
        _isGrounded = characterController.isGrounded;

        // Calculate velocity relative to player's local space
        Vector3 worldVelocity = characterController.velocity;
        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

        // Smooth the velocity for cleaner blend tree transitions
        _smoothVelocityX = Mathf.Lerp(_smoothVelocityX, localVelocity.x, Time.deltaTime / velocityDampTime);
        _smoothVelocityZ = Mathf.Lerp(_smoothVelocityZ, localVelocity.z, Time.deltaTime / velocityDampTime);

        // Calculate overall speed
        float horizontalSpeed = new Vector3(worldVelocity.x, 0, worldVelocity.z).magnitude;

        // Normalize velocities for blend tree (blend trees typically work with -1 to 1 or 0 to 1 ranges)
        // We'll normalize based on max speed
        float normalizedVelX = Mathf.Clamp(_smoothVelocityX / playerController.sprintSpeed, -1f, 1f);
        float normalizedVelZ = Mathf.Clamp(_smoothVelocityZ / playerController.sprintSpeed, -1f, 1f);

        // Update animator parameters for blend trees
        _animator.SetFloat(_velocityXHash, normalizedVelX);
        _animator.SetFloat(_velocityZHash, normalizedVelZ);
        _animator.SetFloat(_speedHash, horizontalSpeed);

        // Update state parameters
        _animator.SetBool(_isGroundedHash, _isGrounded);
        _animator.SetBool(_isCrouchingHash, playerController.IsCrouching);
        bool isSprinting = playerController.IsSprinting;
        _animator.SetBool(_isSprintingHash, isSprinting);

        // Control animation speed based on movement state
        // Note: Set speed AFTER setting parameters to avoid interfering with transitions
        if (isSprinting)
        {
            _animator.speed = sprintAnimationSpeed;
        }
        else if (horizontalSpeed > idleSpeedThreshold && !playerController.IsCrouching)
        {
            // Running - use increased animation speed
            _animator.speed = runAnimationSpeed;
        }
        else
        {
            // Idle or crouching - normal speed
            _animator.speed = 1.0f;
        }

        // Handle aiming state (aiming when shooting only)
        if (aimWhenShooting && _isShooting)
        {
            _aimTimer = _aimDuration;
        }

        bool isAiming = _aimTimer > 0f;
        _animator.SetBool(_isAimingHash, isAiming);

        if (_aimTimer > 0f)
        {
            _aimTimer -= Time.deltaTime;
        }

        // Detect landing (transition from air to ground)
        if (_isGrounded && !_wasGrounded)
        {
            // Landing detected - could trigger landing animation/effect here
        }

        _wasGrounded = _isGrounded;

        // Handle shoot animation timer
        if (_isShooting)
        {
            _shootTimer -= Time.deltaTime;
            if (_shootTimer <= 0)
            {
                _isShooting = false;
            }
        }
    }

    /// <summary>
    /// Call this method when the player shoots
    /// </summary>
    public void TriggerShootAnimation()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(_shootTriggerHash);
            _isShooting = true;
            _shootTimer = shootAnimationDuration;
        }
    }

    /// <summary>
    /// Gets current animation state for debugging
    /// </summary>
    public string GetCurrentState()
    {
        if (_animator == null) return "No Animator";

        if (!_isGrounded)
            return "Jumping/Falling";

        if (playerController.IsCrouching)
            return "Crouching";

        if (_isShooting)
            return "Shooting";

        if (playerController.IsSprinting)
            return "Sprinting";

        float speed = _animator.GetFloat(_speedHash);
        if (speed > idleSpeedThreshold)
            return "Moving";

        return "Idle";
    }

    /// <summary>
    /// Debug visualization in Scene view
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (characterController == null) return;

        // Draw velocity direction
        Vector3 velocity = characterController.velocity;
        if (velocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position + Vector3.up, velocity.normalized * 2f);
        }
    }
}
