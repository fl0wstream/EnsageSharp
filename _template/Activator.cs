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

        private static bool loaded;
        private static Hero me;

        #endregion

        #region Public Methods and Operators

        public static void Init()
        {
            Game.OnUpdate += Game_OnUpdate;
            loaded = false;
        }

        #endregion

        #region Methods

        private static void Game_OnUpdate(EventArgs args)
        {
        }

        #endregion
    }
}