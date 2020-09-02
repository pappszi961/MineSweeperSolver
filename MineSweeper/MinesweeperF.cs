using MineSweeper.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MineSweeper
{
    public partial class MinesweeperF : Form
    {
        private MinesweeperF()
        {
            InitializeComponent();
        }

        public static MinesweeperF Instance { get { return Nested.instance; } }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly MinesweeperF instance = new MinesweeperF();
        }


        public enum Difficulty { Expert, Intermediate, Beginner }

        internal const int MINE = 9;
        private Difficulty difficulty = Difficulty.Beginner;
        public void setD(Difficulty d)
        {
            this.difficulty = d;
        }

        private void LoadGame(object sender, EventArgs e)
        {
            startBtn.Enabled = false;
            int x, y, mines;
            //Sets boardsize, and mine count
            switch (this.difficulty)
            {
                case Difficulty.Beginner:
                    x = y = 9;
                    mines = 10;
                    break;
                case Difficulty.Intermediate:
                    x = y = 16;
                    mines = 40;
                    break;
                case Difficulty.Expert:
                    x = 30;
                    y = 16;
                    mines = 99;
                    break;
                default:
                    throw new InvalidOperationException("invalid difficulty");
            }

            this.cellGrid.Controls.Clear();
            label1.Text = mines.ToString();
            this.cellGrid.LoadGrid(new Size(x, y), mines);

            this.MaximumSize = MinimumSize = new Size(this.cellGrid.Width + 36, this.cellGrid.Height + 98);
            this.label1.Location = new System.Drawing.Point(this.cellGrid.Width - 30, 18);
        }

        public void DecreaseMineCount()
        {
            if (this.label1.InvokeRequired)
            {
                this.label1.BeginInvoke((MethodInvoker)delegate () { this.label1.Text = (int.Parse(this.label1.Text) - 1).ToString(); ; });
            }
            else
            {
                this.label1.Text = (int.Parse(this.label1.Text) - 1).ToString();
            }
        }

        internal void SetStartBtnClickable(bool val)
        {
            if (this.startBtn.InvokeRequired)
            {
                this.startBtn.BeginInvoke((MethodInvoker)delegate () { this.startBtn.Enabled = val; ; });
            }
            else
            {
                this.startBtn.Enabled = val;
            }
        }

        public class CellGrid : Panel
        {
            const int FLAG = 9;
            private Solver solver;
            private Size gridSize;
            private int mines;
            private int flags;
            private bool opened;
            private int[,] board;
            Thread thread;

            private Bitmap bitmap_0 = new Bitmap(Resources._0);
            private Bitmap bitmap_1 = new Bitmap(Resources._1);
            private Bitmap bitmap_2 = new Bitmap(Resources._2);
            private Bitmap bitmap_3 = new Bitmap(Resources._3);
            private Bitmap bitmap_4 = new Bitmap(Resources._4);
            private Bitmap bitmap_5 = new Bitmap(Resources._5);
            private Bitmap bitmap_6 = new Bitmap(Resources._6);
            private Bitmap bitmap_7 = new Bitmap(Resources._7);
            private Bitmap bitmap_8 = new Bitmap(Resources._8);
            private Bitmap bitmap_mine = new Bitmap(Resources.bomb);

            // What to do when a cell is clicked.
            public void PerformStep(int x, int y)
            {
                Cell cell = cell = (Cell)this.Controls[$"Cell{x}_{y}"];
                if (!cell.flagged && !cell.revealed)
                {
                    if (!this.opened)
                    {
                        GenerateMines(x, y);
                    }
                    cell.revealed = true;
                    this.opened = true;
                    //MessageBox.Show(this.opened.ToString() + " " + this.flags + " " + this.mines);
                    switch (cell.Value)
                    {
                        case 0:
                            cell.Image = (Image)bitmap_0.Clone();
                            Nully(cell.Name);
                            break;
                        case 1:
                            cell.Image = (Image)bitmap_1.Clone();
                            break;
                        case 2:
                            cell.Image = (Image)bitmap_2.Clone();
                            break;
                        case 3:
                            cell.Image = (Image)bitmap_3.Clone();
                            break;
                        case 4:
                            cell.Image = (Image)bitmap_4.Clone();
                            break;
                        case 5:
                            cell.Image = (Image)bitmap_5.Clone();
                            break;
                        case 6:
                            cell.Image = (Image)bitmap_6.Clone();
                            break;
                        case 7:
                            cell.Image = (Image)bitmap_7.Clone();
                            break;
                        case 8:
                            cell.Image = (Image)bitmap_8.Clone();
                            break;
                        case MINE:
                            cell.Image = bitmap_mine;
                            GameOver();
                            AbortSolver();
                            break;
                    }
                    solver.CellsLeft--;
                    solver.board[x, y] = cell.Value;
                    solver.CheckCell();
                    //solver.SetCell(x, y, cell.Value);
                }
            }

            private void GameOver()
            {
                WriteStats(0);
            }

            public void WonGame()
            {
                WriteStats(1);
            }

            private void WriteStats(int gameResult)
            {
                int lineToWrite = 0;
                if (gridSize.Width == 9)
                {
                    lineToWrite = 1-gameResult;
                }
                if (gridSize.Width == 16)
                {
                    lineToWrite = 4 - gameResult;
                }
                if (gridSize.Width == 30)
                {
                    lineToWrite = 7 - gameResult;
                }

                string[] lines = File.ReadAllLines(@".\stats.txt");

                using (StreamWriter writer = new StreamWriter("stats.txt"))
                {
                    for (int currentLine = 0; currentLine < lines.Length; currentLine++)
                    {
                        if (currentLine == lineToWrite)
                        {
                            writer.WriteLine(((int.Parse(lines[currentLine])) + 1).ToString());
                        }
                        else
                        {
                            if (currentLine == lineToWrite + 1 + gameResult)
                            {
                                string[] str = lines[currentLine].Split(' ');
                                double a = Double.Parse(lines[currentLine - 2]);
                                double b = Double.Parse(lines[currentLine - 1]);

                                if(gameResult == 0)
                                {
                                    b += 1;
                                }
                                else
                                {
                                    a += 1;
                                }

                                double percentage = Math.Round(((a / (a + b)) * 100.0), 2);


                                str[1] = (int.Parse(lines[currentLine - 2]) + int.Parse(lines[currentLine - 1]) + 1).ToString();

                                str[3] = percentage + "";

                                writer.WriteLine(str[0] + " " + str[1] + " " + str[2] + " " + str[3] + " " + str[4]);
                            }
                            else
                            {
                                writer.WriteLine(lines[currentLine]);
                            }
                        }
                        
                    }
                }
            }

            internal void AbortSolver()
            {
                //MessageBox.Show(solver.CellsLeft + "");
                solver.SetStartBtn(true);
                thread.Abort();
                GC.Collect();
            }

            public void Flag(int x, int y)
            {
                Cell cell = cell = (Cell)this.Controls[$"Cell{x}_{y}"];
                if (!cell.revealed)
                {
                    cell.revealed = true;
                    cell.flagged = true;
                    cell.Image = Resources.Flag;
                    solver.Mines--;
                    solver.CellsLeft--;
                    solver.board[x, y] = FLAG;
                    solver.CheckCell();
                    //solver.SetCell(x, y, FLAG);
                }
            }

            //If the player hit an empty cell, it will open the board.
            private void Nully(string ogName)
            {
                string[] re = ogName.Remove(0, 4).Split('_');
                int x = Int32.Parse(re[0]);
                int y = Int32.Parse(re[1]);
                Cell cell;

                for (int i = x - 1; i < x + 2; i++)
                {
                    for (int j = y - 1; j < y + 2; j++)
                    {
                        if (!(i == x && j == y))
                        {
                            if (i >= 0 && i <= gridSize.Width - 1 && j >= 0 && j <= gridSize.Height - 1)
                            {
                                cell = (Cell)this.Controls[$"Cell{i}_{j}"];
                                if (!cell.revealed)
                                {
                                    PerformStep(i, j);
                                }
                            }
                        }
                    }
                }
            }
            //Generates the grid.
            internal void LoadGrid(Size gridSize, int mines)
            {
                GC.Collect();
                solver = new Solver(gridSize.Width, gridSize.Height, mines, this);
                this.opened = false;
                this.gridSize = gridSize;
                this.mines = this.flags = mines;
                board = new int[gridSize.Width, gridSize.Height];
                //Generator(mines, gridSize.Width, gridSize.Height);
                this.Controls.Clear();
                this.Size = new Size(gridSize.Width * Cell.LENGTH, gridSize.Height * Cell.LENGTH);
                for (int x = 0; x < gridSize.Width; x++)
                {
                    for (int y = 0; y < gridSize.Height; y++)
                    {
                        Cell cell = new Cell(x, y);
                        //cell.MouseDown += Cell_MD;
                        this.Controls.Add(cell);
                    }
                }
                solver.FirstStep();
                thread = new Thread(solver.Start);
                thread.Start();
            }

            //Generate the board.

            internal void GenerateMines(int x, int y)
            {
                Random rnd = new Random();
                int m = 0;
                int xtemp;
                int ytemp;

                while (m != this.mines)
                {
                    xtemp = rnd.Next(0, this.gridSize.Width);
                    ytemp = rnd.Next(0, this.gridSize.Height);

                    if (!((xtemp == x - 1 || xtemp == x || xtemp == x + 1) && (ytemp == y - 1 || ytemp == y || ytemp == y + 1)))
                    {
                        if(board[xtemp, ytemp] != MINE)
                        {
                            board[xtemp, ytemp] = MINE;
                            m++;
                        }
                    }
                }

                Generator();
            }
            internal void Generator()
            {
                int x = this.gridSize.Width;
                int y = this.gridSize.Height;
                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        if (board[i, j] != MINE)
                        {
                            board[i, j] = CalcMine(i, j, x, y);
                        }
                    }
                }
                GiveValues();
            }
            internal int CalcMine(int xp, int yp, int x, int y)
            {
                int counter = 0;
                for (int i = xp - 1; i < xp + 2; i++)
                {
                    for (int j = yp - 1; j < yp + 2; j++)
                    {
                        if (i >= 0 && j >= 0 && i < x && j < y)
                        {
                            if (board[i, j] == MINE)
                                counter++;
                        }
                    }
                }

                return counter;
            }
            internal void GiveValues()
            {
                string[] re;
                int x, y;
                foreach (Cell cell in this.Controls)
                {
                    re = cell.Name.Remove(0, 4).Split('_');
                    x = Int32.Parse(re[0]);
                    y = Int32.Parse(re[1]);

                    cell.Value = board[x, y];
                }
            }

            //Cell and its properties
            private class Cell : PictureBox
            {
                internal Boolean revealed;
                internal Boolean flagged;
                internal const int LENGTH = 25;
                internal int Value { get; set; }
                internal Point Pos { get; }
                internal Cell(int x, int y)
                {
                    this.Name = $"Cell{x}_{y}";
                    this.Location = new Point(x * LENGTH, y * LENGTH);
                    this.Pos = new Point(x, y);
                    this.Size = new Size(LENGTH, LENGTH);
                    this.Image = Resources.Cell;
                    this.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        //Difficulty selector
        private void Beginner_Click(object sender, EventArgs e)
        {
            setD(Difficulty.Beginner);
        }

        private void Intermediate_Click(object sender, EventArgs e)
        {
            setD(Difficulty.Intermediate);
        }

        private void Expert_Click(object sender, EventArgs e)
        {
            setD(Difficulty.Expert);
        }
    }
}