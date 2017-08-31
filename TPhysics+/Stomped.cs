using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace TPhysicsPlus
{
    public class Stomped
    {
        public NPC StompedNpc { get; set; }
        public int FramesSinceStomped { get; set; }

        public Stomped(NPC npc)
        {
            StompedNpc = npc;
            FramesSinceStomped = 0;
        }
    }
}
