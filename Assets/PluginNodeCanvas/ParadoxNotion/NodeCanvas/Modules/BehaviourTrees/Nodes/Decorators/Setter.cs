using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Override Agent")]
    [Category("Decorators")]
    [Description("从此节点开始，动态地为行为树的剩余部分切换到另一个智能体（Agent）。该节点下的所有子节点都将使用新的智能体执行。你也可以用此装饰器切回原始行为树所属的智能体。主角触发一个技能后，短暂接管队友的行为树，让队友执行一段特定动作（比如“掩护我！”），结束后自动归还控制权。")]
    [ParadoxNotion.Design.Icon("Agent")]
    public class Setter : BTDecorator
    {

        [Tooltip("If enabled, will revert back to the original graph agent.")]
        public bool revertToOriginal;
        [ShowIf("revertToOriginal", 0), Tooltip("The new agent to use.")]
        public BBParameter<GameObject> newAgent;

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( decoratedConnection == null ) {
                return Status.Optional;
            }

            agent = revertToOriginal ? graphAgent : newAgent.value.transform;
            return decoratedConnection.Execute(agent, blackboard);
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnNodeGUI() {
            GUILayout.Label(string.Format("Agent = {0}", revertToOriginal ? "Original" : newAgent.ToString()));
        }

#endif
    }
}