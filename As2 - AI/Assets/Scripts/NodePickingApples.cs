using UnityEngine;

public class NodePickingApples : VillagerNode {
	public NodePickingApples(FSM fsm) : base(fsm) {}

	public override void enter() {
		villager.setAnimTrigger(VillagerFSM.PICK);
	}

	public override FSMNode checkExitNode(FSM fsm) {
		if (villager.apples >= 2) {
			return new NodeGoingToVillage(fsm);
		} else if (villager.targetTreeApples <= 0) {
			return new NodeGoingToTree(fsm);
		} else {
			return null;
		}
	}
}
