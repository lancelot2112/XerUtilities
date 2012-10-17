using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XerUtilities.Input;

namespace XerUtilities.Rendering
{
    public class DeltaFreeCamera : Camera
    {
        private Quaternion _rotation;
        public Matrix _rotationMat;
        public Vector3 Translation;

        public float TranslationSpeed;
        public float RotationSpeed;

        public DeltaFreeCamera(XerInput input, GraphicsDevice device, Vector3 initialOffset = default(Vector3), float fieldOfView = MathHelper.PiOver4, float nearClip = .1f, float farClip = 10000f, float translateSpeed = 25f, float rotationSpeed = .1f)
            : base(input, device, fieldOfView, nearClip, farClip)
        {
            TranslationSpeed = translateSpeed;
            RotationSpeed = rotationSpeed;

            _rotation = Quaternion.Identity;
            _rotationMat = Matrix.Identity;
            Translation = initialOffset;

            CalculateMatrices();
        }
        public override void Update(float dt)
        {
            HandleUserInput(dt);
            CalculateMatrices();
            CalculateFrustum();
        }

        private void CalculateMatrices()
        {
            _rotationMat = Matrix.CreateFromQuaternion(_rotation);

            Translation = Vector3.Transform(Translation, _rotationMat);

            Vector3 forward = Vector3.Transform(Vector3.Forward, _rotationMat);
            Vector3 up = Vector3.Transform(Vector3.Up, _rotationMat);

            View = Matrix.CreateLookAt(Vector3.Zero, forward, up);
        }

        private void CalculateFrustum()
        {
            ViewFrustum = new BoundingFrustum(View * Projection);
        }

        private void HandleUserInput(float dt)
        {
            Translation = Vector3.Zero;

            float deltaX = -_input.Mouse.DeltaX;
            float deltaY = -_input.Mouse.DeltaY;
            float scale = RotationSpeed * dt;

            _rotation = Quaternion.Multiply(Quaternion.CreateFromAxisAngle(Vector3.Up,deltaX * scale),Quaternion.Multiply(_rotation, Quaternion.CreateFromYawPitchRoll(0, deltaY * scale,0)));
            _rotation.Normalize();
            //m_rotationMat = Matrix.Multiply(Matrix.CreateFromYawPitchRoll(deltaX * scale, deltaY * scale, 0.0f),m_rotationMat);

            if (_input.Keyboard.QPressedAndHeld) Translation += Vector3.Left;
            if (_input.Keyboard.WPressedAndHeld) Translation += Vector3.Forward;
            if (_input.Keyboard.EPressedAndHeld) Translation += Vector3.Right;
            if (_input.Keyboard.SPressedAndHeld) Translation += Vector3.Backward;
            if (_input.Keyboard.SpacePressedAndHeld) Translation += _input.Keyboard.ShiftPressed ? Vector3.Down : Vector3.Up;
            Translation = Translation * TranslationSpeed * dt;
        }
    }
}
