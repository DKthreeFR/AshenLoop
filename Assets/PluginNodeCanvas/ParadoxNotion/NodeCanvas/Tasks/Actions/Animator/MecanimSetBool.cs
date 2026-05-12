using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Name("Set Parameter Bool")]
    [Category("Animator")]
    [Description("你可以使用参数名（Parameter Name）或哈希 ID（HashID）来指定参数。如果留空参数名（或不填写），系统将自动改用 HashID。想读取一个叫 \"PlayerHealth\" 的黑板参数 → 填 Parameter Name = \"PlayerHealth\"；\r\n但如果你在代码里用的是 HashID（比如 0x3A7B2C1D），那就把 Parameter Name 留空，直接填 HashID，系统照样能找到它。.")]
    public class MecanimSetBool : ActionTask<Animator>
    {

        public BBParameter<string> parameter;
        public BBParameter<int> parameterHashID;
        public BBParameter<bool> setTo;

        protected override string info {
            get { return string.Format("Mec.SetBool {0} to {1}", string.IsNullOrEmpty(parameter.value) && !parameter.useBlackboard ? parameterHashID.ToString() : parameter.ToString(), setTo); }
        }

        protected override void OnExecute() {
            if ( !string.IsNullOrEmpty(parameter.value) ) {
                agent.SetBool(parameter.value, setTo.value);
            } else {
                agent.SetBool(parameterHashID.value, setTo.value);
            }
            EndAction(true);
        }
    }
}