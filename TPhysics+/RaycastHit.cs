using Microsoft.Xna.Framework;
using Terraria;

namespace TPhysicsPlus
{
    public class RaycastHit
    {
        public Vector2 Origin;
        public NPC Npc;
        public Tile Tile;
        public Player Player;

        public RaycastHit()
        {
            Origin = Vector2.Zero;
            Npc = null;
            Tile = null;
            Player = null;
        }
    }
}
