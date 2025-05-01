using Stride.Engine;
using Stride.Audio;
using Stride.Particles;
using System.Diagnostics;
using Stride.Core.Mathematics;
using Stride.Particles.Components;
using ServiceWire;
using Stride.Physics;
using System;
using Stride.UI.Controls;
using Stride.Core;

namespace IronNations.Battle.Core
{
    public enum UnitType
    {
         Infantery,
         Spearman,
         Musketeer,
         Cavalry,
         Artillery,
         Building
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
        public float searchEnemyRate = 10f;

        private float maxHealth = 0;

        //Effects
        public Sound attackSound; //Hit sound, shoot sound, cast sound, etc.
        public ParticleSystemComponent attackEffect; //Shoot effect, smoke, etc.
        public EntityComponent projectile; //(if is a range unit) Bullets, arrows, magic, etc.

        //Components
        [Display("Sprite of the unit (leave null)")]
        public SpriteComponent spriteUnit;
        [Display("Building sprite (in case is a building, if not leave null)")]
        public SpriteComponent buildingSprite;
        private UnitController unitController;
        private EntityComponent healthBar;

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

            CheckComponents();

            attackEffect.ParticleSystem.Stop();

            maxHealth = health;

            healthBar = Entity.GetChild(2).Get<EntityComponent>();
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

            if (healthBar != null)
            {
                healthBar.Entity.Transform.Scale = new Vector3 (spriteUnit.Intensity, 1, 1);
            }
        }

        public void CheckComponents()
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
