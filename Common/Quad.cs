using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace XerUtilities.Common
{
    public class Quad
    {
        public GraphicsDevice Device;
        VertexBuffer vertexBuffer;

        public VertexBuffer VertexBuffer 
        { 
            get 
            {
                if (vertexBuffer == null)
                    CreateQuad();

                return vertexBuffer; 
            }
        }

        public Quad(GraphicsDevice device)
        {
            this.Device = device;
        }

        
        private void CreateQuad()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0].Position = new Vector3(0.5f, 0, 0.5f);
            vertices[1].Position = new Vector3(0.5f, 0, -0.5f);
            vertices[2].Position = new Vector3(-0.5f, 0, 0.5f);
            vertices[3].Position = new Vector3(-0.5f, 0, -0.5f);
            
            vertices[0].TextureCoordinate = new Vector2(1, 0);
            vertices[1].TextureCoordinate = new Vector2(1, 1);
            vertices[2].TextureCoordinate = new Vector2(0, 0);
            vertices[3].TextureCoordinate = new Vector2(0, 1);

            //Create the vertex buffer from the vertices
            vertexBuffer = new VertexBuffer(Device, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
        }

        public void SetBuffer()
        {
            if (vertexBuffer == null)
                CreateQuad();

            Device.SetVertexBuffer(vertexBuffer);
        }
    }
}
