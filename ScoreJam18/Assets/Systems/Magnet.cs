using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class Magnet : MonoBehaviour
{
    public bool CanAttract => m_childMagnets.Count == 0 && m_parentMagnet == null;

    public void AddRelativeVelocity(Vector3 velocity)
    {
        if (!m_allowAttraction)
        {
            return;
        }

        if (m_rigidbody != null)
        {
            m_rigidbody.velocity += velocity;
        }
        else
        {
            transform.position += velocity;
        }
    }

    public Vector3 GetAttractionVelocity(Magnet otherMagnet)
    {
        if (!otherMagnet.CanAttract)
        {
            return Vector3.zero;
        }

        var distance = Vector3.Distance(otherMagnet.transform.position, transform.position);
        var totalRadius = m_radius + otherMagnet.m_radius;
        var closeEnough = distance <= totalRadius;
        if (!closeEnough)
        {
            return Vector3.zero;
        }

        var areAttracting = otherMagnet.m_isNegative != m_isNegative;
        if (areAttracting)
        {
            var direction = (otherMagnet.transform.position - transform.position).normalized * (totalRadius - distance);
            return direction;
        }
        else
        {
            var direction = (transform.position - otherMagnet.transform.position).normalized * (totalRadius - distance);
            return direction;
        }
    }

    [SerializeField] private bool m_allowAttraction = true;
    private readonly List<Magnet> m_childMagnets = new();

    private readonly List<Magnet> m_collidingMagnets = new();

    [SerializeField] private Vector3 m_connectionOffset = Vector3.zero;

    [SerializeField] private bool m_isNegative;
    private bool? m_lastPolarity;

    // Zack: We store the rigidbody local pos/rot so we can have the rigidbody follow a parent rigidbody.
    //       New unity does not support rigidbodies as children
    private Vector3 m_localPosition = Vector3.zero;
    private Quaternion m_localRotation = Quaternion.identity;

    /// <summary>
    ///     Fired when this magnet combines with another. The GameObject passed in is the other magnet.
    /// </summary>
    [SerializeField] private UnityEvent<GameObject> m_onCombine;

    private Magnet m_parentMagnet;

    [SerializeField] private int m_priority;
    [SerializeField] private float m_radius = 1f;
    [SerializeField] private Rigidbody m_rigidbody;

    [SerializeField] private float m_rotationOffset = -90f;
    [SerializeField] private SpriteRenderer m_spriteRenderer;

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
        if (m_parentMagnet == null)
        {
            return;
        }

        transform.position = m_parentMagnet.transform.TransformPoint(m_parentMagnet.m_connectionOffset);
        var distance = m_parentMagnet.transform.position - transform.position;
        var angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg + m_rotationOffset;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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
        Gizmos.DrawWireSphere(transform.position, m_radius);

        var relativeVelocity = Vector3.zero;
        MagnetManager.VisitMagnets
        (
            otherMagnet =>
            {
                if (otherMagnet == this)
                {
                    return;
                }

                var directionToOther = GetAttractionVelocity(otherMagnet);
                if (directionToOther.magnitude > float.Epsilon)
                {
                    Gizmos.DrawWireSphere(otherMagnet.transform.position, otherMagnet.m_radius);
                }

                Gizmos.DrawLine(transform.position, transform.position + directionToOther);
                relativeVelocity += directionToOther;
            }
        );

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + relativeVelocity);
        Gizmos.color = Color.white;

        Gizmos.DrawSphere(transform.TransformPoint(m_connectionOffset), 1f);
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
        if (m_spriteRenderer == null)
        {
            return;
        }

        m_spriteRenderer.color = isNegative ? Color.blue : Color.red;
    }

    private void Update()
    {
        if (!m_lastPolarity.HasValue || m_isNegative != m_lastPolarity)
        {
            SetSpriteColor(m_isNegative);
            m_lastPolarity = m_isNegative;
        }
    }
}