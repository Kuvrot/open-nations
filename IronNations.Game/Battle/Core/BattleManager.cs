using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Physics;
using System.Windows.Media.Media3D;
using Valve.VR;

namespace IronNations.Battle.Core
{
    public class BattleManager : SyncScript
    {
        //Timer
        public short minutes = 0;
        public float seconds = 0;

        public TransformComponent destinationMarker;

        private int currentUnitSelected = -1;

        public List<UnitController> Player1Units = [];
        public List<UnitController> Player2Units = [];

        static float deltaTime = 0;

        public CameraComponent camera;
        private Vector3 cameraInitialPosition;

        #region Singleton
        public static BattleManager Instance { get; private set; }

        public override void Start()
        {
            Instance = this;
            cameraInitialPosition = camera.Entity.Transform.Position;
        }
        #endregion

        private bool isZooming = false;

        public override void Update()
        {
            deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

            if (currentUnitSelected > 0)
            {
                if (Input.IsMouseButtonReleased(MouseButton.Right))
                {
                    HitResult hitResult = ScreenPositionToWorldPositionRaycast(Input.MousePosition, camera, this.GetSimulation());
                    destinationMarker.Position = hitResult.Point;
                }
            }
            else
            {

            }

            //Zoom
            if (Input.IsKeyDown(Keys.Space) && !isZooming)
            {
                HitResult hitResult = ScreenPositionToWorldPositionRaycast(Input.MousePosition, camera, this.GetSimulation());
                camera.Entity.Transform.Position = new Vector3(hitResult.Point.X , 12 , hitResult.Point.Z);
                camera.OrthographicSize = 10;
                isZooming = true;
            }
            else if(Input.IsKeyReleased(Keys.Space) && isZooming)
            {
                camera.OrthographicSize = 20;
                camera.Entity.Transform.Position = cameraInitialPosition;
                isZooming = false;
            }
        }

        private HitResult ScreenPositionToWorldPositionRaycast (Vector2 screenPos, CameraComponent camera, Simulation simulation)
        {
            Matrix invViewProj = Matrix.Invert(camera.ViewProjectionMatrix);

            // Reconstruct the projection-space position in the (-1, +1) range.
            //    Don't forget that Y is down in screen coordinates, but up in projection space
            Vector3 sPos;
            sPos.X = screenPos.X * 2f - 1f;
            sPos.Y = 1f - screenPos.Y * 2f;

            // Compute the near (start) point for the raycast
            // It's assumed to have the same projection space (x,y) coordinates and z = 0 (lying on the near plane)
            // We need to unproject it to world space
            sPos.Z = 0f;
            Vector4 vectorNear = Vector3.Transform(sPos, invViewProj);
            vectorNear /= vectorNear.W;

            // Compute the far (end) point for the raycast
            // It's assumed to have the same projection space (x,y) coordinates and z = 1 (lying on the far plane)
            // We need to unproject it to world space
            sPos.Z = 1f;
            Vector4 vectorFar = Vector3.Transform(sPos, invViewProj);
            vectorFar /= vectorFar.W;

            // Raycast from the point on the near plane to the point on the far plane and get the collision result
            return simulation.Raycast(vectorNear.XYZ(), vectorFar.XYZ());
        }
    }
}
