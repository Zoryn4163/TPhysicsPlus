﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace TPhysicsPlus
{
    public static class Raycast
    {
        public static RaycastHit PointToPoint()
        {
            throw new NotImplementedException();
            return null;
        }

        public static bool? PointDirection(out RaycastHit hit, Vector2 origin, Vector2 direction, float dist, RaycastFlags flags, List<object> ignored = null)
        {
            bool? ret = new bool?();
            if (ignored == null)
                ignored = new List<object>();

            hit = new RaycastHit();

            direction.Normalize();
            Vector2 end = origin + (direction * dist);
            Vector2 min = new Vector2(Math.Min(origin.X, end.X), Math.Min(origin.Y, end.Y));
            Vector2 max = new Vector2(Math.Max(origin.X, end.X), Math.Max(origin.Y, end.Y));

            Rectangle rayRect = new Rectangle(min.X.Round(), min.Y.Round(), (max.X - min.X).Round(), (max.Y - min.Y).Round());

            if (flags == RaycastFlags.HitPlayer)
            {
                Player pc = Main.clientPlayer;
                var playerHits = Main.player.Where(x => x.active && !x.dead && (flags == RaycastFlags.HitPlayerIgnoreFriendly || x.team != pc.team)  && x.getRect().Intersects(rayRect)).ToArray();
                hit.Player = playerHits.FirstOrDefault(x=>!ignored.Contains(x));
                ret = hit.Player != null;
            }

            if (flags == RaycastFlags.HitTile)
            {
                //TODO
                throw new NotImplementedException();
            }

            if (flags == RaycastFlags.HitNpcHostile || flags == RaycastFlags.HitNpcNeutral || flags == RaycastFlags.HitNpcFriendly)
            {
                var npcHits = Main.npc.Where(x => x.active && x.life > 0 && !x.dontTakeDamage && x.getRect().Intersects(rayRect));
                hit.Npc = npcHits.FirstOrDefault(x => !ignored.Contains(x) && ((flags == RaycastFlags.HitNpcFriendly && x.townNPC) || (flags == RaycastFlags.HitNpcHostile && !x.friendly) || (flags == RaycastFlags.HitNpcNeutral && x.friendly && !x.townNPC)));
                ret = hit.Npc != null;
            }

            return ret;
        }
    }

    [Flags]
    public enum RaycastFlags
    {
        HitNpcHostile = 1,
        HitNpcNeutral = 2, //May not work properly
        HitNpcFriendly = 4,
        HitTile = 8,
        HitPlayer = 16,
        HitPlayerIgnoreFriendly = 32
    }
}
