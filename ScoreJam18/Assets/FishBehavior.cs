using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ValueBehavior), typeof(Magnet))]
public class FishBehavior : MonoBehaviour
{
    [Header("Base Fish Stats")]
    [SerializeField]
    float SIZE;
    [SerializeField]
    float STAMINA;
    [SerializeField]
    int BASEPOINTVALUE = 0;
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
    [SerializeField]
    private float pullForce = 1;
    [SerializeField]
    private float MaxpullForce = 2;

    private Vector3 fightVelocity;
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
    
    bool doingWallCheck = false;
    Magnet myMagnet = null;
    

    // Start is called before the first frame update
    // Set tween to Keep The parent Rotation, for both

    void Start()
    {
        //Init();
    }

    public void Init()
    {
        RB = GetComponent<Rigidbody>();
        SIZE = Random.Range(0.1f, 1.5f);
        STAMINA = 3 - SIZE;
        BASEPOINTVALUE += Mathf.CeilToInt(SIZE * 100);
        GetComponent<ValueBehavior>().Value = BASEPOINTVALUE;
        gameObject.transform.localScale = new Vector3(SIZE, SIZE, SIZE);

        transform.position = GameManager.GM.MakeRarityDepthPos(GetComponent<ValueBehavior>().Value);
        myMagnet = GetComponent<Magnet>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(RB.position, RB.transform.GetChild(0).right * whiskerDistance, Color.red);
        Debug.DrawRay(RB.position, -RB.transform.GetChild(0).right * whiskerDistance, Color.red);
    }

    private void FixedUpdate()
    {
        RB.velocity = (transform.right * speed) + myMagnet.Velocity;

        Debug.DrawRay(transform.position, RB.velocity, Color.black);
        Debug.DrawRay(transform.position, myMagnet.Velocity, Color.yellow);

        if (timeSinceLastUpdate >= updateTime)
        {
            UpdateState();
            timeSinceLastUpdate = 0;
        }
        else
        {
            timeSinceLastUpdate += Time.fixedDeltaTime;
        }


        if (curState == FishState.WANDERING)
        {
            if (Physics.Raycast(RB.position, RB.transform.right, out whiskerhit, whiskerDistance))
            {
                changeDirection(whiskerhit.normal);
                Debug.Log(whiskerhit.collider.name);
            }

            if (transform.position.y >= GameManager.GM.SeaLevel)
            {
                Destroy(gameObject);
            }
        }
    }

    public void UpdateState()
    {
        if (!Caught)
        {
            switch (curState)
            {
                case FishState.WANDERING:
                    if (!doingWallCheck)
                    {
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
                    }
                    break;

                case FishState.COMBAT:
                    isResting = isResting == false;
                    ChanghFightVelocity();
                    break;

                case FishState.WALLCHECK:
                    break;

                default:
                    Debug.Log("There is no functionality for this state");
                    break;
            }
        }
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

        if (!doingWallCheck)
        {
            doingWallCheck = true;

            Vector3 Rbrot = RB.transform.eulerAngles;
            Rbrot.y += 180;
            Rbrot.z = newZ;
            Rbrot.x = 0;
            LeanTween.cancel(gameObject);
            LeanTween.rotate(gameObject, Rbrot, 1f);
            LeanTween.delayedCall(1.5f, () => doingWallCheck = false);
        }

        /*if (RB.transform.GetChild(0).rotation.eulerAngles.y != 0)
        {

        }*/
    }

    private void OnDestroy()
    {
        GameManager.GM.RemoveFish(gameObject);
    }

    void ChanghFightVelocity()
    {
        fightVelocity = (Quaternion.AngleAxis(Random.Range(-45f, 45f), Vector3.forward) * Vector3.down) *  (isResting ? pullForce : MaxpullForce);
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
