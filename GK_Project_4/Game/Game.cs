using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_Project_4.Game
{
    class Game
    {
        Table table;
        Ball[] balls;

        public Game(Table table, Ball[] balls)
        {
            this.table = table;
            this.balls = balls;
        }

    }
}
