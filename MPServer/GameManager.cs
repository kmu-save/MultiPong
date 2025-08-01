using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPServer
{
    internal class GameManager
    {
        public readonly Size MapSize = new Size(800, 600);
        public readonly Size StickSize = new Size(5, 200);
        public readonly Size BallSize = new Size(20, 20);
        public readonly int GoalMargin = 5;

        public Point Player1 { get; set; }
        public Point Player2 { get; set; }
        public Point Ball { get; set; }

        public bool WithPlayer1 { get; set; }
        public bool WithPlayer2 { get; set; }
        public bool WithBall { get; set; }

        public GameManager()
        {
            WithPlayer1 = false;
            WithPlayer2 = false;
            WithBall = false;
        }

        public void EnterPlayer1()
        {
            WithPlayer1 = true;

            Player1 = new Point(GoalMargin, (MapSize.Height / 2) - (StickSize.Height / 2));
        }

        public void EnterPlayer2()
        {
            WithPlayer2 = true;

            Player2 = new Point(MapSize.Width - GoalMargin - StickSize.Width, (MapSize.Height / 2) - (StickSize.Height / 2));
        }

        public void RequestSetPlayer1Location(int newY)
        {
            if (newY < 0 || newY + StickSize.Height > MapSize.Height)
            {
                return;
            }

            Player1 = new Point(Player1.X, newY);
        }

        public void RequestSetPlayer2Location(int newY)
        {
            if (newY < 0 || newY + StickSize.Height > MapSize.Height)
            {
                return;
            }

            Player2 = new Point(Player2.X, newY);
        }

        public string[] SyncInit()
        {
            string[] data =
            [
                $"{MapSize.Width}",
                $"{MapSize.Height}",
                $"{StickSize.Width}",
                $"{StickSize.Height}",
                $"{BallSize.Width}",
                $"{BallSize.Height}",
                $"{GoalMargin}",

                $"{Player1.Y}",
                $"{Player2.Y}",
                $"{Ball.X}",
                $"{Ball.Y}",
                $"{WithPlayer1}",
                $"{WithPlayer2}",
                $"{WithBall}"
            ];

            return data;
        }

        public string[] SyncUpdate()
        {
            string[] data =
            [
                $"{Player1.Y}",
                $"{Player2.Y}",
                $"{Ball.X}",
                $"{Ball.Y}",
                $"{WithPlayer1}",
                $"{WithPlayer2}",
                $"{WithBall}"
            ];

            return data;
        }
    }
}
