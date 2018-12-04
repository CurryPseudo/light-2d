using UnityEngine;
public class RigidbodyModel : PhysicModel {
    private Rigidbody2D rigidbody;
    public RigidbodyModel(Rigidbody2D rigidbody) {
        this.rigidbody = rigidbody;
    }
    public override Vector2 Velocity {
        get {
            return rigidbody.velocity;
        }
        set {
            rigidbody.velocity = value;
        }
    }
    public override Vector2 Position {
        get {
            return rigidbody.position;
        }
        set {
            rigidbody.position = value;
        }
    }
}