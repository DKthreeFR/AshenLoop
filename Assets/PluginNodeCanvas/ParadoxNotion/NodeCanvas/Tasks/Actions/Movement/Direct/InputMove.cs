using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Category("Movement/Direct")]
    [Description("根据每秒提供的输入值（范围为 -1 到 1）移动并旋转智能体（Agent），计算时已结合 deltaTime 以确保帧率无关的平滑运动。")]
    public class InputMove : ActionTask<Transform>
    {

        [BlackboardOnly]
        public BBParameter<float> strafe;
        [BlackboardOnly]
        public BBParameter<float> turn;
        [BlackboardOnly]
        public BBParameter<float> forward;
        [BlackboardOnly]
        public BBParameter<float> up;

        public BBParameter<float> moveSpeed = 1;
        public BBParameter<float> rotationSpeed = 1;

        public bool repeat;

        protected override void OnUpdate() {
            var targetRotation = agent.rotation * Quaternion.Euler(Vector3.up * turn.value * 10);
            agent.rotation = Quaternion.Slerp(agent.rotation, targetRotation, rotationSpeed.value * Time.deltaTime);

            var forwardMovement = agent.forward * forward.value * moveSpeed.value * Time.deltaTime;
            var strafeMovement = agent.right * strafe.value * moveSpeed.value * Time.deltaTime;
            var upMovement = agent.up * up.value * moveSpeed.value * Time.deltaTime;
            agent.position += strafeMovement + forwardMovement + upMovement;

            if ( !repeat ) {
                EndAction();
            }
        }
    }
}