using SoT_Helper.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoT_Helper.Services
{
    public class MathHelper
    {
        static int SOT_WINDOW_W { get { return SoT_Tool.SOT_WINDOW_W; } }
        static int SOT_WINDOW_H { get { return SoT_Tool.SOT_WINDOW_H; } }
        static Coordinates my_coords { get { return SoT_Tool.my_coords; } }
        static MemoryReader mem { get { return SoT_Tool.mem; } }


        public static Vector2 Rotate(Vector2 vector, float angleDegrees)
        {
            float angleRadians = (float)(Math.PI / 180) * angleDegrees;
            float cosTheta = (float)Math.Cos(angleRadians);
            float sinTheta = (float)Math.Sin(angleRadians);

            float x = vector.X * cosTheta - vector.Y * sinTheta;
            float y = vector.X * sinTheta + vector.Y * cosTheta;

            return new Vector2(x, y);
        }

        public static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, float angle, bool angleInRadians = false)
        {
            if (!angleInRadians)
                angle = (float)(angle * (Math.PI / 180f));
            float cosTheta = (float)Math.Cos(angle);
            float sinTheta = (float)Math.Sin(angle);
            Vector2 returnVec = new Vector2(
                cosTheta * (pointToRotate.X - centerPoint.X) - sinTheta * (pointToRotate.Y - centerPoint.Y),
                sinTheta * (pointToRotate.X - centerPoint.X) + cosTheta * (pointToRotate.Y - centerPoint.Y)
            );
            returnVec += centerPoint;
            return returnVec;
        }

        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            dir = ToEuler(new Quaternion(angles, 0)) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }

        public const double Rad2Deg = 180.0 / Math.PI;

        // Convert a Quaternion to Euler angles
        public static Vector3 ToEuler(Quaternion q)
        {
            Vector3 euler;
            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            euler.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                euler.Y = (float)(Math.CopySign(Math.PI / 2, sinp)); // use 90 degrees if out of range
            else
                euler.Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            euler.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return euler;
        }

        public static Coordinates CoordBuilder(ulong actor_address, int offset = 0x78, bool camera = true, bool fov = false)
        {
            byte[] actor_bytes;
            // PlayerController -> playerCameraManager -> playerCamera -> FieldOfView
            if (fov)
            {
                actor_bytes = mem.ReadBytes((IntPtr)actor_address + offset, 50); //0x18 = 24 bytes
            }
            else
            {
                actor_bytes = mem.ReadBytes((IntPtr)actor_address + offset, 24);
            }

            var unpacked = fov ? new Coordinates { fov = BitConverter.ToSingle(actor_bytes, 0x28) } :
                                 new Coordinates();

            unpacked.x = BitConverter.ToSingle(actor_bytes, 0) / 100;
            unpacked.y = BitConverter.ToSingle(actor_bytes, 4) / 100;
            unpacked.z = BitConverter.ToSingle(actor_bytes, 8) / 100;
            if (camera)
            {
                unpacked.rot_x = BitConverter.ToSingle(actor_bytes, 12);
                unpacked.rot_y = BitConverter.ToSingle(actor_bytes, 16);
                unpacked.rot_z = BitConverter.ToSingle(actor_bytes, 20);
            }

            return unpacked;
        }

        public static Vector2? PointOnScreenClosestToPosition(Vector3 targetPosition)
        {
            // Calculate the vector from the camera to the object
            Vector3 cameraToObject = my_coords.GetPosition() - targetPosition;

            // Project that vector onto the camera's direction vector to find
            // the point on the camera's line of sight that's closest to the object
            Vector3 closestPoint = my_coords.GetPosition() + cameraToObject.Project(my_coords.GetRotation());

            // Now we transform this 3D position to 2D screen space
            Vector2? screenPoint = ObjectToScreen(my_coords, closestPoint);

            return screenPoint;
        }

        private static double Dot(Tuple<double, double, double> array1, Tuple<double, double, double> array2)
        {
            if (array2.Item1 == 0 && array2.Item2 == 0 && array2.Item3 == 0)
            {
                return 0.0;
            }

            return array1.Item1 * array2.Item1 + array1.Item2 * array2.Item2 + array1.Item3 * array2.Item3;
        }

        

        //Vector3 Rotator(Quaternion quart)
        //{
        //    List<List<double>> RotMatrix(*this);
        //        FRotator Rotator;

        //        float SP = RotMatrix.m[0][2];
        //        float CP = std::sqrt(1.f - SP * SP);
        //        Rotator.pitch = std::atan2(SP, CP)* 180.f / MYPI;

        //    if (std::abs(CP) < SMALL_NUMBER)
        //    {
        //        Rotator.yaw = std::atan2(-RotMatrix.m[2][0], RotMatrix.m[1][1]) * 180.f / MYPI;
        //        Rotator.roll = 0.f;
        //    }
        //    else
        //    {
        //        Rotator.yaw = std::atan2(RotMatrix.m[0][1], RotMatrix.m[0][0])* 180.f / MYPI;
        //        Rotator.roll = std::atan2(RotMatrix.m[1][2], RotMatrix.m[2][2])* 180.f / MYPI;
        //    }

        //    return Rotator;
        //}

        public static Vector3 RotationToVector(Vector3 angle)
        {
            double pitch = (angle.X * Math.PI / 180.0);
            double yaw = (angle.Y * Math.PI / 180.0);
            double cosPitch = Math.Cos(pitch);
            double sinPitch = Math.Sin(pitch);
            double cosYaw = Math.Cos(yaw);
            double sinYaw = Math.Sin(yaw);
            return new Vector3((float)(cosPitch * cosYaw), (float)(cosPitch * sinYaw), (float)sinPitch);
        }

        public static Vector2? ObjectTracePointToScreen(Vector3 position)
        {
            return ObjectTracePointToScreen(new Coordinates() { x = position.X, y = position.Y, z = position.Z });
        }

        public static Vector2? ObjectTracePointToScreen(Coordinates actor)
        {
            Coordinates player = my_coords;

            Tuple<double, double, double> player_camera = Tuple.Create((double)player.rot_x, (double)player.rot_y, (double)player.rot_z);
            List<List<double>> temp = MakeVMatrix(player_camera);

            Tuple<double, double, double> v_axis_x = Tuple.Create(temp[0][0], temp[0][1], temp[0][2]);
            Tuple<double, double, double> v_axis_y = Tuple.Create(temp[1][0], temp[1][1], temp[1][2]);
            Tuple<double, double, double> v_axis_z = Tuple.Create(temp[2][0], temp[2][1], temp[2][2]);

            Tuple<double, double, double> v_delta = Tuple.Create((double)actor.x - (double)player.x,
                                                                 (double)actor.y - (double)player.y,
                                                                 (double)actor.z - (double)player.z);
            List<double> v_transformed = new List<double>
            {
                Dot(v_delta, v_axis_y),
                Dot(v_delta, v_axis_z),
                Dot(v_delta, v_axis_x)
            };

            double fov = player.fov;
            double screen_center_x = SOT_WINDOW_W / 2;
            double screen_center_y = SOT_WINDOW_H / 2;

            double tmp_fov = Math.Tan(fov * Math.PI / 360);

            double x = screen_center_x + v_transformed[0] * (screen_center_x / tmp_fov)
                        / v_transformed[2];
            double y = screen_center_y - v_transformed[1] * (screen_center_x / tmp_fov)
                        / v_transformed[2];
            // Credit https://github.com/AlexBurneikis
            // Checks if target is behind us
            if (v_transformed[2] < 1.0)
            {
                // If it is, we need to do a different calculation to get the correct screen position
                x = screen_center_x + v_transformed[0] * (screen_center_x / tmp_fov)
                        / 1.0;
                y = screen_center_y - v_transformed[1] * (screen_center_x / tmp_fov)
                        / 1.0;
                // Some issue with this calculation causes it to point up or down instead of the opposite way at certain vertical angles
            }

            // Cut off the screen position if it's outside of the screen
            if (x > SOT_WINDOW_W) x = SOT_WINDOW_W;
            if (x < 0) x = 0;
            if (y > SOT_WINDOW_H) y = SOT_WINDOW_H;
            if (y < 0) y = 0;

            return new Vector2((float)x, (float)y);
        }

        public static Vector2? ObjectToScreen(Coordinates player, Vector3 actor)
        {
            return ObjectToScreen(player, new Coordinates() { x = actor.X, y = actor.Y, z = actor.Z });
        }

        public static Vector2? ObjectToScreen(Coordinates player, Coordinates actor)
        {
            Tuple<double, double, double> player_camera = Tuple.Create((double)player.rot_x, (double)player.rot_y, (double)player.rot_z);
            List<List<double>> temp = MakeVMatrix(player_camera);

            Tuple<double, double, double> v_axis_x = Tuple.Create(temp[0][0], temp[0][1], temp[0][2]);
            Tuple<double, double, double> v_axis_y = Tuple.Create(temp[1][0], temp[1][1], temp[1][2]);
            Tuple<double, double, double> v_axis_z = Tuple.Create(temp[2][0], temp[2][1], temp[2][2]);

            Tuple<double, double, double> v_delta = Tuple.Create((double)actor.x - (double)player.x,
                                                                    (double)actor.y - (double)player.y,
                                                                    (double)actor.z - (double)player.z);
            List<double> v_transformed = new List<double>
            {
                Dot(v_delta, v_axis_y),
                Dot(v_delta, v_axis_z),
                Dot(v_delta, v_axis_x)
            };

            if (v_transformed[2] < 1.0) // No valid screen coordinates if its behind us
            {
                return null;
            }

            /* https://www.unknowncheats.me/forum/2348124-post130.html : A solution to strange aspect ratios like ultra wide monitors
             auto Ratio = SizeX / SizeY;
if (Ratio < 4.0f / 3.0f)
	Ratio = 4.0f / 3.0f;
 
auto FOV = Ratio / (16.0f / 9.0f) * tan(Camera.FOV * PI / 360.0f);
 
FVector Location;
Location.X = SizeX + Transform.X * SizeX / FOV / Transform.Z;
Location.Y = SizeY - Transform.Y * SizeX / FOV / Transform.Z;
             */

            double fov = player.fov; // horizontal FoV
            double screen_center_x = (double)SOT_WINDOW_W / 2;
            double screen_center_y = (double)SOT_WINDOW_H / 2;
            double aspect_ratio = (double)SOT_WINDOW_W / (double)SOT_WINDOW_H;

            // only use this if the aspect ratio is less than 4:3
            if (aspect_ratio < 4.0f / 3.0f)
                aspect_ratio = 4.0f / 3.0f;

            var tmp_fov = aspect_ratio / (16.0f / 9.0f) * Math.Tan(fov * Math.PI / 360.0f);

            double x = screen_center_x + (v_transformed[0] * (screen_center_x / tmp_fov) / v_transformed[2]);
            double y = screen_center_y - (v_transformed[1] * (screen_center_x / tmp_fov) / v_transformed[2]);

            if (x > SOT_WINDOW_W || x < 0 || y > SOT_WINDOW_H || y < 0)
            {
                return null;
            }

            return new Vector2((float)x, (float)y);
        }


        public static Coordinates CoordBuilder(ulong rootCompPtr, int offset)
        {
            /*
            Given an actor, loads the coordinates for that actor
            :param int root_comp_ptr: Actors root component memory address
            :param int offset: Offset from root component to beginning of coords,
            Often determined manually with Cheat Engine
            :rtype: dict
            :return: A dictionary containing the coordinate information
            for a specific actor
            */
            byte[] actorBytes = mem.ReadBytes((UIntPtr)rootCompPtr + (uint)offset, 24);
            float[] unpacked = new float[6];

            for (int i = 0; i < unpacked.Length; i++)
            {
                unpacked[i] = BitConverter.ToSingle(actorBytes, i * 4);
            }

            return new Coordinates()
            {
                x = unpacked[0] / 100,
                y = unpacked[1] / 100,
                z = unpacked[2] / 100,
            };
        }

        public static List<List<double>> FMatrix(Quaternion quaternion, Vector3 origin)
        {
            float x = quaternion.X;
            float y = quaternion.Y;
            float z = quaternion.Z;
            float w = quaternion.W;

            float x2 = x + x;
            float y2 = y + y;
            float z2 = z + z;
            float xx = x * x2;
            float xy = x * y2;
            float xz = x * z2;
            float yy = y * y2;
            float yz = y * z2;
            float zz = z * z2;
            float wx = w * x2;
            float wy = w * y2;
            float wz = w * z2;
            List<List<double>> matrix = new List<List<double>>();
            for (int i = 0; i < 4; i++)
            {
                matrix.Add(new List<double>());
                for (int j = 0; j < 4; j++)
                {
                    matrix[i].Add(0.0);
                }
            }

            matrix[0][0] = 1f - (yy + zz);
            matrix[0][1] = xy - wz;
            matrix[0][2] = xz + wy;
            matrix[0][3] = 0f;
            matrix[1][0] = xy + wz;
            matrix[1][1] = 1f - (xx + zz);
            matrix[1][2] = yz - wx;
            matrix[1][3] = 0f;
            matrix[2][0] = xz - wy;
            matrix[2][1] = yz + wx;
            matrix[2][2] = 1f - (xx + yy);
            matrix[2][3] = 0f;
            matrix[3][0] = origin.X;
            matrix[3][1] = origin.Y;
            matrix[3][2] = origin.Z;
            matrix[3][3] = 1f;
            return matrix;
        }

        public static List<List<double>> MakeVMatrix(Tuple<double, double, double> rot)
        {
            double rad_pitch = (rot.Item1 * Math.PI / 180);
            double rad_yaw = (rot.Item2 * Math.PI / 180);
            double rad_roll = (rot.Item3 * Math.PI / 180);

            double sin_pitch = Math.Sin(rad_pitch);
            double cos_pitch = Math.Cos(rad_pitch);
            double sin_yaw = Math.Sin(rad_yaw);
            double cos_yaw = Math.Cos(rad_yaw);
            double sin_roll = Math.Sin(rad_roll);
            double cos_roll = Math.Cos(rad_roll);

            List<List<double>> matrix = new List<List<double>>();
            for (int i = 0; i < 3; i++)
            {
                matrix.Add(new List<double>());
                for (int j = 0; j < 3; j++)
                {
                    matrix[i].Add(0.0);
                }
            }

            matrix[0][0] = cos_pitch * cos_yaw;
            matrix[0][1] = cos_pitch * sin_yaw;
            matrix[0][2] = sin_pitch;

            matrix[1][0] = sin_roll * sin_pitch * cos_yaw - cos_roll * sin_yaw;
            matrix[1][1] = sin_roll * sin_pitch * sin_yaw + cos_roll * cos_yaw;
            matrix[1][2] = -sin_roll * cos_pitch;

            matrix[2][0] = -(cos_roll * sin_pitch * cos_yaw + sin_roll * sin_yaw);
            matrix[2][1] = cos_yaw * sin_roll - cos_roll * sin_pitch * sin_yaw;
            matrix[2][2] = cos_roll * cos_pitch;

            return matrix;
        }

        public static int CalculateDistance(Vector3 obj_to, Vector3 obj_from)
        {
            double dx = obj_to.X - obj_from.X;
            double dy = obj_to.Y - obj_from.Y;
            double dz = obj_to.Z - obj_from.Z;
            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            return (int)Math.Round(distance);
        }

        public static int CalculateDistance(Vector3 obj_to, Coordinates obj_from)
        {
            double dx = obj_to.X - obj_from.x;
            double dy = obj_to.Y - obj_from.y;
            double dz = obj_to.Z - obj_from.z;
            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            return (int)Math.Round(distance);
        }

        public static int CalculateDistance(Coordinates obj_to, Coordinates obj_from)
        {
            double dx = obj_to.x - obj_from.x;
            double dy = obj_to.y - obj_from.y;
            double dz = obj_to.z - obj_from.z;
            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            return (int)Math.Round(distance);
        }
    }
}
