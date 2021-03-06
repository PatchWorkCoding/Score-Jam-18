using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LureBehavior : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField]
    GameObject myCamera;
    [SerializeField]
    Magnet myMagnet = null;

    [SerializeField] private Variables propellerVariablesObject = null;

    [Header("Outside Combat Properties")]
    [SerializeField]
    bool doGlobalMovement = false;
    [SerializeField]
    float rotationSpeed = 1;
    [SerializeField]
    bool doClamp = false;
    [SerializeField]
    Vector2 rotationMinMax = Vector2.zero;
    [SerializeField]
    float forwardMoveSpeed = 1, backwardMoveSpeed = 2;
    [SerializeField]
    float gravity = 0.5f;

    Rigidbody RB = null;
    float rotZ = 0;
    bool movingForward = false, movingBack = false;
    Vector2 input = Vector2.zero;

    [Header("Outside Combat Properties")]
    [SerializeField]
    float pullForce = 1;
    [SerializeField]
    float overdriveForce = 1, overdriveTime = 0;
    [SerializeField]
    bool doGlobalCombatMovement = false;

    bool isInCombat = false;
    FishBehavior curFish = null;
    GameObject attachedObject = null;
    float curOverdriveTime = 0;

    bool usingOverdrive = false;
    [Header("Fish Meter")]
    [SerializeField]
    GameObject fishMeter = null;
    bool isInLimbo = false;
    [SerializeField]
    Gradient fuelGauge;
    [SerializeField]
    private float combatRotation;
    private float nonCombatRotation;

    private void Start()
    {
        Init();
    }

    // Start is called before the first frame update
    public void Init()
    {
        RB = GetComponent<Rigidbody>();
        transform.GetChild(1).gameObject.SetActive(false);
        nonCombatRotation = rotationSpeed;
        myCamera = GameManager.GM.camera.gameObject;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateVelocity(Time.fixedDeltaTime);
        UpdatePropellerSpin();
    }

    private void UpdateVelocity(float deltaTime)
    {
        if (!isInCombat)
        {
            if (doGlobalMovement)
            {
                RB.velocity = new Vector3(input.x * forwardMoveSpeed, (input.y > 0 ? backwardMoveSpeed : input.y < 0 ? -forwardMoveSpeed : 0) + -gravity, 0);
            }

            else
            {
                if (doClamp)
                {
                    rotZ = Mathf.Clamp(rotZ + (input.x * rotationSpeed * deltaTime), rotationMinMax.x, rotationMinMax.y);
                }

                else
                {
                    rotZ += (input.x * rotationSpeed * deltaTime);
                }

                transform.rotation = Quaternion.Euler(0, 0, rotZ);
                RB.velocity = (Vector3.down * gravity) + (transform.up * -(movingForward ? forwardMoveSpeed : (movingBack ? -backwardMoveSpeed : 0)));
            }
        }

        else
        {
            if (curFish != null)
            {
                if (doGlobalCombatMovement)
                {
                    RB.velocity = ((Vector3)input.normalized * (movingBack ? overdriveForce : pullForce)) + curFish.FightVelocity;

                    fishMeter.transform.GetChild(0).up = input.normalized;
                }

                else
                {
                    if (doClamp)
                    {
                        rotZ = Mathf.Clamp(rotZ + (input.x * rotationSpeed * deltaTime), rotationMinMax.x, rotationMinMax.y);
                    }
                    else
                    {
                        rotZ += (input.x * rotationSpeed * deltaTime);
                    }
                    float velocityScale = 1f;

                    if (usingOverdrive || ((movingBack || movingForward) && usingOverdrive))
                    {
                        velocityScale = overdriveForce;
                    }
                    else if (movingBack || movingForward)
                    {
                        velocityScale = pullForce;
                    }
                    
                    RB.velocity = ((Quaternion.AngleAxis(rotZ, Vector3.forward) * Vector3.right) * (velocityScale)) + curFish.FightVelocity;

                    //Debug.DrawRay(transform.position, RB.velocity * 2, Color.yellow);
                    fishMeter.transform.GetChild(0).up = (Quaternion.AngleAxis(rotZ, Vector3.forward) * Vector3.right);

                }

                if (usingOverdrive)
                {
                    if (curOverdriveTime >= overdriveTime)
                    {
                        GoTopside();
                        //This is where player should die!!
                        //Debug.Log("Die");
                    }
                    else
                    {
                        curOverdriveTime += deltaTime;

                    }
                }
                else if (curOverdriveTime > 0)
                {
                    curOverdriveTime -= deltaTime;
                }

                fishMeter.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().color =
                    fuelGauge.Evaluate((1 / overdriveTime) * curOverdriveTime);
                if (curFish != null)
                {
                    fishMeter.transform.rotation = Quaternion.identity;
                    fishMeter.SetActive(true);
                    fishMeter.transform.GetChild(1).up = -curFish.FightVelocity.normalized;
                    //Debug.DrawRay(transform.position, (((Vector3)input.normalized * pullForce) + curFish.FightVelocity) * 2, Color.green);
                    //Debug.DrawRay(curFish.transform.position, curFish.FightVelocity * 2, Color.yellow);
                }
            }
        }
        
        if (transform.position.y >= GameManager.GM.SeaLevel)
        {
            GoTopside();
        }

        if (transform.position.y < 0)
        {
            myCamera.transform.position = Vector3.Lerp(myCamera.transform.position, new Vector3(myCamera.transform.position.x, transform.position.y, myCamera.transform.position.z), Time.fixedDeltaTime);
        }
    }
    public void OnGUI()
    {
        GUILayout.Label(RB.velocity.ToString());
    }

    private void GoTopside()
    {
        transform.GetChild(1).gameObject.SetActive(true);
        LeanTween.scale(transform.GetChild(1).GetChild(1).gameObject, new Vector3(3, 3, 3), 1f);

        if (attachedObject != null)
        {
            if (attachedObject.GetComponent<ValueBehavior>())
            {
                if (curOverdriveTime < overdriveTime)
                {
                    GameManager.GM.AddScore(attachedObject.GetComponent<ValueBehavior>().Value);
                }

                attachedObject = null;
                Destroy(attachedObject);
                print("called");
            }

            rotationSpeed = nonCombatRotation;
            doClamp = true;
        }

        GameManager.GM.TransitionTopSide(curFish != null, curOverdriveTime >= overdriveTime);

        if (isInCombat)
        {
            fishMeter.SetActive(false);
            isInCombat = false;
            Destroy(curFish.gameObject);
            curFish = null;
            
        }

        curOverdriveTime = 0;

        myMagnet.enabled = true;
        myMagnet.GetComponent<Collider>().enabled = true;

        RB.velocity = Vector3.zero;
    }

    private void UpdatePropellerSpin()
    {
        var propellerDot = Vector3.Dot(RB.velocity, transform.up);
        Variables.Object(propellerVariablesObject.gameObject).Set("SpeedMultiplier", propellerDot);
    }

    public void RotateDrone(InputAction.CallbackContext _ctx)
    {
        input = _ctx.ReadValue<Vector2>();
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

    public void MoveBackward(InputAction.CallbackContext _ctx)
    {
        if (_ctx.started)
        {
            movingBack = true;
        }

        else if (_ctx.canceled)
        {
            movingBack = false;
        }
    }

    public void UsingOverdrive(InputAction.CallbackContext _ctx)
    {
        if (_ctx.started)
        {
            usingOverdrive = true;
            GameManager.GM.DroneMoveFaster(true);
        }

        else if (_ctx.canceled)
        {
            usingOverdrive = false;
            GameManager.GM.DroneMoveFaster(false);
        }
    }

    public void StickToObject(GameObject _obj)
    {
        if (_obj.tag == "fish")
        {
            if (_obj.GetComponent<FishBehavior>())
            {
                curFish = _obj.GetComponent<FishBehavior>();
                doClamp = false;
                rotationSpeed = combatRotation;
                isInCombat = true;
                curFish.ChangeFishState(FishState.COMBAT);
                //Debug.Log("Attach to Fish");
            }
        }

        myMagnet.enabled = false;
        myMagnet.GetComponent<Collider>().enabled = false;
        attachedObject = _obj;
        print("Object Attached: " + _obj.name);

        GameManager.GM.PlayMagnetConncet();
    }

    public void IncreaseFuel(float _value) 
    {
        overdriveTime += _value;
    }

    public void IncreaseBasePullForce(float _value)
    {
        pullForce = _value;
    }

    public void IncreaseOverdriveForce(float _value)
    {
        overdriveForce += _value;
    }

    private void OnEnable()
    {
        myMagnet.enabled = true;
    }

    private void OnDisable()
    {
        myMagnet.enabled = false;
    }
}
