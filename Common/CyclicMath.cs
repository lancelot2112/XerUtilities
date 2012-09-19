using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XerUtilities.Common
{
    public static class CyclicMath
    {
        /// <summary>
        /// Helper computes a value that oscillates over time.
        /// </summary>
        public static float Pulsate(GameTime gameTime, float speed, float min, float max)
        {
            double time = gameTime.TotalGameTime.TotalSeconds * speed;

            return min + ((float)Math.Sin(time) + 1) / 2 * (max - min);
        }

        /// <summary>
        /// Helper for moving a value around in a circle.
        /// </summary>
        public static Vector2 MoveInCircle(GameTime gameTime, float speed)
        {
            double time = gameTime.TotalGameTime.TotalSeconds * speed;

            float x = (float)Math.Cos(time);
            float y = (float)Math.Sin(time);

            return new Vector2(x, y);
        }
    }
}
