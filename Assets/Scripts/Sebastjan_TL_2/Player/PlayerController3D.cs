using UnityEngine;
using UnityEngine.InputSystem; // << Input System
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController3D : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 15f;
    public float runSpeed = 4f;         // Running speed (default) - reduced from 6f
    public float sprintSpeed = 9f;      // Sprint speed (when holding shift)
    public float crouchSpeed = 2f;      // Crouching movement speed
    public float jumpSpeed = 5.5f;
    public float gravity = -9.81f;

    [Header("Look")]
    public Transform cam;             // drag your camera here (NOT a child for TPS)
    public float lookSensitivity = 0.12f;   // tweak to taste
    public float minPitch = -80f, maxPitch = 80f;

    [Header("Crouch")]
    public float standingHeight = 2f;       // Normal CharacterController height
    public float crouchingHeight = 1f;      // Crouching CharacterController height
    public float crouchTransitionSpeed = 10f; // How fast to transition between crouch states

    [Header("Third-Person Camera")]
    public float cameraDistance = 2.75f;       // Distance behind player
    public float cameraHeight = 1f;          // Height above player
    public LayerMask cameraCollisionMask = -1;  // What camera collides with
    public float cameraCollisionBuffer = 0.2f;  // Extra space from walls

    [Header("Weapon")]
    public WeaponInventory inventory;
    public Transform muzzle;

    [Header("Animation")]
    public PlayerAnimationController animationController;  // Reference to animation controller

    CharacterController _controller;
    PlayerInput _playerInput;
    InputAction _moveAction, _lookAction, _jumpAction, _fireAction, _reloadAction, _nextAction, _prevAction, _sprintAction, _crouchAction;

    [Header("Items")]
    public Health HealthComponent; // needed right now for items to access health class easily
    public WeaponBase WeaponComponent; // needed for items to access weapon base class easily
    private Coroutine speedTimer; // time for temp speed buffs
    private Coroutine jumpTimer; // time for temp jump buffs
    public float baseDefense = 0;
    public float currentDefense { get; private set; } = 0;
    public readonly Dictionary<string, Armor> equippedArmor = new Dictionary<string, Armor>();
    public static int damageBonus = 0; // additional damage from items

    Vector3 _velocity; // for gravity
    float _pitch;      // camera pitch
    float _yaw;        // camera yaw (horizontal orbit angle)

    // Movement state
    private bool _isCrouching;
    private float _currentHeight;  // For smooth crouch transitions
    private Vector3 _initialCenter; // Store the initial center configuration

    // Public accessors for animation system
    public bool IsCrouching => _isCrouching;
    public bool IsSprinting { get; private set; }
    public Vector3 CurrentVelocity => _controller.velocity;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        // Cache actions by name (must match your asset)
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _jumpAction = _playerInput.actions["Jump"];
        _fireAction = _playerInput.actions["Fire"];
        _reloadAction = _playerInput.actions["Reload"];
        _prevAction = _playerInput.actions["Previous"];
        _nextAction = _playerInput.actions["Next"];

        // Sprint and Crouch actions (will need to add these to Input Actions asset)
        _sprintAction = _playerInput.actions.FindAction("Sprint");
        _crouchAction = _playerInput.actions.FindAction("Crouch");

        // Initialize crouch state - store the initial center configuration from Inspector
        _initialCenter = _controller.center;
        _currentHeight = standingHeight;
        _controller.height = standingHeight;

        if (cam == null)
        {
            // Try to auto-find a child camera or main camera
            var childCam = GetComponentInChildren<Camera>();
            cam = childCam ? childCam.transform : Camera.main?.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (HealthComponent == null)
        {
            HealthComponent = GetComponent<Health>();
        }

    }

    void Update()
    {
        // ----- Crouch Input -----
        if (_crouchAction != null && _crouchAction.triggered)
        {
            _isCrouching = !_isCrouching;
        }

        // Smoothly transition CharacterController height
        float targetHeight = _isCrouching ? crouchingHeight : standingHeight;
        _currentHeight = Mathf.Lerp(_currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        _controller.height = _currentHeight;

        // Adjust center to keep bottom of capsule at the same position (maintain initial setup)
        // Calculate where the bottom of the capsule should be based on initial configuration
        float initialBottom = _initialCenter.y - standingHeight / 2f;
        // Adjust center Y to maintain that bottom position with the new height
        float newCenterY = initialBottom + _currentHeight / 2f;
        _controller.center = new Vector3(_initialCenter.x, newCenterY, _initialCenter.z);

        // ----- Third-Person Camera Orbit (mouse/gamepad) -----
        Vector2 look = _lookAction.ReadValue<Vector2>();
        const float mouseScale = 0.12f;
        float yawDelta = look.x * lookSensitivity * mouseScale;
        float pitchDelta = look.y * lookSensitivity * mouseScale;

        // Update camera orbit angles
        _yaw += yawDelta;
        _pitch = Mathf.Clamp(_pitch - pitchDelta, minPitch, maxPitch);

        // ----- Move (WASD/left stick) relative to camera -----
        Vector2 move = _moveAction.ReadValue<Vector2>();
        Vector3 input = new Vector3(move.x, 0f, move.y);

        // Get camera's forward and right directions (flattened to horizontal plane)
        Vector3 camForward = cam ? cam.forward : transform.forward;
        Vector3 camRight = cam ? cam.right : transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // ----- Determine Movement Speed (crouch, walk, run, or sprint) -----
        bool isSprinting = _sprintAction != null && _sprintAction.IsPressed();
        IsSprinting = isSprinting && !_isCrouching; // Can't sprint while crouching

        // Debug sprint input - only log when shift is pressed or sprint action is null
        if (_sprintAction == null)
        {
            Debug.LogWarning("Sprint Action is NULL! Check if 'Sprint' action exists in Input Actions asset.");
        }
        else if (_sprintAction.IsPressed() || IsSprinting)
        {
            Debug.Log($"=== SPRINT INPUT DEBUG ===");
            Debug.Log($"Sprint Action IsPressed: {_sprintAction.IsPressed()}");
            Debug.Log($"IsCrouching: {_isCrouching}");
            Debug.Log($"Final IsSprinting Property: {IsSprinting}");
        }

        float currentMoveSpeed;
        if (_isCrouching)
        {
            currentMoveSpeed = crouchSpeed;
        }
        else if (IsSprinting)
        {
            currentMoveSpeed = sprintSpeed;
        }
        else
        {
            currentMoveSpeed = runSpeed; // Default running speed
        }

        // Calculate movement direction relative to camera
        Vector3 moveDir = (camForward * input.z + camRight * input.x).normalized * currentMoveSpeed;

        // Rotate player to face camera direction (mouse yaw)
        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);

        // ----- Jump + Gravity -----
        _velocity.y += gravity * Time.deltaTime;

        bool grounded = _controller.isGrounded;

        if (grounded && _velocity.y < 0f)
            _velocity.y = -2f;

        if (grounded && _jumpAction.triggered)
            _velocity.y = jumpSpeed;

        // Combine horizontal + vertical movement
        Vector3 motion = (moveDir + _velocity) * Time.deltaTime;
        CollisionFlags flags = _controller.Move(motion);

        if ((flags & CollisionFlags.Below) != 0 && _velocity.y < 0f)
            _velocity.y = -2f;

        // ----- Third-Person Camera Positioning -----
        UpdateCameraPosition();

        // ----- Weapon Switching -----
        if (inventory)
        {
            if (_nextAction.triggered) inventory.Next();
            if (_prevAction.triggered) inventory.Prev();
        }

        // ----- Weapon Fire / Reload -----
        var weapon = inventory ? inventory.Current : null;
        if (weapon)
        {
            if (_fireAction.IsPressed())
            {
                Vector3 origin = muzzle ? muzzle.position : cam.position;
                Vector3 forward = cam ? cam.forward : transform.forward;
                bool didFire = weapon.TryFire(origin, forward);

                // Trigger shoot animation if weapon fired and animation controller exists
                if (didFire && animationController != null)
                {
                    animationController.TriggerShootAnimation();
                }
            }

            if (_reloadAction.triggered)
                weapon.Reload();
        }
    }

    /// <summary>
    /// Updates the third-person camera position with collision detection
    /// </summary>
    void UpdateCameraPosition()
    {
        if (!cam) return;

        // Calculate desired camera position based on orbit angles
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -cameraDistance);
        Vector3 targetPosition = transform.position + Vector3.up * cameraHeight + offset;

        // Perform collision detection (raycast from player to camera)
        Vector3 rayOrigin = transform.position + Vector3.up * cameraHeight;
        Vector3 rayDirection = targetPosition - rayOrigin;
        float rayDistance = rayDirection.magnitude;

        if (Physics.Raycast(rayOrigin, rayDirection.normalized, out RaycastHit hit, rayDistance, cameraCollisionMask))
        {
            // Camera would collide with something - move it closer
            targetPosition = hit.point + hit.normal * cameraCollisionBuffer;
        }

        // Set camera position instantly for responsive aiming
        cam.position = targetPosition;

        // Make camera look at player (with height offset for better framing)
        Vector3 lookTarget = transform.position + Vector3.up * cameraHeight;
        cam.LookAt(lookTarget);
    }

    public void ApplySpeed(float amount, int duration)
    {
        if (speedTimer != null)
        {
            StopCoroutine(speedTimer);
            speedTimer = null;
        }

        // Apply speed buff to all movement speeds
        runSpeed += amount;
        sprintSpeed += amount;
        Debug.Log($"Speed was increased by {amount} for a total run speed of {runSpeed}");

        if (duration > 0) // If duration is 0 then permanent speed buff
        {
            speedTimer = StartCoroutine(SpeedBuffCoroutine(amount, duration));
        }
    }

    private IEnumerator SpeedBuffCoroutine(float amount, int duration)
    {
        // Wait for the duration time
        yield return new WaitForSeconds(duration);

        // Remove speed buff from all movement speeds
        runSpeed -= amount;
        sprintSpeed -= amount;

        Debug.Log($"Temporary speed boost expired. Total run speed reset to {runSpeed}.");

        speedTimer = null;
    }

    public void ApplyJumpBoost(float amount, int duration)
    {
        if (jumpTimer != null)
        {
            StopCoroutine(jumpTimer);
            jumpTimer = null;
        }
        
        jumpSpeed += amount;
        Debug.Log($"Jumping speed was increased by {amount} for a total speed of {jumpSpeed}");

        if (duration > 0) // If duration is 0 then permanent jump buff
        {
            jumpTimer = StartCoroutine(JumpBuffCoroutine(amount, duration));
        }
    }

    private IEnumerator JumpBuffCoroutine(float amount, int duration)
    {
        // Wait for the duration time
        yield return new WaitForSeconds(duration);

        jumpSpeed -= amount;

        Debug.Log($"Temporary jump speed boost expired. Total jump speed reset to {jumpSpeed}.");

        jumpTimer = null;
    }


    public bool EquipArmor(Armor newArmor, out Armor replacedArmor)
    {
        replacedArmor = null;
        string type = newArmor.ArmorType;

        if (equippedArmor.TryGetValue(type, out Armor oldArmor))
        {
            if (newArmor.Defense <= oldArmor.Defense)
            {
                // Worse or equal — do not equip
                return false;
            }

            // Better — replace
            replacedArmor = oldArmor;
            equippedArmor[type] = newArmor;
            CalculateDefense();
            Debug.Log($"Replaced {oldArmor.Name} with {newArmor.Name}. Total defense: {currentDefense}");
            return true;
        }
        else
        {
            // No armor yet — equip new
            equippedArmor[type] = newArmor;
            CalculateDefense();
            Debug.Log($"Equipped new {newArmor.Name}. Total defense: {currentDefense}");
            return true;
        }
    }
    
    private void CalculateDefense()
    {
        float total = baseDefense;
        foreach (var armor in equippedArmor.Values)
        {
            total += armor.Defense;
        }
        currentDefense = total;
        Debug.Log($"Total defense: {currentDefense}");
    }

}


