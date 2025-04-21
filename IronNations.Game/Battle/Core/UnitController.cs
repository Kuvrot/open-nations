using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using System.Printing;
using ServiceWire;
using Stride.Physics;
using System.Security.RightsManagement;
using Stride.Audio;
using Silk.NET.SDL;

namespace IronNations.Battle.Core
{
    public class UnitController : SyncScript
    {
        public int idUnit = 0; //unique identifier for the unit

        public TransformComponent target;

        private CharacterComponent characterComponent;
        private UnitStats unitStats;
        private AudioManager audioManager;
        private float clock = 0;

        public bool isAttacking = false;
        public bool moving = true;
        SoundInstance soundInstance;

        public override void Start()
        {
            characterComponent = Entity.Get<CharacterComponent>();
            unitStats = Entity.Get<UnitStats>();
            audioManager = Entity.Get<AudioManager>();

            clock = new Random().NextSingle();
        }

        //Context of the variable: 
        /*
            There was a problem, because the unit didn't discriminate between going to a position or attacking an enemy,
            so the units didin't reached the exact position marked by the player, they always had a distance of the attack range.
            so this variable can be 0.5f (wich is the minimum distance) or the value of attackRange. With this variable the units will go to the exact place marked by the player, or leaving the required distance in case of attack.
         */
        private float auxRange = 0.5f;
        public override void Update()
        {
            if (target != null)
            {
                LookTarget();

                float distance = Vector3.Distance(Entity.Transform.Position , target.Position);
                
                DebugText.Print(distance.ToString(), new Int2(500, 300));

                if (isAttacking)
                {
                    auxRange = unitStats.attackRange;
                }
                else
                {
                    auxRange = 0.5f;
                }

                if (distance >= auxRange)
                {
                    MoveToTarget(target);
                }
                else
                {
                    if (Counter() && isAttacking)
                    {
                        unitStats.attackEffect.ParticleSystem.Stop();
                        unitStats.attackEffect.ParticleSystem.Play();

                        MakeDamage();

                        // if the unit has been melee attacked, the new target will be the unit that is attacking
                        if (target.Entity.Get<UnitStats>().isMeleeMode
                            && target.Entity.Get<UnitController>().target != Entity.Transform
                            && target.Entity.Get<UnitStats>().canMelee)
                        {
                            target.Entity.Get<UnitController>().target = Entity.Transform;
                            target.Entity.Get<UnitStats>().isMeleeMode = true;
                        }

                        audioManager.PlaySoundOnce(unitStats.attackSound);

                        if (target.Entity.Get<UnitStats>().health <= 0)
                        {
                            target = null;
                        }
                    }
                    StopMoving();
                }
                LookTarget();
            }
            else
            {
                if (unitStats.isEnemy)
                {
                    if (BattleManager.Instance.Player1Units.Count > 0)
                    {
                        isAttacking = true;
                        int random = new Random().Next(0 , BattleManager.Instance.Player1Units.Count);
                        target = BattleManager.Instance.Player1Units[random].Entity.Transform;
                    }
                }
            }
        
            //Kill unit
            if (unitStats.health <= 0)
            {
                StopMoving();
                characterComponent.Enabled = false;
                unitStats.spriteUnit.Entity.Transform.Position += new Vector3(0 , 0.51f , 0); //Decreases the sprite height a little bit, so the alive units can pass above the killed unit
                target = null;
            }
        
        }
        public void LookTarget()
        {
           if (target != null)
            {
                Vector2 lookAngle = GetLookAtAngle(Entity.Transform.Position, target.Position);
                Quaternion result = Quaternion.RotationYawPitchRoll(lookAngle.Y, 0, 0);
                Entity.Transform.Rotation = result;
            }
        }

        private Vector2 GetLookAtAngle(Vector3 source, Vector3 destination)
        {
            Vector3 dist = source - destination;
            float altitude = (float)Math.Atan2(dist.Y, Math.Sqrt(dist.X * dist.X + dist.Z * dist.Z));
            float azimuth = (float)Math.Atan2(dist.X, dist.Z);
            return new Vector2(altitude, azimuth);
        }

        public void MoveToTarget(TransformComponent target)
        {
            Vector3 direction = target.Position - Entity.Transform.Position;
            direction.Normalize();
            characterComponent.SetVelocity(new Vector3(direction.X, 0f, direction.Z) * unitStats.movementSpeed * (float)Game.UpdateTime.Elapsed.TotalSeconds);
        }

        public void StopMoving()
        {
            characterComponent.SetVelocity(Vector3.Zero);
        }

        private bool Counter()
        {
            if (clock >= unitStats.attackRate)
            {
                clock = 0;
                return true;
            }

            clock += 1 * (float)Game.UpdateTime.Elapsed.TotalSeconds;

            return false;
        }

        private void MakeDamage ()
        {
            UnitStats enemyUnitStats = target.Entity.Get<UnitStats>();

            if (unitStats.unitType == UnitType.Cavalry && enemyUnitStats.unitType == UnitType.Musketeer)
            {
                target.Entity.Get<UnitStats>().health -= unitStats.damage * 2;
                return;
            }

            if (unitStats.unitType == UnitType.Cavalry && enemyUnitStats.unitType == UnitType.Infantery)
            {
                target.Entity.Get<UnitStats>().health -= unitStats.damage / 2;
                return;
            }

            target.Entity.Get<UnitStats>().health -= unitStats.damage;
        }
    }
}
