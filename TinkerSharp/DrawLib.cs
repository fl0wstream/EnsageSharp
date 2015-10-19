using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;
using SharpDX.Direct3D9;

namespace DrawLib
{
    class Draw
    {
        public static Line _line;
        public static void Init()
        {
            _line = new Line(Drawing.Direct3DDevice9);

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

            Console.WriteLine("> DrawLib loaded!");
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

        public static void DrawPanel(float x, float y, float w, float h, float px, Color outline)
        {
            DrawBox(x, y, w, h, px, outline);
            DrawFilledBox(x, y, w, h, new ColorBGRA(0, 0, 0, 100));
        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            _line.Dispose();
        }

        static void Drawing_OnPostReset(EventArgs args)
        {
            _line.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            _line.OnLostDevice();
        }
    }
}
