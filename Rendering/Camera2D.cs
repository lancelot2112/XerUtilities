using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XerUtilities.Rendering
{
    public class Camera2D
    {
        private Vector2 offset;
        public Vector2 Position;
        public float Rotation;
        public float Zoom;
        public float ZoomRate = 0.25f;
        public Matrix Projection;
        public Matrix View;
        public Matrix Transform;
        public Matrix Scroll;

        private float minZoom = 1.0f;
        private float maxZoom = 5000.0f;

        public Camera2D(GraphicsDevice graphics)
            : this(graphics, Vector2.Zero, 0.0f, 100.0f) { }

        public Camera2D(GraphicsDevice graphics, Vector2 position, float rotation, float zoom)
        {
            float xOffset = graphics.Viewport.Width*0.5f;
            float yOffset = graphics.Viewport.Height*0.5f;
            offset = new Vector2(xOffset, yOffset);            
            Rotation = rotation;
            Zoom = zoom;

            CalculateProjection(graphics.Viewport.AspectRatio);
            CalculateView();

            Update(position, 0f);
        }

        public void Update(Vector2 newPosition, float deltaZoom)
        {
            Position = newPosition;
            Zoom -= deltaZoom*ZoomRate;
            Zoom = MathHelper.Clamp(Zoom, minZoom, maxZoom);
            //CalculateScroll();
            CalculateView();
        }

        public void CalculateScroll()
        {
            Scroll = Matrix.CreateTranslation(-Position.X,-Position.Y, 0.0f)* Matrix.CreateScale(1/Zoom) * Matrix.CreateRotationZ(Rotation)*Matrix.CreateTranslation(new Vector3(offset,0));
        }

        private void CalculateProjection(float aspect)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 1, 10000);
        }
        private void CalculateView()
        {
            View = Matrix.CreateLookAt(new Vector3(Position.X,Position.Y,Zoom),new Vector3(Position.X,Position.Y,0.0f),Vector3.UnitY);
        }

    }
}
