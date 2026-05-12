using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.BehaviourTrees
{

    [Category("Decorators")]
    [ParadoxNotion.Design.Icon("Eye")]
    [Description("监控被修饰的子节点返回的状态，当该状态出现时，执行一个指定的动作。\r\n\r\n最终返回给父节点的状态，可以是子节点原本的状态，也可以是这个装饰器中动作执行后的新状态（取决于配置）。“一边看着孩子做事，一边准备‘应急预案’——不管他是成功还是失败，只要结果一出来，我就立刻做点别的（比如记录日志、播放音效、触发警报），然后再决定是如实上报结果，还是替他‘美化’一下再交差。”\r\n\r\n举个例子：角色尝试“开门”（子节点）。如果门打不开（返回 Failure），你可以在装饰器里立刻执行“播放‘咔哒’锁住音效”；然后选择是把“Failure”原样传上去（让上层处理失败），还是强行改成“Success”（假装没事发生，继续流程）。灵活又带感！")]
    public class Monitor : BTDecorator, ITaskAssignable<ActionTask>
    {

        public enum MonitorMode
        {
            Failure = 0,
            Success = 1,
            AnyStatus = 10,
        }

        public enum ReturnStatusMode
        {
            OriginalDecoratedChildStatus,
            NewDecoratorActionStatus,
        }

        [Name("Monitor"), Tooltip("The Status to monitor for.")]
        public MonitorMode monitorMode;
        [Name("Return"), Tooltip("The Status to return after (and if) the Action is executed.")]
        public ReturnStatusMode returnMode;

        private Status decoratorActionStatus;

        [SerializeField]
        private ActionTask _action;

        public ActionTask action {
            get { return _action; }
            set { _action = value; }
        }

        public Task task {
            get { return action; }
            set { action = (ActionTask)value; }
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( decoratedConnection == null ) {
                return Status.Optional;
            }

            var newChildStatus = decoratedConnection.Execute(agent, blackboard);
            if ( action == null ) {
                return newChildStatus;
            }

            if ( status != newChildStatus ) {
                var execute = false;
                execute |= newChildStatus == Status.Success && monitorMode == MonitorMode.Success;
                execute |= newChildStatus == Status.Failure && monitorMode == MonitorMode.Failure;
                execute |= monitorMode == MonitorMode.AnyStatus && newChildStatus != Status.Running;

                if ( execute ) {
                    decoratorActionStatus = action.Execute(agent, blackboard);
                    if ( decoratorActionStatus == Status.Running ) {
                        return Status.Running;
                    }
                }
            }

            return returnMode == ReturnStatusMode.NewDecoratorActionStatus && decoratorActionStatus != Status.Resting ? decoratorActionStatus : newChildStatus;
        }

        protected override void OnReset() {
            if ( action != null ) {
                action.EndAction(null);
                decoratorActionStatus = Status.Resting;
            }
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnNodeGUI() {
            GUILayout.Label(string.Format("<b>[On {0}]</b>", monitorMode.ToString()));
        }

#endif
        ///---------------------------------------UNITY EDITOR-------------------------------------------
    }
}