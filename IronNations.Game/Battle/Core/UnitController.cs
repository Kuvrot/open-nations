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

namespace IronNations.Battle.Core
{
    public class UnitController : SyncScript
    {

        public int idUnit = 0; //unique identifier for the unit

        public TransformComponent target;

        private CharacterComponent characterComponent;
        private UnitStats unitStats;

        public override void Start()
        {
            characterComponent = Entity.Get<CharacterComponent>();
            unitStats = Entity.Get<UnitStats>();
        }

        public override void Update()
        {
            if (target != null)
            {
                LookTarget();

                float distance = Vector3.Distance(Entity.Transform.Position , target.Position);
                DebugText.Print(distance.ToString(), new Int2(500, 300));
                if (distance >= unitStats.attackRange)
                {
                    MoveToTarget(target);
                }
                else
                {
                    StopMoving();
                }

                LookTarget();
            }
        }

        public void LookTarget()
        {
            Vector2 lookAngle = GetLookAtAngle(Entity.Transform.Position, target.Position);
            Quaternion result = Quaternion.RotationYawPitchRoll(lookAngle.Y, 0, 0);
            Entity.Transform.Rotation = result;
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
    }
}
