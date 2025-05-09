using System;
using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using Stride.Audio;
using Stride.Rendering.Sprites;

namespace IronNations.Battle.Core
{
    public class UnitController : SyncScript
    {
        public int idUnit = 0; //unique identifier for the unit

        public TransformComponent target;

        private CharacterComponent characterComponent;
        private UnitStats unitStats;
        private AudioManager audioManager;

        public bool isAttacking = false;
        private bool isUnitDead = false;
        private SoundInstance soundInstance;
        private UnitStats enemyUnitStats;
        //Timers
        private float tick = 0;
        private float clock = 0;

        public override void Start()
        {
            characterComponent = Entity.Get<CharacterComponent>();
            unitStats = Entity.Get<UnitStats>();
            audioManager = Entity.Get<AudioManager>();

            clock = new Random().NextSingle();
            tick = new Random().Next(0 , 6);
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
            //Kill unit
            if (unitStats.health <= 0)
            {
                if (!isUnitDead)
                {
                    StopMoving();
                    characterComponent.Enabled = false;
                    unitStats.spriteUnit.Entity.Transform.Position -= new Vector3(0, 0.02f, 0); //Decreases the sprite height a little bit, so the alive units can pass above the killed unit
                    target = null;

                    if (unitStats.isEnemy)
                    {
                        BattleManager.Instance.Player2Units[idUnit] = null;
                    }
                    else
                    {
                        BattleManager.Instance.Player1Units[idUnit] = null;
                    }
                    isUnitDead = true;
                }
                else
                {
                    return;
                }
            }

            if (target != null)
            {
                LookTarget();

                //Search a new enemy every x seconds, in order to find the closest one
                if (unitStats.isEnemy && BattleManager.Instance.Player1Units.Count > 0)
                {
                    SearchForNewEnemy();
                }

                float distance = Vector3.Distance(Entity.Transform.Position , target.Position);

                if (isAttacking)
                {
                    enemyUnitStats = target.Entity.Get<UnitStats>();
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
                        //Check if friendly fire is possible
                        HitResult hit = this.GetSimulation().Raycast(Entity.Transform.Position, target.Position);

                        //If friendly fire is possible then don't shoot
                        if (hit.Collider != null && hit.Collider.Entity.Get<UnitStats>().isEnemy != unitStats.isEnemy)
                        {
                            unitStats.attackEffect.ParticleSystem.Stop();
                            unitStats.attackEffect.ParticleSystem.Play();

                            MakeDamage();

                            audioManager.PlaySoundOnce(unitStats.attackSound);
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
                        int random = new Random().Next(0, BattleManager.Instance.Player1Units.Count);

                        if (BattleManager.Instance.Player1Units[random] != null)
                        {
                            target = BattleManager.Instance.Player1Units[random].Entity.Transform;
                        }
                    }
                }
            }

            if (unitStats.unitType == UnitType.Building)
            {
                SpriteComponent buildingSprite = unitStats.buildingSprite;

                if (buildingSprite != null)
                {
                    DebugText.Print(unitStats.spriteUnit.Intensity.ToString(), new Int2(200,200));

                    SpriteFromSheet spriteSheet = buildingSprite.SpriteProvider as SpriteFromSheet;
                    if (unitStats.spriteUnit.Intensity >= 0.67f)
                    {
                        spriteSheet.CurrentFrame = 0;
                    }
                    else if (unitStats.spriteUnit.Intensity < 0.67f && unitStats.spriteUnit.Intensity >= 0.33f)
                    {
                        spriteSheet.CurrentFrame = 1;
                    }
                    else if (unitStats.spriteUnit.Intensity < 0.33f && unitStats.spriteUnit.Intensity >= 0.2f)
                    {
                        spriteSheet.CurrentFrame = 2;
                    }

                    if (unitStats.health <= 0f)
                    {
                        spriteSheet.CurrentFrame = 3;
                    }
                }
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

            clock += 1 * BattleManager.deltaTime;

            return false;
        }

        private void MakeDamage ()
        {
            if (target.Entity.Get<UnitStats>().health <= 0)
            {
                target = null;
                return;
            }

            // The infantry make 150% more damage to Cavalry
            if (unitStats.unitType == UnitType.Infantery && enemyUnitStats.unitType == UnitType.Cavalry)
            {
                target.Entity.Get<UnitStats>().health -= unitStats.damage * 2.5f;
                return;
            }

            // The Cavalry make 200% more damage to Musketeer
            if (unitStats.unitType == UnitType.Cavalry && enemyUnitStats.unitType == UnitType.Musketeer)
            {
                target.Entity.Get<UnitStats>().health -= unitStats.damage * 3f;
                return;
            }

            // The infantry make 100% more damage to Musketeer
            if (unitStats.unitType == UnitType.Infantery && enemyUnitStats.unitType == UnitType.Musketeer)
            {
                target.Entity.Get<UnitStats>().health -= unitStats.damage * 2f;
                return;
            }

            // The musketeers make half the damage to cavalry
            if (unitStats.unitType == UnitType.Musketeer && enemyUnitStats.unitType == UnitType.Cavalry)
            {
                target.Entity.Get<UnitStats>().health -= unitStats.damage * 0.5f;
                return;
            }

            // The musketeers make half the damage to cavalry
            if (unitStats.unitType == UnitType.Artillery)
            {
                int random = new Random().Next(0 , 101);

                Vector3 hitPosition = new ();

                //Cannon has a 50% chance of hitting the target
                if (random >= 50)
                {
                    if (unitStats.unitType == UnitType.Artillery && enemyUnitStats.unitType == UnitType.Building)
                    {
                        target.Entity.Get<UnitStats>().health -= unitStats.damage * 2;
                    }
                    else
                    {
                        target.Entity.Get<UnitStats>().health -= unitStats.damage;
                    }
                    
                    int randomZ = new Random().Next(0, 2);
                    int randomX = new Random().Next(0, 2);
                    hitPosition = target.Position + new Vector3(randomX / 2, 0, randomZ / 2);
                }
                else
                {
                    int randomZ = new Random().Next(1, 3);
                    int randomX = new Random().Next(1,3);
                    hitPosition = target.Position + new Vector3(randomX , 0 , randomZ);
                }

                if (BattleManager.Instance.CannonDamage != null)
                {
                    List<Entity> cannonHole = BattleManager.Instance.CannonDamage.Instantiate();
                    cannonHole[0].Transform.Position = hitPosition;
                    Entity.Scene.Entities.AddRange(cannonHole);
                }
                return;
            }

            target.Entity.Get<UnitStats>().health -= unitStats.damage;
        }

        private void SearchForNewEnemy()
        {
            if (tick >= unitStats.searchEnemyRate)
            {
                float closestEnemyDistance = 500;
                int selectedEnemy = 0;

                for (int i = BattleManager.Instance.Player1Units.Count - 1; i >= 0; i--)
                {
                    if (BattleManager.Instance.Player1Units[i] == null)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(Entity.Transform.Position, BattleManager.Instance.Player1Units[i].Entity.Transform.Position);

                    if (distance <= closestEnemyDistance)
                    {
                        closestEnemyDistance = distance;
                        selectedEnemy = i;
                    }
                }

                target = BattleManager.Instance.Player1Units[selectedEnemy].Entity.Transform;

                tick = 0;
            }
            tick += 1 * BattleManager.deltaTime;
        }

        private bool IsRangeUnit()
        {
            if (unitStats.unitType == UnitType.Musketeer)
            {
                return true;
            }

            return false;
        }
    }
}
