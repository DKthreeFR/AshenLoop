using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Remap")]
    [Category("Decorators")]
    [Description("将子节点的状态重新映射为另一个状态。可用于反转子节点的返回状态，或强制始终返回某个指定状态。你有个“播放胜利动画”的叶节点，无论它播没播完（Success/Failure），你都希望上层继续执行“显示结算界面”，于是用 Remap 把所有结果统一改成 Success。")]
    [ParadoxNotion.Design.Icon("Remap")]
    public class Remapper : BTDecorator
    {

        public enum RemapStatus
        {
            Failure = 0,
            Success = 1,
        }

        public RemapStatus successRemap = RemapStatus.Success;
        public RemapStatus failureRemap = RemapStatus.Failure;

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( decoratedConnection == null ) {
                return Status.Optional;
            }

            status = decoratedConnection.Execute(agent, blackboard);

            switch ( status ) {
                case Status.Success:
                    return (Status)successRemap;
                case Status.Failure:
                    return (Status)failureRemap;
            }

            return status;
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnNodeGUI() {

            if ( (int)successRemap != (int)Status.Success )
                GUILayout.Label("Success → " + successRemap);

            if ( (int)failureRemap != (int)Status.Failure )
                GUILayout.Label("Failure → " + failureRemap);
        }

#endif
    }
}