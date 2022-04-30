using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;
    
    [SerializeField]
    float seaLevel = 10;

    [SerializeField]
    uint score = 0;
    [SerializeField]
    GameObject camera = null;
    [SerializeField]
    GameObject canvas = null;
    [SerializeField]
    Transform drone = null;
    [SerializeField]
    Transform ballon = null, waterSurface = null;

    private void Awake()
    {
        if (GM == null)
        {
            GM = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float SeaLevel
    { 
        get { return seaLevel; }
    }

    public void AddScore(uint _value)
    {
        score += _value;
    }

    public void CastDrone() 
    {
        LeanTween.move(drone.gameObject, waterSurface.position, 2);
        LeanTween.delayedCall(2f, () => drone.GetComponent<LureBehavior>().enabled = true);
        
        canvas.SetActive(false);
    }

    public void TransitionTopSide()
    {
        drone.GetComponent<LureBehavior>().enabled = false;

        LeanTween.move(drone.gameObject, ballon.position, 2);
        LeanTween.move(camera, new Vector3(0, 8, -90), 2);
        
        canvas.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(-100, seaLevel, 0), new Vector3(100, seaLevel, 0));
    }
}
