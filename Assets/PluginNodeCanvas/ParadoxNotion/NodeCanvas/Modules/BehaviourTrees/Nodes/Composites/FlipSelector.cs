using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Category("Composites")]
    [Description("工作方式像普通的选择节点，但当某个子节点返回“成功”后，该子节点会被移动到队列的末尾。\n结果是，之前“失败”过的子节点总是会被优先检查，而最近“成功”过的子节点会被放在最后检查。试着做这件事，不行就试下一件”，但它有个特殊机制：一旦做成了，就把自己排到队尾去，让别人先试试。比如一个角色有“寻找食物”和“寻找水源”两个需求。如果它刚刚“找到食物”成功了，这个任务就会被排到最后。下一次循环时，它会优先检查“寻找水源”（之前可能因为没水而失败），从而实现需求的动态平衡。")]
    [ParadoxNotion.Design.Icon("FlipSelector")]
    [Color("b3ff7f")]
    public class FlipSelector : BTComposite
    {

        private int current;

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            for ( var i = current; i < outConnections.Count; i++ ) {

                status = outConnections[i].Execute(agent, blackboard);

                if ( status == Status.Running ) {
                    current = i;
                    return Status.Running;
                }

                if ( status == Status.Success ) {
                    SendToBack(i);
                    return Status.Success;
                }
            }

            return Status.Failure;
        }

        void SendToBack(int i) {
            var c = outConnections[i];
            outConnections.RemoveAt(i);
            outConnections.Add(c);
        }

        protected override void OnReset() {
            current = 0;
        }
    }
}