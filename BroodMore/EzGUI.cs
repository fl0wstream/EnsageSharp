using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common;

using SharpDX;
using SharpDX.Direct3D9;

namespace EZGUI
{
    #region Help classes
    #region Drawer
    internal class Drawer
    {
        #region Fields
        private static Line line;
        private static Font font;
        private static EzGUI owner;
        #endregion

        public static void Init(EzGUI _owner)
        {
            owner = _owner;
            line = new Line(Drawing.Direct3DDevice9);
            font = new Font(
            Drawing.Direct3DDevice9,
            new FontDescription
            {
                FaceName = "Tahoma",
                Height = 15,
                OutputPrecision = FontPrecision.Outline,
                Quality = FontQuality.Proof
            });
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        #region Drawing

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Game.IsInGame) return;
            if (Game.IsKeyDown(0x2E)) { owner.isMoving = true; }
            else owner.isMoving = false;
            if (owner.isVisible) owner.Draw();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            Drawer.GetFont().OnResetDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            Drawer.GetFont().OnLostDevice();
        }

        #endregion

        #region Methods
        public static void DrawLine(float x1, float y1, float x2, float y2, float w, ColorBGRA Color)
        {
            Vector2[] vLine = new Vector2[2] { new Vector2(x1, y1), new Vector2(x2, y2) };

            line.GLLines = true;
            line.Antialias = true;
            line.Width = w;

            line.Begin();
            line.Draw(vLine, Color);
            line.End();

        }

        public static void DrawFilledBox(float x, float y, float w, float h, ColorBGRA Color)
        {
            Vector2[] vLine = new Vector2[2];

            line.GLLines = true;
            line.Antialias = false;
            line.Width = w;

            vLine[0].X = x + w / 2;
            vLine[0].Y = y;
            vLine[1].X = x + w / 2;
            vLine[1].Y = y + h;

            line.Begin();
            line.Draw(vLine, Color);
            line.End();
        }

        public static void DrawBox(float x, float y, float w, float h, float px, ColorBGRA Color)
        {
            DrawLine(x, y, x + w, y, 1, new ColorBGRA(27, 31, 28, 166));
            DrawLine(x, y, x, y + h, 1, new ColorBGRA(27, 31, 28, 166));
            DrawLine(x, y + h, x + w, y + h, 1, new ColorBGRA(27, 31, 28, 166));
            DrawLine(x + w, y, x + w, y + h, 1, new ColorBGRA(27, 31, 28, 166));
            DrawFilledBox(x, y, w, h, Color);
            Color.A = 166;
            DrawFilledBox(x, y + h, w, px, Color);
            DrawFilledBox(x - px, y, px, h, Color);
            DrawFilledBox(x + w, y, px, h, Color);
            Color.A = 177;
            DrawFilledBox(x, y - px - 10, w, px + 10, Color);
        }

        #region DrawText
        public static void DrawShadowText(string text, float x, float y, ColorBGRA color)
        {
            font.DrawText(null, text, (int)x, (int)y, color);
        }
        #endregion

