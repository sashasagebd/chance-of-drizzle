using UnityEngine;

public class CharacterAiming : MonoBehaviour
{
    [SerializeField] private float turnSpeed = 15f;

    private Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        
        
    }

    // LateUpdate is called after Update, ensuring smooth rotation after movement
    void LateUpdate()
    {
        float yawCamera = mainCamera.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yawCamera, 0), turnSpeed * Time.deltaTime);

    }
}
