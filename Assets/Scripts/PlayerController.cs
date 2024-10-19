using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

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

    // Referencia al sistema de vida
    public GameDataController gameDataController;
    public int currentLife;



    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<CharacterController>();
        playerAnimatorController = GetComponent<Animator>();

        // Inicializa el controlador de vida
        gameDataController = FindObjectOfType<GameDataController>();

        // Asigna la vida inicial desde el controlador de datos
        currentLife = gameDataController.life;
    }

    //Update is called once per frame
    void Update()
    {
        if (currentLife <= 0)
        {
            // Aquí puedes manejar lo que ocurre cuando el jugador muere
            playerAnimatorController.SetTrigger("PlayerDeath");
            return; // Si la vida es 0, el jugador no puede moverse
        }

        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");

        playerInput = new Vector3(horizontalMove, 0, verticalMove);
        playerInput = Vector3.ClampMagnitude(playerInput, 1); // Control de velocidad desp-Max10

        playerAnimatorController.SetFloat("PlayerWalkVelocity", playerInput.magnitude * playerSpeed);

        // Calcula la dirección de la cámara
        CamDirection();

        // Mueve al jugador basado en la dirección de la cámara
        movePlayer = playerInput.x * camRight + playerInput.z * camForward;
        movePlayer = movePlayer * playerSpeed;

        player.transform.LookAt(player.transform.position + movePlayer); // Mirar en la dirección del movimiento

        SetGravity();
        PlayerSkills();

        player.Move(movePlayer * Time.deltaTime);
    }

    void CamDirection()
    {
        // Actualiza las direcciones camForward y camRight según la cámara activa
        camForward = mainCamera.transform.forward;
        camRight = mainCamera.transform.right;

        camForward.y = 0; // Ignora la inclinación vertical de la cámara
        camRight.y = 0;

        camForward = camForward.normalized;
        camRight = camRight.normalized;
    }


    void PlayerSkills()
    {
        if (player.isGrounded && Input.GetButtonDown("Jump"))
        {
            fallVelocity = jumpForce;
            movePlayer.y = fallVelocity;
            playerAnimatorController.SetTrigger("PlayerJump");
        }
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

    //// Método para restar vida al personaje
    //public void TakeDamage(int damage)
    //{
    //    gameDataController.RestarVida(damage); // Llamar a la función en el controlador de datos
    //    currentLife = gameDataController.life; // Actualiza la vida actual del jugador
    //    if (currentLife <= 0)
    //    {
    //        // Lógica para cuando el jugador muere
    //        playerAnimatorController.SetTrigger("PlayerDeath");
    //    }
    //}
}