using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    [SerializeField]
    float pullForce = 1, throwForce = 20;
    [SerializeField]
    GameObject lure = null, fish = null;

    float angle = 0;

    FishBehavior curFish = null;

    bool isPullingFish = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(angle, -Vector3.forward) * Vector3.right);

        if (curFish != null)
        {
            curFish.PullFish(isPullingFish ? -pullForce : 0);
        }
        
    }

    public void CastLine(InputAction.CallbackContext _ctx)
    {
        if (curFish == null)
        {
            if (_ctx.performed)
            {
                Instantiate(lure, transform.position, Quaternion.identity).GetComponent<LureBehavior>().Init(throwForce,
                    fish.transform.position - transform.position, this);
            }
        }
        else
        {
            if (_ctx.started)
            {
                isPullingFish = true;
            }
            else if(_ctx.canceled)
            {
                isPullingFish = false;
            }
        }
    }

    public void AngleRod(InputAction.CallbackContext _ctx) 
    {
        angle += _ctx.ReadValue<float>();
    }

    public void CaughtFish(FishBehavior _fish) 
    {
        curFish = _fish;
    }
}
