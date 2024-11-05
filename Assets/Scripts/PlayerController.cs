using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    public int maxHealth = 3;         // Maximum health (3 hearts)
    private int currentHealth;        
    public Image[] hearts;            // UI hearts array

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sp;
    private AudioSource audioSource;

    private bool facingRight = true;
    public float xSpeed = 5f;
    public float jumpForce = 20f;
    public float superJumpForce = 25f;

    private bool grounded;
    public Transform groundCheck;
    public LayerMask groundLayer;
    
    public Transform shootPosition;
    public GameObject shotPrefab;
    private bool shooting = false;
    public float shotTime = .5f;
    private IEnumerator shootingCoroutine;

    private bool superJump = false;
    [HideInInspector] public bool superJumpUnlocked = false;
    public LayerMask superGroundLayer;
    public ParticleSystem superParticles;

    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip superJumpSound;
    public AudioClip superJumpGroundSound;
    public AudioClip hurtSound;

    public static PlayerController instance;

    void Start() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(gameObject);
        }
        
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        shootingCoroutine = ShootingLoop();
        superParticles.Stop();

        UpdateHeartsUI();
    }

public void UpdateHeartsUI() {
    for (int i = 0; i < hearts.Length; i++) {
        if (hearts[i] != null) {
            hearts[i].enabled = i < currentHealth;
        } else {
            Debug.LogWarning("Heart UI element is missing at index " + i);
        }
    }
}

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        UpdateHeartsUI();
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0) {
            Die();
        }
    }

    void Die() {
        // Handle player death (restart level, game over screen, etc.)
        GameManager.instance.RestartLevel();
        // You could add a respawn or reload the scene here
    }

    public void ResetHealth() {
        currentHealth = 3;  // Reset to full health

        // Reinitialize hearts array by finding all heart UI elements with the "Heart" tag
        GameObject[] heartObjects = GameObject.FindGameObjectsWithTag("Heart");
        hearts = new Image[heartObjects.Length];
        for (int i = 0; i < heartObjects.Length; i++) {
            hearts[i] = heartObjects[i].GetComponent<Image>();
    }

        UpdateHeartsUI();
    }

    void Update() {
        // Velocity
        float yVel = rb.velocity.y;
        float xVel = Input.GetAxis("Horizontal") * xSpeed;

        // Start Shooting (with Left Mouse Button or Mousepad Tap)
        if (!shooting && Input.GetMouseButtonDown(0)) {  // Left mouse button
            shooting = true;
            StartCoroutine(shootingCoroutine);
        }
        
        // Stop shooting when Left Mouse Button or Mousepad Tap is released
        if (shooting && Input.GetMouseButtonUp(0)) {  // Left mouse button release
            shooting = false;
            StopCoroutine(shootingCoroutine);
        }

        // Direction Facing Test
        if (xVel != 0f) {
            facingRight = xVel > 0f;
            transform.localScale = new Vector2(facingRight ? 1f : -1f, 1f);
        }

        // Super Jump Ground Check
        if (superJumpUnlocked) {
            bool wasSuper = superJump;
            superJump  = Physics2D.OverlapCircle(groundCheck.position, .02f, superGroundLayer);
            if (!wasSuper && superJump) {
                superParticles.Play();
                audioSource.PlayOneShot(superJumpGroundSound);
            } else if (wasSuper && !superJump) {
                superParticles.Stop();
                superParticles.Clear();
            }
        }

        // Grounded Check
        bool wasGrounded = grounded;
        grounded = Physics2D.OverlapCircle(groundCheck.position, .02f, groundLayer);
        if (!wasGrounded && grounded) {
            audioSource.PlayOneShot(landSound);
        }

    // Jump (with Up Arrow key, W, Space)
    if (grounded && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) ) {
        grounded = false;
        if (superJump) {
            yVel = superJumpForce;
            audioSource.PlayOneShot(superJumpSound);
        } else {
            yVel = jumpForce;
            audioSource.PlayOneShot(jumpSound);
        }
        animator.SetTrigger("Jump");
    }

        // Set Rigidbody2D Velocity
        rb.velocity = new Vector2(xVel, yVel);

        // Set Animator Values
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Running", xVel != 0f);
        animator.SetBool("Shooting", shooting);

    // Crouch (with Down Arrow, Left Shift, or S key)
    bool isCrouching = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
    animator.SetBool("Crouching", isCrouching);
         

    }

    // Coroutine that spawns shot prefabs
    IEnumerator ShootingLoop() {
        while (true) {
            GameObject shot = Instantiate(shotPrefab, shootPosition.position, Quaternion.identity);
            shot.GetComponent<ShotController>().right = facingRight;
            yield return new WaitForSeconds(shotTime);
        }
    }

    // Trigger hurt animation when touching enemy
    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Enemy") {
            TakeDamage(1);
            animator.SetTrigger("Hurt");
            audioSource.PlayOneShot(hurtSound);
            rb.velocity = new Vector2(0, 8);
        }
    }

    public void ResetPowerUps() {
        superJumpUnlocked = false;
        superJump = false;
        superParticles.Stop();  
        superParticles.Clear(); 
    }
}
