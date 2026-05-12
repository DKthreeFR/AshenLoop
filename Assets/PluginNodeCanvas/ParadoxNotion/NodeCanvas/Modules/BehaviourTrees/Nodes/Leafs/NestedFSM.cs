using System.Linq;
using NodeCanvas.Framework;
using NodeCanvas.StateMachines;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Sub FSM")]
    [Description("执行一个子状态机（Sub FSM）。在子状态机处于活跃状态期间，该节点持续返回“运行中”（Running）。\r\n\r\n如果子状态机进入了被标记为“成功”（Success）或“失败”（Failure）的特定状态，则立即停止子状态机，并返回对应的成功或失败状态。\r\n\r\n如果子状态机以其他方式正常结束（未进入 Success/Failure 状态），则本节点返回“成功”（Success）。你让一个 NPC 执行“修理载具”子状态机，里面包含【检查零件】→【焊接】→【测试引擎】等状态。\r\n\r\n    如果在【焊接】时油箱爆炸（进入 Failure 状态），立刻中断并返回 Failure；\r\n    如果顺利修好并手动跳转到【任务完成】（Success 状态），就返回 Success；\r\n    如果状态机自然走完最后一个状态（比如【测试引擎】后自动结束），没有显式指定结果，默认当作 Success 返回。\r\n")]
    [ParadoxNotion.Design.Icon("FSM")]
    [DropReferenceType(typeof(FSM))]
    public class NestedFSM : BTNodeNested<FSM>
    {

        [SerializeField, ExposeField, Name("Sub FSM")]
        private BBParameter<FSM> _nestedFSM = null;

        [HideInInspector] public string successState;
        [HideInInspector] public string failureState;

        public override FSM subGraph { get { return _nestedFSM.value; } set { _nestedFSM.value = value; } }
        public override BBParameter subGraphParameter => _nestedFSM;

        ///----------------------------------------------------------------------------------------------

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( subGraph == null || subGraph.primeNode == null ) {
                return Status.Optional;
            }

            if ( status == Status.Resting ) {
                status = Status.Running;
                this.TryStartSubGraph(agent, OnFSMFinish);
            }

            if ( status == Status.Running ) {
                currentInstance.UpdateGraph(this.graph.deltaTime);
            }

            if ( !string.IsNullOrEmpty(successState) && currentInstance.currentStateName == successState ) {
                currentInstance.Stop(true);
                return Status.Success;
            }

            if ( !string.IsNullOrEmpty(failureState) && currentInstance.currentStateName == failureState ) {
                currentInstance.Stop(false);
                return Status.Failure;
            }

            return status;
        }

        void OnFSMFinish(bool success) {
            if ( status == Status.Running ) {
                status = success ? Status.Success : Status.Failure;
            }
        }

        protected override void OnReset() {
            if ( currentInstance != null ) {
                currentInstance.Stop();
            }
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR
        protected override void OnNodeInspectorGUI() {
            base.OnNodeInspectorGUI();
            if ( subGraph != null ) {
                successState = EditorUtils.Popup<string>("Success State", successState, subGraph.GetStateNames());
                failureState = EditorUtils.Popup<string>("Failure State", failureState, subGraph.GetStateNames());
            }
        }
#endif
        ///----------------------------------------------------------------------------------------------

    }
}