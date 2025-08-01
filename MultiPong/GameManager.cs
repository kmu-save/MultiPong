using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiPong
{
    internal class GameManager
    {
        public Size MapSize { get; set; }
        public Size StickSize { get; set; }
        public Size BallSize { get; set; }
        public int GoalMargin { get; set; }

        public Point Player1 { get; set; }
        public Point Player2 { get; set; }
        public Point Ball { get; set; }

        public bool WithPlayer1 { get; set; }
        public bool WithPlayer2 { get; set; }
        public bool WithBall { get; set; }

        private PictureBox? p1, p2, ball;

        public void Redraw(MainForm form)
        {
            form.ClientSize = MapSize;

            if (p1 == null)
            {
                p1 = new PictureBox
                {
                    BackColor = Color.Blue,
                    Size = StickSize,
                    Location = Player1,
                    Visible = WithPlayer1,
                };
                form.Controls.Add(p1);
            }
            else
            {
                p1.Location = Player1;
                p1.Visible = WithPlayer1;
            }

            if (p2 == null)
            {
                p2 = new PictureBox
                {
                    BackColor = Color.Red,
                    Size = StickSize,
                    Location = Player2,
                    Visible = WithPlayer2,
                };
                form.Controls.Add(p2);
            }
            else
            {
                p2.Location = Player2;
                p2.Visible = WithPlayer2;
            }

            if (ball == null)
            {
                ball = new PictureBox
                {
                    BackColor = Color.Green,
                    Size = BallSize,
                    Location = Ball,
                    Visible = WithBall,
                };
                form.Controls.Add(ball);
            }
            else
            {
                ball.Location = Ball;
                ball.Visible = WithBall;
            }
        }
    }
}
