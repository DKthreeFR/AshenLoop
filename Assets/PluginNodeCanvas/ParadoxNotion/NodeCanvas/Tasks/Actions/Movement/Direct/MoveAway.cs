using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Category("Movement/Direct")]
    [Description("每帧将智能体（Agent）沿远离目标的方向移动，不使用寻路（Pathfinding）。")]
    public class MoveAway : ActionTask<Transform>
    {

        [RequiredField]
        public BBParameter<GameObject> target;
        public BBParameter<float> speed = 2;
        public BBParameter<float> stopDistance = 3;
        public bool waitActionFinish;

        protected override void OnUpdate() {
            if ( ( agent.position - target.value.transform.position ).magnitude >= stopDistance.value ) {
                EndAction();
                return;
            }

            agent.position = Vector3.MoveTowards(agent.position, target.value.transform.position, -speed.value * Time.deltaTime);
            if ( !waitActionFinish ) {
                EndAction();
            }
        }
    }
}