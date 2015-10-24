using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading;

using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;
using SharpDX.Direct3D9;
using EZGUI;

namespace InvokerSharp
{
    internal class Program
    {
        private static Ability Exort, Quas, Wex, Invoke, coldsnap, forge, blast, alacrity, tornado, meteor, icewall, emp, sunstrike;
        private static Item Hex, Refresher;
        private static Hero target, me;
        private static Key activeKey = Key.Space;
        private static bool active;
        private static int[] lift_duration = new int[8] { 800, 1100, 1400, 1700, 2000, 2300, 2600, 2900 };
        private static EzGUI gui;
        //
        private static EzElement toggle, smartmode, combo;

        static void Main(string[] args)
        {

            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;

            Console.WriteLine("> Invoker# loaded!");

            gui = new EzGUI(100, 60, "Invoker# v0.1.8.0 @Evervolv1337");
            toggle = new EzElement(ElementType.CHECKBOX, "Enabled", true);
            smartmode = new EzElement(ElementType.CHECKBOX, "Smartmode", true);
            gui.AddMainElement(toggle);
            combo = new EzElement(ElementType.CATEGORY, "Combo", true);
            gui.AddMainElement(combo);
            combo.AddElement(smartmode);
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectMgr.LocalHero;

            if (me == null || !Game.IsInGame || Game.IsWatchingGame || me.ClassID != ClassID.CDOTA_Unit_Hero_Invoker)
            {
                Game.OnUpdate -= Game_OnUpdate;
                Console.WriteLine("> Invoker# Unloaded!");         
                return;
            }

            // Ability init
            Exort = me.Spellbook.Spell3;
            Quas = me.Spellbook.Spell1;
            Wex = me.Spellbook.Spell2;
            Invoke = me.Spellbook.Spell6;
            coldsnap = me.FindSpell("invoker_cold_snap");
            forge = me.FindSpell("invoker_forge_spirit");
            blast = me.FindSpell("invoker_deafening_blast");
            alacrity = me.FindSpell("invoker_alacrity");
            tornado = me.FindSpell("invoker_tornado");
            meteor = me.FindSpell("invoker_chaos_meteor");
            icewall = me.FindSpell("invoker_ice_wall");
            emp = me.FindSpell("invoker_emp");
            sunstrike = me.FindSpell("invoker_sun_strike");

            // Item init
            Hex = me.FindItem("item_sheepstick");
            Refresher = me.FindItem("item_refresher");

            // Smartmode selector BETA
            if (active && toggle.isActive && smartmode.isActive)
            {
                if ((target == null || !target.IsVisible) && !me.IsChanneling())
                    me.Move(Game.MousePosition);

                target = me.ClosestToMouseTarget(1000);
                if (target != null && target.IsAlive && !target.IsIllusion && !target.IsMagicImmune())
                {
                    // Fullcombo
                    if (target.MaximumHealth > 600 && Wex.Level > 3 && Quas.Level > 2)
                    {
                        CastFullCombo();
                        Utils.Sleep(5000, "comboing");
                    }

                    // SnapDPS
                    if (forge.CanBeCasted() && Exort.Level > 1 && coldsnap.CanBeCasted() && Utils.SleepCheck("comboing"))
                    {
                        CastSnapDPS();
                    }
                }
            }
        }

        // FullCombo: Tornado -> EMP -> Meteor -> Blast -> Cold Snap -> Forge Spirit -> Sunstrike
        private static void CastFullCombo()
        {
            var ModifR = target.Modifiers.Any(y => y.Name == "modifier_invoker_tornado");

            // Tornado
            if (Utils.SleepCheck("tornado") && tornado.CanBeCasted() && (Invoke.CanBeCasted() || HasSpell("invoker_tornado")))
            {
                GetSpell("invoker_tornado").UseAbility(target.Position);
                me.Move(target.Position);
                Utils.Sleep(1000, "tornado");

                var tornadoDuration = lift_duration[Quas.Level];

                Utils.Sleep((tornadoDuration + Game.AvgPing) - 800, "tornado1");
                Utils.Sleep((tornadoDuration + Game.AvgPing) - 2800, "tornado2");
                Utils.Sleep(tornadoDuration, "tornado3");
                Utils.Sleep(tornadoDuration - 1900 + Game.AvgPing, "tornado4");
            }

            // EMP
            if (Utils.SleepCheck("emp") && emp.CanBeCasted() && (Invoke.CanBeCasted() || HasSpell("invoker_emp")) && Utils.SleepCheck("tornado2"))
            {
                GetSpell("invoker_emp").UseAbility(target.Position);
                Utils.Sleep(1000, "emp");
            }

            // Meteor
            if (Utils.SleepCheck("meteor") && meteor.CanBeCasted() && (Invoke.CanBeCasted() || HasSpell("invoker_chaos_meteor")) && (ModifR || !target.IsMoving) && Utils.SleepCheck("tornado1"))
            {
                GetSpell("invoker_chaos_meteor").UseAbility(target.Position);
                me.Move(target.Position);
                Utils.Sleep(1000, "meteor");
            }

            // Blast
            if (Utils.SleepCheck("blast") && blast.CanBeCasted() && (Invoke.CanBeCasted() || HasSpell("invoker_deafening_blast")) && !ModifR && Utils.SleepCheck("tornado1"))
            {
                GetSpell("invoker_deafening_blast").UseAbility(target.Position);
                Utils.Sleep(1000, "blast");
            }

            // Cold Snap
            if (Utils.SleepCheck("coldsnap") && coldsnap.CanBeCasted() && (Invoke.CanBeCasted() || HasSpell("invoker_cold_snap")) && Utils.SleepCheck("tornado3"))
            {
                GetSpell("invoker_cold_snap").UseAbility(target);
                Utils.Sleep(1000, "coldsnap");
            }

            // Forge Spirit
            if (Utils.SleepCheck("forge") && forge.CanBeCasted() && (Invoke.CanBeCasted() || HasSpell("invoker_forge_spirit")) && Utils.SleepCheck("tornado3"))
            {
                GetSpell("invoker_forge_spirit").UseAbility();
                Utils.Sleep(1000, "forge");
            }

            // Sunstrike - TODO: Prediction
            if (Utils.SleepCheck("sunstrike") && sunstrike.CanBeCasted() && (Invoke.CanBeCasted() || HasSpell("invoker_sun_strike")) && Utils.SleepCheck("tornado4") && !target.IsMoving)
            {
                GetSpell("invoker_sun_strike").UseAbility(target.Position);
                Utils.Sleep(1000, "sunstrike");
            }

            // Attack
            else if (me.CanAttack() && !target.IsInvul() && Utils.SleepCheck("attack"))
            {
                me.Attack(target);
                Utils.Sleep(150 + Game.Ping, "attack");
            }
        }

