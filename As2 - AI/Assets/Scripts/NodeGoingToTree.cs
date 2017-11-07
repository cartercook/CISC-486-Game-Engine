public class NodeGoingToTree : VillagerNode {
	public NodeGoingToTree(FSM fsm) : base(fsm) {}

	public override void enter() {
		villager.setTarget(villager.findNextTree().transform);
		villager.setAnimTrigger(VillagerFSM.WALK);
	}

	public override void update(float elapsed) {
		villager.moveToTarget(elapsed);
	}

	public override FSMNode checkExitNode(FSM fsm) {
		if (villager.distanceToTarget <= villager.pickingDistance) {
			return new NodePickingApples(fsm);
		} else if (villager.findNextTree() == null) {
			return new NodeGoingToVillage(fsm);
		} else {
			return null;
		}
	}
}
