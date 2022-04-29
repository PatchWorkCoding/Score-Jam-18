using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LureBehavior : MonoBehaviour
{
    [SerializeField]
    AnimationCurve lureDeccelerationCurve;
    [SerializeField]
    float fallTimeScale = 0.5f;

    Rigidbody RB = null;
    bool isFalling = false;

    float fallTime = 0;
    float speed = 0;

    PlayerBehavior myPlayer = null;

    // Start is called before the first frame update
    public void Init(float _speed, Vector3 _dir, PlayerBehavior _player)
    {
        fallTime = 1;
        speed = _speed;
        myPlayer = _player;
        isFalling = true;

        RB = GetComponent<Rigidbody>();
        RB.velocity = _dir * _speed;
    }

    // Update is called once per frame
    void Update()
    {
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "fish")
        {
            myPlayer.CaughtFish(collision.gameObject.GetComponent<FishBehavior>());
            collision.gameObject.GetComponent<FishBehavior>().ChangeFishState(FishState.COMBAT);
            Destroy(gameObject);
        }
    }
}
