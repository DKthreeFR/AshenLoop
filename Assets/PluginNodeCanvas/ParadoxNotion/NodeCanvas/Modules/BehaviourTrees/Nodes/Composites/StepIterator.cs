using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Step Sequencer步进顺序节点")]
    [Category("Composites")]
    [Description("与普通的 Sequencer（顺序节点）不同，普通的 Sequencer 会一直执行其所有子节点直到某一个失败为止；而 Step Sequencer（步进顺序节点）则是每次执行时只按顺序执行一个子节点。无论该子节点执行的结果是成功还是失败，都会直接返回其状态。更有耐心，每次只做一个小任务就汇报一次结果，不管这任务成没成，它都先回来报个到，下次执行时再做下一个任务。")]
    [ParadoxNotion.Design.Icon("StepIterator")]
    [Color("bf7fff")]
    public class StepIterator : BTComposite
    {

        private int current;

        public override void OnGraphStarted() {
            current = 0;
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {
            current = current % outConnections.Count;
            return outConnections[current].Execute(agent, blackboard);
        }

        protected override void OnReset() {
            current++;
        }
    }
}