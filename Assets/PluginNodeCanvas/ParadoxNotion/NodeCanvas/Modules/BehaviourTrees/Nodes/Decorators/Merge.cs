using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.BehaviourTrees
{

    [Name("Merge", -1)]
    [Description("“合并”（Merge）节点可以接受多个输入连接，因此允许多个父节点复用同一个叶节点。请注意，此功能尚属实验性质，可能导致意外行为。你有一个“播放受伤音效”的叶节点，既想在“被攻击”时用，又想在“掉下悬崖”时用。通过 Merge 节点，可以让这两个不同分支都连到同一个音效节点上，避免重复创建。但要小心——如果两个事件几乎同时触发，可能会出现状态冲突或执行顺序混乱。建议先小范围测试！")]
    [Category("Decorators")]
    public class Merge : BTDecorator
    {

        public override int maxInConnections => -1;

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {
            if ( status != Status.Running ) { decoratedConnection.Reset(); }
            return decoratedConnection.Execute(agent, blackboard);
        }
    }
}