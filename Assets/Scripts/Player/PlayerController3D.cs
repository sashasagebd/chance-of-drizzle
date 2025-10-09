using UnityEngine;
using UnityEngine.InputSystem; // << Input System

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController3D : MonoBehaviour
{
    [Header("Movement")]
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

}
