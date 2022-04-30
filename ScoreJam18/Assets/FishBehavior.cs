using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FishBehavior : MonoBehaviour
{

    [Header("Fish Stats")]
    [SerializeField]
    float str;
    [SerializeField]
    float size;
    [SerializeField]
    float pointValue;
    [SerializeField]
    float Stamina;
    [SerializeField]
    AnimationCurve flaten;
    [SerializeField]
    PlayerBehavior myPlayer = null;
    [SerializeField]
    float pullForce = 10, updateTime = 0.5f;

    Vector3 fightVelocity = Vector3.zero;

    [SerializeField]
    float timer = 0f;
    [SerializeField]
    GameObject MagnetLatch;
    Quaternion temp;
    bool timeKeeper;
    bool shouldChangeDirection;
    [SerializeField]
    float turnTImer = 0;
    [SerializeField]
    FishState curState = FishState.WANDERING;
    Rigidbody RB = null;
    RaycastHit feeler;
    RaycastHit feeler2;

    bool isResting = false;
    float timeSinceLastUpdate = 0;

    // Start is called before the first frame update
    // Set tween to Keep The parent Rotation, for both

    void Start()
    {
        Init();
    }

    void Init()
    {
        RB = GetComponent<Rigidbody>();

        shouldChangeDirection = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeSinceLastUpdate >= updateTime)
        {
            UpdateState();
            timeSinceLastUpdate = 0;
        }
        else
        {
            timeSinceLastUpdate += Time.deltaTime;

        }
        if (timeKeeper)
        {
            timer += Time.deltaTime;
        }
        timer += Time.deltaTime;
        turnTImer += Time.deltaTime;


        if (Physics.Raycast(RB.position, -gameObject.transform.GetChild(0).up, out feeler) ||
        Physics.Raycast(RB.position, gameObject.transform.GetChild(0).up, out feeler2))
        {
            curState = FishState.WALLCHECK;
        }
        

        Debug.DrawRay(RB.position, -gameObject.transform.GetChild(0).up * 10f, Color.black);
        Debug.DrawRay(RB.position, gameObject.transform.GetChild(0).up * 10f, Color.red);
    }

    private void FixedUpdate()
    {

        if (turnTImer >= 4f)
        {
            if (RB.velocity.normalized.x == 1f)
            {
                RB.velocity = transform.GetChild(0).up;
                turnTImer = 0;
            }
            else
            {
                RB.velocity = -transform.GetChild(0).up;
                turnTImer = 0;
            }
            print("called");
        }

    }
    public void UpdateState()
    {
        switch (curState)
        {
            case FishState.WANDERING:

                if (shouldChangeDirection)
                {
                    changeDirection();
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
                if (feeler.collider.name == "Cube" || feeler2.collider.name == "Cube")
                {
                    if (feeler.collider == true && feeler.distance > 10f)
                    {
                        LeanTween.rotate(gameObject, new Vector3(0, 180, 0), .5f);
                        RB.velocity = gameObject.transform.GetChild(0).forward;
                    }
                    else if (feeler2.collider == false)
                    {
                        curState = FishState.WANDERING;
                    }
                }

                break;
            default:
                Debug.Log("There is no functionality for this state");
                break;
        }
    }

    public void PullFish(float _force)
    {

    }

    public void ChangeFishState(FishState _state)
    {
        curState = _state;
    }

    void changeDirection()
    {
        if (!Physics.Raycast(RB.position, -gameObject.transform.GetChild(0).up, out feeler) &&
        !Physics.Raycast(RB.position, gameObject.transform.GetChild(0).up, out feeler2))
        {
            float rand2 = Random.Range(0, 2);
            float rand3 = Random.Range(0, 2);
            if (rand2 >= 1)
            {
                Vector3 rotation = new Vector3(0, 0, Random.Range(45f, 130f));
                LeanTween.rotate(transform.GetChild(0).gameObject, rotation, 1f);
                LeanTween.delayedCall(1f, () => timeKeeper = true);
            }
            LeanTween.rotate(transform.GetChild(0).gameObject, new Vector3(0, 0, 90), flaten.Evaluate(timer));
            if (timer >= 3)
            {
                timer = 0;
                timeKeeper = false;
            }

            if (rand3 >= 1)
            {
                if (gameObject.transform.rotation.y > 0f)
                {
                    LeanTween.delayedCall(2f, () => LeanTween.rotate(gameObject, new Vector3(0, 0, 0), .5f));
                }
                else
                {
                    LeanTween.delayedCall(2f, () => LeanTween.rotate(gameObject, new Vector3(0, 180, 0), .5f));
                }

            }
        }
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
