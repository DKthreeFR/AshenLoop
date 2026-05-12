using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Sub Tree")]
    [Description("执行一个子行为树（Sub Behaviour Tree），并直接返回该子树根节点的状态。主树调用这个子树后，就完全交由子树自己决策；等子树跑完，无论最终是成功干掉敌人、失败逃跑，还是还在追击中（Running），主树都照单全收，不做干预。")]
    [ParadoxNotion.Design.Icon("BT")]
    [DropReferenceType(typeof(BehaviourTree))]
    public class SubTree : BTNodeNested<BehaviourTree>
    {

        [SerializeField, ExposeField]
        private BBParameter<BehaviourTree> _subTree = null;

        public override BehaviourTree subGraph { get { return _subTree.value; } set { _subTree.value = value; } }
        public override BBParameter subGraphParameter => _subTree;

        ///----------------------------------------------------------------------------------------------

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( subGraph == null || subGraph.primeNode == null ) {
                return Status.Optional;
            }

            if ( status == Status.Resting ) {
                this.TryStartSubGraph(agent);
            }

            currentInstance.UpdateGraph(this.graph.deltaTime);

            if ( currentInstance.repeat && currentInstance.rootStatus != Status.Running ) {
                this.TryReadAndUnbindMappedVariables();
            }

            return currentInstance.rootStatus;
        }

        protected override void OnReset() {
            if ( currentInstance != null ) {
                currentInstance.Stop();
            }
        }
    }
}