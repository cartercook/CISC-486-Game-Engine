using UnityEngine;

public class VillagerFSM : FSM {
	// Visible in editor
	[SerializeField] float _speed = 5.0f;
	[SerializeField] float _pickingDistance = 5.0f;
	[SerializeField] float _dropOffDistance = 0.1f;
	[SerializeField] float _turnSpeed = 5;
	[SerializeField] float _updatesPerSeconds = 60;

	public static readonly string
	// Triggers used to change state
		WALK = "Walk",
		IDLE = "Idle",
		DROP = "Drop",
		PICK = "Pickup",	
	// Names of animation states
		IDLE_STATE = "idle",
		WALKING_STATE = "walking",
		DROP_STATE = "dropping",
		PICKUP_STATE = "pickingFruit";

	int _apples = 0;
	float _updateCounter = 0;
	Transform target = null;
	Animator _anim;



	// Initialization and update loop

	new void Start() {
		//initialize character
		_anim = GetComponent<Animator>();

		//initialize FSM
		base.Start();
	}

	void Update() {
		_updateCounter += Time.deltaTime;

		if (_updateCounter >= 1/_updatesPerSeconds) {
			//update state and handle transitions
			base.Update(_updateCounter);
			_updateCounter = 0;
		}
	}



	// Getters

	public float pickingDistance {
		get { return _pickingDistance; }
	}

	public float dropOffDistance {
		get { return _dropOffDistance; }
	}

	public float distanceToTarget {
		get { return (transform.position-target.transform.position).magnitude; }
	}

	public int targetTreeApples {
		get {
			FruitTree tree = target.GetComponent<FruitTree>();
			if (tree != null) {
				return tree.apples;
			} else {
				return 0;
			}
		}
	}

	public bool inAnimState(string name) {
		return _anim.GetCurrentAnimatorStateInfo(0).IsName(name) && !_anim.IsInTransition(0);
	}


	//Setters

	public int apples {
		get { return _apples; }
		set { _apples = value; }
	}

	public void setAnimTrigger(string name) {
		_anim.SetTrigger(name);
	}

	public void setTarget(Transform target) {
		this.target = target;
	}



	// Public functions

	//Triggered by mecanim during the picking animation
	public void incrementApples() {
		FruitTree tree = target.GetComponent<FruitTree>();
		if (tree != null) {
			tree.apples--;
			apples++;
		}
	}

	public bool moveToTarget(float elapsed) {
		//set turn rotation (remove pitch)
		Vector3 direction = target.position - transform.position;
		direction = Vector3.ProjectOnPlane(direction, Vector3.up);
		Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
		transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, _turnSpeed * elapsed);

		//move forward
		transform.position += transform.forward * _speed * elapsed;
		transform.position = new Vector3(
			transform.position.x,
			Terrain.activeTerrain.SampleHeight(transform.position),
			transform.position.z);
		return false;
	}

	public FruitTree findNextTree() {
		foreach (GameObject g in GameObject.FindGameObjectsWithTag("fruitTree")) {
			FruitTree tree = g.GetComponent<FruitTree>();
			if (tree.apples > 0) {
				return tree;
			}
		}

		return null;
	}
}
