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
        private Quaternion m_rotation;
        private Vector3 m_translation;
        public Matrix InverseTranslation;

        public float TranslationSpeed;
        public float RotationSpeed;

        public DeltaFreeCamera(XerInput input, GraphicsDevice device, Vector3 initialOffset = default(Vector3), float fieldOfView = MathHelper.PiOver4, float nearClip = .01f, float farClip = 10000f, float translateSpeed = 2f, float rotationSpeed = 0.1f)
            : base(input, device, fieldOfView, nearClip, farClip)
        {
            TranslationSpeed = translateSpeed;
            RotationSpeed = rotationSpeed;

            m_rotation = Quaternion.Identity;
            m_translation = initialOffset;
            InverseTranslation = Matrix.Identity;

            CalculateMatrices();
        }
        public override void Update(float dt)
        {
            HandleUserInput(dt);
            CalculateMatrices();
        }

        private void CalculateMatrices()
        {
            Matrix rotation = Matrix.CreateFromQuaternion(m_rotation);

            m_translation = Vector3.Transform(m_translation, rotation);
            Matrix translation;
            Matrix.CreateTranslation(ref m_translation, out translation);
            Matrix.Invert(ref translation, out InverseTranslation);

            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);

            View = Matrix.CreateLookAt(Vector3.Zero, forward, up);
        }

        private void HandleUserInput(float dt)
        {
            m_translation = Vector3.Zero;

            float deltaX = -m_input.Mouse.DeltaX;
            float deltaY = -m_input.Mouse.DeltaY;
            float roll = 0;
            if (m_input.Keyboard.APressedAndHeld) roll += 5;
            if (m_input.Keyboard.DPressedAndHeld) roll -= 5;
            float scale = RotationSpeed * dt;

            m_rotation = Quaternion.Multiply(m_rotation, Quaternion.CreateFromYawPitchRoll(deltaX * scale, deltaY * scale, roll * scale));

            if (m_input.Keyboard.QPressedAndHeld) m_translation += Vector3.Left;
            if (m_input.Keyboard.WPressedAndHeld) m_translation += Vector3.Forward;
            if (m_input.Keyboard.EPressedAndHeld) m_translation += Vector3.Right;
            if (m_input.Keyboard.SPressedAndHeld) m_translation += Vector3.Backward;
            if (m_input.Keyboard.SpacePressedAndHeld) m_translation += m_input.Keyboard.ShiftPressed ? Vector3.Down : Vector3.Up;
            m_translation = m_translation * TranslationSpeed * dt;
        }
    }
}
