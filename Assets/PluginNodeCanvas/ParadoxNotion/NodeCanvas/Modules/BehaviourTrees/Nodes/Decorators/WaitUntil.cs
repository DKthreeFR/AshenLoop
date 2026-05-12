using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Category("Decorators")]
    [Description("持续返回“运行中”（Running），直到指定的条件变为真；条件满足后，才会执行被修饰的子节点。角色想“发起攻击”，但必须等“技能冷却结束”这个条件为真。在冷却期间，这个节点一直返回 Running（行为树卡在这儿不动），一旦冷却好了，立刻执行“攻击”子节点。")]
    [ParadoxNotion.Design.Icon("Halt")]
    public class WaitUntil : BTDecorator, ITaskAssignable<ConditionTask>
    {

        [SerializeField]
        private ConditionTask _condition;
        private bool accessed;

        public Task task {
            get { return condition; }
            set { condition = (ConditionTask)value; }
        }

        private ConditionTask condition {
            get { return _condition; }
            set { _condition = value; }
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( decoratedConnection == null ) {
                //this part is so that Wait node can be used as a leaf too, by user request
                if ( condition != null ) {
                    if ( status == Status.Resting ) {
                        condition.Enable(agent, blackboard);
                    }
                    return condition.Check(agent, blackboard) ? Status.Success : Status.Running;
                }
                //-----
                return Status.Optional;
            }

            if ( condition == null ) {
                return decoratedConnection.Execute(agent, blackboard);
            }

            if ( status == Status.Resting ) {
                condition.Enable(agent, blackboard);
            }

            if ( accessed ) return decoratedConnection.Execute(agent, blackboard);

            if ( condition.Check(agent, blackboard) ) {
                accessed = true;
            }

            return accessed ? decoratedConnection.Execute(agent, blackboard) : Status.Running;
        }

        protected override void OnReset() {
            if ( condition != null ) { condition.Disable(); }
            accessed = false;
        }
    }
}