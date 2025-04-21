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

            soundInstance = unitStats.moveSound.CreateInstance();
        }

        private float auxRange = 0;
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
                        
                        if (distance <= auxRange / 2)
                        {
                            target.Entity.Get<UnitStats>().health -= unitStats.damage;
                        }
                        else
                        {
                            target.Entity.Get<UnitStats>().health -= unitStats.damage * 2;
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
    }
}
