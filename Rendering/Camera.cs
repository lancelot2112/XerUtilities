using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XerUtilities.Input;

namespace XerUtilities.Rendering
{
    public class Camera
    {
        public Matrix View;
        public Matrix Projection;
        public BoundingFrustum ViewFrustum;

        protected XerInput _input;

        public Camera(XerInput input, GraphicsDevice device, float fieldOfView, float nearClip, float farClip)
        {
            this._input = input;
            this.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), device.Viewport.AspectRatio, nearClip, farClip);
        }

        public virtual void Update(float dt)
        {
        }
    }
}
