using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class PlayerProbMovement : MonoBehaviour
{
    public float moveSpeed = 5; // Default movement speed
    public float sprintMultiplier = 1f; // Multiplier for sprinting
    public float moveDecline = 5;
    private CharacterController controller;
   

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Get input for movement
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 move = transform.right * moveX + transform.forward * moveY;

        // Check for sprinting
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * sprintMultiplier : moveSpeed;

       if(Input.GetKeyDown(KeyCode.Space))
       {
            currentSpeed = (moveSpeed - moveDecline);
            moveSpeed = currentSpeed;
            
       }
       else if(moveSpeed <= 0)
       {
            moveSpeed = 0;
       }
            // Apply movement
            controller.Move(move * currentSpeed * Time.deltaTime);
    }
}
