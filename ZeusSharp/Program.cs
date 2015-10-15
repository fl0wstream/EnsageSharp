using System;
using System.Linq;
using System.Collections.Generic;

using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;
using SharpDX.Direct3D9;

namespace ZeusSharp
{
    internal class Program
    {
        private static bool active;
        private static bool toggle = true;
        private static bool stealToggle = true;
        private static int manaForQ = 235;
        private static Font _text;
        private static System.Windows.Input.Key enableKey = System.Windows.Input.Key.Space;
        private static System.Windows.Input.Key toggleKey = System.Windows.Input.Key.J;
        private static System.Windows.Input.Key stealToggleKey = System.Windows.Input.Key.L;

        private static int[] QDmg = new int[4] { 100, 175, 275, 350 };
        private static float QRange = 325;

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            //Game.OnUpdate += Killsteal;
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
                Drawing.OnPreReset += Drawing_OnPreReset;
                Drawing.OnPostReset += Drawing_OnPostReset;
                Drawing.OnEndScene += Drawing_OnEndScene;
                AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;
            if (!Game.IsInGame || Game.IsWatchingGame || me.ClassID != ClassID.CDOTA_Unit_Hero_Zuus)
            {
                return;
            }

            // Items

            var orchid = me.FindItem("item_orchid");
            var sheepstick = me.FindItem("item_sheepstick");
            var veil = me.FindItem("item_veil_of_discord");
            var soulring = me.FindItem("item_soul_ring");
            var arcane = me.FindItem("item_arcane_boots");
            var blink = me.FindItem("item_blink");
            var shiva = me.FindItem("item_shivas_guard");
            var dagon = me.GetDagon();
            var blinkRange = 1200;

            // Main combo

            if (active && toggle)
            {
                var target = me.ClosestToMouseTarget(1000);
                if (target.IsAlive && target.IsVisible)
                {
                    var targetPos = target.Position;

                    if (blink.CanBeCasted() && me.Distance2D(target) > (blinkRange - 400) && Utils.SleepCheck("blink") && Utils.SleepCheck("blink1"))
                    {
                        blink.UseAbility(targetPos);
                        Utils.Sleep(1000 + Game.Ping, "blink1");
                    }

                    Utils.Sleep(me.GetTurnTime(target) + Game.Ping, "blink");

                    if (me.Health > 300 && me.Mana < me.Spellbook.Spell2.ManaCost && soulring.CanBeCasted())
                    {
                        soulring.UseAbility();
                    }

                    if (me.Mana < me.Spellbook.Spell2.ManaCost && arcane.CanBeCasted())
                    {
                        arcane.UseAbility();
                    }

                    if (sheepstick.CanBeCasted() && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("sheepstick"))
                    {
                        sheepstick.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "sheepstick");
                    }

                    if (orchid.CanBeCasted() && !target.IsMagicImmune() && !target.IsIllusion && !target.IsHexed() && Utils.SleepCheck("orchid"))
                    {
                        orchid.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "orchid");
                    }

                    if (veil.CanBeCasted() && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("veil"))
                    {
                        veil.UseAbility(target.Position);
                        Utils.Sleep(150 + Game.Ping, "veil");
                    }

                    if (dagon.CanBeCasted() && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("dagon"))
                    {
                        dagon.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "dagon");
                    }

                    if (shiva.CanBeCasted() && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("shiva"))
                    {
                        shiva.UseAbility();
                        Utils.Sleep(150 + Game.Ping, "shiva");
                    }

                    if (me.Spellbook.SpellQ.CanBeCasted() && me.Mana > me.Spellbook.Spell1.ManaCost && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("Q") && (!me.Spellbook.Spell2.CanBeCasted() || me.Distance2D(target) > 700) &&
                        !(me.Mana < manaForQ))
                    {
                        me.Spellbook.SpellQ.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "Q");
                    }

                    if (me.Spellbook.Spell2.CanBeCasted() && me.Mana > me.Spellbook.Spell2.ManaCost && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("W"))
                    {
                        me.Spellbook.Spell2.UseAbility(target);
                        Utils.Sleep(200 + Game.Ping, "W");
                    }

                    if ((!(me.Spellbook.Spell2.CanBeCasted() && me.Spellbook.Spell1.CanBeCasted()) || target.IsMagicImmune()) && me.CanAttack())
                    {
                        me.Attack(target);
                    }

                    }
                }
        }

        //public static void Killsteal(EventArgs args)
        //{
        //    var me = ObjectMgr.LocalHero;
        //    var dagon = me.GetDagon();

            // Killsteal

            //if (stealToggle)
            //{
            //    var enemy = ObjectMgr.GetEntities<Hero>().Where(e => e.Team != me.Team && e.IsAlive && e.IsVisible && !e.IsIllusion && !e.UnitState.HasFlag(UnitState.MagicImmune)).ToList();
            //    foreach (var v in enemy)
            //    {
            //        if (!me.Spellbook.Spell2.CanBeCasted() || me.Mana < me.Spellbook.Spell2.ManaCost)
            //            return;
            //
            //        var damage = Math.Floor(QDmg[me.Spellbook.Spell2.Level - 1] * (1 - v.MagicDamageResist / 100));
            //        if ((me.Distance2D(v) <= QRange) && v.Health < damage)
            //        {
            //            me.Spellbook.Spell2.UseAbility(v);
            //        }
            //    }
            //}
            //}

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
            if (player == null || player.Team == Team.Observer || me.ClassID != ClassID.CDOTA_Unit_Hero_Zuus)
                return;

            if (active && toggle)
            {
                _text.DrawText(null, "Zeus#: Comboing!", 4, 150, Color.Green);
            }

            if (toggle && !active)
            {
                //_text.DrawText(null, "Zeus#: Enabled | Killsteal: " + stealToggle + " [" + stealToggleKey + "] for toggle killsteal | [" + enableKey + "] for combo | [" + toggleKey + "] for toggle combo", 4, 150, Color.White);
                _text.DrawText(null, "Zeus#: Enabled | [" + enableKey + "] for combo | [" + toggleKey + "] for toggle combo", 4, 150, Color.White);
            }
            if (!toggle)
            {
                _text.DrawText(null, "Zeus#: Disabled | [" + toggleKey + "] for toggle", 4, 150, Color.WhiteSmoke);
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
 
 
 