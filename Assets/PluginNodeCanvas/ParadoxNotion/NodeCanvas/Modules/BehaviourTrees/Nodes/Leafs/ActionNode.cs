using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Action")]
    [Description("执行一个动作，并在动作完成时返回“成功”或“失败”。\n在动作完成之前，会持续返回“运行中”。")]
    [ParadoxNotion.Design.Icon("Action")]
    // [Color("ff6d53")]
    public class ActionNode : BTNode, ITaskAssignable<ActionTask>
    {

        [SerializeField]
        private ActionTask _action;

        public Task task {
            get { return action; }
            set { action = (ActionTask)value; }
        }

        public ActionTask action {
            get { return _action; }
            set { _action = value; }
        }

        public override string name {
            get { return base.name.ToUpper(); }
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( action == null ) {
                return Status.Optional;
            }

            if ( status == Status.Resting || status == Status.Running ) {
                return action.Execute(agent, blackboard);
            }

            return status;
        }

        protected override void OnReset() {
            if ( action != null ) {
                action.EndAction(null);
            }
        }

        public override void OnGraphPaused() {
            if ( action != null ) {
                action.Pause();
            }
        }
    }
}