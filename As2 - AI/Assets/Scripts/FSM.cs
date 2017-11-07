using UnityEngine;

public abstract class FSM : MonoBehaviour {
	protected FSMNode current;

	protected void Start() {
		current = new NodeGoingToTree(this);
		current.enter();
	}
	
	protected void Update(float elapsed) {
		FSMNode nextNode = current.checkExitNode(this);
		if (nextNode != null) {
			current.exit();
			current = nextNode;
			current.enter();
		}

		current.update(elapsed);
	}
}
