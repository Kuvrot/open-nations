using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Physics;
using Valve.VR;

namespace IronNations.Battle.Core
{
    public class BattleManager : SyncScript
    {
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

        //Timer
        public short minutes = 0;
        public float seconds = 0;

        public TransformComponent destinationMarker;

        private int currentUnitSelected = -1;

        public List<UnitController> Player1Units = [];
        public List<UnitController> Player2Units = [];

        public Color selectionColor;

        public static float deltaTime = 0;

        private bool isZooming = false;

        public Prefab CannonDamage;

        public override void Update()
        {
            deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

            //Select unit
            if (Input.IsMouseButtonReleased(MouseButton.Left))
            {
                SelectUnit();
            }

            if (currentUnitSelected >= 0) // if a unit is selected
            {
                // Set destination
                if (Input.IsMouseButtonReleased(MouseButton.Right))
                {
                    SetTarget();
                }
            }

            //Zoom
            ZoomSystem();
        }

        private void SelectUnit()
        {
            HitResult hitResult = ScreenPositionToWorldPositionRaycast(Input.MousePosition, camera, this.GetSimulation());

            //Check if the player clicked the map or an enemy unit
            if (hitResult.Collider.Entity.Get<UnitController>() != null)
            {
                if (hitResult.Collider.Entity.Get<UnitStats>() != null && !hitResult.Collider.Entity.Get<UnitStats>().isEnemy)
                {
                    deselectPreviousUnit();
                    currentUnitSelected = hitResult.Collider.Entity.Get<UnitController>().idUnit;
                    Player1Units[currentUnitSelected].Entity.Get<UnitStats>().spriteUnit.Color = selectionColor;
                }
            }
            else
            {
                deselectPreviousUnit();
                currentUnitSelected = -1;
            }
        }

        private void ZoomSystem() {
            if (Input.IsKeyDown(Keys.Space) && !isZooming)
            {
                HitResult hitResult = ScreenPositionToWorldPositionRaycast(Input.MousePosition, camera, this.GetSimulation());
                camera.Entity.Transform.Position = new Vector3(hitResult.Point.X, 12, hitResult.Point.Z);
                camera.OrthographicSize = 10;
                isZooming = true;
            }
            else if (Input.IsKeyReleased(Keys.Space) && isZooming)
            {
                camera.OrthographicSize = 20;
                camera.Entity.Transform.Position = cameraInitialPosition;
                isZooming = false;
            }
        }

        private void SetTarget()
        {
            HitResult hitResult = ScreenPositionToWorldPositionRaycast(Input.MousePosition, camera, this.GetSimulation());

            if (Player1Units[currentUnitSelected] == null)
            {
                return;
            }

            //Check if the player clicked the map or an enemy unit
            if (hitResult.Collider.Entity.Get<UnitController>() != null)
            {
                if (hitResult.Collider.Entity.Get<UnitStats>() != null && hitResult.Collider.Entity.Get<UnitStats>().isEnemy)
                {
                    Player1Units[currentUnitSelected].target = hitResult.Collider.Entity.Transform;
                    destinationMarker.Position = new Vector3(0 , 0 , 15);
                    Player1Units[currentUnitSelected].isAttacking = true;
                    Entity.Get<BattleAudioManager>().PlayTrumpet();
                }
            }
            else // (in case the player clicks a position in the map)
            {
                destinationMarker.Position = hitResult.Point;
                TransformComponent auxTarget = new ();
                auxTarget.Position = destinationMarker.Position;
                Player1Units[currentUnitSelected].target = auxTarget;
                Player1Units[currentUnitSelected].isAttacking = false;
                Entity.Get<BattleAudioManager>().PlayDrums();
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

        private void deselectPreviousUnit()
        {
            if (currentUnitSelected >= 0)
            {
                if (Player1Units[currentUnitSelected] != null)
                {
                    Player1Units[currentUnitSelected].Entity.Get<UnitStats>().spriteUnit.Color = Color.White;
                }
            }
        }
    }
}
