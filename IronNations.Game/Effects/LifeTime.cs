using Stride.Engine;
using IronNations.Battle.Core;
using Stride.Core;

namespace IronNations.Effects
{
    public class LifeTime : SyncScript
    {
        [Display("Life time (in seconds)")]
        public float lifeTime = 1;
        private float clock = 0;

        public override void Start()
        {
            // Initialization of the script.
        }

        public override void Update()
        {
            if (Counter())
            {
                Entity.Scene.Entities.Remove(Entity);
            }
        }

        private bool Counter()
        {
            if (clock >= lifeTime)
            {
                clock = 0;
                return true;
            }

            clock += 1 * BattleManager.deltaTime;

            return false;
        }
    }
}
