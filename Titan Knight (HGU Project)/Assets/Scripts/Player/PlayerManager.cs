/* Mark Gaskins
 * THIS SHOULD BE ATTATCHED TO THE "PLAYER CONTROLLER" OBJECT AT ALL TIMES
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CharacterController))]
public class PlayerManager : MonoBehaviour
{
    [Space(20)]
    [Header("Player Preferences")]
    [Tooltip("The walkspeed of the player")] [Range(4.0f, 12.0f)] public float speed = 6.0f;
    [Tooltip("The walkspeed multiplayer when the player's running")] [Range(0.1f, 4.5f)] public float runSpeedMultiplier = 1.5f;
    [Tooltip("The jump force")] [Range(1.0f, 12)] public float jumpPower = 8.0f;
    [Tooltip("Gravity's influence on the player")] [Range(10.0f, 50.0f)] public float gravity = 20.0f;
    [Tooltip("The speed in which the player rotates when input is recieved")] [SerializeField] [Range(300,1000)] float pModelRotSpeed = 800;
    [Space(5)]
    //[Tooltip("The amount of health that the player starts with. (default: 100)")] [SerializeField] private int startPlayerHealth = 100; // Do not use this to reference current player health.
    //[Tooltip("A non-static reference for the current Player Health. (read values only!)")] public int currentPlayerHealth;


    [Space(10)]
    [Header("Camera Preferences")]
    [Tooltip("Should the Camera FOV change when the player sprints?")] [SerializeField] public bool doSprintingFOV = true;
    [Tooltip("The sprinting FOV of the Camera. (FOV changes to this, when player sprints.)")] [Range(60, 100)] public float sprintingFOV = 90;
    [Tooltip("The standard FOV of the Camera. (FOV reverts to this, when not sprinting.)")] [Range(60, 100)] public float normalFOV = 65;
    [Tooltip("The speed in which FOV changes when the player sprints, or stops sprinting.")] [SerializeField] [Range(0, 20)] int smoothFOVSpeed = 10;

    [Space(26)]
    [Header("Player References")]
    [Tooltip("The parent object of the player model, which rotates the player model in the direction that the player is facing.")] [SerializeField] Transform pModelRotation;

    [Space(20)]
    [Header("Camera References")]
    [Tooltip("This is the player camera. It has a Fixed Position!")] [SerializeField] GameObject pCamera; // The pos/rot of this will update with the pCamera_Position.
    [Tooltip("This is the game object that updates the position and rotation of the pCamera.")] [SerializeField] GameObject pCamera_Position;


    [Space(5)]
    [Header("Misc. References")]
    [Tooltip("This is currently a Screen Space canvas, but later it should be modified to be a World Space canvas near the player.")][SerializeField] Slider healthDisplay;
    [SerializeField] TextMeshProUGUI healthText;

    //private Vector3 moveDirection = Vector3.zero;

    private CharacterController controller;

    private Health playerHealth; // This manages all player health.

    private bool isRunning;
    float verticalSpeed;


    private bool isDead;

    //public static int playerHealth; // The active player health (always updating)

    [HideInInspector] public Vector3 movementDirection;

    private void Awake() // Assign defaults
    {
        controller = GetComponent<CharacterController>();
        playerHealth = GetComponent<Health>();

        Camera.main.fieldOfView = normalFOV;
    }

    public void Die() // Automatically called when the player dies. This is just for the proto-stage of Titan Knights.
    {
        Debug.Log($"<color=\"red\"><b>Player has died!</b></color>");

        isDead = true;


        //Destroy(pModelRotation.gameObject); Discontinued
    }


    private void FixedUpdate()
    {
        HandleVisuals();

        HandleMovement();
        HandleSprinting();

        HandleJumping();


        if (isDead)
        {
            pModelRotation.gameObject.SetActive(false);

            controller.enabled = false;
            this.enabled = false;
        }


        // Update temporary health display (shown for debug reasons)
        healthDisplay.value = playerHealth.currentHealth;
        healthDisplay.maxValue = playerHealth.startHealth;
        healthText.text = $"{playerHealth.currentHealth}%";
    }


    private void HandleJumping()
    {
        // Check for standing on the ground
        if (controller.isGrounded)
        {
            verticalSpeed = -1; // Vertical speed is used to make the jumping smooth, instead of instantly teleporting player upwards

            //Jumping has been discontinued.
            //if (Input.GetButton("Jump"))
            //    verticalSpeed = jumpPower;

        }

        verticalSpeed -= gravity * Time.deltaTime; // Apply gravity
        movementDirection.y = verticalSpeed;

    }

    private void HandleSprinting()
    {
        // Handle sprinting 
        if (!isRunning) // Not sprinting
        {
            if (doSprintingFOV && Camera.main.fieldOfView > normalFOV) // Smoothly transition back to normal FOV, at smoothFOVSpeed speed
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, normalFOV, smoothFOVSpeed * Time.deltaTime);

            controller.Move(movementDirection * speed * Time.deltaTime);
        }
        else // Sprinting 
        {
            if (doSprintingFOV && Camera.main.fieldOfView < sprintingFOV) // Smoothly transition to sprinting FOV, at smoothFOVSpeed speed
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, sprintingFOV, smoothFOVSpeed * Time.deltaTime);


            controller.Move(new Vector3(
            movementDirection.x * (speed * runSpeedMultiplier),
            movementDirection.y * speed, /* Prevent the jump power from multiplying! */
            movementDirection.z * (speed * runSpeedMultiplier)) * Time.deltaTime);
        }

    }


    //private float horizontalInput, verticalInput;

    private void HandleMovement() // The movement will be completely rescripted in order to rotate movement grid by 45??
    {

        if (Input.GetKey(KeyCode.LeftShift)) isRunning = true; else isRunning = false; // Handle sprinting with left shift

       
        // Will be modified later
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        movementDirection = new Vector3(horizontalInput, 0, verticalInput); // WASD/Aw.Keys control
        movementDirection.Normalize();


        if (movementDirection != Vector3.zero) // Handle the Player Model Rotion, which rotates the player model in the direction that the Player is moving in
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            pModelRotation.transform.rotation = Quaternion.RotateTowards(pModelRotation.transform.rotation, toRotation, pModelRotSpeed * Time.deltaTime);
        }

    }

    private void HandleVisuals()
    {
        // Update position of player's "isometric" camera
        pCamera.transform.position = pCamera_Position.transform.position;
        pCamera.transform.rotation = pCamera_Position.transform.rotation;

        
        //// Handle the Player Model Rotion, which rotates the player model in the direction that the Player is moving in
        //if (movementDirection != Vector3.zero) 
        //{
        //    Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        //    pModelRotation.transform.rotation = Quaternion.RotateTowards(pModelRotation.transform.rotation, toRotation, pModelRotSpeed * Time.deltaTime);

        //}


        

    }
}
