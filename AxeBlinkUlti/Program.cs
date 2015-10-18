using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;

using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;
using SharpDX.Direct3D9;

namespace AxeBlinkUlti
{
    internal class Program
    {
        private static Ability Chop;
        private static Item Blink;
        private static Hero me;
        private static Hero target;
        private static Key toggleKey = Key.J;
        private static bool toggle = true;
        private static Font _text;
        private static int[] _ChopDmg = new int[3];
        private static int minimumDistance = 400;

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Console.WriteLine("> AxeBlinkUlti loaded!");

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

            if (me.ClassID != ClassID.CDOTA_Unit_Hero_Axe)
                return;
            if (me == null)
                return;

            if (Blink == null)
                Blink = me.FindItem("item_blink");

            if (Chop == null)
                Chop = me.Spellbook.Spell4;

            if (me.HasItem(ClassID.CDOTA_Item_UltimateScepter))
            {
                _ChopDmg = new int[3] { 300, 425, 550 };
            }
            else
            {
                _ChopDmg = new int[3] { 250, 325, 400 };
            }

            var ChopDmg = _ChopDmg[Chop.Level - 1];

            if (toggle)
            {
                var enemies = ObjectMgr.GetEntities<Hero>().Where(x => x.IsVisible && x.IsAlive && !x.IsIllusion && x.Team != me.Team && x.Health <= ChopDmg).ToList();

                foreach (var hero in enemies)
                {
                    var distance = me.Distance2D(hero);
                    if (target == null || distance < minimumDistance)
                    {
                        target = hero;
                    }
                }

                if (target != null && Utils.SleepCheck("chop") && Chop.CanBeCasted() && me.IsAlive && target.IsAlive)
                {
                    if (Blink != null && me.Distance2D(target) > 400 && me.Distance2D(target) < Blink.CastRange && Utils.SleepCheck("blink") && Blink.CanBeCasted() && me.Health > 250)
                    {
                        Blink.UseAbility(target.Position);
                        Utils.Sleep(150 + Game.Ping, "blink");
                    }
                    else
                    {
                        if (target != null && Chop != null)
                        {
                            Chop.UseAbility(target);
                            target = null;
                            Utils.Sleep(150 + Game.Ping, "chop");
                        }
                    }
                }
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen)
            {
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
            if (player == null || player.Team == Team.Observer || me.ClassID != ClassID.CDOTA_Unit_Hero_Axe)
                return;

            if (toggle)
            {
                _text.DrawText(null, "AxeBlinkUlti: Enabled | [" + toggleKey + "] for toggle", 4, 180, Color.White);
            }
            else
            {
                _text.DrawText(null, "AxeBlinkUlti: Disabled | [" + toggleKey + "] for toggle", 4, 180, Color.WhiteSmoke);
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
 
 
 