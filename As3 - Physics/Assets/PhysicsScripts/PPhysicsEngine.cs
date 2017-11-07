using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is the physics engine. It should be attached to an empty game object in
/// the scene. It is invoked via Unity's FixedUpdate function. It manages a set of
/// rigid bodies of type PPhysicsBody. These must be added to the physics engine
/// via a call to AddBody.
/// </summary>
public class PPhysicsEngine : MonoBehaviour {

	private const float groundedDistanceThreshold = 0.1f;

	List<PPhysicsBody> bodies = new List<PPhysicsBody>();

	public void AddBody(PPhysicsBody newBody) {
		Debug.Assert(newBody != null);
		Debug.Assert(bodies!=null);
		bodies.Add(newBody);
	}

	/// <summary>
	/// Determines whether two axis-aligned bounded boxes (rectangles) intersect.
	/// </summary>
	/// <param name="LL1">lower-left corner of first box</param>
	/// <param name="UR1">upper-right corner of first box</param>
	/// <param name="LL2">lower-left corner of second box</param>
	/// <param name="UR2">upper-right corner of second box</param>
	bool Intersecting(Vector2 LL1, Vector2 UR1, Vector2 LL2, Vector2 UR2) {
		return !(UR1.x<LL2.x || UR2.x<LL1.x || UR1.y<LL2.y || UR2.y<LL1.y);
	}

	/// <summary>
	/// Returns true if the two bodies were not intersecting, but now are intersecting
	/// </summary>
	/// <param name="b1">first body</param>
	/// <param name="b2">second body</param>
	bool Colliding(PPhysicsBody b1, PPhysicsBody b2) {
		return Intersecting(b1.LL, b1.UR, b2.LL, b2.UR) && !Intersecting(b1.LLold, b1.URold, b2.LLold, b2.URold);
	}

	/// <summary>
	/// Returns a list of all <i>new</i> collisions. Collisions describe pairs of bodies
	/// that did not collide during the last iteration of the physics engine, but
	/// do collide now.
	/// </summary>
	/// <returns>The for collisions.</returns>
	List<Contact> CheckForCollisions() {
		List<Contact> contacts = new List<Contact>();

		// Compare each pair of bodies to see if they have collided. Be
		// careful to consider each pair only once.
		for(int i=0; i<bodies.Count-1; i++) {
			for(int j = i+1; j<bodies.Count; j++) {
				if(Colliding(bodies[i], bodies[j])) {
					Contact c = new Contact(bodies[i], bodies[j]);
					contacts.Add(c);
				}
			}
		}
		return contacts;
	}

	void IntegrateAll(float deltaTime) {
		foreach(PPhysicsBody pb in bodies) {
			pb.Integrate(deltaTime);
		}
	}

	/// <summary>
	/// Returns true if b is in resting on (or just above) another body. Specifically,
	/// is there another component below this one within the distance
	/// 'groundedDistanceThreshold'.
	/// </summary>
	/// <param name="b">The component we are checking for grounded</param>
	public bool Grounded(PPhysicsBody b) {
		// iterate through all bodies; if b is just above any of them, return true
		foreach(PPhysicsBody b2 in bodies) {
			if(b != b2) {
				if(b.LL.x < b2.UR.x && b.UR.x > b2.LL.x
						&& Mathf.Abs(b.LL.y - b2.UR.y) <= groundedDistanceThreshold) {
					return true;
				}
			}
		} 
		return false;
	}
	
	void ResolveCollisions(List<Contact> contacts) {
		foreach(Contact c in contacts) {
			c.ResolveContact();
		}
	}
	
	void CommitAll() {
		foreach (PPhysicsBody pb in bodies) {
			pb.Commit();
		}
	}

	/// <summary>
	/// The main method of the physics engine - integrates, checks for collisions,
	/// resolves all collisisons.
	/// </summary>
	void UpdatePhysics() {
		IntegrateAll(Time.fixedDeltaTime);
		List<Contact> contacts = CheckForCollisions();
		ResolveCollisions(contacts);
		CommitAll();
	}


	/// <summary>
	/// On each update tick, run the physic engine - integrate, detect collisions,
	/// respond to collisions
	/// </summary>
	void FixedUpdate () {
		UpdatePhysics();
	}
}

/// <summary>
/// Represents a collision between two rigid bodies. A collision requires that the
/// bodies were not overlapping in the last physics engine frame, but are overlapping now.
/// This class also resolves contacts, applying necessary impulses to the two bodies
/// to enact the results of the collision.
/// </summary>
class Contact {
	public PPhysicsBody _b1;
	public PPhysicsBody _b2;

	/// <summary>
	/// Records the colliding physics bodies and computes the contact normal
	/// </summary>
	/// <param name="b1">B1.</param>
	/// <param name="b2">B2.</param>
	public Contact(PPhysicsBody b1, PPhysicsBody b2) {
		_b1 = b1;
		_b2 = b2;
	}

