using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.DialogueTrees
{

    [Name("FINISH")]
    [Category("Control")]
    [Description("以“成功”（Success）或“失败”（Failure）结束对话。\r\n\r\n注意：如果对话自然运行到一个没有子连接的节点，系统会默认以“成功”结束。因此，此节点主要用于主动以“失败”状态结束对话。.")]
    [ParadoxNotion.Design.Icon("Halt")]
    [Color("6ebbff")]
    public class FinishNode : DTNode
    {

        public CompactStatus finishState = CompactStatus.Success;

        public override int maxOutConnections { get { return 0; } }
        public override bool requireActorSelection { get { return false; } }

        protected override Status OnExecute(Component agent, IBlackboard bb) {
            status = (Status)finishState;
            DLGTree.Stop(finishState == CompactStatus.Success ? true : false);
            return status;
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnNodeGUI() {
            GUILayout.Label("<b>" + finishState.ToString() + "</b>");
        }

        protected override void OnNodeInspectorGUI() {
            DrawDefaultInspector();
        }

#endif
    }
}