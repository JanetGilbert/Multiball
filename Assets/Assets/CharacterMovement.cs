using UnityEngine;

public class CharacterMovement : MonoBehaviour
{

    [SerializeField] private Camera cam;

    void Start()
    {
     }

   
    void Update()
    {
      transform.rotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);
       
    }
}
