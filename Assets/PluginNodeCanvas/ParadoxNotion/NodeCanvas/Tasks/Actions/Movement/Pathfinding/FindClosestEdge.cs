using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

using NavMesh = UnityEngine.AI.NavMesh;
using NavMeshHit = UnityEngine.AI.NavMeshHit;

namespace NodeCanvas.Tasks.Actions
{

    [Name("Find Closest NavMesh Edge")]
    [Category("Movement/Pathfinding")]
    [Description("查找距离目标位置最近的导航网格（Navigation Mesh）上的有效位置。")]
    public class FindClosestEdge : ActionTask
    {

        public BBParameter<Vector3> targetPosition;
        [BlackboardOnly]
        public BBParameter<Vector3> saveFoundPosition;

        private NavMeshHit hit;

        protected override void OnExecute() {
            if ( NavMesh.FindClosestEdge(targetPosition.value, out hit, -1) ) {
                saveFoundPosition.value = hit.position;
                EndAction(true);
                return;
            }

            EndAction(false);
        }
    }
}