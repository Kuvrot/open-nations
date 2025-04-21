using Stride.Engine;
using Stride.Audio;
using Stride.Particles;
using System.Diagnostics;
using Stride.Core.Mathematics;
using Stride.Particles.Components;
using ServiceWire;
using Stride.Physics;
using System;

namespace IronNations.Battle.Core
{
    public enum UnitType
    {
         Infantery,
         Spearman,
         Musketeer,
         Cavalry
    };

    public class UnitStats : SyncScript
    {
        public UnitType unitType;
        public bool isEnemy = false;
        
        //stats
        public float health = 450; // Number of units
        public float damage = 20;
        public float attackRange = 20;
        public float movementSpeed = 0.5f;
        public float attackRate = 2; // in seconds
        public bool isEliteUnit = false;
        public bool canMelee = true; // if the unit can melee attack or not
        public bool isMeleeMode = false; // if the unit is on melee mode
        public float rotationSpeed = 0.5f;

        private float maxHealth = 0;

        //Effects
        public Sound attackSound; //Hit sound, shoot sound, cast sound, etc.
        public ParticleSystemComponent attackEffect; //Shoot effect, smoke, etc.
        public EntityComponent projectile; //(if is a range unit) Bullets, arrows, magic, etc.

        //Components
        public SpriteComponent spriteUnit;
        private UnitController unitController;

        private bool initUnit = false;

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
                movementSpeed *= 1.25f;
                attackRate *= 0.75f;
                attackRange *= 1.25f;
            }

            spriteUnit = Entity.GetChild(0).Get<SpriteComponent>();
            unitController = Entity.Get<UnitController>();

            checkComponents();

            attackEffect.ParticleSystem.Stop();

            maxHealth = health;
        }

        public override void Update()
        {
            if (!initUnit)
            {
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

                initUnit = true;
            }

            spriteUnit.Intensity = health / maxHealth;
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
