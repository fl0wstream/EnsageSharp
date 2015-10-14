namespace Activator
{
    using System;
    using System.Linq;
    using System.Windows.Input;

    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;

    using SharpDX;
    using SharpDX.Direct3D9;

    using unitDB = Ensage.Common.UnitDatabase;

    internal class Activator
    {
        #region Static Fields

        private static bool _loaded;
        private static Hero me;

        #endregion

        #region Public Methods and Operators

        public static void Init()
        {
            _loaded = false;

            Game.OnUpdate += Game_OnUpdate;
        }

        #endregion

        #region Methods

        private static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectMgr.LocalHero;

            if (!Game.IsInGame)
            {
                return;
            }

            if (Game.IsInGame)
            {
                _loaded = true;
            }
            
            var bottleBuff = me.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_item_bottle_active");
            if (!Utils.SleepCheck("item_bottle"))
            {
                return;
            }

            if (bottleBuff != null)
            {
                return;
            }
        }

        #endregion
    }
}