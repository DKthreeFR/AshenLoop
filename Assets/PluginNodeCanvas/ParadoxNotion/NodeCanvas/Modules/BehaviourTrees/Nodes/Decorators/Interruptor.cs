using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Interrupt")]
    [Category("Decorators")]
    [Description("执行并返回子节点的状态。如果条件为真，或在执行过程中变为真，则立即中断子节点，并返回“失败”（Failure）。角色正在“搜寻物资”，但一旦检测到“生命值低于30%”（条件变真），就马上中断搜寻，返回失败，好让上层行为树切换到“逃跑”或“治疗”逻辑。")]
    [ParadoxNotion.Design.Icon("Interruptor")]
    public class Interruptor : BTDecorator, ITaskAssignable<ConditionTask>
    {

        [SerializeField]
        private ConditionTask _condition;

        public ConditionTask condition {
            get { return _condition; }
            set { _condition = value; }
        }

        public Task task {
            get { return condition; }
            set { condition = (ConditionTask)value; }
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( decoratedConnection == null ) {
                return Status.Optional;
            }

            if ( condition == null ) {
                return decoratedConnection.Execute(agent, blackboard);
            }

            if ( status == Status.Resting ) {
                condition.Enable(agent, blackboard);
            }

            if ( condition.Check(agent, blackboard) == false ) {
                return decoratedConnection.Execute(agent, blackboard);
            }

            if ( decoratedConnection.status == Status.Running ) {
                decoratedConnection.Reset();
            }

            return Status.Failure;
        }

        protected override void OnReset() {
            if ( condition != null ) { condition.Disable(); }
        }
    }
}