        public static Font GetFont()
        {
            return font;
        }
        #endregion
    }
    #endregion
    #region Other
    public enum ElementType
    {
        CHECKBOX, TEXT, CATEGORY
    }
    public class EzElement
    {
        public ElementType Type = ElementType.TEXT;
        private List<EzElement> Inside = new List<EzElement>();
        public string Content = "";
        public bool isActive = false;
        public Entity Attached = null;
        public string Data = null;
        public float[] Position = new float[4] { 0, 0, 0, 0 };
        public List<EzElement> GetElements()
        {
            return Inside;
        }
        public void AddElement(EzElement element)
        {
            Inside.Add(element);
        }
        public EzElement(ElementType _Type, string _Content, bool _Active)
        {
            Type = _Type;
            Content = _Content;
            isActive = _Active;
        }
    }
    #endregion
    #endregion
    public class EzGUI
    {
        #region Fields
        private float x = 0;
        private float y = 0;
        private float w = 300;
        private float h = 250;
        private string title = "EzGUI";

        public bool isMoving = false;
        public bool isVisible = true;

        private int cachedCount = 0;

        public EzElement Main;
        #endregion

        public EzGUI(float _x, float _y, string _title)
        {
            Main = new EzElement(ElementType.CATEGORY, "MAIN_CAT", true);
            x = _x;
            y = _y;
            title = _title;
            Drawer.Init(this);
            Game.OnWndProc += Game_OnWndProc;
        }

        #region GameAPI
        void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsInGame)
            {
                switch (args.Msg)
                {
                    case (uint)Utils.WindowsMessages.WM_KEYDOWN:
                        switch (args.WParam)
                        {
                            case 0x24:
                                isVisible = !isVisible;
                                break;
                        }
                        break;
                    case (uint)Utils.WindowsMessages.WM_LBUTTONDOWN:
                        MouseClick(Main);
                        break;
                }
            }
        }
        #endregion

        #region Drawing
        public void Draw()
        {
            if (isMoving)
            {
                Vector2 mPos = Game.MouseScreenPosition;
                x = mPos.X;
                y = mPos.Y;
            }
            DrawBase();
            int i = 0;
            int n = 1;
            DrawElements(Main.GetElements(), ref n, ref i);
        }

        public void DrawElements(List<EzElement> category, ref int n, ref int i)
        {
            foreach (EzElement element in category)
            {
                i++;
                DrawElement(element, i, n);
                if (element.Type == ElementType.CATEGORY)
                {
                    if (element.isActive)
                    {
                        int n2 = n + 1;
                        DrawElements(element.GetElements(), ref n2, ref i);
                    }
                }
            }
        }

        public void DrawElement(EzElement element, int i, int incat)
        {
            byte alpha = 10;
            if (element.isActive) alpha = 60;
            //
            int xoffset = 5 * incat;
            int yoffset = 20;
            int width = 15;
            int height = 15;
            int textoffset = 10;
            int menuoffset = 18;
            //
            ColorBGRA color = new ColorBGRA(100, 255, 0, alpha);
            //
            element.Position = new float[4] { x + xoffset, x + xoffset + width, y + yoffset * i - menuoffset, y + yoffset * i };
            //
            if (MouseIn(element.Position)) color.R = 10;
            //
            switch (element.Type)
            {
                case ElementType.CATEGORY:
                    Drawer.DrawFilledBox(element.Position[0], element.Position[2], width, height, color);
                    Drawer.DrawShadowText("> " + element.Content, x + xoffset + menuoffset, element.Position[2], new ColorBGRA(199, 199, 199, 255));
                    break;
                case ElementType.CHECKBOX:
                    Drawer.DrawFilledBox(element.Position[0], element.Position[2], width, height, color);
                    Drawer.DrawShadowText(element.Content, x + xoffset + menuoffset, element.Position[2], new ColorBGRA(199, 199, 199, 255));
                    break;
                case ElementType.TEXT:
                    Drawer.DrawShadowText(element.Content, element.Position[0] + textoffset, element.Position[2], new ColorBGRA(199, 199, 199, 255));
                    break;
            }
        }

        public void DrawBase()
        {
            h = 5 + (Length() * 20);
            Drawer.DrawBox(x, y, w, h, 1, new ColorBGRA(0, 0, 0, 50));
            Drawer.DrawShadowText(title, x + 3, y - 15, new ColorBGRA(100, 255, 0, 255));
            Drawer.DrawShadowText("Created by Vick & Evervolv1337", x + w - 176, y + h - 15, Color.Red);
        }
        #endregion

        #region Methods

        public void SetPos(float _x, float _y)
        {
            x = _x;
            y = _y;
        }

        public void AddMainElement(EzElement en)
        {
            Main.GetElements().Add(en);
        }

        public void Count(EzElement cat, ref int i)
        {
            foreach (EzElement element in cat.GetElements())
            {
                i++;
                if (element.Type == ElementType.CATEGORY && element.isActive) Count(element, ref i);
            }
        }

        public int Length()
        {
            if (Utils.SleepCheck("ezmenu_count"))
            {
                int i = 0;
                Count(Main, ref i);
                cachedCount = i;
                Utils.Sleep(125, "ezmenu_count");
                return cachedCount;
            }
            else return cachedCount;
        }
        #endregion

        #region Events
        public bool MouseIn(float[] pos)
        {
            if (Game.MouseScreenPosition.X >= pos[0] && Game.MouseScreenPosition.X <= pos[1] && Game.MouseScreenPosition.Y >= pos[2] && Game.MouseScreenPosition.Y <= pos[3]) { return true; }
            else return false;
        }

        public void MouseClick(EzElement cat)
        {
            foreach (EzElement element in cat.GetElements())
            {
                bool mouseIn = MouseIn(element.Position);
                if (mouseIn) { element.isActive = !element.isActive; return; }
                if (element.Type == ElementType.CATEGORY)
                    if (element.isActive) MouseClick(element);
            }

        }
        #endregion
    }
}
