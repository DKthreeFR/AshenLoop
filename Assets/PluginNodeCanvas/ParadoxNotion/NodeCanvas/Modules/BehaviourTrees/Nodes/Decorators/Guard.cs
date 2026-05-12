using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Guard")]
    [Category("Decorators")]
    [ParadoxNotion.Design.Icon("Shield")]
    [Description("如果另一个具有相同令牌（token）的守卫节点已在运行（即正在“守护”该令牌），则阻止当前被修饰的子节点执行。\r\n\r\n该守卫机制在所有智能体（Agent）的行为树中全局生效。比如：多个行为树都想使用“开门”这个动作，但门只能被一个人操作。只要有一个守卫节点拿着“开门令牌”在运行，其他想用同一令牌的行为就会被拦住，避免冲突。\r\n")]
    public class Guard : BTDecorator
    {

        public enum GuardMode
        {
            ReturnFailure,
            WaitUntilReleased
        }

        [Tooltip("A unique Token to use for guarding.")]
        public BBParameter<string> token;
        [Tooltip("What to return in case the token is already guarded by another Guard.")]
        public GuardMode ifGuarded = GuardMode.ReturnFailure;

        private bool isGuarding;

        private static readonly Dictionary<GameObject, List<Guard>> guards = new Dictionary<GameObject, List<Guard>>();
        private static List<Guard> AgentGuards(Component agent) { return guards[agent.gameObject]; }

        public override void OnGraphStarted() {
            SetGuards(graphAgent);
        }

        public override void OnGraphStoped() {
            foreach ( var runningGraph in Graph.runningGraphs ) {
                if ( runningGraph.agent != null && runningGraph.agent.gameObject == this.graphAgent.gameObject ) {
                    return;
                }
            }
            guards.Remove(graphAgent.gameObject);
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( decoratedConnection == null ) {
                return Status.Optional;
            }

            if ( agent != graphAgent ) {
                SetGuards(agent);
            }

            for ( var i = 0; i < AgentGuards(agent).Count; i++ ) {
                var guard = AgentGuards(agent)[i];
                if ( guard != this && guard.isGuarding && guard.token.value == this.token.value ) {
                    return ifGuarded == GuardMode.ReturnFailure ? Status.Failure : Status.Running;
                }
            }

            status = decoratedConnection.Execute(agent, blackboard);
            if ( status == Status.Running ) {
                isGuarding = true;
                return Status.Running;
            }

            isGuarding = false;
            return status;
        }

        protected override void OnReset() {
            isGuarding = false;
        }

        void SetGuards(Component guardAgent) {
            if ( !guards.ContainsKey(guardAgent.gameObject) ) {
                guards[guardAgent.gameObject] = new List<Guard>();
            }

            if ( !AgentGuards(guardAgent).Contains(this) && !string.IsNullOrEmpty(token.value) ) {
                AgentGuards(guardAgent).Add(this);
            }
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnNodeGUI() {
            GUILayout.Label(string.Format("<b>' {0} '</b>", string.IsNullOrEmpty(token.value) ? "NONE" : token.value));
        }

#endif
    }
}