using UnityEngine;
using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.DialogueTrees
{

    [ParadoxNotion.Design.Icon("Selector")]
    [Name("Multiple Task Condition")]
    [Category("Branch")]
    [Description("依次检查各子节点的条件，执行第一个条件返回为真的子节点。条件判断将基于所选的对话角色（Dialogue Actor）进行。你有一段对话分支，想根据当前说话的角色状态决定下一步：\r\n\r\n    子节点1（条件：Actor.HasKey == true）→ “用钥匙开门”\r\n    子节点2（条件：Actor.IsStrong == true）→ “暴力破门”\r\n    子节点3（无条件）→ “算了，绕路吧”\r\n\r\n如果当前角色有钥匙，直接走第一条；如果没有钥匙但力气大，就走第二条；如果两项都不满足，就默认走第三条。\r\n\r\n整个过程就像一个“优先级队列”，只执行第一个满足条件的选项，其余忽略。")]
    [Color("b3ff7f")]
    public class MultipleConditionNode : DTNode
    {

        [SerializeField, AutoSortWithChildrenConnections]
        private List<ConditionTask> conditions = new List<ConditionTask>();

        public override int maxOutConnections {
            get { return -1; }
        }

        public override void OnChildConnected(int index) {
            if ( conditions.Count < outConnections.Count ) {
                conditions.Insert(index, null);
            }
        }

        public override void OnChildDisconnected(int index) {
            conditions.RemoveAt(index);
        }

        protected override Status OnExecute(Component agent, IBlackboard bb) {

            if ( outConnections.Count == 0 ) {
                return Error("There are no connections on the Dialogue Condition Node");
            }

            for ( var i = 0; i < outConnections.Count; i++ ) {
                if ( conditions[i] == null || conditions[i].CheckOnce(finalActor.transform, graphBlackboard) ) {
                    DLGTree.Continue(i);
                    return Status.Success;
                }
            }

            ParadoxNotion.Services.Logger.LogWarning("No condition is true. Dialogue Ends.", LogTag.EXECUTION, this);
            DLGTree.Stop(false);
            return Status.Failure;
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        public override void OnConnectionInspectorGUI(int i) {
            NodeCanvas.Editor.TaskEditor.TaskFieldMulti<ConditionTask>(conditions[i], DLGTree, (c) => { conditions[i] = c; });
        }

        public override string GetConnectionInfo(int i) {
            return conditions[i] != null ? conditions[i].summaryInfo : "TRUE";
        }

#endif
    }
}