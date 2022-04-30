using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueBehavior : MonoBehaviour
{
    [SerializeField]
    uint value = 0;

    public uint Value 
    { 
        get { return value; }
    }
}
