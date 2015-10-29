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
        private static Ability Duel, Heal, Odds;
        private static Item Blink, armlet, mjollnir, medallion, solar, soulRing, urn, dust, bladeMail, bkb, abyssal;
        private static Hero me, target;
        private static Key toggleKey = Key.J;
        private static Key activeKey = Key.Space;
        private static Key bkbToggleKey = Key.G;
        private static Key killstealToggleKey = Key.K;
        private static bool toggle = true;
        private static bool active;
        private static bool bkbToggle = false;
        private static bool killstealToggle = true;
        private static Font _text;
        private static Font _comboing;
        private static int[] qDmg = new int[4] { 40, 80, 120, 160 };

        static void Main(string[] args)
        {
            Game.OnUpdate += Killsteal;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

            Console.WriteLine("> DuelistSharp loaded!");

            DrawLib.Draw.Init();

            _text = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Segoe UI",
                   Height = 16,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.ClearType
               });

            _comboing = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Segoe UI",
                   Height = 22,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.ClearType
               });
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

            if (Duel == null)
                Duel = me.Spellbook.Spell4;
            if (Heal == null)
                Heal = me.Spellbook.Spell2;
            if (Odds == null)
                Odds = me.Spellbook.Spell1;

            if (Blink == null)
                Blink = me.FindItem("item_blink");
            if (armlet == null)
                armlet = me.FindItem("item_armlet");
            if (mjollnir == null)
                mjollnir = me.FindItem("item_mjollnir");
            if (medallion == null)
                medallion = me.FindItem("item_medallion_of_courage");
            if (solar == null)
                solar = me.FindItem("item_solar_crest");
            if (soulRing == null)
                soulRing = me.FindItem("item_soul_ring");
            if (bkb == null)
                bkb = me.FindItem("item_black_king_bar");
            if (abyssal == null)
                abyssal = me.FindItem("item_abyssal_blade");
            if (dust == null)
                dust = me.FindItem("item_dust");
            if (bladeMail == null)
                bladeMail = me.FindItem("item_blade_mail");

            var duelManacost = Heal.ManaCost + Duel.ManaCost;

            // Main combo

            if (active && toggle)
            {
                target = me.ClosestToMouseTarget(1000);
                if (target != null && target.IsAlive && !target.IsInvul() && !target.IsIllusion)
                {
                    if (me.CanAttack() && me.CanCast()) {

                        var linkens = target.Modifiers.Any(x => x.Name == "modifier_item_spheretarget") || target.Inventory.Items.Any(x => x.Name == "item_sphere");

                        // here allied skills & items

                        if (soulRing != null && soulRing.CanBeCasted() && me.Mana < duelManacost && me.Health > 300 && Utils.SleepCheck("soulring"))
                        {
                            soulRing.UseAbility();
                            Utils.Sleep(150 + Game.Ping, "soulring");
                        }

                        if (bladeMail != null && bladeMail.CanBeCasted() && Utils.SleepCheck("blademail"))
                        {
                            bladeMail.UseAbility();
                            Utils.Sleep(150 + Game.Ping, "blademail");
                        }

                        if (armlet != null && armlet.CanBeCasted() && Utils.SleepCheck("armlet1") && !armlet.IsToggled)
                        {
                            armlet.ToggleAbility();
                            Utils.Sleep(150 + Game.Ping, "armlet1");
                        }

                        if (mjollnir != null && mjollnir.CanBeCasted() && Utils.SleepCheck("mjollnir"))
                        {
                            mjollnir.UseAbility(me);
                            Utils.Sleep(150 + Game.Ping, "mjollnir");
                        }

                        if (Heal.CanBeCasted() && Utils.SleepCheck("heal") && (!bkb.CanBeCasted() || !me.IsMagicImmune()))
                        {
                            Heal.UseAbility(me);
                            Utils.Sleep(150 + Game.Ping, "heal");
                        }
                        Utils.ChainStun(me, 100, null, false);

                        if (bkb != null && bkb.CanBeCasted() && Utils.SleepCheck("bkb") && bkbToggle)
                        {
                            bkb.UseAbility();
                            Utils.Sleep(150 + Game.Ping, "bkb");
                        }

                        Utils.ChainStun(me, 200, null, false);

                        // Blink

                        if (Blink != null && Blink.CanBeCasted() && me.Distance2D(target) > 300 && me.Distance2D(target) <= 1170 && Utils.SleepCheck("blink1"))
                        {
                            Blink.UseAbility(target.Position);
                            Utils.Sleep(150 + Game.Ping, "blink1");
                        }

                        // Enemy items & skills

                        //if (Odds.CanBeCasted() && Utils.SleepCheck("odds") && !target.IsMoving)
                        //{
                        //    Odds.UseAbility(target.Position);
                        //    Utils.Sleep(200 + Game.Ping, "odds");
                        //}

                        if (!Odds.CanBeCasted())
                            Utils.ChainStun(me, 200, null, false);

                        if (abyssal != null && abyssal.CanBeCasted() && Utils.SleepCheck("abyssal"))
                        {
                            abyssal.UseAbility(target);
                            Utils.Sleep(400 + Game.Ping, "abyssal");
                        }

                        if (abyssal != null)
                            Utils.ChainStun(me, 310, null, false);

                        if (medallion != null && medallion.CanBeCasted() && Utils.SleepCheck("medallion"))
                        {
                            medallion.UseAbility(target);
                            Utils.Sleep(150 + Game.Ping, "medallion");
                        }

                        if (solar != null && solar.CanBeCasted() && Utils.SleepCheck("solar"))
                        {
                            solar.UseAbility(target);
                            Utils.Sleep(200 + Game.Ping, "solar");
                        }

                        if (dust != null && dust.CanBeCasted() && (target.CanGoInvis() || target.IsInvisible()) && Utils.SleepCheck("dust"))
                        {
                            dust.UseAbility();
                            Utils.Sleep(200 + Game.Ping, "dust");
                        }

                        if (Duel.CanBeCasted() && me.CanAttack() && !target.IsInvul() && Utils.SleepCheck("duel") && !linkens)
                        {
                            Duel.UseAbility(target);
                            Utils.Sleep(150 + Game.Ping, "duel");
                        }

                        if (!Duel.CanBeCasted() && Utils.SleepCheck("attack2"))
                        {
                            me.Attack(target);
                            Utils.Sleep(Game.Ping + 1000, "attack2");
                        }

                        if (armlet != null && Utils.SleepCheck("armlet") && me.CanCast() && armlet.IsToggled && (target == null || !target.IsAlive || !target.IsVisible))
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
                        if (Utils.SleepCheck("attack1"))
                        {
                            me.Attack(target);
                            Utils.Sleep(1000, "attack1");
                        }
                    }
                }
            }
        }

        public static void Killsteal(EventArgs args)
        {
            if (me == null || !Game.IsInGame || Odds == null)
                killstealToggle = false;

            if (Utils.SleepCheck("killstealQ") && Game.IsInGame && killstealToggle)
            {
                if (!active && toggle && Odds.CanBeCasted() && me.Mana > Odds.ManaCost)
                {
                    var enemy = ObjectMgr.GetEntities<Hero>().Where(e => e.Team != me.Team && e.IsAlive && e.IsVisible && !e.IsIllusion && !e.UnitState.HasFlag(UnitState.MagicImmune) && me.Distance2D(e) < 700).ToList();
                    foreach (var v in enemy)
                    {
                        var damage = Math.Floor(qDmg[Odds.Level - 1] * (1 - v.MagicDamageResist / 100));
                        if (v.Health < damage && me.Distance2D(v) < Odds.CastRange)
                        {
                            Odds.UseAbility(v.Position);
                            Utils.Sleep(300, "killstealQ");
                        }
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

                if (Game.IsKeyDown(bkbToggleKey) && Utils.SleepCheck("toggleBKB"))
                {
                    bkbToggle = !bkbToggle;
                    Utils.Sleep(300, "toggleBKB");
                }

                if (Game.IsKeyDown(killstealToggleKey) && Utils.SleepCheck("toggleKS"))
                {
                    killstealToggle = !killstealToggle;
                    Utils.Sleep(300, "toggleKS");
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
                DrawLib.Draw.DrawPanel(2, 400, 278, 46, 1, new ColorBGRA(0, 0, 100, 100));
                DrawLib.Draw.DrawShadowText("BKB: " + bkbToggle + " | [" + toggleKey + "] for toggle combo | [" + bkbToggleKey + "] for toggle BKB\nKillsteal: " + killstealToggle + " | [" + killstealToggleKey + "] for killsteal toggle", 4, 415, Color.LawnGreen, _text);
                DrawLib.Draw.DrawShadowText("Duelist#: Enabled", 4, 400, Color.LightSteelBlue, _text);
            }
            if (!toggle)
            {
                DrawLib.Draw.DrawPanel(2, 400, 175, 17, 1, new ColorBGRA(0, 0, 100, 100));
                DrawLib.Draw.DrawShadowText("Duelist#: Disabled | [" + toggleKey + "] for toggle", 4, 400, Color.DarkGray, _text);
            }
            if (active && toggle)
            {
                DrawLib.Draw.DrawPanel(2, 400, 150, 25, 1, new ColorBGRA(0, 0, 100, 100));
                DrawLib.Draw.DrawShadowText("Duelist#: Comboing!", 4, 400, Color.YellowGreen, _comboing);
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
 
 
 
