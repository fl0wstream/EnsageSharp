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
        private static Key activeKey = Key.Space;
        private static bool toggle = true;
        private static bool active;
        private static Font _text;
        private static Line _line;

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

            Console.WriteLine("> Tinker# loaded!");

            _text = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Segoe UI",
                   Height = 17,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.ClearType
               });
       
            _line = new Line(Drawing.Direct3DDevice9);
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectMgr.LocalHero;

            if (me == null || !Game.IsInGame || Game.IsWatchingGame)
                return;

            if (me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;

            // Ability init
            if (Laser == null)
                Laser = me.Spellbook.Spell1;
            if (Rocket == null)
                Rocket = me.Spellbook.Spell2;
            if (Refresh == null)
                Refresh = me.Spellbook.Spell4;

            // Item init
            if (Blink == null)
                Blink = me.FindItem("item_blink");
            if (Dagon == null)
                Blink = me.GetDagon();
            if (Hex == null)
                Hex = me.FindItem("item_sheepstick");
            if (Soulring == null)
                Soulring = me.FindItem("item_soul_ring");
            if (Ethereal == null)
                Ethereal = me.FindItem("item_ethereal_blade");
            if (Veil == null)
                Veil = me.FindItem("item_veil_of_discord");

            // Main combo
            if (active && toggle)
            {
                target = me.ClosestToMouseTarget();

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
            _line.Dispose();
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
                return;

            var player = ObjectMgr.LocalPlayer;
            var me = ObjectMgr.LocalHero;
            if (player == null || player.Team == Team.Observer || me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
            DrawBox(2, 350, 310, 20, 1, new ColorBGRA(0, 0, 100, 100));
            DrawFilledBox(2, 350, 310, 20, new ColorBGRA(0, 0, 0, 100));
            if (toggle && !active)
            {
                DrawShadowText("Tinker#: Enabled | [" + toggleKey + "] for toggle | [" + activeKey + "] for combo", 4, 350, Color.LawnGreen, _text);
            }
            if (!toggle)
            {
                DrawShadowText("Tinker#: Disabled | [" + toggleKey + "] for toggle", 4, 350, Color.DarkGray, _text);
            }
            if (toggle && active)
            {
                DrawShadowText("Tinker#: Comboing!", 4, 350, Color.GreenYellow, _text);
            }
        }

        public static void DrawFilledBox(float x, float y, float w, float h, Color color)
        {
            var vLine = new Vector2[2];

            _line.GLLines = true;
            _line.Antialias = false;
            _line.Width = w;

            vLine[0].X = x + w / 2;
            vLine[0].Y = y;
            vLine[1].X = x + w / 2;
            vLine[1].Y = y + h;

            _line.Begin();
            _line.Draw(vLine, color);
            _line.End();
        }

        public static void DrawBox(float x, float y, float w, float h, float px, Color color)
        {
            DrawFilledBox(x, y + h, w, px, color);
            DrawFilledBox(x - px, y, px, h, color);
            DrawFilledBox(x, y - px, w, px, color);
            DrawFilledBox(x + w, y, px, h, color);
        }

        public static void DrawShadowText(string stext, int x, int y, Color color, Font f)
        {
            f.DrawText(null, stext, x + 1, y + 1, Color.Black);
            f.DrawText(null, stext, x, y, color);
        }

        static void Drawing_OnPostReset(EventArgs args)
        {
            _text.OnResetDevice();
            _line.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            _text.OnLostDevice();
            _line.OnLostDevice();
        }
    }
}
 
 
 