using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float horizontalMove;
    private float verticalMove;
    private Vector3 playerInput;
    private Vector3 movePlayer;

    public CharacterController player;
    public float playerSpeed;
    public float jumpForce; //8f
    public float gravity; //20f
    public float maxFallSpeed; //20f
    private bool isJumping = false;
    private float verticalVelocity;
    private bool wasGrounded;
    public float groundCheckDistance = 0.1f; // Nueva variable para verificar el suelo

    // Variables para la cámara
    public Camera mainCamera;
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;
    public float maxLookAngle = 80f;
    private Vector3 originalCameraPos;
    public float cameraForwardAmount = 0.5f;

    // Variables existentes
    public bool isOnSlope = false;
    private Vector3 hitNormal;
    public float slideVelocity;
    public float slopeForceDown;
    public Animator playerAnimatorController;
    public GameDataController gameDataController;
    public int currentLife;

    // Variables para el doble tap S
    private float doubleTapTimeThreshold = 0.3f;
    private float lastSKeyPressTime;
    private bool isTurning180 = false;
    private float turnSpeed = 720f; // Velocidad de giro en grados por segundo
    private float targetRotation;

    void Start()
    {
        player = GetComponent<CharacterController>();
        playerAnimatorController = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mainCamera == null)
            mainCamera = Camera.main;

        originalCameraPos = mainCamera.transform.localPosition;

        gameDataController = FindObjectOfType<GameDataController>();
        currentLife = gameDataController.life;
    }

    void Update()
    {
        if (currentLife <= 0)
        {
            playerAnimatorController.SetTrigger("PlayerDeath");
            return;
        }

        CheckGroundStatus(); // Nueva función para verificar el estado del suelo
        HandleMouseLook();
        HandleMovement();
        HandleCameraPosition();        
        PlayerSkills();
        Handle180Turn();

        player.Move(movePlayer * Time.deltaTime);
    }

    private void CheckGroundStatus()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + player.center;
        bool isGroundBelow = Physics.Raycast(rayStart, Vector3.down, out hit,
            player.height / 2 + groundCheckDistance);

        // Si estamos en el suelo o hay suelo muy cerca debajo
        if (player.isGrounded || isGroundBelow)
        {
            if (!wasGrounded)
            {
                isJumping = false;
                verticalVelocity = -0.5f;
            }
            wasGrounded = true;
        }
        else
        {
            wasGrounded = false;
        }
    }

    void HandleMouseLook()
    {
        if (!isTurning180)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);

            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
            mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");

        playerInput = new Vector3(horizontalMove, 0, verticalMove);
        playerInput = Vector3.ClampMagnitude(playerInput, 1);

        playerAnimatorController.SetFloat("PlayerWalkVelocity", playerInput.magnitude * playerSpeed);

        // Calcula el movimiento horizontal
        Vector3 horizontalMovement = transform.right * playerInput.x + transform.forward * playerInput.z;
        movePlayer.x = horizontalMovement.x * playerSpeed;
        movePlayer.z = horizontalMovement.z * playerSpeed;

        // Detectar doble tap S
        if (Input.GetKeyDown(KeyCode.S))
        {
            float timeSinceLastPress = Time.time - lastSKeyPressTime;
            if (timeSinceLastPress <= doubleTapTimeThreshold)
            {
                StartTurn180();
            }
            lastSKeyPressTime = Time.time;
        }
    }

    void HandleCameraPosition()
    {
        Vector3 targetPos = originalCameraPos;

        // Mover la cámara hacia adelante al correr o caminar hacia adelante
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            targetPos += Vector3.forward * cameraForwardAmount;
        }

        // Interpola suavemente la posición de la cámara
        mainCamera.transform.localPosition = Vector3.Lerp(
            mainCamera.transform.localPosition,
            targetPos,
            Time.deltaTime * 10f
        );
    }

    void StartTurn180()
    {
        if (!isTurning180)
        {
            isTurning180 = true;
            targetRotation = transform.eulerAngles.y + 180f;
        }
    }

    void Handle180Turn()
    {
        if (isTurning180)
        {
            float step = turnSpeed * Time.deltaTime;
            Vector3 newRotation = transform.eulerAngles;
            newRotation.y = Mathf.MoveTowardsAngle(newRotation.y, targetRotation, step);
            transform.eulerAngles = newRotation;

            if (Mathf.Approximately(Mathf.DeltaAngle(transform.eulerAngles.y, targetRotation), 0))
            {
                isTurning180 = false;
            }
        }
    }

    void PlayerSkills()
    {
        // Manejo del salto mejorado
        if (wasGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = jumpForce;
                isJumping = true;
                playerAnimatorController.SetTrigger("PlayerJump");
            }
            else if (!isJumping)
            {
                // Mantener una pequeña fuerza hacia abajo cuando está en el suelo
                verticalVelocity = -0.5f;
            }
        }
        else
        {
            // Aplicar gravedad de manera más consistente
            verticalVelocity -= gravity * Time.deltaTime;

            // Limitar la velocidad máxima de caída
            if (verticalVelocity < -maxFallSpeed)
            {
                verticalVelocity = -maxFallSpeed;
            }

            // Salto variable (salto más alto si se mantiene presionado el botón)
            if (isJumping && Input.GetButton("Jump") && verticalVelocity > 0)
            {
                verticalVelocity *= 0.95f;
            }
        }

        // Aplicar el movimiento vertical
        movePlayer.y = verticalVelocity;

        // Actualizar el animator
        playerAnimatorController.SetFloat("PlayerVerticalVelocity", verticalVelocity);
        playerAnimatorController.SetBool("IsGrounded", wasGrounded);

        // Sprint
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerSpeed += 5f;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            playerSpeed -= 5f;
        }
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
}