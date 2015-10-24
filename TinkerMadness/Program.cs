using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Ensage;
using Ensage.Common.Extensions;
using SharpDX;
namespace TinkerMadness
{
    class Program
    {
        const int WM_KEYUP = 0x0101;
        const int WM_KEYDOWN = 0x0105;
        private static Hero target;
        private static bool activated;


        private static float lastStack;
        private readonly static Timer Timer = new Timer();

        static void Main(string[] args)
        {
            Timer.Tick += Timer_Tick;
            Game.OnUpdate += ComboTick;
            Game.OnWndProc += Game_OnWndProc;
        }

        static void Timer_Tick(object sender, EventArgs e)
        {
            Timer.Enabled = false;
        }

        /// <summary>
        /// Wait until we're ingame and picked a hero
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param> 

        private static void Game_OnWndProc(WndEventArgs args)
        {
            {

                var me = ObjectMgr.LocalHero;
                activated = me != null && me.ClassID == ClassID.CDOTA_Unit_Hero_Tinker;
                Console.WriteLine(activated ? "Got tinker" : "Got the wrong hero");
            }

                if (!Game.IsChatOpen)
            {
                if (Game.IsKeyDown(System.Windows.Input.Key.D))
                    activated = true;
                else
                    activated = false;
                //
                // disable
                if (target != null)
                {
                    target = null;
                    Console.WriteLine("Script disabled");
                    return;
                }
            }
        }
       
            static void ComboTick(EventArgs args)

