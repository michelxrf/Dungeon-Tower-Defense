using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PowerUp", menuName = "New PowerUp")]
public class PowerUp : ScriptableObject
{
    public string powerUpName;
    public List<PowerUp> prerequisites = new List<PowerUp>();
    public bool isAcquired;
    public int cost;

    /// <summary>
    /// Returns true if all prerequisites are acquired and this PowerUp is not yet acquired.
    /// </summary>
    public bool CanAcquire()
    {
        if (isAcquired)
            return false;

        if (prerequisites == null)
            return true;

        foreach (PowerUp prerequisite in prerequisites)
        {
            if (prerequisite == null)
                continue;

            if (!prerequisite.isAcquired)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Acquires this PowerUp if prerequisites are met.
    /// Returns true on success, false otherwise.
    /// </summary>
    public bool Acquire()
    {
        if (!CanAcquire())
            return false;

        isAcquired = true;
        return true;
    }
}