	public void ResolveContact() {
		Vector2 contactNormal = calculateContactNormal(_b1, _b2);

		//Debug.Log(_b1.name+" contactNormal: "+contactNormal);

		float t = Time.fixedDeltaTime;
		if (contactNormal == Vector2.up) {
			//b1's upper face collides with b2's lower face
			t = timeToImpact(_b1.URold.y, _b1.Velocity.y, _b2.LLold.y, _b2.Velocity.y);
		} else if (contactNormal == Vector2.down) {
			//b1's lower face collides with b2's upper face
			t = timeToImpact(_b1.LLold.y, _b1.Velocity.y, _b2.URold.y, _b2.Velocity.y);
		} else if (contactNormal == Vector2.right) {
			//b1's right face collides with b2's left face
			t = timeToImpact(_b1.URold.x, _b1.Velocity.x, _b2.LLold.x, _b2.Velocity.x);
		} else if (contactNormal == Vector2.left) {
			//b1's  face collides with b2's lower face
			t = timeToImpact(_b1.LLold.x, _b1.Velocity.x, _b2.URold.x, _b2.Velocity.x);
		} else {
			Debug.Assert(false);
		}

		//Debug.Log("t: "+t+", Time.fixedDeltaTime: "+Time.fixedDeltaTime);

		_b1.Revert();
		_b2.Revert();

		float closingVelocity = Vector2.Dot(_b1.Velocity - _b2.Velocity, contactNormal); //portion of velocity parrallel to contactNormal
		float inverseMass = 1/_b1.mass + 1/_b2.mass;

		//move to collision
		_b1.position += _b1.Velocity*t;
		_b2.position += _b2.Velocity*t;

		applyCollision(_b1, closingVelocity, inverseMass, contactNormal, t);
		applyCollision(_b2, closingVelocity, inverseMass, -contactNormal, t);
	}

	//Algorithm: https://gamedev.stackexchange.com/questions/47888/find-the-contact-normal-of-rectangle-collision
	Vector2 calculateContactNormal(PPhysicsBody b1, PPhysicsBody b2) {
		//ray at b1.position pointing towards combined AABB
		Ray2D ray = new Ray2D(b1.OldPosition, b1.position - b1.OldPosition);

		Vector2 rayNormal = ray.direction.normalized;
		Vector2 dirfrac = new Vector2(1/rayNormal.x, 1/rayNormal.y); // Vector2(1/cos(theta), 1/sin(theta))

		Vector2 aabbLL = b2.LLold - (b1.URold - b1.LLold)/2; //b2.LLold - 1/2 size of b1
		Vector2 aabbUR = b2.URold + (b1.URold - b1.LLold)/2; //b2.URold + 1/2 size of b1

		Debug.DrawLine(ray.origin, ray.origin + ray.direction, Color.red);
		DebugDrawBox(aabbLL, aabbUR);

		//(vector from b1.pos to LL and UR corners of combined AABB) / (cos(rayAngle), sin(rayAngle))
		//Hyp=Opp/Sin(angle), Hyp=Adj/Cos(angle)
		Vector2 t1 = Vector2.Scale(aabbLL - ray.origin, dirfrac);
		Vector2 t2 = Vector2.Scale(aabbUR - ray.origin, dirfrac);

		//Debug.Log(t1 +", "+ t2);

		float t = Mathf.Max(Mathf.Min(t1.x, t2.x), Mathf.Min(t1.y, t2.y));

		//return the face that intersects the ray
		if (t == t1.x) return Vector2.right;
		if (t == t2.x) return Vector2.left;
		if (t == t1.y) return Vector2.up;
		if (t == t2.y) return Vector2.down;

		//oops
		Debug.Assert(false);
		return Vector2.zero;
	}

	//solve for time when p'1 == p'2 (where p is a substitute for b1.UR x or y, or b1.LL x or y, depending on direction of collisionNormal)
	// p1 + v1*t = p2 + v2*t    v1.y*t - v2.y*t = (p2 - p1)    (v1 - v2)*t = (p2 - p1)    t = (p2 - p1)/(v1 - v2)
	float timeToImpact(float p1, float v1, float p2, float v2) {
		return (p2 - p1)/(v1 - v2);
	}

	void applyCollision(PPhysicsBody b, float closingVelocity, float inverseMass, Vector2 contactNormal, float t) {
		float c; //coefficient of restitution
		if (b.rebounds) {
			c = 0.9f;
		} else {
			c = 0;
		}

		float deltaV = closingVelocity + c*closingVelocity;
		Vector2 impulse = (deltaV/inverseMass)*contactNormal;

		//set post-collision velocity
		b.Velocity -= impulse/b.mass;
		//update position using remaining time after collision
		b.position += b.Velocity*(Time.fixedDeltaTime - t);

		//Debug.Log("closingVelocity: "+closingVelocity+
		//	      ", c: "+c+
		//	      ", deltaV: "+deltaV+
		//	      ", inverseMass: "+inverseMass+
		//	      ", contactNormal: "+contactNormal+
		//	      ", impulse: "+impulse+
		//	      ", b.mass: "+b.mass+
		//	      ", b.Velocity: "+b.Velocity+
		//	      ", b.OldVelocity: "+b.OldVelocity);
	}

	void DebugDrawBox(Vector2 LL, Vector2 UR) {
		Vector2 UL = new Vector2(LL.x, UR.y);
		Vector2 LR = new Vector2(UR.x, LL.y);
		Debug.DrawLine(LL, UL, Color.green);
		Debug.DrawLine(UL, UR, Color.green);
		Debug.DrawLine(UR, LR, Color.green);
		Debug.DrawLine(LR, LL, Color.green);
	}
}