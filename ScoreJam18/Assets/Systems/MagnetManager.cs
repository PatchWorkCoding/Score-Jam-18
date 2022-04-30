using System;
using System.Collections.Generic;
using UnityEngine;

public class MagnetManager : MonoBehaviour
{
    public static void DeregisterMagnet(Magnet magnet)
    {
        s_registeredMagnets.Remove(magnet);
    }

    public static void RegisterMagnet(Magnet magnet)
    {
        s_registeredMagnets.Add(magnet);
    }

    public static void VisitMagnets(Action<Magnet> visitorCallback)
    {
        for (var i = 0; i < s_registeredMagnets.Count; i++)
        {
            if (s_registeredMagnets[i] == null)
            {
                continue;
            }

            visitorCallback?.Invoke(s_registeredMagnets[i]);
        }
    }

    private static readonly List<Magnet> s_registeredMagnets = new();

    private void FixedUpdate()
    {
        VisitMagnets
        (
            currentMagnet =>
            {
                var currentRelativeVelocity = Vector3.zero;
                VisitMagnets
                (
                    otherMagnet =>
                    {
                        if (currentMagnet == otherMagnet)
                        {
                            return;
                        }

                        currentRelativeVelocity += currentMagnet.GetAttractionVelocity(otherMagnet);
                    }
                );

                currentMagnet.AddRelativeVelocity(currentRelativeVelocity * Time.fixedDeltaTime);
            }
        );
    }
}