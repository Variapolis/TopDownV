using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Rage;
using Rage.Attributes;
using Rage.Native;
using Debug = Rage.Debug;

[assembly: Plugin("TopDown", Description = "A plugin that makes GTA V into GTA 2",
    Author = "Variapolis",
    PrefersSingleInstance = true)]

namespace TestRPHPlugin
{
    public static class Entry
    {
        private static Vector3 GetPointPedIsAimingAt(this Ped ped)
        {
            if (!ped.IsAiming) return Vector3.Zero;
            var weapon = ped.Inventory.EquippedWeaponObject;

            var hitResult = World.TraceLine(weapon.Position, weapon.Position + 10000 * weapon.RightVector,
                TraceFlags.IntersectEverything, weapon, ped);
            return hitResult.Hit ? hitResult.HitPosition : Vector3.Zero;
        }

        public static void Main()
        {
            Camera.DeleteAllCameras();
            Camera cam = new Camera(true);
            NativeFunction.Natives.RENDER_SCRIPT_CAMS(true, false);
            Game.DisplayNotification("TopDown Camera loaded, press K to deactive, and L to activate at any time!");
            while (true)
            {
                if (Game.IsKeyDown(Keys.L))
                {
                    cam.Active = true;
                }

                if (Game.IsKeyDown(Keys.K))
                {
                    // Game.LocalPlayer.Character.Tasks.Clear();
                    cam.Active = false;
                }

                GameFiber.Yield();
                if (cam.Active == false)
                {
                    continue;
                }

                cam.Position = new Vector3(Game.LocalPlayer.Character.Position.X, Game.LocalPlayer.Character.Position.Y,
                    Game.LocalPlayer.Character.Position.Z + 80f);
                cam.FOV = 20f;
                cam.Direction = new Vector3(0, 0, -1);
                // cam.SetRotationRoll(-Game.LocalPlayer.Character.Heading);
                NativeFunction.Natives.SetCamAffectsAiming(cam, false);
                NativeFunction.Natives.SetMicrophonePosition(true, Game.LocalPlayer.Character.Position,
                    Game.LocalPlayer.Character.GetOffsetPositionRight(-1f),
                    Game.LocalPlayer.Character.GetOffsetPositionRight(1f));
                var pedsNear = Game.LocalPlayer.Character.GetNearbyPeds(5);
                float upInput = NativeFunction.Natives.xEC3C9B8D5327B563<float>(0, 32);
                float downInput = -NativeFunction.Natives.xEC3C9B8D5327B563<float>(0, 33);
                float leftInput = -NativeFunction.Natives.xEC3C9B8D5327B563<float>(0, 34);
                float rightInput = NativeFunction.Natives.xEC3C9B8D5327B563<float>(0, 35);
                Vector3 inputDir = new Vector3(leftInput + rightInput, upInput + downInput, 0);

                // Game.DisplaySubtitle(
                //     $"{(upInput + downInput).ToString(CultureInfo.InvariantCulture)}, {(rightInput + leftInput).ToString(CultureInfo.InvariantCulture)}");

                if (Game.GetMouseState().IsRightButtonUp) //&& pedsNear.Length > 0
                {
                    NativeFunction.Natives.SET_PED_RESET_FLAG(Game.LocalPlayer.Character, 69, false);
                }

                if (Game.GetMouseState().IsRightButtonDown) //&& pedsNear.Length > 0
                {
                    Debug.DrawSphere(GetPointPedIsAimingAt(Game.LocalPlayer.Character), 0.25f, Color.White);
                    // NativeFunction.Natives.SET_PED_RESET_FLAG(Game.LocalPlayer.Character, 69, false);
                    // bool flagSet = NativeFunction.Natives.GET_PED_RESET_FLAG<bool>(Game.LocalPlayer.Character, 69);
                    // Game.LogTrivial(flagSet.ToString());
                    // NativeFunction.Natives.SET_PED_DESIRED_HEADING(Game.LocalPlayer.Character,
                    //     MathHelper.ConvertDirectionToHeading(inputDir));

                    // // Making player aim at nearest ped, not finished.
                    // Game.DisplaySubtitle(rnd.Next().ToString());
                    // Vector3 gamcampos = NativeFunction.Natives.GetGameplayCamCoord<Vector3>();
                    // Vector3 gamcamrot = NativeFunction.Natives.GetGameplayCamRot<Vector3>();
                    // var dir = Game.LocalPlayer.Character.Position - pedsNear[0].Position;
                    //
                    // NativeFunction.Natives.SetPedDesiredHeading(Game.LocalPlayer.Character, MathHelper.ConvertDirectionToHeading(dir));
                    //
                    // var rot = new Rotator(dir.X, gamcamrot.Y, gamcamrot.Z);
                    // // NativeFunction.Natives.x48608C3464F58AB4(rot.Roll, rot.Pitch, rot.Yaw);
                }

                if (inputDir != Vector3.Zero)
                {
                    if (!Game.LocalPlayer.Character.IsInAnyVehicle(false))
                    {
                        NativeFunction.Natives.SetEntityHeading(Game.LocalPlayer.Character,
                            MathHelper.ConvertDirectionToHeading(inputDir));
                    }
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}