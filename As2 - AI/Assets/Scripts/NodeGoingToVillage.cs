using UnityEngine;

public class NodeGoingToVillage : VillagerNode {
	public NodeGoingToVillage(FSM fsm) : base(fsm) {}

	public override void enter() {
		villager.setAnimTrigger(VillagerFSM.WALK);
		villager.setTarget(GameObject.FindGameObjectWithTag("village").transform);
	}

	public override void update(float elapsed) {
		villager.moveToTarget(elapsed);
	}

	public override FSMNode checkExitNode(FSM fsm) {
		if (villager.distanceToTarget <= villager.dropOffDistance) {
			return new NodeInVillage(fsm);
		} else {
			return null;
		}
	}
}