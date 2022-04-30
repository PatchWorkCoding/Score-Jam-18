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
    [SerializeField]
    float whiskerDistance = 2f;
    float whiskerDistance2 = 4f;
    bool isResting = false;
    float timeSinceLastUpdate = 0;

    bool shouldTurn;
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


        if (Physics.Raycast(RB.position, -gameObject.transform.GetChild(0).up, out feeler, whiskerDistance) ||
        Physics.Raycast(RB.position, gameObject.transform.GetChild(0).up, out feeler2, whiskerDistance2))
        {
            curState = FishState.WALLCHECK;
        }


        Debug.DrawRay(RB.position, -gameObject.transform.GetChild(0).up * whiskerDistance, Color.black);
        Debug.DrawRay(RB.position, gameObject.transform.GetChild(0).up * whiskerDistance2, Color.red);
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
                //fix
                if (feeler.collider == true)
                {
                    if (feeler.collider == true)
                    {
                        LeanTween.rotate(gameObject, new Vector3(0, 180, 0), .5f);
                        RB.velocity = gameObject.transform.GetChild(0).up;
                    }
                }
                else if (true)
                {

                }
                else
                {
                    curState = FishState.WANDERING;
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


        float rand2 = Random.Range(0, 10);
        

        if (!LeanTween.isTweening(transform.GetChild(0).gameObject))
        {
            if (rand2 >= 5)
            {
                Vector3 rotation = new Vector3(RB.transform.rotation.y, 0, Random.Range(45f, 130f));
                LeanTween.rotate(transform.GetChild(0).gameObject, rotation, 1f);
                
            }
            LeanTween.rotate(transform.GetChild(0).gameObject, new Vector3(RB.transform.rotation.y,0, 90), .5f);

            if (rand2 <= 5)
            {
                LeanTween.delayedCall(.1f, () => timeKeeper = true);
                if (gameObject.transform.rotation.y > 0f)
                {
                    Quaternion rotation2 = Quaternion.Euler(0f, 180f, 0f);
                    Quaternion.Lerp(RB.gameObject.transform.rotation, rotation2, flaten.Evaluate(timer));
                }
                else
                {
                    Quaternion rotation3 = Quaternion.Euler(0f, 180f, 0f);
                    Quaternion.Lerp(RB.gameObject.transform.rotation, rotation3, flaten.Evaluate(timer));
                }
            }
        }
        if (timer == 2)
        {
            timer = 0;
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
