using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees
{

    [Name("Sequencer顺序节点", 10)]
    [Category("Composites")]
    [Description("按顺序执行其子节点，仅当所有子节点都返回“成功”时，它才返回“成功”。一旦有任意子节点返回“失败”，序列节点将立即停止执行，并同样返回“失败”。必须做完第一项才能做第二项。\r\n如果中间任何一步失败了（比如“开门”失败了），后面的步骤（比如“进屋”）就不会执行，整个序列直接判定为失败。")]
    [ParadoxNotion.Design.Icon("Sequencer")]
    [Color("bf7fff")]
    public class Sequencer : BTComposite
    {

        [Tooltip("If true, then higher priority children are re-evaluated per frame and if either returns Failure, then the Sequencer will immediately stop and return Failure as well.")]
        public bool dynamic;
        [Tooltip("If true, the children order of execution is shuffled each time the Sequencer resets.")]
        public bool random;

        private int lastRunningNodeIndex = 0;

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            for ( var i = dynamic ? 0 : lastRunningNodeIndex; i < outConnections.Count; i++ ) {

                status = outConnections[i].Execute(agent, blackboard);

                switch ( status ) {
                    case Status.Running:

                        if ( dynamic && i < lastRunningNodeIndex ) {
                            for ( var j = i + 1; j <= lastRunningNodeIndex; j++ ) {
                                outConnections[j].Reset();
                            }
                        }

                        lastRunningNodeIndex = i;
                        return Status.Running;

                    case Status.Failure:

                        if ( dynamic && i < lastRunningNodeIndex ) {
                            for ( var j = i + 1; j <= lastRunningNodeIndex; j++ ) {
                                outConnections[j].Reset();
                            }
                        }

                        return Status.Failure;
                }
            }

            return Status.Success;
        }

        protected override void OnReset() {
            lastRunningNodeIndex = 0;
            if ( random ) { outConnections = outConnections.Shuffle(); }
        }

        public override void OnChildDisconnected(int index) {
            if ( index != 0 && index == lastRunningNodeIndex ) {
                lastRunningNodeIndex--;
            }
        }

        public override void OnGraphStarted() { OnReset(); }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR
        protected override void OnNodeGUI() {
            if ( dynamic ) { GUILayout.Label("<b>DYNAMIC</b>"); }
            if ( random ) { GUILayout.Label("<b>RANDOM</b>"); }
        }
#endif

    }
}