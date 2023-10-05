
using SoT_Helper.Extensions;
using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace SoT_Helper.Models
{
    public class Cannon : DisplayObject
    {
        private static readonly Color ACTOR_COLOR = Color.Yellow;
        private const int CIRCLE_SIZE = 10;

        private readonly string _rawName;
        private Coordinates _coords;
        public Coordinates Coords { get => _coords; set => _coords = value; }
        //public int Size { get; set; }

        private Vector3 Velocity { get; set; }
        private Vector3 Position { get; set; }
        private Vector3 Forward { get; set; }
        private Vector3 Rotation { get; set; }
        private List<Vector3> Targets { get; set; }

        private float Gravity { get; set; }

        public Cannon(MemoryReader memoryReader, int actorId, ulong address, string rawName)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = actorId;
            ActorAddress = address;
            Rawname= _rawName = rawName;

            actor_root_comp_ptr = GetRootComponentAddress(address);

            // Generate our Actors's info
            if (SoT_DataManager.ActorName_keys.ContainsKey(_rawName))
                Name = SoT_DataManager.ActorName_keys[_rawName];
            else
                Name = _rawName;

            // All of our actual display information & rendering
            Color = ACTOR_COLOR;
            Text = BuildTextString();
            //Icon = new Icon(Shape.Circle, 5, Color, 0, 0);
            Size = 5;
            DisplayText = new DisplayText(10, Size + 2, -10/2);
            // Used to track if the display object needs to be removed
            //ToDelete = false;
        }

        protected override string BuildTextString()
        {
            return $"{Name} - {Distance}m";
        }

        public override void Update(Coordinates myCoords)
        {
            if (ToDelete || !bool.Parse(ConfigurationManager.AppSettings["ShowCannonTrajectoryPrediction"]))
            {
                this.ShowText = false;
                this.ShowIcon = false;
                return;
            }
            try
            {
                if (!CheckRawNameAndActorId(ActorAddress))
                {
                    return;
                }
                //Coords = CoordBuilder(actor_root_comp_ptr, coord_offset);

                // The cannons first child is the barrel, so we need to get that
                var AttachChildren = rm.ReadULong(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.AttachChildren"));
                var barrelMesh = rm.ReadULong(AttachChildren);
                //var coordinates = GetWorldCoordinates(barrelMesh);
                var barrelPos = rm.ReadVector3(barrelMesh + (ulong)SDKService.GetOffset("SceneComponent.ActorCoordinates"));
                var barrelRot = rm.ReadVector3(barrelMesh + (ulong)SDKService.GetOffset("SceneComponent.ActorRotation"));
                var coordinates = new Coordinates();
                coordinates.SetPosition(barrelPos);
                coordinates.SetRotation(barrelRot);
                Coords = coordinates;
                float newDistance = MathHelper.CalculateDistance(this.Coords, myCoords);
                Distance = newDistance;

                if (Distance > 10)
                {
                    this.ShowText = false;
                    this.ShowIcon = false;
                    return;
                }

                bool attachReplication = rm.ReadBool(ActorAddress + (ulong)SDKService.GetOffset("Actor.bReplicateAttachment"));
                ulong attachmentreplication = ActorAddress + (ulong)SDKService.GetOffset("Actor.AttachmentReplication");
                ulong parent = rm.ReadULong(attachmentreplication + (ulong)SDKService.GetOffset("RepAttachment.AttachParent"));
                Vector3 rotationOffset = rm.ReadVector3(attachmentreplication + (ulong)SDKService.GetOffset("RepAttachment.RotationOffset"));
                Vector3 locationOffset = rm.ReadVector3(attachmentreplication + (ulong)SDKService.GetOffset("RepAttachment.LocationOffset"));
                ulong parentComponentActor = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("Actor.ParentComponentActor"));
                string parentRawname = rm.ReadGname(GetActorId(parentComponentActor));

                actor_root_comp_ptr = GetRootComponentAddress(ActorAddress);
                var testCoords = GetWorldCoordinates(actor_root_comp_ptr);

                //coordinates = GetWorldCoordinates(barrelMesh);

                float GravityScale = rm.ReadFloat(ActorAddress + (ulong)SDKService.GetOffset("Cannon.ProjectileGravityScale"));
                Gravity = 981.0f * GravityScale;
                float LaunchSpeed = rm.ReadFloat(ActorAddress + (ulong)SDKService.GetOffset("Cannon.ProjectileSpeed"));
                float pitch = rm.ReadFloat(ActorAddress + (ulong)SDKService.GetOffset("Cannon.ServerPitch"));
                float yaw = rm.ReadFloat(ActorAddress + (ulong)SDKService.GetOffset("Cannon.ServerYaw"));
                var cannonrotation = new Vector3(pitch, yaw, 0);
                var cannonforward = (cannonrotation +  testCoords.GetRotation()).Forward(); //testCoords
                Rotation = cannonrotation;
                var pos2 = coordinates.GetPosition() + (cannonforward * 1.2f);
                Position = pos2;
                Forward = cannonforward;
                Vector3 Vel = cannonforward * LaunchSpeed;

                var cannonship = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("Actor.ParentComponentActor"));
                //var shipVelocity = rm.ReadVector3(ship + (ulong)SDKService.GetOffset("SceneComponent.ComponentVelocity"));
                ulong repMovement = cannonship + (ulong)SDKService.GetOffset("Actor.ReplicatedMovement");
                var shipLinearVelocity = rm.ReadVector3(repMovement + (ulong)SDKService.GetOffset("RepMovement.LinearVelocity")); // / new Vector3(100, 100, 100);
                var shipAngularVelocity = rm.ReadVector3(repMovement + (ulong)SDKService.GetOffset("RepMovement.AngularVelocity")) * new Vector3(1,1,0); // We ignore the Z axis because waves could offset it
                //var Vel2 = Vel + Ship.PlayerShip.LinearVelocity;
                var Vel2 = Vel + shipLinearVelocity + shipAngularVelocity;
                Velocity = Vel2;

                ScreenCoords = MathHelper.ObjectToScreen(myCoords, this.Coords);

                Targets = new List<Vector3>();

                if (SoT_DataManager.DisplayObjects
                        .Any(a => a is Ship && a.Distance < 550 && !a.ToDelete
                        && ((Ship)a).CrewId != SoT_Tool.LocalPlayerCrewId))
                {
                    int shipNo = 0;
                    var ships = SoT_DataManager.DisplayObjects
                        .Where(a => a is Ship && a.Distance < 550 && !a.ToDelete
                        && ((Ship)a).CrewId != SoT_Tool.LocalPlayerCrewId)
                        .Select(s => (Ship)s).OrderBy(s => s.ActorAddress).ToList();

                    foreach (var ship in ships)
                    {
                        //ship.DrawShipStatus(renderer, shipNo);
                        //shipNo++;
                        var angularVelocity = ship.AngularVelocity * new Vector3(1, 1, 0);
                        var linearVelocity = ship.LinearVelocity;
                        var shipVelocity = angularVelocity + linearVelocity;
                        //var targetRot = TargetAimAngle(ship.Coords.GetPosition(), shipVelocity);
                        //if(targetRot != null && targetRot != Vector3.Zero)
                        Targets.Add(ship.Coords.GetPosition());
                    }
                }
                if(SoT_DataManager.DisplayObjects.Any(a => a is Marker && a.Distance < 1750 && !a.ToDelete))
                {
                    var markers = SoT_DataManager.DisplayObjects
                        .Where(a => a is Marker && a.Distance < 1750 && !a.ToDelete)
                        .Select(s => (Marker)s).OrderBy(s => s.ActorAddress).ToList();

                    foreach (var marker in markers)
                    {
                        var targetRot = TargetAimAngle(marker.Coords.GetPosition(), Vector3.Zero);
                        if (targetRot != null && targetRot != Vector3.Zero)
                            Targets.Add(targetRot);
                    }
                }

                //if (this.ScreenCoords != null)
                //{
                //    this.ShowText = true;
                //    this.ShowIcon = true;

                //    // Update our text to reflect our new distance
                //    this.Distance = newDistance;
                //    this.Text = BuildTextString();
                //}
                //else
                //{
                //    // If it isn't on our screen, set it to invisible to save resources
                //    this.ShowText = false;
                //    this.ShowIcon = false;
                //}
            }
            catch (Exception ex) 
            {
                var test1 = Rawname;
                var test2 = ActorId;
                var test3 = ToDelete;

                ShowIcon = false;
                ShowText = false;
                //ToDelete = true;
            }
        }

        public Vector3[] GetProjectilePath(int MaxInterations, Vector3 Vel, Vector3 Pos, float Gravity)
        {
            List<Vector3> path = new List<Vector3>();
            float interval = 0.033f/50;
            Gravity= Gravity * 100;
            //float interval = 0.033f;
            for (int i = 0; i < MaxInterations; ++i)
            {
                path.Add(Pos);
                Vector3 move;
                move.X = (Vel.X) * interval;
                move.Y = (Vel.Y) * interval;
                float newZ = Vel.Z - (Gravity * interval);
                move.Z = ((Vel.Z + newZ) * 0.5f) * interval;
                Vel.Z = newZ;
                Vector3 nextPos = Pos + move;
                //bool res = Trace(Pos, nextPos);
                Pos = nextPos;
                if (Pos.Z < -0.5f) break; // we hit water?
            }
            return path.ToArray();
        }

        public Vector3 TargetAimAngle(Vector3 targetPosition, Vector3 targetVelocity, int maxIterations = 10)
        {
            float GravityScale = rm.ReadFloat(ActorAddress + (ulong)SDKService.GetOffset("Cannon.ProjectileGravityScale"));
            var gravity = 981.0f * GravityScale;
            Vector3 relativeTargetPosition = targetPosition - Coords.GetPosition();
            float LaunchSpeed = rm.ReadFloat(ActorAddress + (ulong)SDKService.GetOffset("Cannon.ProjectileSpeed"));

            Vector3 cannonVelocity;
            var ship = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("Actor.ParentComponentActor"));
            ulong repMovement = ship + (ulong)SDKService.GetOffset("Actor.ReplicatedMovement");
            var shipLinearVelocity = rm.ReadVector3(repMovement + (ulong)SDKService.GetOffset("RepMovement.LinearVelocity")); // / new Vector3(100, 100, 100);
            var shipAngularVelocity = rm.ReadVector3(repMovement + (ulong)SDKService.GetOffset("RepMovement.AngularVelocity"));
            //var Vel2 = Vel + Ship.PlayerShip.LinearVelocity;
            cannonVelocity = shipLinearVelocity + shipAngularVelocity;

            Vector3 relativeTargetVelocity = targetVelocity - cannonVelocity;
            Vector3 predictedTargetPosition = relativeTargetPosition;

            for (int i = 0; i < maxIterations; i++)
            {
                float flightTime = predictedTargetPosition.Magnitude() / LaunchSpeed;
                predictedTargetPosition = relativeTargetPosition + relativeTargetVelocity * flightTime;

                Vector3 toTarget = predictedTargetPosition;
                Vector3 toTargetXZ = toTarget;
                toTargetXZ.Y = 0f;

                float x = toTargetXZ.Magnitude();
                float y = toTarget.Y;
                float v = LaunchSpeed;
                float g = gravity;
                //float g = Physics.gravity.y;

                float underTheSquareRoot = (v * v * v * v) - g * (g * x * x + 2 * y * v * v);

                if (underTheSquareRoot >= 0f)
                {
                    float root = MathF.Sqrt(underTheSquareRoot);
                    float highAngle = MathF.Atan((v * v + root) / (g * x)) * (float)MathHelper.Rad2Deg;
                    float lowAngle = MathF.Atan((v * v - root) / (g * x)) * (float)MathHelper.Rad2Deg;

                    // Usually you want the low angle, because it results in a faster shot
                    float launchAngle = lowAngle;

                    // Calculate the Quaternion for the cannon to rotate towards.
                    //Quaternion targetRotation = Quaternion.LookRotation(toTargetXZ);
                    //targetRotation *= Quaternion.Euler(launchAngle, 0f, 0f);
                    //transform.rotation = targetRotation;

                    var targetRotation = toTargetXZ + new Vector3(launchAngle,0,0);
                    return targetRotation;
                }
            }
            return Vector3.Zero;
        }

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            if (ToDelete || !bool.Parse(ConfigurationManager.AppSettings["ShowCannonTrajectoryPrediction"]))
            {
                return;
            }
            if (Distance > 10)
            {
                return;
            }

            if (ScreenCoords != null)
            {
                // Text
                CharmService.DrawOutlinedString(renderer,ScreenCoords.Value.X + DisplayText.Offset_X,
                    ScreenCoords.Value.Y + DisplayText.Offset_Y,
                    Text, Color, 0);
            }

            var path = GetProjectilePath(5000, Velocity, Position, Gravity);
            int i = 0;
            foreach (var trajectoryPathPosition in path)
            {
                var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, trajectoryPathPosition);
                if (spotCoords != null && spotCoords.HasValue)
                {
                    renderer.DrawCircle(spotCoords.Value.X, spotCoords.Value.Y,
                    3, 1, Color.Yellow, false);
                    if(i == path.Length-1)
                    {
                        var distance = MathHelper.CalculateDistance(trajectoryPathPosition, SoT_Tool.my_coords);
                        CharmService.DrawOutlinedString(renderer, spotCoords.Value.X, spotCoords.Value.Y,
                                                       $"Impact [{distance}m]", Color.Yellow, 10);
                    }
                }
                i++;
            }
            if (Targets.Any())
            {
                int shipNo = 0;
                var ships = SoT_DataManager.DisplayObjects
                    .Where(a => a is Ship && a.Distance < 1750 && !a.ToDelete
                    && ((Ship)a).CrewId != SoT_Tool.LocalPlayerCrewId)
                    .Select(s => (Ship)s).OrderBy(s => s.ActorAddress).ToList();

                float interval = 0.033f / 50;
                var hit = path.Last();
                float secondsToHit = interval * path.Count();

                foreach (var ship in ships)
                {
                    //ship.DrawShipStatus(renderer, shipNo);
                    //shipNo++;
                    var angularVelocity = ship.AngularVelocity * new Vector3(1, 1, 0);
                    var linearVelocity = ship.LinearVelocity;
                    var shipVelocity = angularVelocity + linearVelocity; //angularVelocity + 
                    var shipPosition = ship.Coords.GetPosition();
                    var shipTargetPos = shipPosition + shipVelocity * secondsToHit;
                    var distance = MathHelper.CalculateDistance(hit, shipTargetPos);
                    string text = $"{distance}";

                    CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X, ScreenCoords.Value.Y, text, Color.DeepPink, 10);
                    //var targetRot = TargetAimAngle(ship.Coords.GetPosition(), shipVelocity);
                    //if (targetRot != null && targetRot != Vector3.Zero)
                    //    Targets.Add(targetRot);
                }

                //if (target.Forward().X < Forward.X)

                

                //var forward = target.Forward();
                //var pos = Position + forward * 3f;
                //var screenpos = MathHelper.ObjectToScreen(SoT_Tool.my_coords, pos);

                //if (screenpos != null && screenpos.HasValue)
                //{
                //    renderer.DrawCircle(screenpos.Value.X, screenpos.Value.Y,
                //                                                      4, 2, Color.DeepPink, false);
                //}
            }

            //var cannonHolePos = MathHelper.ObjectToScreen(SoT_Tool.my_coords, Position);
            //if (cannonHolePos != null && cannonHolePos.HasValue)
            //{
            //    renderer.DrawCircle(cannonHolePos.Value.X, cannonHolePos.Value.Y,
            //        4, 2, Color.OrangeRed, true);
            //}
        }
        
        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (ToDelete || !bool.Parse(ConfigurationManager.AppSettings["ShowCannonTrajectoryPrediction"]))
            {
                return;
            }
            if (Distance > 10)
            {
                return;
            }

            if (ScreenCoords != null)
            {
                // Text
                renderer.DrawOutlinedString(ScreenCoords.Value.X + DisplayText.Offset_X,
                    ScreenCoords.Value.Y + DisplayText.Offset_Y,
                    Text, Color, 0);
            }

            var path = GetProjectilePath(5000, Velocity, Position, Gravity);
            int i = 0;
            foreach (var trajectoryPathPosition in path)
            {
                var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, trajectoryPathPosition);
                if (spotCoords != null && spotCoords.HasValue)
                {
                    renderer.Graphics.DrawCircle(spotCoords.Value.X, spotCoords.Value.Y,
                    3, 1, Color.Yellow, false);
                    if (i == path.Length - 1)
                    {
                        var distance = MathHelper.CalculateDistance(trajectoryPathPosition, SoT_Tool.my_coords);
                        renderer.DrawOutlinedString(spotCoords.Value.X, spotCoords.Value.Y,
                                                       $"Impact [{distance}m]", Color.Yellow, 10);
                    }
                }
                i++;
            }
            if (Targets.Any())
            {
                int shipNo = 0;
                var ships = SoT_DataManager.DisplayObjects
                    .Where(a => a is Ship && a.Distance < 1750 && !a.ToDelete
                    && ((Ship)a).CrewId != SoT_Tool.LocalPlayerCrewId)
                    .Select(s => (Ship)s).OrderBy(s => s.ActorAddress).ToList();

                float interval = 0.033f / 50;
                var hit = path.Last();
                float secondsToHit = interval * path.Count();

                foreach (var ship in ships)
                {
                    var angularVelocity = ship.AngularVelocity * new Vector3(1, 1, 0);
                    var linearVelocity = ship.LinearVelocity;
                    var shipVelocity = angularVelocity + linearVelocity; //angularVelocity + 
                    var shipPosition = ship.Coords.GetPosition();
                    var shipTargetPos = shipPosition + shipVelocity * secondsToHit;
                    var distance = MathHelper.CalculateDistance(hit, shipTargetPos);
                    string text = $"{distance}";

                    renderer.DrawOutlinedString(ScreenCoords.Value.X, ScreenCoords.Value.Y, text, Color.DeepPink, 10);
                }
            }
        }

        private Coordinates GetWorldCoordinates(ulong rootAddress)
        {
            var relativePosition = rm.ReadVector3(rootAddress + (ulong)SDKService.GetOffset("SceneComponent.RelativeLocation"));
            var relativeRotation = rm.ReadVector3(rootAddress + (ulong)SDKService.GetOffset("SceneComponent.RelativeRotation"));
            var RelativeScale3D = rm.ReadVector3(rootAddress + (ulong)SDKService.GetOffset("SceneComponent.RelativeScale3D")) * 100;

            var quart = rm.ReadQuaternion(rootAddress + (ulong)SDKService.GetOffset("SceneComponent.ComponentToWorld"));
            //var rot = quart.Euler();
            var rot = quart.Conjugate().Rotator();
            var position = rm.ReadVector3(rootAddress + (ulong)SDKService.GetOffset("SceneComponent.ComponentToWorld") + 16);

            var coords = new Coordinates() { x = position.X, y = position.Y, z = position.Z, rot_x = rot.X, rot_y = rot.Y, rot_z = rot.Z };

            return coords;
        }

        private Vector3 GetWorldPosition(ulong rootAddress, ref Vector3 position)
        {
            var relativePosition = rm.ReadVector3(rootAddress + (ulong)SDKService.GetOffset("SceneComponent.RelativeLocation"));
            var relativeRotation = rm.ReadVector3(rootAddress + (ulong)SDKService.GetOffset("SceneComponent.RelativeRotation"));
            var RelativeScale3D = rm.ReadVector3(rootAddress + (ulong)SDKService.GetOffset("SceneComponent.RelativeScale3D")) * 100;

            var rawname = rm.ReadGname(GetActorId(rootAddress));

            if (RelativeScale3D * 100 == Vector3.Zero)
                RelativeScale3D = new Vector3(1, 1, 1);
            position += relativePosition * RelativeScale3D;

            var attachParent = rm.ReadULong(rootAddress + (ulong)SDKService.GetOffset("SceneComponent.AttachParent"));
            if (attachParent != 0)
                return GetWorldPosition(attachParent, ref position);
            else
                return position;
        }

        private Vector3 GetWorldRotation(ulong rootAddress, ref Vector3 rotation)
        {
            var relativeRotation = rm.ReadVector3(rootAddress + (ulong)SDKService.GetOffset("SceneComponent.RelativeRotation"));

            rotation += relativeRotation;

            var attachParent = rm.ReadULong(rootAddress + (ulong)SDKService.GetOffset("SceneComponent.AttachParent"));
            if (attachParent != 0)
                return GetWorldRotation(attachParent, ref rotation);
            else
                return rotation;
        }
    }
}
