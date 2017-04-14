using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    interface IPhase
    {
        void Run();
    }

    interface IAdvancedPhase : IPhase
    {
        void ActivatePhase(RoboPhase previousPhase);
        void DeactivatePhase(RoboPhase nextPhase);

        RoboPhase Tick();
    }

    abstract class AdvancedPhase : IAdvancedPhase
    {
        protected YoloBot Robot { get; set; }

        public AdvancedPhase(YoloBot robot)
        {
            Robot = robot;
        }

        public virtual void ActivatePhase(RoboPhase previousPhase) { }

        public virtual void DeactivatePhase(RoboPhase nextPhase) { }

        public abstract void Run();

        public virtual RoboPhase Tick()
        {
            Run();

            return Robot.CurrentPhase;
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }
    }
}
