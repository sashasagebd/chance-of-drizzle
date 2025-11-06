using UnityEngine;
using UnityEngine.InputSystem; // << Input System
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController3D : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 15f;
    public float moveSpeed = 6f;
    public float jumpSpeed = 5.5f;
    public float gravity = -9.81f;

    [Header("Look")]
    public Transform cam;             // drag your camera here (child of Player)
    public float lookSensitivity = 0.12f;   // tweak to taste
    public float minPitch = -80f, maxPitch = 80f;

    [Header("Weapon")]
    public WeaponInventory inventory;
    public Transform muzzle;

    CharacterController _controller;
    PlayerInput _playerInput;
    InputAction _moveAction, _lookAction, _jumpAction, _fireAction, _reloadAction, _nextAction, _prevAction;

    [Header("Items")]
    public Health HealthComponent; // needed right now for items to access health class easily
    public WeaponBase WeaponComponent; // needed for items to access weapon base class easily
    private Coroutine speedTimer; // time for temp speed buffs
    private Coroutine jumpTimer; // time for temp jump buffs
    public int baseDefense = 0;
    public int currentDefense { get; private set; } = 0;
    public readonly Dictionary<string, Armor> equippedArmor = new Dictionary<string, Armor>();
    public static int damageBonus = 0; // additional damage from items

    Vector3 _velocity; // for gravity
    float _pitch;      // camera pitch

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
        // ----- Look (mouse/gamepad) -----
        Vector2 look = _lookAction.ReadValue<Vector2>();
        const float mouseScale = 0.12f;
        float yawDelta = look.x * lookSensitivity * mouseScale;
        float pitchDelta = look.y * lookSensitivity * mouseScale;

        transform.Rotate(0f, yawDelta, 0f);
        _pitch = Mathf.Clamp(_pitch - pitchDelta, minPitch, maxPitch);
        if (cam) cam.localEulerAngles = new Vector3(_pitch, 0f, 0f);

        // ----- Move (WASD/left stick) -----
        Vector2 move = _moveAction.ReadValue<Vector2>();
        Vector3 input = new Vector3(move.x, 0f, move.y);
        Vector3 moveDir = transform.TransformDirection(input) * moveSpeed;

        // ----- Jump + Gravity -----
        // Apply gravity continuously
        _velocity.y += gravity * Time.deltaTime;

        // Ground check: after the last Move(), CharacterController caches it correctly
        bool grounded = _controller.isGrounded;

        if (grounded && _velocity.y < 0f)
            _velocity.y = -2f;  // small downward bias keeps you stuck to ground

        if (grounded && _jumpAction.triggered)
            _velocity.y = jumpSpeed;

        // Combine horizontal + vertical
        Vector3 motion = (moveDir + _velocity) * Time.deltaTime;
        CollisionFlags flags = _controller.Move(motion);

        // If we hit the ground, reset vertical velocity again
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
                Vector3 origin = muzzle ? muzzle.position : cam.position;
                Vector3 forward = cam ? cam.forward : transform.forward;
                weapon.TryFire(origin, forward);
            }

            if (_reloadAction.triggered)
                weapon.Reload();
        }
    }

    public void ApplySpeed(float amount, int duration)
    {
        if (speedTimer != null)
        {
            StopCoroutine(speedTimer);
            speedTimer = null;
        }
        
        moveSpeed += amount;
        Debug.Log($"Speed was increased by {amount} for a total speed of {moveSpeed}");

        if (duration > 0) // If duration is 0 then permanent speed buff
        {
            speedTimer = StartCoroutine(SpeedBuffCoroutine(amount, duration));
        }
    }

    private IEnumerator SpeedBuffCoroutine(float amount, int duration)
    {
        // Wait for the duration time
        yield return new WaitForSeconds(duration);

        moveSpeed -= amount;

        Debug.Log($"Temporary speed boost expired. Total speed reset to {moveSpeed}.");

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

        Debug.Log($"Temporary jump speed boost expired. Total jump speed reset to {moveSpeed}.");

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
        int total = baseDefense;
        foreach (var armor in equippedArmor.Values)
        {
            total += armor.Defense;
        }
        currentDefense = total;
        Debug.Log($"Total defense: {currentDefense}");
    }

}


