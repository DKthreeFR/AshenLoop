using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization.FullSerializer;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    ///----------------------------------------------------------------------------------------------
    class PrioritySelector_0 : BTComposite
    {
        [SerializeField] public List<BBParameter<float>> priorities = null;
    }
    ///----------------------------------------------------------------------------------------------

    [Category("Composites")]
    [Description("需要先连接节点。用于效用 AI（Utility AI）。优先级选择器会执行具有最高“效用权重”的子节点。如果该节点执行失败，它会继续尝试效用权重次高的子节点，直到有一个成功，或者所有子节点都失败（这类似于普通选择节点的工作方式）。\r\n每个子分支代表一种“欲望”，而每种欲望包含一个或多个“考量因素”，这些因素会被取平均值。\r\n“考量因素”由一对“输入值”和“曲线”组成，两者结合产生该考量因素的效用权重。\r\n如果启用了“动态”选项，系统将连续评估效用权重，并始终执行权重最高的子节点，而不受子节点当前返回状态的影响。")]
    [ParadoxNotion.Design.Icon("Priority")]
    [Color("b3ff7f")]
    [fsMigrateVersions(typeof(PrioritySelector_0))]
    public class PrioritySelector : BTComposite, IMigratable<PrioritySelector_0>
    {
        ///----------------------------------------------------------------------------------------------
        void IMigratable<PrioritySelector_0>.Migrate(PrioritySelector_0 model) {
            this.desires = new List<Desire>();
            foreach ( var priority in model.priorities ) {
                var desire = new Desire();
                this.desires.Add(desire);
                var consideration = desire.AddConsideration(graphBlackboard);
                consideration.input = priority;
            }
        }
        ///----------------------------------------------------------------------------------------------

        [System.Serializable]
        public class Desire
        {
            [ParadoxNotion.Serialization.FullSerializer.fsIgnoreInBuild]
            public string name;
            [ParadoxNotion.Serialization.FullSerializer.fsIgnoreInBuild]
            public bool foldout;
            public List<Consideration> considerations = new List<Consideration>();

            public Consideration AddConsideration(IBlackboard bb) {
                var result = new Consideration(bb);
                considerations.Add(result);
                return result;
            }

            public void RemoveConsideration(Consideration consideration) { considerations.Remove(consideration); }

            public float GetCompoundUtility() {
                float total = 0;
                for ( var i = 0; i < considerations.Count; i++ ) {
                    total += considerations[i].utility;
                }
                return total / considerations.Count;
            }
        }

        [System.Serializable]
        public class Consideration
        {
            public BBParameter<float> input;
            public BBParameter<AnimationCurve> function;
            public float utility => function.value != null ? function.value.Evaluate(input.value) : input.value;
            public Consideration(IBlackboard blackboard) {
                input = new BBParameter<float> { value = 1f, bb = blackboard };
                function = new BBParameter<AnimationCurve> { bb = blackboard };
            }
        }

        ///----------------------------------------------------------------------------------------------

        [Tooltip("If enabled, will continously evaluate utility weights and execute the child with the highest one accordingly. In this mode child return status does not matter.")]
        public bool dynamic;
        [AutoSortWithChildrenConnections]
        public List<Desire> desires;

        private Connection[] orderedConnections;
        private int current = 0;

        public override void OnChildConnected(int index) {
            if ( desires == null ) { desires = new List<Desire>(); }
            if ( desires.Count < outConnections.Count ) { desires.Insert(index, new Desire()); }
        }

        public override void OnChildDisconnected(int index) { desires.RemoveAt(index); }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( dynamic ) {
                var highestPriority = float.NegativeInfinity;
                var best = 0;
                for ( var i = 0; i < outConnections.Count; i++ ) {
                    var priority = desires[i].GetCompoundUtility();
                    if ( priority > highestPriority ) {
                        highestPriority = priority;
                        best = i;
                    }
                }

                if ( best != current ) {
                    outConnections[current].Reset();
                    current = best;
                }
                return outConnections[current].Execute(agent, blackboard);
            }

            ///----------------------------------------------------------------------------------------------

            if ( status == Status.Resting ) {
                orderedConnections = outConnections.OrderBy(c => desires[outConnections.IndexOf(c)].GetCompoundUtility()).ToArray();
            }

            for ( var i = orderedConnections.Length; i-- > 0; ) {
                status = orderedConnections[i].Execute(agent, blackboard);
                if ( status == Status.Success ) {
                    return Status.Success;
                }

                if ( status == Status.Running ) {
                    current = i;
                    return Status.Running;
                }
            }

            return Status.Failure;
        }

        protected override void OnReset() { current = 0; }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------

#if UNITY_EDITOR

        //..
        public override string GetConnectionInfo(int i) {
            var desire = desires[i];
            var desireName = string.IsNullOrEmpty(desire.name) ? "DESIRE " + i.ToString() : desire.name;
            var result = desireName.ToUpper() + "\n";
            for ( var j = 0; j < desire.considerations.Count; j++ ) {
                result += desire.considerations[j].input.ToString() + " (" + desire.considerations[j].utility.ToString("0.00") + ")" + "\n";
            }
            return result += string.Format("<b>Avg.</b> ({0})", desire.GetCompoundUtility().ToString("0.00"));
        }

        //..
        public override void OnConnectionInspectorGUI(int i) {
            var desire = desires[i];
            var optionsB = new EditorUtils.ReorderableListOptions();
            optionsB.allowAdd = false;
            optionsB.allowRemove = true;
            EditorUtils.ReorderableList(desire.considerations, optionsB, (j, pickedB) =>
            {
                var consideration = desire.considerations[j];
                GUILayout.BeginVertical("box");
                consideration.input = (BBParameter<float>)NodeCanvas.Editor.BBParameterEditor.ParameterField("Input", consideration.input, true);
                consideration.function = (BBParameter<AnimationCurve>)NodeCanvas.Editor.BBParameterEditor.ParameterField("Curve", consideration.function);
                GUILayout.EndVertical();
            });

            if ( GUILayout.Button("Add Consideration") ) { desire.AddConsideration(graphBlackboard); }
            EditorUtils.Separator();
        }

        protected override void OnNodeGUI() {
            if ( dynamic ) { GUILayout.Label("<b>DYNAMIC</b>"); }
        }

        //..
        protected override void OnNodeInspectorGUI() {

            if ( outConnections.Count == 0 ) {
                GUILayout.Label("Make some connections first");
                return;
            }

            dynamic = UnityEditor.EditorGUILayout.Toggle(new GUIContent("Dynamic", "If enabled, will continously evaluate utility weights and execute the child with the highest one accordingly. In this mode child return status does not matter."), dynamic);

            EditorUtils.Separator();
            EditorUtils.CoolLabel("Desires");

            var optionsA = new EditorUtils.ReorderableListOptions();
            optionsA.allowAdd = false;
            optionsA.allowRemove = false;
            EditorUtils.ReorderableList(desires, optionsA, (i, pickedA) =>
            {
                var desire = desires[i];
                var desireName = string.IsNullOrEmpty(desire.name) ? "DESIRE " + i.ToString() : desire.name;
                desire.foldout = UnityEditor.EditorGUILayout.Foldout(desire.foldout, new GUIContent(desireName));
                if ( desire.foldout ) {
                    desire.name = UnityEditor.EditorGUILayout.TextField("   Friendly Name", desire.name);
                    OnConnectionInspectorGUI(i);
                }
            });
        }
#endif

    }
}