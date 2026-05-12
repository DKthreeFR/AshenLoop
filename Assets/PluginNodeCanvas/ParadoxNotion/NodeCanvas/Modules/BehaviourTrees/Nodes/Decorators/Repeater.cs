using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Repeat")]
    [Category("Decorators")]
    [Description("重复执行子节点，可选择以下任一方式：\r\n\r\n    重复指定次数（x 次）；\r\n    一直重复，直到子节点返回指定的状态；\r\n    或无限循环执行（forever）。\r\n固定次数：角色“挥剑攻击”3 次（不管打没打中），用于连招系统；\r\n直到成功：不断尝试“拾取道具”，直到成功拿到（一旦成功就停）；\r\n无限循环：NPC 在“巡逻 → 等待 → 再巡逻”中永不停歇，除非被外部事件打断（比如玩家靠近）")]
    [ParadoxNotion.Design.Icon("Repeat")]
    public class Repeater : BTDecorator
    {

        public enum RepeaterMode
        {
            RepeatTimes = 0,
            RepeatUntil = 1,
            RepeatForever = 2
        }

        public enum RepeatUntilStatus
        {
            Failure = 0,
            Success = 1
        }

        public RepeaterMode repeaterMode = RepeaterMode.RepeatTimes;
        [ShowIf("repeaterMode", 0)]
        public BBParameter<int> repeatTimes = 1;
        [ShowIf("repeaterMode", 1)]
        public RepeatUntilStatus repeatUntilStatus = RepeatUntilStatus.Success;

        private int currentIteration = 1;

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( decoratedConnection == null ) {
                return Status.Optional;
            }

            if ( decoratedConnection.status != Status.Running ) {
                decoratedConnection.Reset();
            }

            status = decoratedConnection.Execute(agent, blackboard);

            switch ( status ) {
                case Status.Resting:
                    return Status.Running;
                case Status.Running:
                    return Status.Running;
            }

            switch ( repeaterMode ) {
                case RepeaterMode.RepeatTimes:

                    if ( currentIteration >= repeatTimes.value ) {
                        return status;
                    }

                    currentIteration++;
                    break;

                case RepeaterMode.RepeatUntil:

                    if ( (int)status == (int)repeatUntilStatus ) {
                        return status;
                    }
                    break;
            }

            return Status.Running;
        }

        protected override void OnReset() {
            currentIteration = 1;
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnNodeGUI() {

            if ( repeaterMode == RepeaterMode.RepeatTimes ) {
                GUILayout.Label(repeatTimes + " Times");
                if ( Application.isPlaying )
                    GUILayout.Label("Iteration: " + currentIteration.ToString());

            } else if ( repeaterMode == RepeaterMode.RepeatUntil ) {

                GUILayout.Label("Until " + repeatUntilStatus);

            } else {

                GUILayout.Label("Repeat Forever");
            }
        }

#endif
    }
}