using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private new Camera camera;
    private new Rigidbody2D rigidbody;
    private new Collider2D collider;

    private Vector2 velocity;
    private float inputAxis;

    public float moveSpeed = 8f;
    public float runMultiplier = 1.25f;
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;
    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow(maxJumpTime / 2f, 2f);

    // responsive control tuning (coyote time, jump buffer, snappy traction)
    public float groundAcceleration = 50f;
    public float groundDeceleration = 65f;
    public float turnAcceleration = 80f;
    public float airAcceleration = 38f;
    public float airDeceleration = 28f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.15f;

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < -0.5f) || (inputAxis < 0f && velocity.x > 0.5f);
    public bool falling => velocity.y < 0f && !grounded;

    // swimming, enabled by a WaterZone (land movement is unchanged)
    public float swimSpeed = 3.5f;
    public float swimStroke = 6f;
    public float swimGravity = -9f;
    public float maxSinkSpeed = -3f;

    public bool swimming { get; private set; }
    public bool stroking => Time.time - lastStrokeTime < 0.3f;
    private float swimCeiling;
    private float lastStrokeTime = -1f;
    private float lastGroundedTime = -10f;
    private float lastJumpPressedTime = -10f;

    private Vector2 carry;

    // called by a MovingPlatform (which runs earlier) while mario stands on it
    public void Carry(Vector2 platformVelocity)
    {
        carry = platformVelocity;
    }

    public void ApplyWaterCurrent(Vector2 acceleration)
    {
        if (!swimming)
            return;

        velocity += acceleration * Time.deltaTime;
        velocity.x = Mathf.Clamp(velocity.x, -swimSpeed * 1.5f, swimSpeed * 1.5f);
        velocity.y = Mathf.Clamp(velocity.y, maxSinkSpeed, swimStroke * 1.2f);
    }

    private void Awake()
    {
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        rigidbody.isKinematic = false;
        collider.enabled = true;
        velocity = Vector2.zero;
        jumping = false;
    }

    private void OnDisable()
    {
        rigidbody.isKinematic = true;
        collider.enabled = false;
        velocity = Vector2.zero;
        jumping = false;
    }

    private void Update()
    {
        if (swimming)
        {
            SwimMovement();
            return;
        }

        if (JumpPressed()) {
            lastJumpPressedTime = Time.time;
        }

        grounded = rigidbody.Raycast(Vector2.down);

        if (grounded && velocity.y <= 0.05f) {
            lastGroundedTime = Time.time;
        }

        HorizontalMovement();

        if (grounded) {
            GroundedMovement();
        } else if (!jumping && Time.time - lastGroundedTime <= coyoteTime) {
            TryExecuteBufferedJump();
        }

        ApplyGravity();
    }

    private void FixedUpdate()
    {
        if (camera == null) {
            camera = Camera.main;
        }

        // move mario based on his velocity
        Vector2 position = rigidbody.position;
        position += velocity * Time.fixedDeltaTime;

        // moving platforms carry mario (zero when not standing on one)
        position += carry * Time.fixedDeltaTime;
        carry = Vector2.zero;

        // clamp within the screen bounds
        if (camera != null)
        {
            Vector2 leftEdge = camera.ScreenToWorldPoint(Vector2.zero);
            Vector2 rightEdge = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            position.x = Mathf.Clamp(position.x, leftEdge.x + 0.5f, rightEdge.x - 0.5f);
        }

        rigidbody.MovePosition(position);
    }

    private void HorizontalMovement()
    {
        // responsive raw horizontal input with analog fallback
        float raw = Input.GetAxisRaw("Horizontal");
        inputAxis = Mathf.Abs(raw) > 0.01f ? raw : Input.GetAxis("Horizontal");

        bool sprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                      || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X);
        float targetSpeed = inputAxis * moveSpeed * (sprinting ? runMultiplier : 1f);

        float accel;
        if (Mathf.Abs(inputAxis) < 0.01f) {
            accel = grounded ? groundDeceleration : airDeceleration;
        } else if ((inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f)) {
            accel = turnAcceleration;
        } else {
            accel = grounded ? groundAcceleration : airAcceleration;
        }

        velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, accel * Time.deltaTime);

        // check if running into a wall
        if (Mathf.Abs(velocity.x) > 0.01f && rigidbody.Raycast(Vector2.right * Mathf.Sign(velocity.x))) {
            velocity.x = 0f;
        }

        // flip sprite to face input / movement direction immediately
        if (inputAxis > 0.01f || (Mathf.Abs(inputAxis) <= 0.01f && velocity.x > 0.1f)) {
            transform.eulerAngles = Vector3.zero;
        } else if (inputAxis < -0.01f || (Mathf.Abs(inputAxis) <= 0.01f && velocity.x < -0.1f)) {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

    private void GroundedMovement()
    {
        // prevent gravity from infinitely building up
        if (velocity.y < 0f) {
            velocity.y = 0f;
        }
        jumping = velocity.y > 0.05f;

        TryExecuteBufferedJump();
    }

    private void TryExecuteBufferedJump()
    {
        if (Time.time - lastJumpPressedTime <= jumpBufferTime)
        {
            velocity.y = jumpForce;
            jumping = true;
            lastJumpPressedTime = -10f;
            lastGroundedTime = -10f;
            Sfx.Play(ChiptuneSynth.Jump);
        }
    }

    private void ApplyGravity()
    {
        bool jumpHeld = JumpHeld();
        bool fallingNow = velocity.y < 0f || !jumpHeld;

        // slight hang-time near jump apex when holding jump for precision platforming
        float multiplier;
        if (jumping && jumpHeld && Mathf.Abs(velocity.y) < 2.2f) {
            multiplier = 0.8f;
        } else {
            multiplier = fallingNow ? 2f : 1f;
        }

        // apply gravity and terminal velocity
        velocity.y += gravity * multiplier * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, gravity / 2f);
    }

    private static bool JumpPressed()
    {
        return Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
    }

    private static bool JumpHeld()
    {
        return Input.GetButton("Jump") || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
    }

    public void SetSwimming(bool swimming, float ceiling = 0f)
    {
        this.swimming = swimming;
        swimCeiling = ceiling;
        jumping = false;

        // enter the water sinking gently instead of keeping the fall speed
        if (swimming) {
            velocity.y = Mathf.Max(velocity.y, maxSinkSpeed);
        }
    }

    private void SwimMovement()
    {
        // slower horizontal movement with water drag
        inputAxis = Input.GetAxis("Horizontal");
        velocity.x = Mathf.MoveTowards(velocity.x, inputAxis * swimSpeed, swimSpeed * 2f * Time.deltaTime);

        if (rigidbody.Raycast(Vector2.right * velocity.x)) {
            velocity.x = 0f;
        }

        if (velocity.x > 0f) {
            transform.eulerAngles = Vector3.zero;
        } else if (velocity.x < 0f) {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }

        grounded = rigidbody.Raycast(Vector2.down);

        // every press of jump is a swim stroke, even in mid water
        if (Input.GetButtonDown("Jump"))
        {
            velocity.y = swimStroke;

            float strokeDirection = Mathf.Abs(inputAxis) > 0.1f
                ? Mathf.Sign(inputAxis)
                : Mathf.Sign(velocity.x == 0f ? 1f : velocity.x);

            velocity.x = Mathf.Clamp(
                velocity.x + strokeDirection * 1.2f,
                -swimSpeed * 1.2f,
                swimSpeed * 1.2f
            );

            lastStrokeTime = Time.time;
            Sfx.Play(ChiptuneSynth.Swim);
        }

        // reduced gravity with a slow terminal sinking speed (buoyancy)
        velocity.y += swimGravity * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, maxSinkSpeed);

        if (grounded) {
            velocity.y = Mathf.Max(velocity.y, 0f);
        }

        // cannot swim above the water surface
        if (rigidbody.position.y >= swimCeiling) {
            velocity.y = Mathf.Min(velocity.y, 0f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // bounce off enemy head
            if (transform.DotTest(collision.transform, Vector2.down))
            {
                velocity.y = jumpForce / 2f;
                jumping = true;
            }
        }
        else if (collision.gameObject.layer != LayerMask.NameToLayer("PowerUp"))
        {
            // stop vertical movement if mario bonks his head
            if (transform.DotTest(collision.transform, Vector2.up)) {
                velocity.y = 0f;
            }
        }
    }

}
