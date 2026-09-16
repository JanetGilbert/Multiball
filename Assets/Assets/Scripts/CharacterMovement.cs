using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{

    [SerializeField] private Camera cam;
    [SerializeField] private InputActionAsset inputActions;
    private InputAction inputForward;
    private InputAction inputBackward;

    private Animator animator;

    private Transform previousCharacterTransform;
    void Start()
    {
        inputForward = inputActions.FindAction("CharacterForward");
        inputBackward = inputActions.FindAction("CharacterBackward");
        previousCharacterTransform = transform;
        animator = GetComponent<Animator>();
    }

   
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);

        previousCharacterTransform = transform;
        bool isMovingForward = false, isMovingBackward = false;
        if (inputForward != null)
        {
            float inputValue = inputForward.ReadValue<float>();
            transform.Translate(Vector3.forward * inputValue * Time.deltaTime);
            isMovingForward = inputValue > 0.0f;
            Debug.Log("Input Forward Value: " + inputValue);
        }

        if (inputBackward != null)
        {
            float inputValue = inputBackward.ReadValue<float>();
            transform.Translate(Vector3.back * inputValue * Time.deltaTime);
            isMovingBackward = inputValue > 0.0f;
            Debug.Log("Input Backward Value: " + inputValue);
        }

        bool isMoving = isMovingForward || isMovingBackward;
        if (animator != null && animator.GetBool("Moving") != isMoving)
        {
            animator.SetBool("Moving", isMoving);
            Debug.Log("Animator Moving State: " + isMoving);
        }

    }
                     
}
