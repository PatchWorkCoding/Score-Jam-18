using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueBehavior : MonoBehaviour
{
    [SerializeField]
    int value = 0;

    public int Value 
    { 
        get { return value; }
        set { this.value = value; }
    }
}
