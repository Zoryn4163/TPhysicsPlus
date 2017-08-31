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

        public override string ToString()
        {
            if (Npc != null)
                return Npc.FullName;

            if (Tile != null)
                return Tile.blockType().ToString();

            return "null";
        }
    }
}
