using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ValueBehavior))]
public class FishBehavior : MonoBehaviour
{
    [Header("Base Fish Stats")]
    [SerializeField]
    float SIZE;
    [SerializeField]
    float STAMINA;
    [SerializeField]
    float POINTVALUE;
    [SerializeField]
    FishState curState = FishState.WANDERING;
    Rigidbody RB = null;
    RaycastHit feeler;
    RaycastHit feeler2;
    [SerializeField]
    float whiskerDistance = 2f;
    bool isResting = false;
    float timeSinceLastUpdate = 0;
    bool shouldTurn;
    private Vector3 fightVelocity;
    private float pullForce;
    private RaycastHit whiskerhit;
    [SerializeField]
    private float updateTime;

    private float speed;
    [SerializeField]
    private float Maxspeed;
    [SerializeField]
    private float Minspeed;
    private bool Caught;
    [SerializeField]
    private float dotDown;
    [SerializeField]
    private float dotup;
    [SerializeField]
    AnimationCurve SizeScaler;
    // Start is called before the first frame update
    // Set tween to Keep The parent Rotation, for both
    
    void Start()
    {
        Init();
    }

    void Init()
    {
        RB = GetComponent<Rigidbody>();
        
        SIZE = Random.Range(0.1f, 1.5f);
        STAMINA = 3 - SIZE;
        POINTVALUE = Mathf.CeilToInt(SIZE * 100);
        GetComponent<ValueBehavior>().Value = (uint)POINTVALUE;
        gameObject.transform.localScale = new Vector3(SIZE, SIZE, SIZE);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(RB.position, RB.transform.GetChild(0).right * whiskerDistance, Color.red);
        Debug.DrawRay(RB.position, -RB.transform.GetChild(0).right * whiskerDistance, Color.red);
    }

    private void FixedUpdate()
    {
        RB.velocity = transform.right * speed;


        if (timeSinceLastUpdate >= updateTime)
        {
            UpdateState();
            timeSinceLastUpdate = 0;
        }
        else
        {
            timeSinceLastUpdate += Time.fixedDeltaTime;
        }
        if (Physics.Raycast(RB.position, RB.transform.right, out whiskerhit, whiskerDistance))
        {
            changeDirection(whiskerhit.normal);

            Debug.Log(whiskerhit.collider.name);
        }

    }
    public void UpdateState()
    {
        if (!Caught)
        {


            switch (curState)
            {
                case FishState.WANDERING:

                    int roll2 = UnityEngine.Random.Range(0, 20);
                    var rotator = RB.rotation.eulerAngles;
                    int rad = UnityEngine.Random.Range(-45, 45);
                    rotator.z = rad;
                    LeanTween.rotate(gameObject, rotator, 1f);

                    if (roll2 % 2 == 0)
                    {
                        speed = Maxspeed;
                    }
                    else
                    {
                        speed = Minspeed;
                    }
                    break;

                case FishState.COMBAT:
                    isResting = isResting == false;
                    if (!isResting)
                    {
                        ChanghFightVelocity();
                    }
                    else
                    {
                        fightVelocity = Vector3.zero;
                    }
                    break;

                case FishState.WALLCHECK:
                    //fix



                    break;
                default:
                    Debug.Log("There is no functionality for this state");
                    break;
            }
        }
    }

    public void PullFish(float _force)
    {

    }

    public void ChangeFishState(FishState _state)
    {
        curState = _state;
    }

    void changeDirection(Vector3 hitNormal)
    {
        float newZ = RB.rotation.eulerAngles.z;
        float DotProd = Vector3.Dot(hitNormal, Vector3.up);
        float DotProd2electricbogaloo = Vector3.Dot(hitNormal, Vector3.down);
        if (DotProd >= dotup)
        {

            newZ = 45f;
        }
        else if (DotProd2electricbogaloo >= dotDown)
        {
            newZ = -45f;
        }
        if (curState != FishState.WALLCHECK)
        {
            curState = FishState.WALLCHECK;
            Vector3 Rbrot = RB.transform.eulerAngles;
            Rbrot.y += 180;
            Rbrot.z = newZ;
            LeanTween.cancel(gameObject);
            LeanTween.rotate(gameObject, Rbrot, 1f);
            LeanTween.delayedCall(1.5f, () => curState = FishState.WANDERING);
        }

        /*if (RB.transform.GetChild(0).rotation.eulerAngles.y != 0)
        {

        }*/
    }
    
    void ChanghFightVelocity()
    {
        fightVelocity = (Quaternion.AngleAxis(Random.Range(-45f, 45f), Vector3.forward) * Vector3.down) * pullForce;
    }

    public Vector3 FightVelocity
    {
        get { return fightVelocity; }
    }
}



public enum FishState
{
    WANDERING,
    COMBAT,
    WALLCHECK,
    SPAWN,
}
