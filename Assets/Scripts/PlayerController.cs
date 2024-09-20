using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float horizontalMove;
    private float verticalMove;
    private Vector3 playerInput;

    public CharacterController player;
    public float playerSpeed;
    public float gravity = 9.8f;
    public float fallVelocity;
    public float jumpForce;

    public Camera mainCamera;
    private Vector3 camForward;
    private Vector3 camRight;
    private Vector3 movePlayer;

    public bool isOnSlope = false;
    private Vector3 hitNormal;
    public float slideVelocity;
    public float slopeForceDown;

    private float contVel;

    //Animations
    public Animator playerAnimatorController;

    public CinemachineVirtualCamera thirdPersonCam;
    public CinemachineVirtualCamera firstPersonCam;

    // Variable para gestionar el estado de la cámara
    private bool isFirstPerson = false;


    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<CharacterController>();
        playerAnimatorController = GetComponent<Animator>();

        // Inicialmente activamos la cámara de tercera persona
        thirdPersonCam.gameObject.SetActive(true);
        firstPersonCam.gameObject.SetActive(false);
    }

    // Update is called once per frame
    //void Update()
    //{
    //    horizontalMove = Input.GetAxis("Horizontal");
    //    verticalMove = Input.GetAxis("Vertical");

    //    playerInput = new Vector3(horizontalMove, 0, verticalMove);
    //    playerInput = Vector3.ClampMagnitude(playerInput, 1);//Control de velocidad desp-Max10

    //    playerAnimatorController.SetFloat("PlayerWalkVelocity", playerInput.magnitude * playerSpeed);


    //    CamDirection();
    //    movePlayer = playerInput.x * camRight + playerInput.z * camForward;
    //    movePlayer = movePlayer * playerSpeed;

    //    player.transform.LookAt(player.transform.position + movePlayer);//Mirar a la direccion de movimiento

    //    SetGravity();

    //    PlayerSkills();

    //    player.Move(movePlayer * Time.deltaTime);

    //}
    void Update()
    {
        // Alternar entre cámaras al presionar la tecla C
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCameraView();
        }

        // Código de movimiento existente...
        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");

        playerInput = new Vector3(horizontalMove, 0, verticalMove);
        playerInput = Vector3.ClampMagnitude(playerInput, 1);

        playerAnimatorController.SetFloat("PlayerWalkVelocity", playerInput.magnitude * playerSpeed);

        CamDirection();
        movePlayer = playerInput.x * camRight + playerInput.z * camForward;
        movePlayer = movePlayer * playerSpeed;

        player.transform.LookAt(player.transform.position + movePlayer);

        SetGravity();

        PlayerSkills();

        player.Move(movePlayer * Time.deltaTime);
    }
    void ToggleCameraView()
    {
        isFirstPerson = !isFirstPerson;

        if (isFirstPerson)
        {
            // Activa la cámara en primera persona y desactiva la de tercera persona
            thirdPersonCam.gameObject.SetActive(false);
            firstPersonCam.gameObject.SetActive(true);
        }
        else
        {
            // Activa la cámara en tercera persona y desactiva la de primera persona
            thirdPersonCam.gameObject.SetActive(true);
            firstPersonCam.gameObject.SetActive(false);
        }
    }
    void PlayerSkills()
    {
        if (player.isGrounded && Input.GetButtonDown("Jump"))
        {
            fallVelocity = jumpForce;
            movePlayer.y = fallVelocity;
            playerAnimatorController.SetTrigger("PlayerJump");
        }
        //if (Input.GetKeyDown(KeyCode.LeftShift))
        //{
        //    contVel = Mathf.Lerp(playerSpeed, playerSpeed + 0.5f, 0.5f);
        //    playerSpeed = Mathf.Clamp(contVel, 0f, 10f);
        //}
        //if (Input.GetKeyUp(KeyCode.LeftShift))
        //{
        //    contVel = Mathf.Lerp(playerSpeed, playerSpeed - 0.5f, 0.5f);
        //    playerSpeed = Mathf.Clamp(contVel, 0f, 10f);
        //}
        // Aumentar velocidad al presionar Shift
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerSpeed += 5f; // Aumenta la velocidad en 5
        }

        // Reducir velocidad al soltar Shift
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            playerSpeed -= 5f; // Restablece la velocidad original
        }
    }
    void CamDirection()
    {
        camForward = mainCamera.transform.forward;
        camRight = mainCamera.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward = camForward.normalized;
        camRight = camRight.normalized;
    }
    void SetGravity()
    {
        if (player.isGrounded)
        {
            fallVelocity = -gravity * Time.deltaTime;
            movePlayer.y = fallVelocity;
        }
        else
        {
            fallVelocity -= gravity * Time.deltaTime;
            movePlayer.y = fallVelocity;
            playerAnimatorController.SetFloat("PlayerVerticalVelocity", player.velocity.y);
        }
        playerAnimatorController.SetBool("IsGrounded", player.isGrounded);
        SlideDown();
    }
    public void SlideDown()
    {
        isOnSlope = Vector3.Angle(Vector3.up, hitNormal) >= player.slopeLimit;
        if (isOnSlope)
        {
            movePlayer.x += ((1f - hitNormal.y) * hitNormal.x) * slideVelocity;
            movePlayer.z += ((1f - hitNormal.y) * hitNormal.z) * slideVelocity;

            movePlayer.y += slopeForceDown;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }
    private void OnAnimatorMove()
    {

    }
}