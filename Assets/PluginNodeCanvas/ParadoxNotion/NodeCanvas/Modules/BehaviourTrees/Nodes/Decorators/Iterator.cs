using System.Collections;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;
using ParadoxNotion;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Iterate")]
    [Category("Decorators")]
    [Description("遍历一个列表，并为其中的每个元素执行一次子节点。持续迭代，直到满足终止策略（Termination Policy），或整个列表遍历完毕；若完整遍历结束，则返回最后一次迭代中子节点的状态。角色要“搜索附近所有箱子”，对每个箱子都尝试“打开”。如果中途有一个箱子成功打开了（且你的终止策略是“任一成功就停”），就立刻停止搜索；如果全都打不开，那就返回最后一个箱子的“失败”状态，告诉上层：“我试完了，没一个能开的。”")]
    [ParadoxNotion.Design.Icon("List")]
    public class Iterator : BTDecorator
    {

        public enum TerminationConditions
        {
            None,
            FirstSuccess,
            FirstFailure
        }

        [RequiredField]
        [BlackboardOnly]
        [Tooltip("The list to iterate.")]
        public BBParameter<IList> targetList;

        [BlackboardOnly]
        [Name("Current Element")]
        [Tooltip("Store the currently iterated list element in a variable.")]
        public BBObjectParameter current;

        [BlackboardOnly]
        [Name("Current Index")]
        [Tooltip("Store the currently iterated list index in a variable.")]
        public BBParameter<int> storeIndex;

        [Name("Termination Policy"), Tooltip("The condition for when to terminate the iteration and return status.")]
        public TerminationConditions terminationCondition = TerminationConditions.None;

        [Tooltip("The maximum allowed iterations. Leave at -1 to iterate the whole list.")]
        public BBParameter<int> maxIteration = -1;

        [Tooltip("Should the iteration start from the begining after the Iterator node resets?")]
        public bool resetIndex = true;

        private int currentIndex;

        private IList list => targetList != null ? targetList.value : null;

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( decoratedConnection == null ) {
                return Status.Optional;
            }

            if ( list == null || list.Count == 0 ) {
                return Status.Failure;
            }

            for ( var i = currentIndex; i < list.Count; i++ ) {

                current.value = list[i];
                storeIndex.value = i;
                status = decoratedConnection.Execute(agent, blackboard);

                if ( status == Status.Success && terminationCondition == TerminationConditions.FirstSuccess ) {
                    return Status.Success;
                }

                if ( status == Status.Failure && terminationCondition == TerminationConditions.FirstFailure ) {
                    return Status.Failure;
                }

                if ( status == Status.Running ) {
                    currentIndex = i;
                    return Status.Running;
                }


                if ( currentIndex == list.Count - 1 || currentIndex == maxIteration.value - 1 ) {
                    if ( resetIndex ) { currentIndex = 0; }
                    return status;
                }

                decoratedConnection.Reset();
                currentIndex++;
            }

            return Status.Running;
        }


        protected override void OnReset() {
            if ( resetIndex ) { currentIndex = 0; }
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnNodeGUI() {

            GUILayout.Label("For Each\t" + current + "\nIn\t" + targetList, Styles.leftLabel);
            if ( terminationCondition != TerminationConditions.None ) {
                GUILayout.Label("Break on " + terminationCondition.ToString());
            }

            if ( Application.isPlaying ) {
                GUILayout.Label("Index: " + currentIndex.ToString() + " / " + ( list != null && list.Count != 0 ? ( list.Count - 1 ).ToString() : "?" ));
            }
        }

        protected override void OnNodeInspectorGUI() {
            DrawDefaultInspector();
            var argType = targetList.refType != null ? targetList.refType.GetEnumerableElementType() : null;
            if ( current.varType != argType ) { current.SetType(argType); }
        }
#endif

    }
}