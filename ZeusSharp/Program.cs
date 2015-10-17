using System;
using System.Linq;
using System.Collections.Generic;

using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;
using SharpDX.Direct3D9;
using System.Windows.Input;

namespace ZeusSharp
{
    internal class Program
    {
        private static Item orchid, sheepstick, veil, soulring, arcane, blink, shiva, dagon, refresher;
        private static bool active;
        private static bool toggle = true;
        private static bool stealToggle = false;
        private static bool blinkToggle = true;
        private static bool drawStealNotice = false;
        private static bool confirmSteal = false;
        private static bool refresherToggle = false;
        private static int manaForQ = 235;
        private static Font _text;
        private static Font _notice;
        private static Key enableKey = Key.Space;
        private static Key toggleKey = Key.J;
        private static Key stealToggleKey = Key.L;
        private static Key blinkToggleKey = Key.P;
        private static Key confirmStealKey = Key.G;
        private static Key refresherToggleKey = Key.O;
        private static string steallableHero;

        private static int[] rDmg = new int[3] { 225, 350, 475 };

        static void Main(string[] args)
        {
            Game.OnUpdate += Killsteal;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Console.WriteLine("> Zeus# loaded!");

            _text = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Tahoma",
                   Height = 11,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            _notice = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Segoe UI",
                   Height = 30,
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
            var me = ObjectMgr.LocalHero;
            if (!Game.IsInGame || me.ClassID != ClassID.CDOTA_Unit_Hero_Zuus || me == null)
            {
                return;
            }

            // Items

            if (orchid == null)
                orchid = me.FindItem("item_orchid");

            if (sheepstick == null)
                sheepstick = me.FindItem("item_sheepstick");

            if (veil == null)
                veil = me.FindItem("item_veil_of_discord");

            if (soulring == null)
                soulring = me.FindItem("item_soul_ring");

            if (arcane == null)
                arcane = me.FindItem("item_arcane_boots");

            if (blink == null)
                blink = me.FindItem("item_blink");

            if (shiva == null)
                shiva = me.FindItem("item_shivas_guard");

            if (dagon == null)
                dagon = me.GetDagon();

            if (refresher == null)
                refresher = me.FindItem("item_refresher");

            var blinkRange = 1200;
            var refresherComboManacost = me.Spellbook.Spell4.ManaCost + me.Spellbook.Spell2.ManaCost + me.Spellbook.Spell1.ManaCost + veil.ManaCost + orchid.ManaCost + sheepstick.ManaCost;

            // Main combo

            if (active && toggle && me.CanCast() && me.IsAlive)
            {
                var target = me.ClosestToMouseTarget(1000);
                if (target != null && target.IsAlive && !target.IsInvul())
                {
                    var targetPos = target.Position;

                    if (blink != null && blink.CanBeCasted() && (me.Distance2D(target) > (blinkRange - 500)) && Utils.SleepCheck("blink") && Utils.SleepCheck("blink1") && blinkToggle)
                    {
                        blink.UseAbility(targetPos);
                        Utils.Sleep(1000 + Game.Ping, "blink1");
                    }

                    Utils.Sleep(me.GetTurnTime(target) + Game.Ping, "blink");

                    if (soulring != null && me.Health > 300 && (me.Mana < me.Spellbook.Spell2.ManaCost || (me.Mana < refresherComboManacost && refresherToggle && refresher.CanBeCasted())) && soulring.CanBeCasted() && Utils.SleepCheck("soulring"))
                    {
                        soulring.UseAbility();
                        Utils.Sleep(150 + Game.Ping, "soulring");
                    }

                    if (arcane != null && (me.Mana < me.Spellbook.Spell2.ManaCost || (me.Mana < refresherComboManacost && refresherToggle && refresher.CanBeCasted())) && arcane.CanBeCasted() && Utils.SleepCheck("arcane"))
                    {
                        arcane.UseAbility();
                        Utils.Sleep(150 + Game.Ping, "arcane");
                    }

                    if (sheepstick != null && sheepstick.CanBeCasted() && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("sheepstick"))
                    {
                        sheepstick.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "sheepstick");
                    }

                    if (orchid != null && orchid.CanBeCasted() && !target.IsMagicImmune() && !target.IsIllusion && !target.IsHexed() && Utils.SleepCheck("orchid"))
                    {
                        orchid.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "orchid");
                    }

                    if (veil != null && veil.CanBeCasted() && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("veil"))
                    {
                        veil.UseAbility(target.Position);
                        Utils.Sleep(150 + Game.Ping, "veil");
                    }

                    if (dagon != null && dagon.CanBeCasted() && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("dagon"))
                    {
                        dagon.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "dagon");
                    }

                    if (shiva != null && shiva.CanBeCasted() && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("shiva"))
                    {
                        shiva.UseAbility();
                        Utils.Sleep(150 + Game.Ping, "shiva");
                    }

                    if (me.Spellbook.SpellQ != null && me.Spellbook.SpellQ.CanBeCasted() && me.Mana > me.Spellbook.Spell1.ManaCost && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("Q") && (!me.Spellbook.Spell2.CanBeCasted() || me.Distance2D(target) > 700) && me.Mana > manaForQ)
                    {
                        me.Spellbook.SpellQ.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "Q");
                    }

                    if (me.Spellbook.Spell2 != null && me.Spellbook.Spell2.CanBeCasted() && me.Mana > me.Spellbook.Spell2.ManaCost && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("W"))
                    {
                        me.Spellbook.Spell2.UseAbility(target);
                        Utils.Sleep(200 + Game.Ping, "W");
                    }