        {
            if (!Game.IsKeyDown(Key.D))
            if (!activated || Timer.Enabled || target == null || !Game.IsInGame || Game.IsPaused)
                    return;

            var me = ObjectMgr.LocalHero;
            // Check if we still got a valid target and we're alive
            if (!target.IsValid || !target.IsAlive || !me.IsAlive || !target.IsVisible || target.UnitState.HasFlag(UnitState.MagicImmune))
                {
                    target = null;
                    Console.WriteLine("Target or tinker dead");
                    return;
                }

                if (!HasCombo())
                    return;
                // Fetch our spells
                var Q = me.Spellbook.SpellQ;
                var W = me.Spellbook.SpellW;
                var R = me.Spellbook.SpellR;

                target = me.ClosestToMouseTarget(1000);
            if (R.IsInAbilityPhase || R.IsChanneling)
                    return;

                // Fetch our combo items
                var dagon = me.FindItem("item_dagon");
                var blink = me.FindItem("item_blink");
                var ethereal = me.FindItem("item_ethereal_blade");
                var soulring = me.FindItem("item_soul_ring");
                var sheep = target.ClassID == ClassID.CDOTA_Unit_Hero_Tidehunter ? null : me.FindItem("item_sheepstick");

                // Test if we need our ulti to refresh cooldowns
                if ((sheep == null || sheep.Cooldown > 0) &&
                    ((sheep != null && R.Level < 3) || Q.Cooldown > 0 || (dagon != null && dagon.Cooldown > 0) ||
                     (ethereal != null && ethereal.Cooldown > 0))
                    && R.AbilityState == AbilityState.Ready)
                {
                    Timer.Start(1000 + Math.Ceiling(R.OverrideCastPoint * 1000));
                    R.UseAbility();
                    Console.WriteLine("Casting ult");
                    return;
                }

                // Check if target is too far away
                var minRange = long.MaxValue;
                if (Q.Level > 0)
                    minRange = Math.Min(minRange, Q.CastRange);
                if (dagon != null)
                    minRange = Math.Min(minRange, dagon.CastRange);
                if (ethereal != null)
                    minRange = Math.Min(minRange, ethereal.CastRange);

                var distance = GetDistance2D(me.Position, target.Position);
                var blinkRange = blink.AbilityData.FirstOrDefault(x => x.Name == "blink_range").Value;
                if (blinkRange + minRange < distance)
                {
                    // Target too far TODO: status text
                    Console.WriteLine("Target too far away");
                    return;
                }

                // Check if we need to blink to the enemy
                if (minRange < distance)
                {
                    // Need to blink
                    if (blink.Cooldown > 0 && R.AbilityState == AbilityState.Ready)
                    {
                        // Cast ulti because blink is on cooldown
                        Timer.Start(1000 + Math.Ceiling(R.OverrideCastPoint * 1000));
                        R.UseAbility();
                        Console.WriteLine("Casting ult");
                        return;
                    }
                    // Calculate blink position
                    Vector3 targetPosition;
                    if (distance > blinkRange)
                    {
                        targetPosition = target.Position - me.Position;
                        targetPosition /= targetPosition.Length();
                        targetPosition *= (distance - minRange * 0.5f);
                        targetPosition += me.Position;
                    }
                    else
                    {
                        targetPosition = target.Position;
                    }
                    if (GetDistance2D(me.Position, targetPosition) > (blinkRange - 100))
                        targetPosition = (targetPosition - me.Position) * (blinkRange - 100) /
                                         GetDistance2D(targetPosition, me.Position) + me.Position;

                    var turn =
                        (Math.Max(
                            Math.Abs(FindAngleR(me) - DegreeToRadian(FindAngleBetween(me.Position, target.Position))) -
                            0.69f, 0) / (0.6f * (1 / 0.03f))) * 1000.0f;
                    // insert in queue
                    Timer.Start(Math.Ceiling(blink.OverrideCastPoint * 1000 + turn));
                    blink.UseAbility(targetPosition);
                    Console.WriteLine("blinking to enemy");
                    return;
                }
                var delay = 0.0;
                var casted = false;
                if (soulring != null && soulring.AbilityState == AbilityState.Ready)
                {
                    soulring.UseAbility();
                    casted = true;
                    delay += 100;
                }
                var linkens = target.Modifiers.Any(x => x.Name == "modifier_item_spheretarget") || target.Inventory.Items.Any(x => x.Name == "item_sphere");
                // if the enemy has linkens, we should break it first with dagon
                if (linkens && dagon != null && dagon.AbilityState == AbilityState.Ready)
                {
                    dagon.UseAbility(target, casted);
                    casted = true;
                    delay += Math.Ceiling(dagon.OverrideCastPoint * 1000);
                }
                if (sheep != null && sheep.AbilityState == AbilityState.Ready)
                {
                    sheep.UseAbility(target, casted);
                    casted = true;
                    delay += Math.Ceiling(sheep.OverrideCastPoint * 1000);
                }
                if (ethereal != null && ethereal.AbilityState == AbilityState.Ready)
                {
                    ethereal.UseAbility(target, casted);
                    casted = true;
                    delay += Math.Ceiling(ethereal.OverrideCastPoint * 1000);
                }
                if (!linkens && dagon != null && dagon.AbilityState == AbilityState.Ready)
                {
                    dagon.UseAbility(target, casted);
                    casted = true;
                    delay += Math.Ceiling(dagon.OverrideCastPoint * 1000);
                }
                if (W.Level > 0 && W.AbilityState == AbilityState.Ready)
                {
                    W.UseAbility(casted);
                    casted = true;
                    delay += Math.Ceiling(W.OverrideCastPoint * 1000);
                }
                if (Q.Level > 0 && Q.AbilityState == AbilityState.Ready)
                {
                    Q.UseAbility(target, casted);
                    casted = true;
                    delay += Math.Ceiling(Q.OverrideCastPoint * 1000);
                }
                if (casted)
                {
                    Timer.Start(delay);
                    Console.WriteLine("casting combo with total delay of: " + delay);
                }
            }

            static bool HasCombo()
        {
                var me = ObjectMgr.LocalHero;
                if (me.Spellbook.Spells.Last().Level == 0)
                    return false;

                // item_blink, item_sheepstick
                var items = me.Inventory.Items.ToList();
                return items.Any(x => x.Name == "item_blink") && items.Any(x => x.Name == "item_sheepstick");
            }




            static float FindAngleR(Entity ent)
        {
                return (float)(ent.RotationRad < 0 ? Math.Abs(ent.RotationRad) : 2 * Math.PI - ent.RotationRad);
            }

            static float FindAngleBetween(Vector3 first, Vector3 second)
            {
        var xAngle = (float)(Math.Atan(Math.Abs(second.X - first.X) / Math.Abs(second.Y - first.Y)) * (180.0 / Math.PI));
        if (first.X <= second.X && first.Y >= second.Y)
            return 90 - xAngle;
        if (first.X >= second.X && first.Y >= second.Y)
            return xAngle + 90;
        if (first.X >= second.X && first.Y <= second.Y)
            return 90 - xAngle + 180;
        if (first.X <= second.X && first.Y <= second.Y)
            return xAngle + 90 + 180;
        return 0;
    }

        static float GetDistance2D(Vector3 p1, Vector3 p2)
        {
    return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
}

        static double DegreeToRadian(double angle)
        {
                return Math.PI * angle / 180.0;
            }
        }
}
