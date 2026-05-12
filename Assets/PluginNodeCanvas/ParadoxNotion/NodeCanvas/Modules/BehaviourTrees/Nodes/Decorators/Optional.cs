using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Optional")]
    [Category("Decorators")]
    [Description("正常执行被修饰的子节点，并返回一个“可选”（Optional）状态，使得该节点对父节点而言在状态返回上是“可忽略的”。\r\n\r\n这与直接禁用该节点的效果相同，但不同之处在于：子节点仍会照常执行。“我照常干活，但干完后悄悄说一句：‘别把我当回事儿～’——父节点会当我没返回任何关键状态，就像我没存在过一样。”\r\n\r\n比如：你在角色巡逻路径中插入一个“播放脚步声”的叶节点，并用 Optional 装饰器包起来。它依然会播放声音（执行了），但无论成功失败，都不会影响上层“序列”（Sequence）或“选择器”（Selector）的判断流程——就好像这个节点只是路过打了个酱油。适合用于日志、特效、调试等“不影响主逻辑”的辅助行为！.")]
    [ParadoxNotion.Design.Icon("UpwardsArrow")]
    public class Optional : BTDecorator
    {

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( decoratedConnection == null ) {
                return Status.Optional;
            }

            if ( status == Status.Resting ) {
                decoratedConnection.Reset();
            }

            status = decoratedConnection.Execute(agent, blackboard);
            return status == Status.Running ? Status.Running : Status.Optional;
        }
    }
}