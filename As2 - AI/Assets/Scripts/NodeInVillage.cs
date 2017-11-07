using UnityEngine;

public class NodeInVillage : VillagerNode {
	public NodeInVillage(FSM fsm) : base(fsm) {}

	public override void enter() {
		villager.setAnimTrigger(VillagerFSM.DROP);
	}

	public override FSMNode checkExitNode(FSM fsm) {
		if (villager.inAnimState(VillagerFSM.IDLE_STATE) && villager.findNextTree() != null) {
			villager.apples = 0;
			return new NodeGoingToTree(fsm);
		} else {
			return null;
		}
	}
}
