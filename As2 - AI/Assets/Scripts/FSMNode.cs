public abstract class FSMNode {	
	public FSMNode(FSM fsm) {}

	public virtual void enter() {}
	public virtual void update(float elapsed) {}
	public virtual FSMNode checkExitNode(FSM fsm) { return null; }
	public virtual void exit() {}
}