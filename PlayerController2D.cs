using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")] 
    
    [SerializeField] float movementSpeed = 1f;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] bool overrideRigidbodySettings = true;

    bool _Grounded;
    Rigidbody2D _Rig;
    SpriteRenderer _SpriteRenderer;
    Collider2D _Collider;

    void Start()
    {

        if (!TryGetComponent(out _SpriteRenderer))
        {
            Debug.LogWarning("Failed to get SpriteRenderer", gameObject);
        }

        if (!TryGetComponent(out _Collider))
        {
            Debug.LogWarning("Failed to get 2D Collider, adding one...", gameObject);
            _Collider = gameObject.AddComponent<BoxCollider2D>(); // Checking if the object has an 2D COllider
        }

        _Rig = GetComponent<Rigidbody2D>();
        if (!overrideRigidbodySettings) return;

        //Setting up the rigidbody
        _Rig.constraints = RigidbodyConstraints2D.FreezeRotation;
        _Rig.sleepMode = RigidbodySleepMode2D.NeverSleep;
        _Rig.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void FixedUpdate()
    {
        MovementHandle();
    }

    void Update()
    {
        GroundCheck();
    }

    void MovementHandle()
    {
        Vector2 axis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Math.Abs(axis.x) > 0.1f)
        {
            _Rig.MovePosition(_Rig.transform.position += new Vector3(axis.x * (movementSpeed / 10), 0));
            if (_SpriteRenderer)
                SpriteHandle(axis.x);
        }

        if (!(axis.y > 0) && !Input.GetKeyDown(KeyCode.Space)) return;
        if (!_Grounded) return;
        _Rig.velocity = new Vector2(_Rig.velocity.x, 0);
        _Rig.AddForce(jumpHeight * transform.up, ForceMode2D.Impulse);
    }

    void GroundCheck()
    {
        Vector2 origin = transform.position;
        origin.y -= transform.lossyScale.y / 2 + 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(origin, -transform.up, 0.1f);
        _Grounded = hit.transform;
    }

    void SpriteHandle(float val)
    {
        _SpriteRenderer.flipX = val < 0;
    }
}