                    if ((!(me.Spellbook.Spell2.CanBeCasted() && me.Spellbook.Spell1.CanBeCasted()) || target.IsMagicImmune()) && me.CanAttack() && target != null)
                    {
                        me.Attack(target);
                    }

                    if (refresherToggle && me.Mana > refresherComboManacost && target != null && !target.IsMagicImmune() && me.Spellbook.Spell4.CanBeCasted() && Utils.SleepCheck("ultiRefresher"))
                    {
                        me.Spellbook.Spell4.UseAbility();
                        Utils.Sleep(50 + Game.Ping, "ultiRefresher");
                    }

                    if (refresherToggle && refresher != null && refresher.CanBeCasted() && Utils.SleepCheck("refresher") && !target.IsMagicImmune() && target != null &&
                        !me.Spellbook.Spell4.CanBeCasted() && !me.Spellbook.Spell2.CanBeCasted() && !me.Spellbook.Spell1.CanBeCasted() && me.Mana > refresherComboManacost)
                    {
                        refresher.UseAbility();
                        Utils.Sleep(300 + Game.Ping, "refresher");
                    }

                }
                }
        }

        public static void Killsteal(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;
            if (me == null || !Game.IsInGame || me.Spellbook.Spell4 == null)
                stealToggle = false;

            if (Utils.SleepCheck("killstealR") && Game.IsInGame)
            {

                drawStealNotice = false;

                if (me.HasItem(ClassID.CDOTA_Item_UltimateScepter))
                {
                    rDmg = new int[3] { 440, 540, 640 };
                }
                else
                {
                    rDmg = new int[3] { 225, 350, 475 };
                }

                if (!active && toggle && me.Spellbook.Spell4.CanBeCasted() && me.Mana > me.Spellbook.Spell4.ManaCost)
                {
                    var enemy = ObjectMgr.GetEntities<Hero>().Where(e => e.Team != me.Team && e.IsAlive && e.IsVisible && !e.IsIllusion && !e.UnitState.HasFlag(UnitState.MagicImmune)).ToList();
                    foreach (var v in enemy)
                    {

                        var damage = Math.Floor(rDmg[me.Spellbook.Spell4.Level - 1] * (1 - v.MagicDamageResist / 100));
                        if (v.Health < (damage - 40) && v != null)
                        {
                            drawStealNotice = true;
                            steallableHero = v.NetworkName;
                            steallableHero = steallableHero.Replace("CDOTA_Unit_Hero_", "");
                            steallableHero = steallableHero.ToUpper();

                            if (confirmSteal || stealToggle && v != null) {
                                me.Spellbook.Spell4.UseAbility();
                                Utils.Sleep(300, "killstealR");
                            }
                        }
                    }
                }
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen)
            {
                if (Game.IsKeyDown(enableKey))
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
                    Utils.Sleep(150, "toggle");
                }

                if (Game.IsKeyDown(stealToggleKey) && Utils.SleepCheck("toggleKS"))
                {
                    stealToggle = !stealToggle;
                    Utils.Sleep(150, "toggleKS");
                }

                if (Game.IsKeyDown(blinkToggleKey) && Utils.SleepCheck("toggleBlink"))
                {
                    blinkToggle = !blinkToggle;
                    Utils.Sleep(150, "toggleBlink");
                }

                if (Game.IsKeyDown(refresherToggleKey) && Utils.SleepCheck("toggleRefresher") && refresher != null)
                {
                    refresherToggle = !refresherToggle;
                    Utils.Sleep(150, "toggleRefresher");
                }

                if (Game.IsKeyDown(confirmStealKey))
                {
                    confirmSteal = true;
                }
                else
                {
                    confirmSteal = false;
                }

                if (refresher == null)
                    refresherToggle = false;
            }
        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            _text.Dispose();
            _notice.Dispose();
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
                return;

            var player = ObjectMgr.LocalPlayer;
            var me = ObjectMgr.LocalHero;
            if (player == null || player.Team == Team.Observer || me.ClassID != ClassID.CDOTA_Unit_Hero_Zuus)
                return;

            if (active && toggle)
            {
                _text.DrawText(null, "Zeus#: Comboing!", 4, 150, Color.Green);
            }

            if (toggle && !active)
            {
                _text.DrawText(null, "Zeus#: Enabled | Blink: " + blinkToggle + " | AutoUltiSteal: " + stealToggle + " | Refresher: " + refresherToggle + " | [" + enableKey + "] for combo | [" + toggleKey + "] for toggle combo | [" + blinkToggleKey + "] for toggle blink | [" + stealToggleKey + "] for toggle AutoUltiSteal | [" + refresherToggleKey + "] for toggle refresher", 4, 150, Color.White);
            }
            if (!toggle)
            {
                _text.DrawText(null, "Zeus#: Disabled | [" + toggleKey + "] for toggle", 4, 150, Color.WhiteSmoke);
            }
            if (drawStealNotice && !confirmSteal && !stealToggle)
            {
                _notice.DrawText(null, "PRESS [" + confirmStealKey + "] FOR STEAL " + steallableHero + "!", 4, 400, Color.Yellow);
            }
        }

        static void Drawing_OnPostReset(EventArgs args)
        {
            _text.OnResetDevice();
            _notice.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            _text.OnLostDevice();
            _notice.OnLostDevice();
        }
    }
}
 
 
 