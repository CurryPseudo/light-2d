using UnityEngine;
public abstract class PhysicModel
{
    public abstract Vector2 Velocity {
        get;
        set;
    }
    public abstract Vector2 Position {
        get;
        set;
    }
}