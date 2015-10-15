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
        private static Key toggleKey = Key.J;
        private static Key enableKey = Key.Space;
        private static bool toggle = true;
        private static bool active = true;
        private static Font _text;

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
            var me = ObjectMgr.LocalHero;
            if (me.ClassID != ClassID.CDOTA_Unit_Hero_Axe)
                return;

            // TODO:
            // Killsteal :)
          
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

            if (active && toggle)
            {
                _text.DrawText(null, "AxeBlinkUlti: Comboing!", 4, 150, Color.Green);
            }

            if (toggle && !active)
            {
                _text.DrawText(null, "AxeBlinkUlti: Enabled | [" + enableKey + "] for combo | [" + toggleKey + "] for toggle combo", 4, 150, Color.White);
            }
            if (!toggle)
            {
                _text.DrawText(null, "AxeBlinkUlti: Disabled | [" + toggleKey + "] for toggle", 4, 150, Color.WhiteSmoke);
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
 
 
 