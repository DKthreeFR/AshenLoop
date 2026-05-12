using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{

    [Category("GameObject")]
    [Description("在智能体（Agent）的 Transform 下，根据名称查找其子级 Transform。你想让角色发射魔法粒子，但粒子必须从“右手”位置发出。你就可以用这个节点，输入 \"RightHand\" 作为名称，在角色模型的层级结构里精准定位到右手的 Transform，后续动作（如生成特效、射线检测）就能以此为起点。")]
    public class FindChildByName : ActionTask<Transform>
    {

        [RequiredField]
        public BBParameter<string> childName;

        [BlackboardOnly]
        public BBParameter<Transform> saveAs;

        protected override string info {
            get { return string.Format("{0} = {1}.FindChild({2})", saveAs, agentInfo, childName); }
        }

        protected override void OnExecute() {
            var result = agent.Find(childName.value);
            saveAs.value = result;
            EndAction(result != null);
        }
    }
}