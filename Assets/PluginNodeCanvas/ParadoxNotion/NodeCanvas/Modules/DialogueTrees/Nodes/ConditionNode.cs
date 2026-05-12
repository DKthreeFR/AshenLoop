using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.DialogueTrees
{

    [ParadoxNotion.Design.Icon("Condition")]
    [Name("Task Condition")]
    [Category("Branch")]
    [Description("为所选的对话角色（Dialogue Actor）执行一个动作任务（Action Task）在一段对话中，当 NPC 说到“看那边！”时，你可以用这个节点让他执行“看向目标点”的动作；或者当玩家选择“愤怒回应”时，触发主角“握紧拳头”的动画。")]
    [Color("b3ff7f")]
    public class ConditionNode : DTNode, ITaskAssignable<ConditionTask>
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

        public override int maxOutConnections { get { return 2; } }
        public override bool requireActorSelection { get { return true; } }

        protected override Status OnExecute(Component agent, IBlackboard bb) {

            if ( outConnections.Count == 0 ) {
                return Error("There are no connections on the Dialogue Condition Node");
            }

            if ( condition == null ) {
                return Error("There is no Conidition on the Dialoge Condition Node");
            }

            var isSuccess = condition.CheckOnce(finalActor.transform, graphBlackboard);
            status = isSuccess ? Status.Success : Status.Failure;
            DLGTree.Continue(isSuccess ? 0 : 1);
            return status;
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        public override string GetConnectionInfo(int i) {
            return i == 0 ? "Then" : "Else";
        }

#endif
    }
}