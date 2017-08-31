using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPhysicsPlus
{
    public static class Extensions
    {
        public static int Round(this float f)
        {
            return (int) Math.Round(f);
        }
    }
}