        // SnapDPS: Coldsnap -> Forge -> Attack (Need exort!)
        private static void CastSnapDPS()
        {
            // Coldsnap
            if (Utils.SleepCheck("coldsnap") && coldsnap.CanBeCasted() && (Invoke.CanBeCasted() || HasSpell("invoker_cold_snap")))
            {
                GetSpell("invoker_cold_snap").UseAbility(target);
                Utils.Sleep(1000, "coldsnap");
            }

            // Forge
            if (Utils.SleepCheck("forge") && forge.CanBeCasted() && (Invoke.CanBeCasted() || HasSpell("invoker_forge_spirit")))
            {
                GetSpell("invoker_forge_spirit").UseAbility();
                Utils.Sleep(1000, "forge");
            }

            // Attack
            else if (me.CanAttack() && !target.IsInvul() && Utils.SleepCheck("attack"))
            {
                me.Attack(target);
                Utils.Sleep(150 + Game.Ping, "attack");
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
            }
        }

        private static Ability castSkill(string name)
        {
                switch (name)
                {
                    case "invoker_cold_snap":
                        Quas.UseAbility(); Quas.UseAbility(); Quas.UseAbility(); Invoke.UseAbility();
                        return me.FindSpell(name);
                    case "invoker_ghost_walk":
                        Quas.UseAbility(); Quas.UseAbility(); Wex.UseAbility(); Invoke.UseAbility();
                        return me.FindSpell(name);
                    case "invoker_forge_spirit":
                        Exort.UseAbility(); Exort.UseAbility(); Quas.UseAbility(); Invoke.UseAbility();
                        return me.FindSpell(name);
                    case "invoker_deafening_blast":
                        Quas.UseAbility(); Exort.UseAbility(); Wex.UseAbility(); Invoke.UseAbility();
                        return me.FindSpell(name);
                    case "invoker_alacrity":
                        Wex.UseAbility(); Wex.UseAbility(); Exort.UseAbility(); Invoke.UseAbility();
                        return me.FindSpell(name);
                    case "invoker_tornado":
                        Wex.UseAbility(); Wex.UseAbility(); Quas.UseAbility(); Invoke.UseAbility();
                        return me.FindSpell(name);
                    case "invoker_chaos_meteor":
                        Exort.UseAbility(); Exort.UseAbility(); Wex.UseAbility(); Invoke.UseAbility();
                        return me.FindSpell(name);
                    case "invoker_ice_wall":
                        Quas.UseAbility(); Quas.UseAbility(); Exort.UseAbility(); Invoke.UseAbility();
                        return me.FindSpell(name);
                    case "invoker_emp":
                        Wex.UseAbility(); Wex.UseAbility(); Wex.UseAbility(); Invoke.UseAbility();
                        return me.FindSpell(name);
                    case "invoker_sun_strike":
                        Exort.UseAbility(); Exort.UseAbility(); Exort.UseAbility(); Invoke.UseAbility();
                        return me.FindSpell(name);

            }
            return me.Spellbook.Spell4;
        }

        private static bool HasSpell(string name)
        {
            return (me.Spellbook.Spell4.Name == name) || (me.Spellbook.Spell5.Name == name);
        }

        private static Ability GetSpell(string name)
        {
            if (HasSpell(name))
            {
                if (me.Spellbook.Spell4.Name == name)
                {
                    return me.Spellbook.Spell4;
                }

                if (me.Spellbook.Spell5.Name == name)
                {
                    return me.Spellbook.Spell5;
                }
            }
            else
            {
                return castSkill(name);
            }
            return null;
        }
    }
}