using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XerUtilities.Rendering
{
    public class PrimitiveBatch : IDisposable
    {
        private const int DefaultBufferSize = 2048;

        // a basic effect, which contains the shaders that we will use to draw our
        // primitives.
        private Effect _effect;

        // the device that we will issue draw calls to.
        private GraphicsDevice _device;

        // hasBegun is flipped to true once Begin is called, and is used to make
        // sure users don't call End before Begin is called.
        private bool _hasBegun;

        private bool _isDisposed;
        private VertexPositionColor[] _lineVertices;
        private int _lineVertsCount;
        private VertexPositionColor[] _triangleVertices;
        private int _triangleVertsCount;
        private EffectParameter _transform;
        


        /// <summary>
        /// the constructor creates a new PrimitiveBatch and sets up all of the internals
        /// that PrimitiveBatch will need.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        public PrimitiveBatch(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, DefaultBufferSize)
        {
        }

        public PrimitiveBatch(GraphicsDevice graphicsDevice, int bufferSize)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            _device = graphicsDevice;

            _triangleVertices = new VertexPositionColor[bufferSize - bufferSize % 3];
            _lineVertices = new VertexPositionColor[bufferSize - bufferSize % 2];

            // set up a new basic effect, and enable vertex colors.
            _effect = new BasicEffect(graphicsDevice);
            ((BasicEffect)_effect).VertexColorEnabled = true;
            ((BasicEffect)_effect).View = Matrix.Identity;
            _transform = _effect.Parameters["WorldViewProj"];
            

            
        }

        public PrimitiveBatch(GraphicsDevice graphicsDevice, int bufferSize, Effect effect)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            _device = graphicsDevice;
            
            _triangleVertices = new VertexPositionColor[bufferSize - bufferSize % 3];
            _lineVertices = new VertexPositionColor[bufferSize - bufferSize % 2];

            _effect = effect;
            _transform = effect.Parameters["Transform"];
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                if (_effect != null)
                    _effect.Dispose();

                _isDisposed = true;
            }
        }


        /// <summary>
        /// Begin is called to tell the PrimitiveBatch what kind of primitives will be
        /// drawn, and to prepare the graphics card to render those primitives.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="view">The view.</param>
        public void Begin(ref Matrix projection, ref Matrix view)
        {
            if (_hasBegun)
            {
                throw new InvalidOperationException("End must be called before Begin can be called again.");
            }

            //tell our basic effect to begin.
            _transform.SetValue(view*projection);            
            _effect.CurrentTechnique.Passes[0].Apply();

            // flip the error checking boolean. It's now ok to call AddVertex, Flush,
            // and End.
            _hasBegun = true;
        }

        public bool IsReady()
        {
            return _hasBegun;
        }

        public void AddVertex(Vector2 vertex, Color color, PrimitiveType primitiveType)
        {
            if (!_hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before AddVertex can be called.");
            }
            if (primitiveType == PrimitiveType.LineStrip ||
                primitiveType == PrimitiveType.TriangleStrip)
            {
                throw new NotSupportedException("The specified primitiveType is not supported by PrimitiveBatch.");
            }

            if (primitiveType == PrimitiveType.TriangleList)
            {
                if (_triangleVertsCount >= _triangleVertices.Length)
                {
                    FlushTriangles();
                }
                _triangleVertices[_triangleVertsCount].Position.X = vertex.X;
                _triangleVertices[_triangleVertsCount].Position.Y = vertex.Y;
                _triangleVertices[_triangleVertsCount].Color = color;
                _triangleVertsCount++;
            }
            if (primitiveType == PrimitiveType.LineList)
            {
                if (_lineVertsCount >= _lineVertices.Length)
                {
                    FlushLines();
                }
                _lineVertices[_lineVertsCount].Position.X = vertex.X;
                _lineVertices[_lineVertsCount].Position.Y = vertex.Y;
                _lineVertices[_lineVertsCount].Color = color;
                _lineVertsCount++;
            }
        }

        public void AddLine(Vector2 start, Vector2 end, Color color)
        {
            if (_lineVertsCount >= _lineVertices.Length - 2) FlushLines();

            _lineVertices[_lineVertsCount].Position.X = start.X;
            _lineVertices[_lineVertsCount].Position.Y = start.Y;
            _lineVertices[_lineVertsCount++].Color = color;

            _lineVertices[_lineVertsCount].Position.X = end.X;
            _lineVertices[_lineVertsCount].Position.Y = end.Y;
            _lineVertices[_lineVertsCount++].Color = color;
        }

        public void AddSquare(Vector2 min, Vector2 max, Color color)
        {
            if (_lineVertsCount >= _lineVertices.Length - 8) FlushLines();

            _lineVertices[_lineVertsCount].Position.X = min.X;
            _lineVertices[_lineVertsCount].Position.Y = min.Y;
            _lineVertices[_lineVertsCount++].Color = color;

            _lineVertices[_lineVertsCount].Position.X = min.X;
            _lineVertices[_lineVertsCount].Position.Y = max.Y;
            _lineVertices[_lineVertsCount++].Color = color;

            _lineVertices[_lineVertsCount].Position.X = min.X;
            _lineVertices[_lineVertsCount].Position.Y = min.Y;
            _lineVertices[_lineVertsCount++].Color = color;

            _lineVertices[_lineVertsCount].Position.X = max.X;
            _lineVertices[_lineVertsCount].Position.Y = min.Y;
            _lineVertices[_lineVertsCount++].Color = color;

            _lineVertices[_lineVertsCount].Position.X = max.X;
            _lineVertices[_lineVertsCount].Position.Y = max.Y;
            _lineVertices[_lineVertsCount++].Color = color;

            _lineVertices[_lineVertsCount].Position.X = max.X;
            _lineVertices[_lineVertsCount].Position.Y = min.Y;
            _lineVertices[_lineVertsCount++].Color = color;

            _lineVertices[_lineVertsCount].Position.X = max.X;
            _lineVertices[_lineVertsCount].Position.Y = max.Y;
            _lineVertices[_lineVertsCount++].Color = color;

            _lineVertices[_lineVertsCount].Position.X = min.X;
            _lineVertices[_lineVertsCount].Position.Y = max.Y;
            _lineVertices[_lineVertsCount++].Color = color;

        }

        /// <summary>
        /// End is called once all the primitives have been drawn using AddVertex.
        /// it will call Flush to actually submit the draw call to the graphics card, and
        /// then tell the basic effect to end.
        /// </summary>
        public void End()
        {
            if (!_hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before End can be called.");
            }

            // Draw whatever the user wanted us to draw
            FlushTriangles();
            FlushLines();

            _hasBegun = false;
        }

        private void FlushTriangles()
        {
            if (!_hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before Flush can be called.");
            }
            if (_triangleVertsCount >= 3)
            {
                int primitiveCount = _triangleVertsCount / 3;
                // submit the draw call to the graphics card
                _device.SamplerStates[0] = SamplerState.AnisotropicClamp;
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, _triangleVertices, 0, primitiveCount);
                _triangleVertsCount -= primitiveCount * 3;
            }
        }

        private void FlushLines()
        {
            if (!_hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before Flush can be called.");
            }
            if (_lineVertsCount >= 2)
            {
                int primitiveCount = _lineVertsCount / 2;
                // submit the draw call to the graphics card
                _device.SamplerStates[0] = SamplerState.AnisotropicClamp;
                _device.DrawUserPrimitives(PrimitiveType.LineList, _lineVertices, 0, primitiveCount);
                _lineVertsCount -= primitiveCount * 2;
            }
        }
    }
}