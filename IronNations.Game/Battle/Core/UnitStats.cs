using Stride.Engine;
using Stride.Audio;
using Stride.Particles;
using System.Diagnostics;
using Stride.Core.Mathematics;

namespace IronNations.Battle.Core
{
    public class UnitStats : SyncScript
    {
        public bool isSelected = false;

        public bool isEnemy = false;
        
        //stats
        public short health = 450; // Number of units
        public float damage = 20;
        public float attackRange = 20;
        public float speedMovement = 0.5f;
        public float attackRate = 2; // in seconds
        public bool isEliteUnit = false;
        public bool canMelee = true; // if the unit can melee attack or not
        public float rotationSpeed = 0.5f;

        //Effects
        public Sound atackSound; //Hit sound, shoot sound, cast sound, etc.
        public ParticleSystem attackEffect; //Shoot effect, smoke, etc.
        public EntityComponent projectile; //(if is a range unit) Bullets, arrows, magic, etc.

        //Components
        SpriteComponent spriteUnit;
        UnitController unitController;

        public override void Start()
        {
            // if is an elite unit, the stats will be improved
            /*
                damage + 25%
                speed  + 25%
                attackRate 25% faster
                attack range + 25%
            */
            if (isEliteUnit)
            {
                damage *= 1.5f;
                speedMovement *= 1.25f;
                attackRate *= 0.75f;
                attackRange *= 1.25f;
            }

            unitController = Entity.Get<UnitController>();

            checkComponents();

            attackEffect.Stop();

            if (isEnemy)
            {
                unitController.idUnit = BattleManager.Instance.Player2Units.Count;
                BattleManager.Instance.Player2Units.Add(unitController);
            }
            else
            {
                unitController.idUnit = BattleManager.Instance.Player1Units.Count;
                BattleManager.Instance.Player1Units.Add(unitController);
            }
        }

        public override void Update()
        {
            // Do stuff every new frame
        }

        public void checkComponents()
        {
            bool incompleteComponents = false;

            if (spriteUnit == null
                || unitController == null)
            {
                incompleteComponents = true;
            }

            if (incompleteComponents)
            {
                DebugText.Print(Entity.Name + " has null components!!", new Int2(500, 300));
            }
        }
    }
}
