public class VillagerNode : FSMNode {
	protected VillagerFSM villager;

	public VillagerNode(FSM fsm) : base(fsm) {
		this.villager = (VillagerFSM)fsm;
	}
}
