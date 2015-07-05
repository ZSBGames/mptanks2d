﻿using MPTanks.Engine.Gamemodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Engine.Tanks
{
    public abstract class SuperTank : Tank
    {
        public SuperTank(GamePlayer player, GameCore game, bool authorized)
            : base(player, game, authorized)
        {

        }
    }
}