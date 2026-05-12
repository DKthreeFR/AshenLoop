using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.DialogueTrees
{

    [Category("Branch")]
    [Description("根据每个子节点被选中的概率，随机选择一个执行。可选地，可以为每个子节点分配一个前置条件任务（Pre-Condition Task），用于在概率选择前将其纳入或排除出候选池。条件判断将基于所选的 Actor（角色）进行。NPC 要对玩家做出反应，有三个可能的行为：\r\n\r\n    嘲讽（概率 50%，条件：Actor.HasTauntSkill == true）\r\n    施法攻击（概率 30%，条件：Actor.Mana >= 20）\r\n    逃跑（概率 20%，无条件）\r\n")]
    [ParadoxNotion.Design.Icon("ProbabilitySelector")]
    [Color("b3ff7f")]
    public class ProbabilitySelector : DTNode
    {

        public class Option
        {
            public BBParameter<float> weight;
            public ConditionTask condition;
            public Option(float weightValue, IBlackboard bbValue) {
                weight = new BBParameter<float> { value = weightValue, bb = bbValue };
                condition = null;
            }
        }

        [SerializeField, AutoSortWithChildrenConnections]
        private List<Option> childOptions = new List<Option>();
        private List<int> successIndeces;

        public override int maxOutConnections { get { return -1; } }

        public override void OnChildConnected(int index) {
            if ( childOptions.Count < outConnections.Count ) {
                childOptions.Insert(index, new Option(1, graphBlackboard));
            }
        }

        public override void OnChildDisconnected(int index) {
            childOptions.RemoveAt(index);
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            successIndeces = new List<int>();
            for ( var i = 0; i < outConnections.Count; i++ ) {
                var condition = childOptions[i].condition;
                if ( condition == null || condition.CheckOnce(finalActor.transform, blackboard) ) {
                    successIndeces.Add(i);
                }
            }

            var probability = Random.Range(0f, GetTotal());
            for ( var i = 0; i < outConnections.Count; i++ ) {

                if ( !successIndeces.Contains(i) ) {
                    continue;
                }

                if ( probability > childOptions[i].weight.value ) {
                    probability -= childOptions[i].weight.value;
                    continue;
                }

                DLGTree.Continue(i);
                return Status.Success;
            }

            return Status.Failure;
        }

        float GetTotal() {
            var total = 0f;
            for ( var i = 0; i < childOptions.Count; i++ ) {
                var option = childOptions[i];
                if ( successIndeces == null || successIndeces.Contains(i) ) {
                    total += option.weight.value;
                    continue;
                }
            }
            return total;
        }

        protected override void OnReset() {
            successIndeces = null;
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        public override string GetConnectionInfo(int i) {
            var result = childOptions[i].condition != null ? childOptions[i].condition.summaryInfo + "\n" : string.Empty;
            if ( successIndeces == null || successIndeces.Contains(i) ) {
                return result + Mathf.Round(( childOptions[i].weight.value / GetTotal() ) * 100) + "%";
            }
            return result + "Condition Failed";
        }

        public override void OnConnectionInspectorGUI(int i) {
            DrawOptionGUI(i, GetTotal());
        }

        protected override void OnNodeInspectorGUI() {

            base.OnNodeInspectorGUI();

            if ( outConnections.Count == 0 ) {
                return;
            }

            var total = GetTotal();
            for ( var i = 0; i < childOptions.Count; i++ ) {
                EditorUtils.BoldSeparator();
                DrawOptionGUI(i, total);
            }
        }

        void DrawOptionGUI(int i, float total) {
            NodeCanvas.Editor.TaskEditor.TaskFieldMulti<ConditionTask>(childOptions[i].condition, DLGTree, (c) => { childOptions[i].condition = c; });
            EditorUtils.Separator();
            GUILayout.BeginHorizontal();
            NodeCanvas.Editor.BBParameterEditor.ParameterField("Weight", childOptions[i].weight);
            GUILayout.Label(Mathf.Round(( childOptions[i].weight.value / total ) * 100) + "%", GUILayout.Width(38));
            GUILayout.EndHorizontal();
        }

#endif
    }
}