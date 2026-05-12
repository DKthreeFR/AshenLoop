using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Invert")]
    [Category("Decorators")]
    [Description("将“成功”（Success）反转为“失败”（Failure），将“失败”（ Failure）反转为“成功”（Success）。")]
    [ParadoxNotion.Design.Icon("Remap")]
    public class Inverter : BTDecorator
    {

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( decoratedConnection == null )
                return Status.Optional;

            status = decoratedConnection.Execute(agent, blackboard);

            switch ( status ) {
                case Status.Success:
                    return Status.Failure;
                case Status.Failure:
                    return Status.Success;
            }

            return status;
        }
    }
}