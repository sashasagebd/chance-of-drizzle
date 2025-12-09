using UnityEngine;
using UnityEngine.InputSystem; // << Input System
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController3D : MonoBehaviour
{
    [Header("Movement")]
    // PUBLIC: Configured via Unity Inspector for gameplay tuning
    public float maxSpeed = 15f;
    public float runSpeed = 4f;         // Running speed (default) - reduced from 6f
    public float sprintSpeed = 9f;      // Sprint speed (when holding shift)
    public float crouchSpeed = 2f;      // Crouching movement speed
    public float jumpSpeed = 5.5f;
    public float gravity = -9.81f;

    [Header("Camera Reference")]
    // PUBLIC: Assigned via Unity Inspector to link player movement with camera direction
    public Transform cam;             // Assign your Cinemachine virtual camera's follow target or main camera

    [Header("Crouch")]
    [SerializeField] private float standingHeight = 2f;       // Normal CharacterController height
    [SerializeField] private float crouchingHeight = 1f;      // Crouching CharacterController height
    [SerializeField] private float crouchTransitionSpeed = 10f; // How fast to transition between crouch states

    [Header("Weapon")]
    // PUBLIC: Accessed by AI system (AIPlayer) for enemy targeting and Inspector configuration
    public WeaponInventory inventory; // Kynan needs it for AIPlayer
    // PUBLIC: Accessed by AI system (AIPlayer) and weapon system for bullet spawn position
    public Transform muzzle;  // Fallback muzzle if weapon doesn't have one (Kynan needs it for AIPlayer)

    [Header("Animation")]
    [SerializeField] private PlayerAnimationController animationController;  // Reference to animation controller
    [SerializeField] private CharacterAiming characterAiming;  // Reference to character aiming (for rig weight control)

    CharacterController _controller;
    PlayerInput _playerInput;
    InputAction _moveAction, _jumpAction, _fireAction, _reloadAction, _nextAction, _prevAction, _sprintAction, _crouchAction;

    [Header("Items")]
    // PUBLIC: Accessed by an item system (Consumable, ItemPickup) to modify player health
    public Health HealthComponent; // needed right now for items to access health class easily
    // PUBLIC: Accessed by an item system to apply weapon modifications and buffs
    public WeaponBase WeaponComponent; // needed for items to access weapon base class easily
    private Coroutine speedTimer; // time for temp speed buffs
    private Coroutine jumpTimer; // time for temp jump buffs
    [SerializeField] private float baseDefense = 0;
    // PUBLIC: Read by Health class to calculate damage reduction from armor
    public float currentDefense { get; private set; } = 0;
    // PUBLIC: Accessed by item system (Armor) to manage equipped armor pieces
    public readonly Dictionary<string, Armor> equippedArmor = new Dictionary<string, Armor>();
    // PUBLIC: Static variable accessed by weapon system (Bullet, Grenade, LazerWeapon) to add bonus damage
    public static int damageBonus = 0; // additional damage from items

    Vector3 _velocity; // for gravity

    // Movement state
    private bool _isCrouching;
    private float _currentHeight;  // For smooth crouch transitions
    private Vector3 _initialCenter; // Store the initial center configuration

    // PUBLIC: Accessors for animation system (PlayerAnimationController) to sync animations with player state
    public bool IsCrouching => _isCrouching;
    public bool IsSprinting { get; private set; }
    public Vector3 CurrentVelocity => _controller != null ? _controller.velocity : Vector3.zero;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        // Cache actions by name (must match your asset)
        // Only set up input if PlayerInput component exists (allows for testing)
        if (_playerInput != null)
        {
            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];
            _fireAction = _playerInput.actions["Fire"];
            _reloadAction = _playerInput.actions["Reload"];
            _prevAction = _playerInput.actions["Previous"];
            _nextAction = _playerInput.actions["Next"];

            // Sprint and Crouch actions (will need to add these to Input Actions asset)
            _sprintAction = _playerInput.actions.FindAction("Sprint");
            _crouchAction = _playerInput.actions.FindAction("Crouch");
        }

        // Initialize crouch state - store the initial center configuration from Inspector
        // Only if CharacterController exists (allows for testing)
        if (_controller != null)
        {
            _initialCenter = _controller.center;
            _currentHeight = standingHeight;
            _controller.height = standingHeight;
        }

        if (cam == null)
        {
            // Try to auto-find main camera (Cinemachine will control it)
            cam = Camera.main?.transform;
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
        // Early return if required components are missing (allows for testing without full setup)
        if (_controller == null || _moveAction == null) return;

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

        // Rotate player to face movement direction (if moving)
        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

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
                // Use weapon's muzzle if available, otherwise fallback to player muzzle or camera
                Transform weaponMuzzle = weapon.muzzle ? weapon.muzzle : muzzle;
                Vector3 origin = weaponMuzzle ? weaponMuzzle.position : cam.position;
                Vector3 forward = cam ? cam.forward : transform.forward;
                bool didFire = weapon.TryFire(origin, forward);

                // Trigger shoot animation and aiming rig if weapon fired
                if (didFire)
                {
                    if (animationController != null)
                    {
                        animationController.TriggerShootAnimation();
                    }

                    if (characterAiming != null)
                    {
                        characterAiming.OnWeaponFired();
                    }
                }
            }

            if (_reloadAction.triggered)
                weapon.Reload();
        }
    }

    // PUBLIC: Called by item system (Consumable) to apply temporary or permanent speed buffs
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

    // PUBLIC: Called by item system (Consumable) to apply temporary or permanent jump height buffs
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


    // PUBLIC: Called by item system (ItemPickup) to equip armor and manage armor slots
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


