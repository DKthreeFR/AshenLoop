using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Category("Decorators")]
    [Description("如果被修饰的子节点在超时时间结束后仍在运行（Running），则中断该子节点并返回“失败”（Failure）。角色正在执行“开门”动作（可能需要2秒动画），但你设了1秒超时。如果1秒后门还没开完（节点还在 Running），系统就强行打断，返回 Failure，让行为树立刻尝试备选方案，比如“踹门”或“绕路”。\r\n\r\n这在实时战斗或高响应需求场景中特别有用，避免 AI 卡在一个耗时动作里“发呆”。.")]
    [ParadoxNotion.Design.Icon("Timeout")]
    public class Timeout : BTDecorator
    {

        [Tooltip("The timeout period in seconds.")]
        public BBParameter<float> timeout = 1;

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( decoratedConnection == null ) {
                return Status.Optional;
            }

            status = decoratedConnection.Execute(agent, blackboard);
            if ( status == Status.Running ) {
                if ( elapsedTime >= timeout.value ) {
                    decoratedConnection.Reset();
                    return Status.Failure;
                }
            }

            return status;
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnNodeGUI() {
            GUILayout.Space(25);
            var pRect = new Rect(5, GUILayoutUtility.GetLastRect().y, rect.width - 10, 20);
            var t = 1 - ( elapsedTime / timeout.value );
            UnityEditor.EditorGUI.ProgressBar(pRect, t, elapsedTime > 0 ? string.Format("({0})", elapsedTime.ToString("0.0")) : "Ready");
        }

#endif
        ///----------------------------------------------------------------------------------------------

    }
}