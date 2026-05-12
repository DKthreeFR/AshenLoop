using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Category("Physics")]
    [Description("获取位于智能体（Agent）当前位置的物理重叠球体（Overlap Sphere）内、且排除智能体自身的所有游戏对象列表。“以我为中心，画个隐形的球——把球里除了我自己以外的所有东西都列出来，看看谁在我身边！”\r\n\r\n举个例子：\r\n\r\n    一个爆炸技能要对周围 3 米内的敌人造成伤害，就可以用这个节点找出所有符合条件的敌方单位；\r\n    或者 NPC 想检测“附近有没有可拾取的道具”，就用一个小范围球体扫描地面物品。\r\n")]
    public class GetOverlapSphereObjects : ActionTask<Transform>
    {

        public LayerMask layerMask = -1;
        public BBParameter<float> radius = 2;
        [BlackboardOnly]
        public BBParameter<List<GameObject>> saveObjectsAs;

        protected override void OnExecute() {

            var hitColliders = Physics.OverlapSphere(agent.position, radius.value, layerMask);
            saveObjectsAs.value = hitColliders.Select(c => c.gameObject).ToList();
            saveObjectsAs.value.Remove(agent.gameObject);

            if ( saveObjectsAs.value.Count == 0 ) {
                EndAction(false);
                return;
            }

            EndAction(true);
        }

        public override void OnDrawGizmosSelected() {
            if ( agent != null ) {
                Gizmos.color = new Color(1, 1, 1, 0.2f);
                Gizmos.DrawSphere(agent.position, radius.value);
            }
        }
    }
}