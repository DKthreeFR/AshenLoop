using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Category("Composites")]
    [Description("基于一个“条件任务”，快速决定是执行左侧子节点还是右侧子节点。“看一眼条件，如果满足就走左边（比如执行动作），如果不满足就直接走右边（比如跳过或做备选）”。")]
    [ParadoxNotion.Design.Icon("Condition")]
    [Color("b3ff7f")]
    public class BinarySelector : BTNode, ITaskAssignable<ConditionTask>
    {

        [Tooltip("If true, the condition will be re-evaluated per frame.")]
        public bool dynamic;

        [SerializeField]
        private ConditionTask _condition;

        private int succeedIndex;

        public override int maxOutConnections { get { return 2; } }
        public override Alignment2x2 commentsAlignment { get { return Alignment2x2.Right; } }

        public override string name {
            get { return base.name.ToUpper(); }
        }

        public Task task {
            get { return condition; }
            set { condition = (ConditionTask)value; }
        }

        private ConditionTask condition {
            get { return _condition; }
            set { _condition = value; }
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( condition == null || outConnections.Count < 2 ) {
                return Status.Optional;
            }

            if ( status == Status.Resting ) {
                condition.Enable(agent, blackboard);
            }

            if ( dynamic || status == Status.Resting ) {
                var lastIndex = succeedIndex;
                succeedIndex = condition.Check(agent, blackboard) ? 0 : 1;
                if ( succeedIndex != lastIndex ) {
                    outConnections[lastIndex].Reset();
                }
            }

            return outConnections[succeedIndex].Execute(agent, blackboard);
        }

        protected override void OnReset() {
            if ( condition != null ) { condition.Disable(); }
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        public override string GetConnectionInfo(int i) {
            return i == 0 ? "TRUE" : "FALSE";
        }

        protected override void OnNodeGUI() {
            if ( dynamic ) {
                GUILayout.Label("<b>DYNAMIC</b>");
            }
        }

#endif
    }
}