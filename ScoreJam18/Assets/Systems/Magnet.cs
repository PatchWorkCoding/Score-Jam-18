using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class Magnet : MonoBehaviour
{
    public bool CanAttract => GetValidChildCount() == 0 && m_parentMagnet == null;

    public Vector3 Velocity { get; private set; } = Vector3.zero;

    public void AddRelativeVelocity(Vector3 velocity)
    {
        if (isFish)
        {
            Velocity += velocity;
        }
        else
        {
            if (m_rigidbody != null)
            {
                m_rigidbody.velocity += velocity;
            }
            else
            {
                transform.position += velocity;
            }
        }
    }

    public Vector3 GetAttractionVelocity(Magnet otherMagnet, float attractionForce, float repelForce)
    {
        if (!otherMagnet.CanAttract)
        {
            return Vector3.zero;
        }

        var areAttracting = otherMagnet.m_isNegative != m_isNegative;
        if (areAttracting)
        {
            var direction = (otherMagnet.transform.position - transform.position).normalized * attractionForce;
            return direction;
        }
        else
        {
            var direction = (transform.position - otherMagnet.transform.position).normalized * repelForce;
            return direction;
        }
    }


    [SerializeField] private bool isFish;

    [SerializeField] private float m_attractionForce = 5f;
    [SerializeField] private Transform m_beamSource;
    private readonly List<Magnet> m_childMagnets = new();

    private readonly List<Magnet> m_collidingMagnets = new();

    [SerializeField] private Vector3 m_connectionOffset = Vector3.zero;
    [SerializeField] private bool m_enableBeam = true;
    private readonly RaycastHit[] m_hits = new RaycastHit[10];
    [SerializeField] private bool m_isNegative;

    private bool? m_lastPolarity;

    // Zack: We store the rigidbody local pos/rot so we can have the rigidbody follow a parent rigidbody.
    //       New unity does not support rigidbodies as children
    private Vector3 m_localPosition = Vector3.zero;
    private Quaternion m_localRotation = Quaternion.identity;
    [SerializeField] private MeshRenderer[] m_meshRenderers = Array.Empty<MeshRenderer>();

    /// <summary>
    ///     Fired when this magnet combines with another. The GameObject passed in is the other magnet.
    /// </summary>
    [SerializeField] private UnityEvent<GameObject> m_onCombine;

    private Magnet m_parentMagnet;

    [SerializeField] private int m_priority;
    [SerializeField] private float m_radius = 1f;
    [SerializeField] private float m_repelForce = 5f;
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private float m_rotationOffset = -90f;

    [SerializeField] private float m_sphereCastDistance = 1f;
    [SerializeField] private float m_sphereCastRadius = 1f;
    [SerializeField] private string m_targetShaderName = "metalPurple(Clone)";

    private void BreakApartFromParent()
    {
        if (m_parentMagnet == null)
        {
            return;
        }

        m_parentMagnet.m_childMagnets.Remove(this);
        m_parentMagnet = null;
        m_rigidbody.isKinematic = false;
    }

    private void CombineMagnets(Magnet firstMagnet, Magnet second)
    {
        var newParent = default(Magnet);
        var newChild = default(Magnet);
        if (firstMagnet.m_priority > second.m_priority)
        {
            newParent = firstMagnet;
            newChild = second;
        }
        else if (second.m_priority > firstMagnet.m_priority)
        {
            newParent = second;
            newChild = firstMagnet;
        }
        else
        {
            // Zack: if equal, take first
            newParent = firstMagnet;
            newChild = second;
        }

        newParent.m_childMagnets.Add(newChild);
        newParent.m_onCombine.Invoke(newChild.gameObject);

        newChild.m_parentMagnet = newParent;
        newChild.m_rigidbody.isKinematic = true;
        newChild.m_onCombine.Invoke(newParent.gameObject);

        // Zack: Find our relative location in terms of the other magnet and store it
        newChild.m_localPosition = newParent.transform.InverseTransformPoint(newChild.transform.position);
        newChild.m_localRotation = Quaternion.Inverse(newParent.transform.rotation) * newChild.transform.rotation;
    }

    private void FixedUpdate()
    {
        UpdateOffset();
        UpdateMagnetism();
    }

    private int GetValidChildCount()
    {
        if (m_childMagnets.Count == 0)
        {
            return 0;
        }

        var validCount = 0;
        for (var i = 0; i < m_childMagnets.Count; i++)
        {
            if (m_childMagnets[i] != null)
            {
                validCount++;
            }
        }

        return validCount;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Zack: If the collision is with ourselves, then ignore
        if (collision.transform.IsChildOf(transform))
        {
            return;
        }

        var otherMagnet = collision.transform.GetComponentInChildren<Magnet>(true);
        if (otherMagnet == null)
        {
            return;
        }

        // Zack: Opposites attract
        if (otherMagnet.m_isNegative == m_isNegative)
        {
            return;
        }

        m_collidingMagnets.Add(otherMagnet);

        // Zack: We have to wait for both to be set
        if (!m_collidingMagnets.Contains(otherMagnet) || !otherMagnet.m_collidingMagnets.Contains(this))
        {
            return;
        }

        var first = otherMagnet.m_collidingMagnets.FirstOrDefault();
        var second = otherMagnet;
        CombineMagnets(first, second);
    }

    private void OnCollisionExit(Collision collision)
    {
        var otherMagnet = collision.transform.GetComponentInChildren<Magnet>(true);
        if (otherMagnet == null)
        {
            return;
        }

        m_collidingMagnets.Remove(otherMagnet);
    }

    private void OnDisable()
    {
        MagnetManager.DeregisterMagnet(this);
    }

    private void OnDrawGizmosSelected()
    {
        if (m_enableBeam && m_beamSource != null)
        {
            var start = m_beamSource.position;
            var finish = start + m_beamSource.forward * m_sphereCastDistance;
            Gizmos.DrawLine(start, finish);
            Gizmos.DrawWireSphere(start, m_sphereCastRadius);
            Gizmos.DrawWireSphere(finish, m_sphereCastRadius);
        }
    }

    private void OnEnable()
    {
        MagnetManager.RegisterMagnet(this);
    }

    private void OnValidate()
    {
        SetSpriteColor(m_isNegative);
    }

    private void SetSpriteColor(bool isNegative)
    {
        var targetColor = isNegative ? Color.blue : Color.red;
        foreach (var meshRenderer in m_meshRenderers)
        {
            if (meshRenderer == null)
            {
                continue;
            }

            foreach (var material in meshRenderer.sharedMaterials)
            {
                if (material == null)
                {
                    continue;
                }

                if (material.name != m_targetShaderName)
                {
                    continue;
                }

                material.color = targetColor;
            }
        }
    }

    private void Update()
    {
        if (!m_lastPolarity.HasValue || m_isNegative != m_lastPolarity)
        {
            SetSpriteColor(m_isNegative);
            m_lastPolarity = m_isNegative;
        }
    }

    private void UpdateMagnetism()
    {
        if (!m_enableBeam)
        {
            return;
        }

        if (m_beamSource == null)
        {
            return;
        }

        var hitCount = Physics.SphereCastNonAlloc(transform.position, m_sphereCastRadius, m_beamSource.forward, m_hits, m_sphereCastDistance);
        for (var i = 0; i < hitCount; i++)
        {
            var hitTransform = m_hits[i].transform;
            if (hitTransform == null)
            {
                continue;
            }

            var otherMagnet = hitTransform.GetComponentInChildren<Magnet>();
            if (otherMagnet == null)
            {
                continue;
            }

            if (otherMagnet == this)
            {
                continue; // Ignore ourselves
            }

            var attractionVelocity = otherMagnet.GetAttractionVelocity(this, m_attractionForce, m_repelForce) * Time.fixedDeltaTime;
            otherMagnet.AddRelativeVelocity(attractionVelocity);
        }
    }

    private void UpdateOffset()
    {
        if (m_parentMagnet == null)
        {
            return;
        }

        transform.position = m_parentMagnet.transform.TransformPoint(m_parentMagnet.m_connectionOffset);
        var distance = m_parentMagnet.transform.position - transform.position;
        var angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg + m_rotationOffset;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}