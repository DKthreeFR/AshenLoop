using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Name("Set Visibility")]
    [Category("GameObject")]
    [Description("设置 Renderer（渲染器）的激活状态，从而控制物体的可见或不可见。")]
    public class SetObjectVisibility : ActionTask<Renderer>
    {

        public enum SetVisibleMode
        {
            Hide = 0,
            Show = 1,
            Toggle = 2
        }

        public SetVisibleMode setTo = SetVisibleMode.Toggle;

        protected override string info {
            get { return string.Format("{0} {1}", setTo, agentInfo); }
        }

        protected override void OnExecute() {

            bool value;

            if ( setTo == SetVisibleMode.Toggle ) {

                value = !agent.enabled;

            } else {

                value = (int)setTo == 1;
            }

            agent.enabled = value;
            EndAction();
        }
    }
}