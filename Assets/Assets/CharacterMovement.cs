using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{

    [SerializeField] private Camera cam;
    [SerializeField] private InputActionAsset inputActions;
    private InputAction inputForward;
    private InputAction inputBackward;
    void Start()
    {
        inputForward = inputActions.FindAction("CharacterForward");
        inputBackward = inputActions.FindAction("CharacterBackward");
    }

   
    void Update()
    {
      transform.rotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);
      transform.position = new Vector3(cam.transform.position.x, 0, cam.transform.position.z) + 
                            new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z) * 0.5f;
   
     /*   if (inputForward != null)
        {
            float inputValue = inputForward.ReadValue<float>();
            transform.Translate(Vector3.forward * inputValue * Time.deltaTime);
        }

        if (inputBackward != null)
        {
            float inputValue = inputBackward.ReadValue<float>();
            Debug.Log($"Input back Value: {inputValue}");
            transform.Translate(Vector3.back * inputValue * Time.deltaTime);
        }*/
    }
                     
}
