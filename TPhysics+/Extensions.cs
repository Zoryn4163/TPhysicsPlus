using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace TPhysicsPlus
{
    public static class Extensions
    {
        public static int Round(this float f)
        {
            return (int) Math.Round(f);
        }

        public static float Distance(this Vector2 v1, Vector2 v2)
        {
            return Vector2.Distance(v1, v2);
        }

        public static Rectangle WorldToScreen(this Rectangle rect)
        {
            return new Rectangle(rect.X - Main.screenPosition.X.Round(), rect.Y - Main.screenPosition.Y.Round(), rect.Width, rect.Height);
        }

        public static Color Random(this Color c)
        {
            Random rand = new Random();
            return new Color(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
        }
    }
}
