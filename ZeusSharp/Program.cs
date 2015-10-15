using System;
using System.Linq;
using System.Collections.Generic;

using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;

namespace ZeusSharp
{
    internal class Program
    {
        private static bool active;

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Console.WriteLine("> Zeus# loaded!");
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;
            if (!Game.IsInGame || Game.IsWatchingGame || me.ClassID != ClassID.CDOTA_Unit_Hero_Zuus)
                return;

            var orchid = me.FindItem("item_orchid");
            var sheepstick = me.FindItem("item_sheepstick");
            var veil = me.FindItem("item_veil_of_discord");
            var soulring = me.FindItem("item_soul_ring");
            var arcane = me.FindItem("item_arcane_boots");
            var blink = me.FindItem("item_blink");
            var blinkRange = 1200;

            if (active && blink != null && me != null)
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

                    if (soulring != null && me.Health > 300 && me.Mana < me.Spellbook.Spell2.ManaCost && soulring.CanBeCasted())
                    {
                        soulring.UseAbility();
                    }

                    if (arcane != null && me.Mana < me.Spellbook.Spell2.ManaCost && arcane.CanBeCasted())
                    {
                        arcane.UseAbility();
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

                    if (me.Spellbook.SpellQ.CanBeCasted() && me.Mana > me.Spellbook.Spell1.ManaCost && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("Q") && (!me.Spellbook.Spell2.CanBeCasted() || me.Distance2D(target) > 700) &&
                        !(me.Mana < 260))
                    {
                        me.Spellbook.SpellQ.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "Q");
                    }

                    if (me.Spellbook.Spell2.CanBeCasted() && me.Mana > me.Spellbook.Spell2.ManaCost && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("W"))
                    {
                        me.Spellbook.Spell2.UseAbility(target);
                        Utils.Sleep(200 + Game.Ping, "W");
                    }

                    if (((!me.Spellbook.Spell2.CanBeCasted() && !me.Spellbook.Spell1.CanBeCasted()) || target.IsMagicImmune()) && me.CanAttack())
                    {
                        me.Attack(target);
                    }

                    }
                    else
                    {
                        me.Attack(target);
                    }
                }
            }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen)
            {
                if (Game.IsKeyDown(System.Windows.Input.Key.Space))
                {
                    active = true;
                }
                else
                {
                    active = false;
                }
            }
        }
    }
}