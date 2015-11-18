using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;

using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;
using SharpDX.Direct3D9;

namespace TinkerSharp
{
    internal class Program
    {
        private static Ability Laser, Rocket, Refresh;
        private static Item Blink, Dagon, Hex, Soulring, Ethereal, Veil, Orchid, Shiva;
        private static Hero me;
        private static Hero target;
        private static Key toggleKey = Key.J;
        private static Key activeKey = Key.Space;
        private static Key blinkToggleKey = Key.P;
        private static bool toggle = true;
        private static bool active;
        private static bool blinkToggle = true;
        private static int maximumDistance = 1500;
        private static Font _text;

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

            Console.WriteLine("> Tinker# loaded!");
            DrawLib.Draw.Init();

            _text = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Segoe UI",
                   Height = 17,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.ClearType
               });
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectMgr.LocalHero;

            if (me == null || !Game.IsInGame || me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
            {      
                return;
            }

            // Ability init
            Laser = me.Spellbook.Spell1;
            Rocket = me.Spellbook.Spell2;
            Refresh = me.Spellbook.Spell4;

            // Item init
            Blink = me.FindItem("item_blink");
            Dagon = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));
            Hex = me.FindItem("item_sheepstick");
            Soulring = me.FindItem("item_soul_ring");
            Ethereal = me.FindItem("item_ethereal_blade");
            Veil = me.FindItem("item_veil_of_discord");
            Orchid = me.FindItem("item_orchid");
            Shiva = me.FindItem("item_shivas_guard");

            // Manacost calculations
            var manaForCombo = Laser.ManaCost + Rocket.ManaCost;
            if (Dagon != null && Dagon.CanBeCasted())
                manaForCombo += 180;
            if (Hex != null && Hex.CanBeCasted())
                manaForCombo += 100;
            if (Ethereal != null && Ethereal.CanBeCasted())
                manaForCombo += 150;
            if (Veil != null && Veil.CanBeCasted())
                manaForCombo += 50;
            if (Shiva != null && Shiva.CanBeCasted())
                manaForCombo += 100;

            // Main combo
            if (active && toggle)
            {
                if ((target == null || !target.IsVisible) && !me.IsChanneling())
                    me.Move(Game.MousePosition);

                target = me.ClosestToMouseTarget(1000);
                if (target != null && target.IsAlive && !target.IsIllusion && !target.IsMagicImmune() && Utils.SleepCheck("refresh") && !Refresh.IsChanneling)
                {
                    if (Soulring != null && Soulring.CanBeCasted() && me.Health > 300 && Utils.SleepCheck("soulring"))
                    {
                        Soulring.UseAbility();
                        Utils.Sleep(150 + Game.Ping, "soulring");
                    }

                    // Blink

                    if (Blink != null && Blink.CanBeCasted() && (me.Distance2D(target) > 500) && Utils.SleepCheck("Blink") && blinkToggle)
                    {
                        Blink.UseAbility(target.Position);
                        Utils.Sleep(1000 + Game.Ping, "Blink");
                    }

                    // Items
                    else if (Shiva != null && Shiva.CanBeCasted() && Utils.SleepCheck("shiva"))
                    {
                        Shiva.UseAbility();
                        Utils.Sleep(100 + Game.Ping, "shiva");
                        Utils.ChainStun(me, 200 + Game.Ping, null, false);
                    }

                    else if (Veil != null && Veil.CanBeCasted() && Utils.SleepCheck("veil"))
                    {
                        Veil.UseAbility(target.Position);
                        Utils.Sleep(150 + Game.Ping, "veil");
                        Utils.Sleep(300 + Game.Ping, "ve");
                        Utils.ChainStun(me, 170 + Game.Ping, null, false);
                    }

                    else if (Hex != null && Hex.CanBeCasted() && Utils.SleepCheck("hex"))
                    {
                        Hex.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "hex");
                        Utils.Sleep(300 + Game.Ping, "h");
                        Utils.ChainStun(me, 170 + Game.Ping, null, false);
                    }

                    else if (Ethereal != null && Ethereal.CanBeCasted() && Utils.SleepCheck("ethereal"))
                    {
                        Ethereal.UseAbility(target);
                        Utils.Sleep(270 + Game.Ping, "ethereal");
                        Utils.ChainStun(me, 200 + Game.Ping, null, false);
                    }

                    else if (Dagon != null && Dagon.CanBeCasted() && Utils.SleepCheck("ethereal") && Utils.SleepCheck("h") && Utils.SleepCheck("dagon") && Utils.SleepCheck("veil"))
                    {
                        Dagon.UseAbility(target);
                        Utils.Sleep(270 + Game.Ping, "dagon");
                        Utils.ChainStun(me, 200 + Game.Ping, null, false);
                    }

                    // Skills
                    else if (Rocket != null && Rocket.CanBeCasted() && Utils.SleepCheck("rocket") && Utils.SleepCheck("ethereal") && Utils.SleepCheck("veil"))
                    {
                        Rocket.UseAbility();
                        Utils.Sleep(150 + Game.Ping, "rocket");
                        Utils.ChainStun(me, 150 + Game.Ping, null, false);
                    }

                    else if (Laser != null && Laser.CanBeCasted() && Utils.SleepCheck("laser") && Utils.SleepCheck("ethereal") && Utils.SleepCheck("rocket"))
                    {
                        Laser.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "laser");
                        Utils.ChainStun(me, 150 + Game.Ping, null, false);
                    }

                    else if (Refresh != null && Refresh.CanBeCasted() && me.Mana > 200 && Utils.SleepCheck("refresh") && !Refresh.IsChanneling && nothingCanCast())
                    {
                        Refresh.UseAbility();
                        Utils.ChainStun(me, (Refresh.ChannelTime * 1000) + Game.Ping + 400, null, false);
                        Utils.Sleep(700 + Game.Ping, "refresh");
                    }

                    else if (!me.IsChanneling() && !Refresh.IsChanneling && nothingCanCast())
                    {
                        me.Attack(target);
                    }
                }
            }
        }

        private static bool nothingCanCast()
        {
            if (!Laser.CanBeCasted() && 
                !Rocket.CanBeCasted() && 
                !Ethereal.CanBeCasted() && 
                !Dagon.CanBeCasted() && 
                !Hex.CanBeCasted() &&
                !Shiva.CanBeCasted() &&
                !Veil.CanBeCasted())
                return true;
            else
            {
                return false;
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen)
            {
                if (Game.IsKeyDown(activeKey))
                {
                    active = true;
                }
                else
                {
                    active = false;
                    //_refreshed = false;
                }

                if (Game.IsKeyDown(toggleKey) && Utils.SleepCheck("toggle"))
                {
                    toggle = !toggle;
                    Utils.Sleep(300, "toggle");

                }

               if (Game.IsKeyDown(blinkToggleKey) && Utils.SleepCheck("toggleBlink"))
               {
                blinkToggle = !blinkToggle;
                Utils.Sleep(150, "toggleBlink");

               }
        }
        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            _text.Dispose();
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
                return;

            var player = ObjectMgr.LocalPlayer;
            var me = ObjectMgr.LocalHero;
            if (player == null || player.Team == Team.Observer || me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
            if (toggle && !active)
            {
                DrawLib.Draw.DrawPanel(2, 350, 310, 20, 1, new ColorBGRA(0, 0, 100, 100));
                DrawLib.Draw.DrawShadowText("Tinker#: Enabled | [" + toggleKey + "] for toggle | [" + activeKey + "] for combo [" + blinkToggleKey + "] for toggle Blink", 4, 350, Color.LawnGreen, _text);
            }
            if (!toggle)
            {
                DrawLib.Draw.DrawPanel(2, 350, 192, 20, 1, new ColorBGRA(0, 0, 100, 100));
                DrawLib.Draw.DrawShadowText("Tinker#: Disabled | [" + toggleKey + "] for toggle", 4, 350, Color.DarkGray, _text);
            }
            if (toggle && active)
            {
                DrawLib.Draw.DrawPanel(2, 350, 118, 20, 1, new ColorBGRA(0, 0, 100, 100));
                DrawLib.Draw.DrawShadowText("Tinker#: Comboing!", 4, 350, Color.GreenYellow, _text);
            }
        }

        static void Drawing_OnPostReset(EventArgs args)
        {
            _text.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            _text.OnLostDevice();
        }
    }
}
