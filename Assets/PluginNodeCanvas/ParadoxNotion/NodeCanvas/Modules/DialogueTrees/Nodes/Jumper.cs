using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees
{

    [Name("JUMP")]
    [Description("选择一个目标节点进行跳转。\r\n\r\n为了方便你在下拉菜单中识别节点，请为你打算以这种方式使用的节点设置一个“标签”（Tag）名称。你在做一个多分支对话，玩家选了“我其实是卧底！”后，不想按顺序走后续选项，而是立刻跳转到早已写好的“卧底暴露”剧情段落。\r\n\r\n只要你在那个“卧底暴露”节点上设置了 Tag（比如叫 BetrayalReveal），就可以在这个 Jump 节点的下拉菜单里轻松选中它，实现一键跳转！")]
    [Category("Control")]
    [ParadoxNotion.Design.Icon("Set")]
    [Color("6ebbff")]
    public class Jumper : DTNode, IHaveNodeReference
    {
        [ParadoxNotion.Serialization.FullSerializer.fsSerializeAs("_sourceNodeUID")]
        public NodeReference<DTNode> _targetNode;

        INodeReference IHaveNodeReference.targetReference => _targetNode;
        private DTNode target => _targetNode?.Get(graph);

        public override int maxOutConnections { get { return 0; } }
        public override bool requireActorSelection { get { return false; } }

        protected override Status OnExecute(Component agent, IBlackboard bb) {
            if ( target == null ) { return Error("Target Node of Jumper node is null"); }
            DLGTree.EnterNode(target);
            return Status.Success;
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR
        protected override void OnNodeGUI() {
            GUILayout.Label(string.Format("<b>{0}</b>", target != null ? target.ToString() : "NONE"));
        }
#endif

    }
}