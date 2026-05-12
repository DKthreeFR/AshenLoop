using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Category("Composites")]
    [Description("给子节点设置概率。根据每个子节点被选中的“几率”来选择执行哪一个。如果被选中的子节点返回“成功”，则它返回“成功”；否则，它会继续挑选另一个子节点。\n如果所有子节点都返回“失败”，或者触发了预设的“失败几率”，则它返回“失败”。比如一个怪物被打死时的掉落逻辑，或者攻击时的招式选择：30% 几率“普通攻击”，10% 几率“释放技能”，60% 几率“什么都不做（发呆）”")]
    [ParadoxNotion.Design.Icon("ProbabilitySelector")]
    [Color("b3ff7f")]
    public class ProbabilitySelector : BTComposite
    {

        [AutoSortWithChildrenConnections, Tooltip("The weights of the children.")]
        public List<BBParameter<float>> childWeights;
        [Tooltip("A chance for the node to fail immediately.")]
        public BBParameter<float> failChance;

        private bool[] indexFailed;
        private float[] tmpWeights;
        private float tmpFailWeight;
        private float tmpTotal;
        private float tmpDice;

        public override void OnChildConnected(int index) {
            if ( childWeights == null ) { childWeights = new List<BBParameter<float>>(); }
            if ( childWeights.Count < outConnections.Count ) {
                childWeights.Insert(index, new BBParameter<float> { value = 1, bb = graphBlackboard });
            }
        }

        public override void OnChildDisconnected(int index) {
            childWeights.RemoveAt(index);
        }

        public override void OnGraphStarted() { OnReset(); }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( status == Status.Resting ) {
                tmpDice = Random.value;
                tmpFailWeight = failChance.value;
                tmpTotal = tmpFailWeight;
                for ( var i = 0; i < childWeights.Count; i++ ) {
                    var childWeight = childWeights[i].value;
                    tmpTotal += childWeight;
                    tmpWeights[i] = childWeight;
                }
            }

            var prob = tmpFailWeight / tmpTotal;
            if ( tmpDice < prob ) {
                return Status.Failure;
            }

            for ( var i = 0; i < outConnections.Count; i++ ) {

                if ( indexFailed[i] ) {
                    continue;
                }

                prob += tmpWeights[i] / tmpTotal;
                if ( tmpDice <= prob ) {
                    status = outConnections[i].Execute(agent, blackboard);
                    if ( status == Status.Success || status == Status.Running ) {
                        return status;
                    }

                    if ( status == Status.Failure ) {
                        indexFailed[i] = true;
                        tmpTotal -= tmpWeights[i];
                        return Status.Running;
                    }
                }
            }

            return Status.Failure;
        }

        protected override void OnReset() {
            tmpWeights = new float[outConnections.Count];
            indexFailed = new bool[outConnections.Count];
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        float GetTotal() {
            var total = failChance.value;
            for ( var i = 0; i < childWeights.Count; i++ ) {
                total += childWeights[i].value;
            }
            return total;
        }

        public override string GetConnectionInfo(int i) {
            return Mathf.Round(( childWeights[i].value / GetTotal() ) * 100) + "%";
        }

        public override void OnConnectionInspectorGUI(int i) {
            NodeCanvas.Editor.BBParameterEditor.ParameterField("Weight", childWeights[i]);
        }

        protected override void OnNodeInspectorGUI() {

            if ( outConnections.Count == 0 ) {
                GUILayout.Label("Make some connections first");
                return;
            }

            var total = GetTotal();
            for ( var i = 0; i < childWeights.Count; i++ ) {
                GUILayout.BeginHorizontal();
                childWeights[i] = (BBParameter<float>)NodeCanvas.Editor.BBParameterEditor.ParameterField("Weight", childWeights[i]);
                GUILayout.Label(Mathf.Round(( childWeights[i].value / total ) * 100) + "%", GUILayout.Width(38));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            failChance = (BBParameter<float>)NodeCanvas.Editor.BBParameterEditor.ParameterField("Direct Failure Chance", failChance);
            GUILayout.Label(Mathf.Round(( failChance.value / total ) * 100) + "%", GUILayout.Width(38));
            GUILayout.EndHorizontal();
        }

#endif
    }
}