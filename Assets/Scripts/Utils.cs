using UnityEngine;

[System.Serializable]
public struct Constraint
{
    public float min;
    public float max;

    public Constraint(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float Clamp(float value)
    {
        return Mathf.Clamp(value, min, max);
    }
}