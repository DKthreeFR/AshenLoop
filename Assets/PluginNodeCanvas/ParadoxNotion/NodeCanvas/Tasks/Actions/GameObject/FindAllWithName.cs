using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Category("GameObject")]
    [Description("请注意，此操作性能较慢。\r\n\r\n如果未找到任何对象，该动作将返回“失败”（Failure）。")]
    public class FindAllWithName : ActionTask
    {

        [RequiredField]
        public BBParameter<string> searchName = "GameObject";
        [BlackboardOnly]
        public BBParameter<List<GameObject>> saveAs;

        protected override string info {
            get { return "GetObjects '" + searchName + "' as " + saveAs; }
        }

        protected override void OnExecute() {

            var gos = new List<GameObject>();
            foreach ( var go in Object.FindObjectsOfType<GameObject>() ) {
                if ( go.name == searchName.value )
                    gos.Add(go);
            }

            saveAs.value = gos;
            EndAction(gos.Count != 0);
        }
    }
}