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
        private static Item Blink, Dagon, Hex, Soulring, Ethereal, Veil;
        private static Hero me;
        private static Hero target;
        private static Key toggleKey = Key.J;
        private static Key activeKey = Key.X;
        private static bool toggle = true;
        private static bool active;
        private static Font _text;

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Console.WriteLine("> Tinker# loaded!");

            _text = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Segoe UI",
                   Height = 11,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.ClearType
               });
                Drawing.OnPreReset += Drawing_OnPreReset;
                Drawing.OnPostReset += Drawing_OnPostReset;
                Drawing.OnEndScene += Drawing_OnEndScene;
                AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectMgr.LocalHero;

            if (me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
            if (me == null)
                return;

            // Ability init
            if (Laser == null)
                Laser = me.Spellbook.Spell1;

            // Item init
            if (Blink == null)
                Blink = me.FindItem("item_blink");

            // Main combo
            if (active && toggle)
            {
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
            if (player == null || player.Team == Team.Observer || me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;

            if (toggle && !active)
            {
                _text.DrawText(null, "Tinker#: Enabled | [" + toggleKey + "] for toggle", 4, 180, Color.White);
            }
            if (!toggle)
            {
                _text.DrawText(null, "Tinker#: Disabled | [" + toggleKey + "] for toggle", 4, 180, Color.WhiteSmoke);
            }
            if (toggle && active)
            {
                _text.DrawText(null, "Tinker#: Comboing!", 4, 180, Color.GreenYellow);
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
 
 
 
