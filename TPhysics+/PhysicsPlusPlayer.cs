using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace TPhysicsPlus
{
    class PhysicsPlusPlayer : ModPlayer
    {
        public bool Grounded => Math.Abs(player.velocity.Y) <= 0.001f;
        public Vector2 PrevVelocity { get; private set; }

        public override void PreUpdate()
        {
            //Jump higher when on ground
            if (Grounded)
            {
                Player.jumpHeight = 0;
                Player.jumpSpeed *= player.slowFall ? 1.1f : 1.52375f;
            }

            bool wingJump = player.wingsLogic > 0 && player.controlJump;
            bool wingGlide = wingJump && player.wingTime == 0.0f;

            //Run acceleration on ground
            player.runAcceleration = Grounded ? 0.15f : (wingGlide ? 0.25f : (wingJump ? 0.2f : 0.125f));
            player.runSlowdown = Grounded ? 0.3f : 0.01f;
            player.runSoundDelay = 5;
            player.maxRunSpeed *= 1.1f;

            //Fall speed cap
            player.maxFallSpeed = wingGlide ? 10f : 1000f;
            if (player.velocity.Y > player.maxFallSpeed)
                player.velocity.Y = player.maxFallSpeed;

            //Gain Horizontal Speed on Standing Jump
            if (player.controlJump && PrevVelocity.Y == 0.0 && player.velocity.Y < 0.0)
            {
                player.velocity.X += player.controlLeft ? -2 : (player.controlRight ? 2 : 0.0f);
            }

            //Stomping
            float stompThreshold = 8f;
            if (player.velocity.Y >= (double) stompThreshold && !player.GoingDownWithGrapple)
            {
                //Hit all NPCs once
                List<NPC> ignoredNpcs = new List<NPC>();
                for (int index = -2; index < 3; ++index)
                {
                    RaycastHit hit;
                    while (Raycast.PointDirection(out hit, player.Center + new Vector2(index * 10, 0), Vector2.UnitY, player.height + player.velocity.Y, RaycastFlags.HitNpcHostile | RaycastFlags.HitNpcNeutral) != null)
                    {
                        ignoredNpcs.Add(hit.Npc);
                        if (hit.Npc.Top.Y + 5.0 > player.Bottom.Y)
                        {
                            //Do a shitload of damage
                            int damage = (int) ((player.velocity.Y - (stompThreshold - 1.0)) * 4.0);

                            //Double it with ninja accessories
                            if (HasAccessory(975) || HasAccessory(976) || HasAccessory(984))
                                damage *= 2;

                            int direction = player.velocity.X <= 0.001f ? 0 : (player.velocity.X >= 0.001f ? 1 : -1);
                            player.ApplyDamageToNPC(hit.Npc, damage, 4f, direction, false);

                            //Confuse and slow stomped for 2 update cycles
                            hit.Npc.AddBuff(31, 120, false);
                            hit.Npc.AddBuff(32, 120, false);

                            player.fallStart = (int) (player.position.Y / 16.0);
                            player.velocity.Y -= stompThreshold;
                        }
                    }
                }
            }

            PrevVelocity = player.velocity;

            base.PreUpdate();
        }

        public bool HasAccessory(int type)
        {
            for (int i = 3; i < 8 + player.extraAccessorySlots; ++i)
            {
                Item item = player.armor[i];
                if (item != null && item.active && item.type == type)
                    return true;
            }
            return false;
        }
    }
}
