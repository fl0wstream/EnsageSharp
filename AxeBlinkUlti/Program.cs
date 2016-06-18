using System;
using System.Linq;

using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common;
using Ensage.Common.Menu;

namespace AxeBlinkUlti
{
    internal class Program
    {
        private static Ability Chop;
        private static Item Blink;
        private static Hero me;
        private static Hero target;
        private static int[] _chopDmg = new int[3];
        private static int minimumDistance = 400;
        private static readonly Menu Menu = new Menu("AxeBlinkUlti", "axeblinkulti", true);




        static void Main(string[] args)
        {
            Menu.AddItem(new MenuItem("toggle", "Enabled").SetValue(true));
            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnUpdate;
            Console.WriteLine("> AxeBlinkUlti loaded!");
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectManager.LocalHero;
            
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
                _chopDmg = new int[3] { 300, 425, 550 };
            }
            else
            {
                _chopDmg = new int[3] { 250, 325, 400 };
            }

            var ChopDmg = _chopDmg[Chop.Level - 1];

            if (Menu.Item("toggle").GetValue<bool>())
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
    }
}
 
 
 
