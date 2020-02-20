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


            //const Int32 BufferSize = 128;
            //string[] str;
            //int testX, testY, testMines;
            //using (var fileStream = File.OpenRead(@".\test.txt"))
            //{
            //    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            //    {
            //        String line = streamReader.ReadLine();
            //        str = line.Split(' ');
            //        testX = int.Parse(str[0]);
            //        testY = int.Parse(str[1]);
            //        testMines = int.Parse(str[2]);
            //    }
            //}
            //label1.Text = testMines.ToString();
            //this.cellGrid.TestGrid(new Size(testX, testY), testMines);


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
                            cell.Image = Resources._0;
                            Nully(cell.Name);
                            break;
                        case 1:
                            cell.Image = Resources._1;
                            break;
                        case 2:
                            cell.Image = Resources._2;
                            break;
                        case 3:
                            cell.Image = Resources._3;
                            break;
                        case 4:
                            cell.Image = Resources._4;
                            break;
                        case 5:
                            cell.Image = Resources._5;
                            break;
                        case 6:
                            cell.Image = Resources._6;
                            break;
                        case 7:
                            cell.Image = Resources._7;
                            break;
                        case 8:
                            cell.Image = Resources._8;
                            break;
                        case MINE:
                            cell.Image = Resources.bomb;
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
                int lineToWrite = 1;
                if (gridSize.Width == 9)
                {
                    lineToWrite = 1;
                }
                if (gridSize.Width == 16)
                {
                    lineToWrite = 4;
                }
                if (gridSize.Width == 30)
                {
                    lineToWrite = 7;
                }

                string[] lines = File.ReadAllLines(@".\stats.txt");

                using (StreamWriter writer = new StreamWriter("stats.txt"))
                {
                    for (int currentLine = 0; currentLine < lines.Length; currentLine++)
                    {
                        if (currentLine == lineToWrite)
                        {
                            writer.WriteLine(((double.Parse(lines[currentLine])) + 1.0).ToString());
                        }
                        else
                        {
                            if (currentLine == lineToWrite + 1)
                            {
                                string[] str = lines[currentLine].Split(' ');
                                double a = Double.Parse(lines[currentLine - 2]);
                                double b = Double.Parse(lines[currentLine - 1]);
                                double res = (a / (a + b)) * 100.0;

                                str[1] = (int.Parse(lines[currentLine - 2]) + int.Parse(lines[currentLine - 1]) + 1).ToString();

                                str[3] = res + "";

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

            public void WonGame()
            {
                int lineToWrite = 0;
                if (gridSize.Width == 9)
                {
                    lineToWrite = 0;
                }
                if (gridSize.Width == 16)
                {
                    lineToWrite = 3;
                }
                if (gridSize.Width == 30)
                {
                    lineToWrite = 6;
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
                            if (currentLine == lineToWrite + 2)
                            {
                                string[] str = lines[currentLine].Split(' ');
                                double a = Double.Parse(lines[currentLine - 2]);
                                double b = Double.Parse(lines[currentLine - 1]);
                                double res = (a / (a + b)) * 100.0;
                                res = Math.Round(res, 2);

                                str[1] = (int.Parse(lines[currentLine - 2]) + int.Parse(lines[currentLine - 1]) + 1).ToString();

                                str[3] = res + "";

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

            //internal void DrawCombinations(int[,] variations, string filepath)
            //{
            //    Image img = new Bitmap(gridSize.Width * 25, gridSize.Height * 25);
            //    Graphics g;
            //    for (int i = 0; i < gridSize.Width; i++)
            //    {
            //        for (int j = 0; j < gridSize.Height; j++)
            //        {
            //            Image cellImage = new Bitmap(25, 25);
            //            switch (variations[i, j])
            //            {
            //                case 10:
            //                    cellImage = new Bitmap(Resources.Cell);
            //                    break;
            //                case 11:
            //                    cellImage = new Bitmap(Resources.Cell);
            //                    break;
            //                case 0:
            //                    cellImage = new Bitmap(Resources._0);
            //                    break;
            //                case 1:
            //                    cellImage = new Bitmap(Resources._1);
            //                    break;
            //                case 2:
            //                    cellImage = new Bitmap(Resources._2);
            //                    break;
            //                case 3:
            //                    cellImage = new Bitmap(Resources._3);
            //                    break;
            //                case 4:
            //                    cellImage = new Bitmap(Resources._4);
            //                    break;
            //                case 5:
            //                    cellImage = new Bitmap(Resources._5);
            //                    break;
            //                case 6:
            //                    cellImage = new Bitmap(Resources._6);
            //                    break;
            //                case 7:
            //                    cellImage = new Bitmap(Resources._7);
            //                    break;
            //                case 8:
            //                    cellImage = new Bitmap(Resources._8);
            //                    break;
            //                case 9:
            //                    cellImage = new Bitmap(Resources.Flag);
            //                    break;
            //            };
            //            Rectangle rect = new Rectangle(i * 25, j * 25, 25, 25);
            //            g = Graphics.FromImage(img);
            //            g.DrawImage(cellImage, rect);
            //        }
            //    }
            //    img.Save(filepath + ".jpg");
            //}

            //public void ColorCell(Color color, int x, int y)
            //{
            //    Cell cell = cell = (Cell)this.Controls[$"Cell{x}_{y}"];
            //    Image img = new Bitmap(200, 200);
            //    cell.Image = img;
            //    Graphics g;
            //    Rectangle rect = new Rectangle(0, 0, 200, 200);
            //    g = Graphics.FromImage(img);
            //    SolidBrush brush = new SolidBrush(color);
            //    g.FillRectangle(brush, rect);
            //}

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

            internal void TestGrid(Size gridSize, int mines)
            {
                GC.Collect();
                solver = new Solver(gridSize.Width, gridSize.Height, mines, this);
                this.opened = true;
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
                        this.Controls.Add(cell);
                    }
                }
                //Fill the board
                Cell cells;
                const Int32 BufferSize = 128;
                using (var fileStream = File.OpenRead(@".\test.txt"))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int i = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (i != 0)
                            {
                                string[] str = line.Split(' ');
                                int testX = int.Parse(str[0]);
                                int testY = int.Parse(str[1]);
                                int value = int.Parse(str[2]);
                                cells = (Cell)this.Controls[$"Cell{str[0]}_{str[1]}"];
                                cells.Value = value;
                                if (value == FLAG)
                                    Flag(testX, testY);
                                if (value >= 0 && value < 9)
                                    PerformStep(testX, testY);
                            }
                            i++;
                        }

                        i = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (i != 0)
                            {
                                string[] str = line.Split(' ');
                                int testX = int.Parse(str[0]);
                                int testY = int.Parse(str[1]);
                                int value = int.Parse(str[2]);
                                if (value == FLAG)
                                    Flag(testX, testY);
                                if (value >= 0 && value < 9)
                                    PerformStep(testX, testY);
                            }
                            i++;
                        }


                    }
                }

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