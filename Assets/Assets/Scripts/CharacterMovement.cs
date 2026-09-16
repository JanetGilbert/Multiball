using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{

    [SerializeField] private Camera cam;
    [SerializeField] private InputActionAsset inputActions;


    private InputAction moveAction;

    private Animator animator;


    void Start()
    {
        moveAction = inputActions.FindAction("CharacterMove");
        animator = GetComponent<Animator>();
    }

   
    void Update()
    {
      //  transform.rotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);

        if (moveAction != null)
        {
            Vector2 move = moveAction.ReadValue<Vector2>();


            const float trigger = 0.1f;

            if (Mathf.Abs(move.y) > trigger)
            {
                transform.Translate(Vector3.forward * move.y * Time.deltaTime);
                animator.SetBool("Moving", true);
            }
            else
            {
                animator.SetBool("Moving", false);
            }

            if (Mathf.Abs(move.x) > trigger)
            {
               //transform.rotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y + (move.x * 90), 0);
                transform.Rotate(0, move.x * 100.0f * Time.deltaTime, 0);

            }
    
        }

    }
                     
}
