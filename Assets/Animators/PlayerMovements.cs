using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerMovements : MonoBehaviour
{
    //for run
    public float _inputX;
    public float _inputY;
    [SerializeField] float _speed = 10f;

    public Rigidbody2D _rb;
    public SpriteRenderer _spriteRenderer;
    public BoxCollider2D _boxCollider;
    public Animator _animator;
    public Transform _groundCheck;
    public LayerMask _groundLayer;

    [SerializeField] bool _colliderChecker = true;

    //for jump
    private bool _isGrounded = false;
    private float _jumpForce = 15f;
    [SerializeField]private int _jumpCounter = 0;

    //for dodging
    private float dodgeTimer;
    private float dodgeCooldownTimer;
    private bool isDodging = false;
    public float dodgeForce = 20f;
    public float dodgeDuration = 0.2f;
    public float dodgeCooldown = 2f;

    //for Attackiing
    private bool isAttacking = false;
    [SerializeField] private int comboStep = 0;
    [SerializeField] private float comboTimer = 0f;
    [SerializeField] public float comboResetTime = 5f;


    void Update()
    {
        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0)
        {
            comboStep = 0; // reset combo if too slow
        }

        // Double Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && _jumpCounter == 1)
        {
            _animator.SetBool("isRunning", false);
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            _animator.SetBool("isGrounded", true);
            _jumpCounter = 0;
        }

        // Player Jump and anumation
        if (Keyboard.current.spaceKey.wasPressedThisFrame && _isGrounded && _jumpCounter <= 0)
        { 
            _animator.SetBool("isRunning", false);
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            _animator.SetBool("isGrounded", true);
            _jumpCounter++;
            _isGrounded = false;


        }
        else if(_isGrounded)
        {
            _animator.SetBool("isGrounded", false);
           
        }

        // Player Attack and animation
        if (Mouse.current.leftButton.wasPressedThisFrame && !Keyboard.current.spaceKey.wasPressedThisFrame && _isGrounded)
        {
            _animator.SetBool("isRunning", false);
            StartCoroutine(AttackRoutine());

        }

        // Player Dodge(Dash) and animation
        if (isDodging)
        {
            _animator.SetBool("isDodging", false);
            dodgeTimer -= Time.deltaTime;
            _rb.gravityScale = 5;

            if (dodgeTimer <= 0)
            {
                isDodging = false;
            }
        }

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && dodgeTimer <= 0 && !isDodging)
        {
            _animator.SetBool("isRunning", false);
            _animator.SetBool("isDodging", true);
            StartDodge();
            
        }


        //Player movement (Left Right)
        if (!isDodging)
        {
            _rb.linearVelocity = new Vector2(_inputX * _speed, _rb.linearVelocity.y);
        }

        if (_boxCollider.enabled == false)
        {
            _colliderChecker = false;
        }

        // Animation for player movement
        if (_inputX > 0 && _colliderChecker)
        {
            _spriteRenderer.flipX = false;
            _animator.SetBool("isRunning", true);
        }
        else if (_inputX < 0)
        {
            _spriteRenderer.flipX = true;
            _animator.SetBool("isRunning", true);
        }
        else
        {
            _animator.SetBool("isRunning", false);
        }


    }

    private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy"))
        {
            _isGrounded = true;
            _jumpCounter = 0;
        }
        
    }

    public void playerMove(InputAction.CallbackContext context)
    {
        _inputX = context.ReadValue<Vector2>().x;
        _inputY = context.ReadValue<Vector2>().y;

    }
    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        _animator.SetTrigger("isAttacking");

        if (comboStep >= 3) comboStep = 0;

        _animator.SetInteger("AttackNum", comboStep);
        comboStep++;

        comboTimer = comboResetTime;

        yield break; // let animation event handle ending
    }

    void StartDodge()
    {
        isDodging = true;
        dodgeTimer = dodgeDuration;
        dodgeCooldownTimer = dodgeCooldown;
        _rb.gravityScale = 0;
        

        float direction = Mathf.Sign(_inputX);

        // If no input, dodge forward (right by default)
        if (direction == 0)
            direction = transform.localScale.x > 0 ? 1 : -1;

        _rb.linearVelocity = new Vector2(direction * dodgeForce, _rb.linearVelocity.y);
        Debug.Log("Dodge started with velocity: " + _rb.linearVelocity);
    }

}
