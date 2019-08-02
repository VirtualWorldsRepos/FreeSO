﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FSO.LotView.Utils.Camera
{
    public class CameraControllerFP : CameraController3D
    {
        public Vector3 FPCamVelocity;
        public float FPCamHeight;
        public bool LastFP;
        public float SavedYRot;

        public override void InvalidateCamera(WorldState state)
        {
            var baseHeight = CamHeight;
            Camera.Position = new Vector3(state.CenterTile.X * WorldSpace.WorldUnitsPerTile, baseHeight + 3 + FPCamHeight, state.CenterTile.Y * WorldSpace.WorldUnitsPerTile);

            var mat = Matrix.CreateRotationZ((_RotationY - (float)Math.PI / 2) * 0.99f) * Matrix.CreateRotationY(_RotationX);
            Camera.Target = Camera.Position + Vector3.Transform(new Vector3(-10, 0, 0), mat);
        }

        public override void Update(UpdateState state, World world)
        {
            var worldState = world.State;
            var terrainHeight = CorrectCameraHeight(world);
            if (state.WindowFocused)
            {
                var mx = (int)worldState.WorldSpace.WorldPxWidth / (2 * PPXDepthEngine.SSAA);
                var my = (int)worldState.WorldSpace.WorldPxHeight / (2 * PPXDepthEngine.SSAA);

                var mpos = state.MouseState.Position;
                var camera = Camera;
                if (LastFP)
                {
                    rcState.RotationX -= ((mpos.X - mx) / 500f) * camera.FOV;
                    rcState.RotationY += ((mpos.Y - my) / 500f) * camera.FOV;
                }
                Mouse.SetPosition(mx, my);

                var speed = (state.KeyboardState.IsKeyDown(Keys.LeftShift)) ? 1.5f : 0.5f;

                if (camera.FOV < Math.PI * 0.6f)
                    if (state.KeyboardState.IsKeyDown(Keys.Z))
                        camera.FOV += 1f / hz;

                if (camera.FOV > Math.PI * 0.025f)
                    if (state.KeyboardState.IsKeyDown(Keys.X))
                        camera.FOV -= 1f / hz;

                if (state.KeyboardState.IsKeyDown(Keys.W))
                    FPCamVelocity.Z -= speed;
                if (state.KeyboardState.IsKeyDown(Keys.S))
                    FPCamVelocity.Z += speed;
                if (state.KeyboardState.IsKeyDown(Keys.A))
                    FPCamVelocity.X -= speed;
                if (state.KeyboardState.IsKeyDown(Keys.D))
                    FPCamVelocity.X += speed;
                if (state.KeyboardState.IsKeyDown(Keys.Q))
                    FPCamVelocity.Y -= speed;
                if (state.KeyboardState.IsKeyDown(Keys.E))
                    FPCamVelocity.Y += speed;
                LastFP = true;
            }
            else
            {
                LastFP = false;
            }

            world.Scroll(new Vector2(FPCamVelocity.X / FSOEnvironment.RefreshRate, FPCamVelocity.Z / FSOEnvironment.RefreshRate));
            FPCamHeight = Math.Max((terrainHeight - CamHeight) - 2, FPCamHeight + (FPCamVelocity.Y * 3) / FSOEnvironment.RefreshRate);
            for (int i = 0; i < FSOEnvironment.RefreshRate / 60; i++)
                FPCamVelocity *= 0.9f;
        }

        public override void SetActive(ICameraController previous, World world)
        {
            if (previous is CameraController3D)
            {
                var c3d = previous as CameraController3D;
                var relative = ComputeCenterRelative();
                SavedYRot = c3d.RotationY;
                var relNorm = relative;
                relNorm.Normalize();
                var rotY = (float)Math.Acos(Vector3.Dot(new Vector3(0, 1, 0), -relNorm));
                //var rotY = (float)((1 - Math.Cos(_RotationY)) * Math.PI * 0.245f);
                c3d.RotationY = rotY;// - (float)Math.PI/2;
                world.State.CenterTile += new Vector2(relative.X / WorldSpace.WorldUnitsPerTile, relative.Z / WorldSpace.WorldUnitsPerTile);
                FPCamHeight = relative.Y;
            }
            else if (previous is CameraController2D)
            {
                //guess from 2d... that might be hard actually
            }

            LastFP = false;
        }
    }
}
