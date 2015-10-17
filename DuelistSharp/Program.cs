using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;

using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;
using SharpDX.Direct3D9;

namespace DuelistSharp
{
    internal class Program
    {
        private static Ability Duel, Heal;
        private static Item Blink, armlet, mjollnir, medallion, solar, soulRing, urn, dust;
        private static Hero me, target;
        private static Key toggleKey = Key.J;
        private static Key activeKey = Key.Space;
        private static bool toggle = true;
        private static bool active;
        private static Font _text;

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Console.WriteLine("> DuelistSharp loaded!");

            _text = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Tahoma",
                   Height = 11,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });
                Drawing.OnPreReset += Drawing_OnPreReset;
                Drawing.OnPostReset += Drawing_OnPostReset;
                Drawing.OnEndScene += Drawing_OnEndScene;
                AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectMgr.LocalHero;

            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            if (me.ClassID != ClassID.CDOTA_Unit_Hero_Legion_Commander)
                return;

            if (me == null)
                return;

            Duel = me.Spellbook.Spell4;
            Heal = me.Spellbook.Spell2;

            Blink = me.FindItem("item_blink");
            armlet = me.FindItem("item_armlet");
            mjollnir = me.FindItem("item_mjollnir");
            medallion = me.FindItem("item_medallion_of_courage");
            solar = me.FindItem("item_solar_crest");
            soulRing = me.FindItem("item_soul_ring");
            urn = me.FindItem("item_urn_of_shadows");
            dust = me.FindItem("item_dust");

            //var duelManacost = Heal.ManaCost + Duel.ManaCost;

            // Main combo

            if (active && toggle)
            {
                target = me.ClosestToMouseTarget(1000);
                if (target.IsAlive && !target.IsInvul())
                {
                    if (Duel.CanBeCasted()) {

                        // here allied skills & items

                        //if (soulRing != null && soulRing.CanBeCasted() && me.Mana < duelManacost && me.Health > 300 && Utils.SleepCheck("soulring"))
                        //{
                        //    soulRing.UseAbility();
                        //    Utils.Sleep(150 + Game.Ping, "soulring");
                        //}

                        if (solar != null && solar.CanBeCasted() && Utils.SleepCheck("solar"))
                        {
                            solar.UseAbility(target);
                            Utils.Sleep(150 + Game.Ping, "solar");
                        }

                        // Blademail here

                        if (armlet != null && armlet.CanBeCasted() && Utils.SleepCheck("armlet"))
                        {
                            armlet.UseAbility();
                            Utils.Sleep(150 + Game.Ping, "armlet");
                        }

                        if (mjollnir != null && mjollnir.CanBeCasted() && Utils.SleepCheck("mjollnir"))
                        {
                            mjollnir.UseAbility(me);
                            Utils.Sleep(150 + Game.Ping, "mjollnir");
                        }

                        if (Heal.CanBeCasted() && Utils.SleepCheck("heal"))
                        {
                            Heal.UseAbility(me);
                            Utils.Sleep(150 + Game.Ping, "heal");
                        }

                        if (urn != null && urn.CanBeCasted() && Utils.SleepCheck("urn") && urn.CurrentCharges > 0 && me.Distance2D(target) < urn.CastRange && !target.IsMagicImmune())
                        {
                            urn.UseAbility(target);
                            Utils.Sleep(150 + Game.Ping, "urn");
                        }

                        // Blink

                        if (Blink != null && Blink.CanBeCasted() && me.Distance2D(target) > 300 && Utils.SleepCheck("blink1"))
                        {
                            Blink.UseAbility(target.Position);
                            Utils.Sleep(150 + Game.Ping, "blink1");
                        }

                        // Enemy items & skills

                        if (medallion != null && medallion.CanBeCasted() && Utils.SleepCheck("medallion"))
                        {
                            medallion.UseAbility(target);
                            Utils.Sleep(150 + Game.Ping, "medallion");
                        }

                        if (dust != null && dust.CanBeCasted() && target.CanGoInvis() && Utils.SleepCheck("dust"))
                        {
                            dust.UseAbility();
                            Utils.Sleep(150 + Game.Ping, "dust");
                        }

                        if (Duel.CanBeCasted() && me.CanAttack() && !target.IsInvul() && Utils.SleepCheck("duel"))
                        {
                            Duel.UseAbility(target);
                            Utils.Sleep(150 + Game.Ping, "duel");
                        }

                        if (armlet != null && Utils.SleepCheck("armlet") && me.CanCast() && armlet.IsActivated)
                        {
                            armlet.ToggleAbility();
                            Utils.Sleep(150 + Game.Ping, "armlet");
                        }

                    }
                    else
                    {
                        if (armlet != null && !armlet.IsActivated) {
                            armlet.ToggleAbility();
                        }
                        me.Attack(target);
                    }
                }
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
                }

                if (Game.IsKeyDown(toggleKey) && Utils.SleepCheck("toggle"))
                {
                    toggle = !toggle;
                    Utils.Sleep(300, "toggle");
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
            if (player == null || player.Team == Team.Observer || me.ClassID != ClassID.CDOTA_Unit_Hero_Legion_Commander)
                return;

            if (toggle && !active)
            {
                _text.DrawText(null, "Duelist#: Enabled | [" + toggleKey + "] for toggle", 4, 400, Color.White);
            }
            if (!toggle)
            {
                _text.DrawText(null, "Duelist#: Disabled | [" + toggleKey + "] for toggle", 4, 400, Color.WhiteSmoke);
            }
            if (active)
            {
                _text.DrawText(null, "Duelist#: Comboing!", 4, 400, Color.YellowGreen);
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
 
 
 