using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LureBehavior : MonoBehaviour
{
    [Header("Outside Combat Properties")]
    [SerializeField]
    float rotationSpeed = 1;
    [SerializeField]
    bool doClamp = false;
    [SerializeField]
    Vector2 rotationMinMax = Vector2.zero;
    [SerializeField]
    float moveSpeed = 1;
    [SerializeField]
    float gravity = 0.5f;
    
    Rigidbody RB = null;
    float rotZ = 0;
    float previousLoggedTime = 0;
    bool movingForward = false;
    PlayerBehavior myPlayer = null;
    Vector2 input = Vector2.zero;

    private void Start()
    {
        Init();
    }

    // Start is called before the first frame update
    public void Init()
    {
        RB = GetComponent<Rigidbody>();
        /*
        fallTime = 1;
        speed = _speed;
        myPlayer = _player;
        isFalling = true;

        
        RB.velocity = _dir * _speed;
        */
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (doClamp)
        {
            rotZ = Mathf.Clamp(rotZ + (input.x * rotationSpeed * Time.deltaTime), rotationMinMax.x, rotationMinMax.y);
            
        }
        else
        {
            rotZ += (input.x * rotationSpeed * Time.deltaTime);
        }

        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        RB.velocity = (Vector3.down * gravity) + (transform.up * -(movingForward ? moveSpeed : 0));

        /*
        if (isFalling && fallTime > 0)
        {
            RB.velocity = RB.velocity.normalized * (lureDeccelerationCurve.Evaluate(fallTime) * speed);
            fallTime = Mathf.Clamp01(fallTime - (Time.deltaTime * fallTimeScale));

            if (lureDeccelerationCurve.Evaluate(fallTime) <= 0)
            {
                isFalling = false;
                RB.velocity = Vector3.zero;
            }
        }
        */
    }

    public void RotateDrone(InputAction.CallbackContext _ctx)
    {
        input = _ctx.ReadValue<Vector2>();
        
        /*
        if (_ctx.performed)
        {
            
            //previousLoggedTime = Time.time;
        }
        */
    }

    public void MoveFoward(InputAction.CallbackContext _ctx) 
    {
        if (_ctx.started)
        {
            movingForward = true;
        }

        else if (_ctx.canceled)
        {
            movingForward = false;
        }
    }


    public void StickToObject() 
    { 
        
    }
}
