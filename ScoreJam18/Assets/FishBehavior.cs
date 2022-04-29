using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FishBehavior : MonoBehaviour
{
    [SerializeField]
    PlayerBehavior myPlayer = null;
    [SerializeField]
    float pullForce = 10, updateTime = 0.5f;

    FishState curState = FishState.WANDERING;
    Rigidbody RB = null;

    bool isResting = false;
    float timeSinceLastUpdate = 0;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Init() 
    {
        RB = GetComponent<Rigidbody>();

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
    }

    public void UpdateState()
    {
        switch (curState)
        {
            case FishState.WANDERING:

                //RB.velocity = new Vector2(1, 0);
                break;

            case FishState.COMBAT:
                isResting = isResting == false;
                break;

            default:
                Debug.Log("There is no functionality for this state");
                break;
        }
    }

    public void PullFish(float _force)
    {
        if (!isResting)
        {
            RB.velocity = (transform.position - myPlayer.transform.position) * (pullForce + _force);
        }
        else
        {
            RB.velocity = (transform.position - myPlayer.transform.position) *  _force;
        }
    }

    public void ChangeFishState(FishState _state)
    {
        curState = _state;
    }
    
}


public enum FishState 
{
    WANDERING,
    COMBAT
}
