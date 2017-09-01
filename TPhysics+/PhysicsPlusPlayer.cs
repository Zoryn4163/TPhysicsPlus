using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace TPhysicsPlus
{
    class PhysicsPlusPlayer : ModPlayer
    {
        public bool ForcedDirEnabled { get; private set; }
        public int ForcedDir { get; private set; }

        public bool Grounded => player.velocity.Y == 0.0f;
        public bool ForceJump { get; private set; }
        public bool JumpPrev { get; private set; }
        public bool CanJump { get; private set; }
        public double LastRealJumpPress { get; private set; }
        public Vector2 PrevVelocity { get; private set; }
        public Vector2 PrevVelocityControls { get; private set; }

        public static List<Rectangle> DebugDraws { get; private set; }

        public static List<Stomped> StompedNpcs { get; private set; }

        public override void Initialize()
        {
            DebugDraws = new List<Rectangle>();
            StompedNpcs = new List<Stomped>();

            /*
            if (!Main.dedServ)
                Main.OnPostDraw += Main_OnPostDraw;
            */

            base.Initialize();
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            if (StompedNpcs.Any(x => x.StompedNpc == npc))
                return false;
            return base.CanBeHitByNPC(npc, ref cooldownSlot);
        }

        private void Main_OnPostDraw(GameTime obj)
        {
            if (Main.dedServ)
                return;

            if (DebugDraws.Any())
            {
                Texture2D t2d = Main.blackTileTexture;
                Main.spriteBatch.Begin();
                foreach (var v in DebugDraws)
                {
                    Main.spriteBatch.Draw(t2d, v, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
                }
                Main.spriteBatch.End();
                DebugDraws.Clear();
            }
        }

        public override void SetControls()
        {
            bool flag = player.controlJump;
            if (!CanJump)
            {
                player.controlJump = false;
                CanJump = true;
            }
            else if (ForceJump)
            {
                player.controlJump = true;
                ForceJump = false;
            }
            else if (Main.GlobalTime - (double) LastRealJumpPress < 0.15 && player.controlJump && Grounded && PrevVelocityControls.Y != 0.0)
            {
                player.controlJump = false;
                ForceJump = true;
            }
            PrevVelocityControls = player.velocity;
            if (flag && !JumpPrev)
                LastRealJumpPress = Main.GlobalTime;
            JumpPrev = flag;
        }

        public override void PreUpdate()
        {
            if (Main.keyState.IsKeyDown(Keys.OemCloseBrackets) && !Main.oldKeyState.IsKeyDown(Keys.OemCloseBrackets))
            {
                ForcedDirEnabled = !ForcedDirEnabled;
            }

            if (ForcedDirEnabled)
            {
                if (Main.MouseWorld.X > player.Hitbox.Right)
                {
                    ForcedDir = 1;
                    player.direction = 1;
                }
                else
                {
                    ForcedDir = -1;
                    player.direction = -1;
                }
            }

            foreach (var v in StompedNpcs)
            {
                v.FramesSinceStomped += 1;
            }

            var rem = StompedNpcs.Where(x => x.FramesSinceStomped == 30).ToArray();
            for (int ss = 0; ss < rem.Count(); ss++)
            {
                StompedNpcs.Remove(rem[ss]);
            }

            //Jump higher when on ground
            if (Grounded)
            {
                Player.jumpHeight = 0;
                Player.jumpSpeed *= player.slowFall ? 1.1f : 1.52375f;
            }

            bool wingJump = player.wingsLogic > 0 && player.controlJump;
            bool wingGlide = wingJump && player.wingTime == 0.0f;

            //Run acceleration on ground
            player.runAcceleration = Grounded ? 0.25f : (wingGlide ? 0.35f : (wingJump ? 0.25f : 0.15f));
            player.runSlowdown = Grounded ? 0.25f : 0.01f;
            player.runSoundDelay = 5;
            player.maxRunSpeed *= 1.15f;

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

            if (player.velocity.Y >= stompThreshold && !player.GoingDownWithGrapple)
            {
                //Hit all NPCs once
                List<NPC> ignoredNpcs = new List<NPC>();
                int i = 0;
                var hb = player.Hitbox;
                RaycastHit hit;
                while (Raycast.Rectangle(out hit, new Rectangle(hb.BottomLeft().X.Round(), hb.BottomLeft().Y.Round(), hb.Width, (player.height / 4) + player.velocity.Y.Round()), RaycastFlags.HitNpcHostile | RaycastFlags.HitNpcNeutral, ignoredNpcs) != null)
                {
                    if (i >= 1000)
                    {
                        throw new StackOverflowException();
                    }

                    if (hit != null)
                    {
                        if (hit.Npc == null)
                            break;

                        if (ignoredNpcs.Contains(hit.Npc))
                            continue;

                        ignoredNpcs.Add(hit.Npc);
                        if (hit.Npc.Top.Y + 5.0 > player.Bottom.Y)
                        {
                            //Do a shitload of damage
                            int damage = (int) (((player.velocity.Y * 1.15f) - (stompThreshold - 1.0)) * 4.0);

                            //Double it with ninja accessories
                            if (HasAccessory(975) || HasAccessory(976) || HasAccessory(984))
                                damage *= 2;

                            int direction = player.velocity.X <= 0.001f ? 0 : (player.velocity.X >= 0.001f ? 1 : -1);
                            player.ApplyDamageToNPC(hit.Npc, damage, 4f, direction, false);

                            //Confuse and slow stomped for 2 update cycles
                            hit.Npc.AddBuff(31, 120, false);
                            hit.Npc.AddBuff(32, 120, false);

                            if (StompedNpcs.Any(x => x.StompedNpc == hit.Npc))
                            {
                                StompedNpcs.First(x => x.StompedNpc == hit.Npc).FramesSinceStomped = 0;
                            }
                            else
                            {
                                StompedNpcs.Add(new Stomped(hit.Npc));
                            }

                            player.fallStart = (int) (player.position.Y / 16.0);
                            player.velocity.Y = -player.velocity.Y * 0.6f;
                            //player.velocity.Y -= stompThreshold;
                        }
                    }
                    else
                        break;

                    i++;
                }
            }

            PrevVelocity = player.velocity;
        }

        public override void PostUpdate()
        {
            if (ForcedDirEnabled)
            {
                player.direction = ForcedDir;
            }
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