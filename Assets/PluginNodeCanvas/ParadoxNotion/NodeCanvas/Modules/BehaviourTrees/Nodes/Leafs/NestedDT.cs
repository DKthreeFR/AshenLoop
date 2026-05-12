using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Sub Dialogue")]
    [Description("执行一个子对话树（Sub Dialogue Tree）。在子对话树处于活跃状态期间，该节点持续返回“运行中”（Running）。你可以通过子对话树中的 “Finish” 节点来结束对话，并返回“成功”（Success）或“失败”（Failure）。NPC 开始一段多分支对话（比如劝说玩家加入阵营）。只要玩家还在看台词、做选择，行为树就卡在这个节点（Running）；当玩家最终点击“我同意！”（触发 Finish → Success），行为树才继续执行后续的“给予任务”；如果选了“滚开！”，则 Finish 返回 Failure，可能触发“敌对战斗”分支。")]
    [ParadoxNotion.Design.Icon("Dialogue")]
    [DropReferenceType(typeof(DialogueTree))]
    public class NestedDT : BTNodeNested<DialogueTree>
    {

        [SerializeField, ExposeField, Name("Sub Tree")]
        private BBParameter<DialogueTree> _nestedDialogueTree = null;

        public override DialogueTree subGraph { get { return _nestedDialogueTree.value; } set { _nestedDialogueTree.value = value; } }
        public override BBParameter subGraphParameter => _nestedDialogueTree;

        //

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( subGraph == null || subGraph.primeNode == null ) {
                return Status.Optional;
            }

            if ( status == Status.Resting ) {
                status = Status.Running;
                this.TryStartSubGraph(agent, OnDLGFinished);
            }

            if ( status == Status.Running ) {
                currentInstance.UpdateGraph(this.graph.deltaTime);
            }

            return status;
        }

        void OnDLGFinished(bool success) {
            if ( status == Status.Running ) {
                status = success ? Status.Success : Status.Failure;
            }
        }

        protected override void OnReset() {
            if ( currentInstance != null ) {
                currentInstance.Stop();
            }
        }
    }
}