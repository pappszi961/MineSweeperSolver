using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MineSweeper
{
    public class Solver
    {
        MinesweeperF.CellGrid cellGrid;
        MinesweeperF mainForm = MinesweeperF.Instance;

        const int FLAG = 9;
        const int UNREVEALED = 10;
        const int NOTMINE = 11;

        public int CellsLeft { get; set; } = 0;

        int x, y;
        public int[,] board { get; set; }

        public int Mines { get; set; }
        private bool gameSolved;

        //combi
        int caseIndicator = 0;
        Point firstPoint = Point.Empty;
        List<List<int[,]>> boardVariations = new List<List<int[,]>>();
        int rootIndex;
        List<Point> actualTestCase;
        bool possibleStep = true;
        bool wasBadMove = false;

        //variables for combinations
        int[] index;
        int k;
        int[] forward;
        List<int[]> res;
        int[] elems;

        //Variables for analyze
        List<List<int>> cellChances = new List<List<int>>();
        List<int> maxVariationsOfSeparatedArea = new List<int>();

        List<Point> mustBeFree = new List<Point>();
        List<Point> mustBeMine = new List<Point>();

        List<List<Point>> separated = new List<List<Point>>();


        List<List<Point>> cellsAroundSeparatedCells = new List<List<Point>>();

        public Solver() { }
        internal Solver(int x, int y, int mines, MinesweeperF.CellGrid cellg)
        {
            this.cellGrid = cellg;
            this.Mines = mines;
            board = new int[x, y];
            this.x = x;
            this.y = y;
            CellsLeft = x * y;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    board[i, j] = UNREVEALED;
                }
            }
        }

        internal void FirstStep()
        {
            Random rand = new Random();
            cellGrid.PerformStep(rand.Next(1, x - 1), rand.Next(1, y - 1));
        }

        internal void Start()
        {
            bool foundStep = true;
            while (true)
            {
                while (foundStep)
                {
                    foundStep = false;
                    for (int i = 0; i < x; i++)
                    {
                        for (int j = 0; j < y; j++)
                        {
                            if (board[i, j] != UNREVEALED && board[i, j] != FLAG && FreeCellsAround(i, j, board).Count != 0)
                            {
                                if (board[i, j] == MinesAround(i, j, board))
                                {
                                    ClickAllCellsAround(i, j);
                                    foundStep = true;
                                }
                                else
                                {
                                    if (FreeCellsAround(i, j, board).Count == board[i, j] - MinesAround(i, j, board))
                                    {
                                        FlagAllCellsAround(i, j);
                                        foundStep = true;
                                    }
                                }
                            }
                        }
                    }
                }

                //Only for test
                //WriteOutCurrentBoard();
                MainSolver(out foundStep);

                cellChances.Clear();
                maxVariationsOfSeparatedArea.Clear();

                if (Mines == CellsLeft)
                {
                    //Flag all cells left
                    HitAllRemainingCells(true);
                }

                if(Mines == 0)
                {
                    // Hit all remaining
                    HitAllRemainingCells(false);
                }

                //MessageBox.Show("" + Mines);
                if (!foundStep)
                {
                    cellGrid.AbortSolver();
                }
            }
        }

        //Test tool
        //internal void WriteOutCurrentBoard()
        //{
        //    //Implement
        //    using (StreamWriter writer = new StreamWriter("test.txt"))
        //    {
        //        writer.WriteLine(x + " " + y + " " + Mines);
        //        for (int i = 0; i < x; i++)
        //        {
        //            for (int j = 0; j < y; j++)
        //            {
        //                if (board[i, j] >= 0 && board[i, j] < 10)
        //                {
        //                    writer.WriteLine(i + " " + j + " " + board[i, j]);
        //                }
        //            }
        //        }
        //    }
        //}

        private void MainSolver(out bool foundStep)
        {
            foundStep = false;
            caseIndicator = 0;
            List<Point> borders = GetBorders();
            //if(borders.Count == 0)
            //{
            //    MessageBox.Show("üres");
            //}
            separated.Clear();
            Separate(borders);
            cellsAroundSeparatedCells.Clear();
            CellsAroundSeparated();
            SortCellsAroundSeparated();

            boardVariations.Clear();
            //Create all possible boards
            for (caseIndicator = 0; caseIndicator < cellsAroundSeparatedCells.Count; caseIndicator++)
            {
                rootIndex = 0;
                boardVariations.Add(new List<int[,]>());
                firstPoint = cellsAroundSeparatedCells[caseIndicator][0];
                actualTestCase = cellsAroundSeparatedCells[caseIndicator];
                int[,] test = board.Clone() as int[,];
                CreateAllPossibleSolution(cellsAroundSeparatedCells[caseIndicator][0], test);
            }

            //#region minecountshit
            ////Before guessing we need to do something with mine count

            ////We have 2 cases, first if there is only 1 problematic area
            ////and if there are more than one.

            //if (boardVariations.Count == 1)
            //{
            //    List<int[,]> temp = new List<int[,]>();
            //    // Only possible solutions will be the ones with the exact same mine count.
            //    foreach (int[,] boards in boardVariations[0])
            //    {
            //        int counter = 0;
            //        foreach (Point p in separated[0])
            //        {
            //            if (boards[p.X, p.Y] == FLAG)
            //            {
            //                counter++;
            //            }
            //        }
            //        if (counter == Mines)
            //        {
            //            temp.Add(boards);
            //        }
            //    }
            //    boardVariations[0] = temp;
            //}
            //if (boardVariations.Count > 1)
            //{
            //    //TODO
            //}
            //#endregion

            //We need the chances of a cell to be mine.
            GetCellChanceToBeMine();

            //Analyze the chances
            CellChanceAnalyzer();

            //If we didn't found anything we need to guess
            if (!(mustBeFree.Count == 0 && mustBeMine.Count == 0))
            {
                foundStep = true;
                //Perform the steps
                foreach (Point free in mustBeFree)
                {
                    cellGrid.PerformStep(free.X, free.Y);
                }
                foreach (Point mine in mustBeMine)
                {
                    mainForm.DecreaseMineCount();
                    cellGrid.Flag(mine.X, mine.Y);
                }
            }
            else
            {
                //Guess
                Point guessedPoint = Guess();
                cellGrid.PerformStep(guessedPoint.X, guessedPoint.Y);
                foundStep = true;
            }
            mustBeFree.Clear();
            mustBeMine.Clear();
        }

        private Point Guess()
        {
            Point bestChoice = Point.Empty;
            int min = 100; ;

            for (int caseIndicator = 0; caseIndicator < cellChances.Count; caseIndicator++)
            {
                for (int pointIndicator = 0; pointIndicator < cellChances[caseIndicator].Count; pointIndicator++)
                {
                    if (cellChances[caseIndicator][pointIndicator] < min)
                    {
                        min = cellChances[caseIndicator][pointIndicator];
                        bestChoice = separated[caseIndicator][pointIndicator];
                    }
                }
            }

            return bestChoice;
        }

        private void CellChanceAnalyzer()
        {
            for (int caseIndicator = 0; caseIndicator < cellChances.Count; caseIndicator++)
            {
                for (int pointIndicator = 0; pointIndicator < cellChances[caseIndicator].Count; pointIndicator++)
                {
                    //No chance
                    if (cellChances[caseIndicator][pointIndicator] == 0)
                    {
                        mustBeFree.Add(separated[caseIndicator][pointIndicator]);
                    }
                    //100% chance
                    if (cellChances[caseIndicator][pointIndicator] == maxVariationsOfSeparatedArea[caseIndicator])
                    {
                        mustBeMine.Add(separated[caseIndicator][pointIndicator]);
                    }
                }
            }

            //TODO Check mineCounter
        }

        private void GetCellChanceToBeMine()
        {
            //We need a rarity indicator for every empty cell
            foreach (List<Point> cases in separated)
            {
                List<int> pointsIndicators = new List<int>();
                foreach (Point p in cases)
                {
                    pointsIndicators.Add(0);
                }
                cellChances.Add(pointsIndicators);
            }

            int caseCounter = 0;
            //Now we need to fill it
            foreach (List<int[,]> cases in boardVariations)
            {
                foreach (int[,] variations in cases)
                {
                    foreach (Point p in separated[caseCounter])
                    {
                        if (variations[p.X, p.Y] == FLAG)
                        {
                            cellChances[caseCounter][separated[caseCounter].IndexOf(p)]++;
                            //MessageBox.Show(p + " ++");
                        }
                    }
                }
                maxVariationsOfSeparatedArea.Add(cases.Count);
                caseCounter++;
            }
        }

        private void SortCellsAroundSeparated()
        {
            List<List<Point>> temp1 = new List<List<Point>>();
            int i = 0;
            foreach (List<Point> cases in cellsAroundSeparatedCells)
            {
                List<Point> temp = new List<Point>(cases.Count);
                Point endPoint = Point.Empty;
                Point lastBorder = separated[i][separated[i].Count - 1];
                bool alreadyFound = false;

                //Need a last cell
                foreach (Point p in cases)
                {
                    if (p.X == lastBorder.X)
                    {
                        if (p.Y == lastBorder.Y - 1 || p.Y == lastBorder.Y + 1)
                        {
                            endPoint = p;
                            alreadyFound = true;
                            break;
                        }
                    }
                    else
                    {
                        if (p.Y == lastBorder.Y)
                        {
                            if (p.X == lastBorder.X - 1 || p.X == lastBorder.X + 1)
                            {
                                endPoint = p;
                                alreadyFound = true;
                                break;
                            }
                        }
                    }
                }

                if (!alreadyFound)
                {
                    foreach (Point p in cases)
                    {
                        if (p.X == lastBorder.X - 1 && p.Y == lastBorder.Y - 1)
                        {
                            endPoint = p;
                            break;
                        }

                        if (p.X == lastBorder.X - 1 && p.Y == lastBorder.Y + 1)
                        {
                            endPoint = p;
                            break;
                        }

                        if (p.X == lastBorder.X + 1 && p.Y == lastBorder.Y - 1)
                        {
                            endPoint = p;
                            break;
                        }

                        if (p.X == lastBorder.X + 1 && p.Y == lastBorder.Y + 1)
                        {
                            endPoint = p;
                            break;
                        }
                    }
                }

                temp.Add(endPoint);
                cases.Remove(endPoint);

                //Create the list

                while (cases.Count != 0)
                {
                    bool found = false;
                    int range = 1;
                    while (!found)
                    {
                        foreach (Point p in cases)
                        {
                            if (p.X == temp[temp.Count - 1].X)
                            {
                                if (p.Y == temp[temp.Count - 1].Y - range || p.Y == temp[temp.Count - 1].Y + range)
                                {
                                    found = true;
                                    temp.Add(p);
                                    break;
                                }
                            }
                            else
                            {
                                if (p.Y == temp[temp.Count - 1].Y)
                                {
                                    if (p.X == temp[temp.Count - 1].X - range || p.X == temp[temp.Count - 1].X + range)
                                    {
                                        found = true;
                                        temp.Add(p);
                                        break;
                                    }
                                }
                            }
                        }

                        if (!found)
                        {
                            foreach (Point p in cases)
                            {
                                if (p.X == temp[temp.Count - 1].X - range && p.Y == temp[temp.Count - 1].Y - range)
                                {
                                    found = true;
                                    temp.Add(p);
                                    break;
                                }

                                if (p.X == temp[temp.Count - 1].X - range && p.Y == temp[temp.Count - 1].Y + range)
                                {
                                    found = true;
                                    temp.Add(p);
                                    break;
                                }

                                if (p.X == temp[temp.Count - 1].X + range && p.Y == temp[temp.Count - 1].Y - range)
                                {
                                    found = true;
                                    temp.Add(p);
                                    break;
                                }

                                if (p.X == temp[temp.Count - 1].X + range && p.Y == temp[temp.Count - 1].Y + range)
                                {
                                    found = true;
                                    temp.Add(p);
                                    break;
                                }
                            }
                        }

                        if (!found)
                        {
                            if (range != 1)
                            {
                                for (int j = temp[temp.Count - 1].X - range; j < temp[temp.Count - 1].X + range; j++)
                                {
                                    if (found)
                                        break;
                                    for (int k = temp[temp.Count - 1].Y - range; k < temp[temp.Count - 1].Y + range; k++)
                                    {
                                        if (found)
                                            break;
                                        foreach (Point p in cases)
                                        {
                                            if (j == p.X && k == p.Y)
                                            {
                                                found = true;
                                                temp.Add(p);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        range++;
                    }

                    if (found)
                    {
                        cases.Remove(temp[temp.Count - 1]);
                    }
                }

                temp1.Add(temp);
                i++;
            }

            cellsAroundSeparatedCells.Clear();
            foreach (List<Point> cases in temp1)
            {
                cellsAroundSeparatedCells.Add(cases);
            }

        }

        internal void SetStartBtn(bool v)
        {
            mainForm.SetStartBtnClickable(v);
        }

        private void CellsAroundSeparated()
        {
            foreach (List<Point> pL in separated)
            {
                List<Point> temp = new List<Point>();
                foreach (Point p in pL)
                {
                    for (int i = p.X - 1; i <= p.X + 1; i++)
                    {
                        for (int j = p.Y - 1; j <= p.Y + 1; j++)
                        {
                            if (i >= 0 && i < x && j >= 0 && j < y)
                            {
                                if (board[i, j] > 0 && board[i, j] < 9)
                                {
                                    if (!temp.Contains(new Point(i, j)))
                                    {
                                        temp.Add(new Point(i, j));
                                    }
                                }
                            }
                        }
                    }
                }
                cellsAroundSeparatedCells.Add(temp);
            }
        }

        private void CreateAllPossibleSolution(Point p, int[,] testBoard)
        {
            if (p != firstPoint)
            {
                //First check if there is not too much mines around
                if (MinesAround(p.X, p.Y, boardVariations[caseIndicator][rootIndex]) > boardVariations[caseIndicator][rootIndex][p.X, p.Y])
                {
                    //Too much mines, the previous step was wrong
                    wasBadMove = true;
                    //We don't need a new board, so set it to false
                    possibleStep = false;
                    //MessageBox.Show("Previous move was bad, too much mine");
                    return;
                }

                //Then check if there is enough free cell left
                if (FreeCellsAround(p.X, p.Y, boardVariations[caseIndicator][rootIndex]).Count < (boardVariations[caseIndicator][rootIndex][p.X, p.Y] - MinesAround(p.X, p.Y, boardVariations[caseIndicator][rootIndex])))
                {
                    //Not enough free space, the previous step was wrong
                    wasBadMove = true;
                    //We don't need a new board, so set it to false
                    possibleStep = false;
                    //MessageBox.Show("Previous move was bad, not enough free space");
                    //MessageBox.Show("Free: " + FreeCellsAround(p.X, p.Y, boardVariations[rootIndex]).Count + "minesaround: " + MinesAround(p.X, p.Y, boardVariations[rootIndex]));
                    return;
                }

                //And lastly check if the cell is "completed"(Enough mines around)
                if (boardVariations[caseIndicator][rootIndex][p.X, p.Y] == MinesAround(p.X, p.Y, boardVariations[caseIndicator][rootIndex]))
                {
                    //MessageBox.Show("There are enough mines around");

                    foreach (Point free in FreeCellsAround(p.X, p.Y, boardVariations[caseIndicator][rootIndex]))
                    {
                        boardVariations[caseIndicator][rootIndex][free.X, free.Y] = NOTMINE;
                        //MessageBox.Show(free + " is not mine");
                    }

                    //Jump to the next then return ????????

                    if (actualTestCase.IndexOf(p) != actualTestCase.Count - 1)
                    {
                        //MessageBox.Show("Jump to the next element");
                        CreateAllPossibleSolution(actualTestCase[actualTestCase.IndexOf(p) + 1], boardVariations[caseIndicator][rootIndex]);
                    }

                    return;
                }
            }

            List<List<Point>> versions;
            List<Point> cellsForBackup;

            //When return from step above it will have filled board, that's not good for us.
            // So when this route has been called first we save the board.
            int[,] boardForBackup = testBoard.Clone() as int[,];

            int versionNumber = 0;

            //get the steps and create backup
            if (p == firstPoint)
            {
                versions = PossibleSteps(p, testBoard);
                cellsForBackup = FreeCellsAround(p.X, p.Y, testBoard);
            }
            else
            {
                versions = PossibleSteps(p, boardVariations[caseIndicator][rootIndex]);
                cellsForBackup = FreeCellsAround(p.X, p.Y, boardVariations[caseIndicator][rootIndex]);
            }
            //

            foreach (List<Point> points in versions)
            {

                if (p == firstPoint && versionNumber == 0)
                {
                    int[,] tempBoard = boardForBackup.Clone() as int[,];
                    //MessageBox.Show("p= " + p.ToString() + " tempBoard added");
                    boardVariations[caseIndicator].Add(tempBoard);
                }

                if ((versionNumber != 0 && p != firstPoint) || (p == firstPoint && versionNumber != 0))
                {
                    //This means it had a step in previous run
                    if (possibleStep)
                    {
                        int[,] tempBoard = boardForBackup.Clone() as int[,];
                        //MessageBox.Show("p= " + p.ToString() + " tempBoard added");
                        boardVariations[caseIndicator].Add(tempBoard);
                        rootIndex++;

                        // When versionNumber > 1 the newly saved board wont have the free cells.
                        foreach (Point backupCell in cellsForBackup)
                        {
                            boardVariations[caseIndicator][rootIndex][backupCell.X, backupCell.Y] = UNREVEALED;
                        }
                    }
                }

                boardVariations[caseIndicator][rootIndex] = boardForBackup.Clone() as int[,];

                possibleStep = true;
                string str = p + " version: " + versionNumber;
                //MessageBox.Show(str + " combinations count: " + versions.Count);

                //Check if the step is possible
                foreach (Point combination in points)
                {
                    if (boardVariations[caseIndicator][rootIndex][combination.X, combination.Y] != UNREVEALED)
                    {
                        possibleStep = false;
                    }
                }


                if (possibleStep)
                {
                    List<Point> frees = FreeCellsAround(p.X, p.Y, boardVariations[caseIndicator][rootIndex]);

                    // Flag the cells
                    foreach (Point combination in points)
                    {
                        boardVariations[caseIndicator][rootIndex][combination.X, combination.Y] = FLAG;
                        //MessageBox.Show(combination + " is mine");
                        if (frees.Contains(combination))
                            frees.Remove(combination);
                    }

                    //Sign NOTMINE cells

                    foreach (Point free in frees)
                    {
                        boardVariations[caseIndicator][rootIndex][free.X, free.Y] = NOTMINE;
                        //MessageBox.Show(free + " is not mine");
                    }

                    //If not last item go to next item
                    if (actualTestCase.IndexOf(p) != actualTestCase.Count - 1)
                    {
                        CreateAllPossibleSolution(actualTestCase[actualTestCase.IndexOf(p) + 1], boardVariations[caseIndicator][rootIndex]);
                    }
                }

                //Reverse previous step from backup
                if (wasBadMove)
                {
                    foreach (Point backupCell in cellsForBackup)
                    {
                        boardVariations[caseIndicator][rootIndex][backupCell.X, backupCell.Y] = UNREVEALED;
                    }

                    //If it was a whole bad route then delete board ???????????

                    if ((versionNumber + 1) == versions.Count)
                    {
                        //MessageBox.Show("Dead end, delete the board.");
                        boardVariations[caseIndicator].RemoveAt(rootIndex);
                        rootIndex--;
                        //When this happen the program will return to the previous cell,
                        // and because of the bad move the possible step is false,
                        //without it the cell can't create new board, so set it true.
                        possibleStep = true;
                    }

                    wasBadMove = false;
                }

                versionNumber++;
            }
        }

        private List<List<Point>> PossibleSteps(Point p, int[,] b)
        {
            List<Point> frees = FreeCellsAround(p.X, p.Y, b);
            int n = frees.Count;
            int k = b[p.X, p.Y] - MinesAround(p.X, p.Y, b);

            List<int[]> combinations = Combinations(n, k);
            List<List<Point>> res = new List<List<Point>>();

            foreach (int[] versions in combinations)
            {
                List<Point> version = new List<Point>();
                foreach (int index in versions)
                {
                    version.Add(frees[index]);
                }
                res.Add(version);
            }
            return res;
        }

        public List<int[]> Combinations(int n, int k)
        {
            index = new int[n];
            for (int i = 0; i < n; i++)
            {
                index[i] = i;
            }

            this.k = k;
            res = new List<int[]>();
            elems = new int[k];
            forward = new int[k];

            int maxSteps = n - k + 1;

            GetCombinations(0, maxSteps);

            return res;
        }

        private void GetCombinations(int c, int maxSteps)
        {
            for (int i = c + forward[c]; i < maxSteps; i++)
            {
                elems[c] = index[i];

                if (c < k - 1)
                    GetCombinations(c + 1, maxSteps + 1);
                else if (c == k - 1)
                {
                    int[] temp = new int[k];
                    elems.CopyTo(temp, 0);
                    res.Add(temp);
                }
            }

            if (c > 0)
            {
                forward[c - 1]++;
                for (int j = c; j < k; j++)
                    forward[j] = forward[c - 1];
            }
        }

        internal void SetCell(int i, int j, int val)
        {
            board[i, j] = val;
            if (CellsLeft == 0)
            {
                gameSolved = true;
                cellGrid.WonGame();
                cellGrid.AbortSolver();
            }
        }

        internal void CheckCell()
        {
            if (CellsLeft == 0)
            {
                gameSolved = true;
                cellGrid.WonGame();
                cellGrid.AbortSolver();
            }
        }

        private List<Point> FreeCellsAround(int posX, int posY, int[,] b)
        {
            List<Point> result = new List<Point>();
            for (int i = posX - 1; i < posX + 2; i++)
            {
                for (int j = posY - 1; j < posY + 2; j++)
                {
                    if (i >= 0 && i < x && j >= 0 && j < y)
                    {
                        if (b[i, j] == UNREVEALED)
                        {
                            result.Add(new Point(i, j));
                        }
                    }
                }
            }

            return result;
        }

        private int MinesAround(int posX, int posY, int[,] b)
        {
            int result = 0;
            for (int i = posX - 1; i < posX + 2; i++)
            {
                for (int j = posY - 1; j < posY + 2; j++)
                {
                    if (i >= 0 && i < x && j >= 0 && j < y)
                    {
                        if (b[i, j] == FLAG)
                        {
                            result++;
                        }
                    }
                }
            }

            return result;
        }

        private void ClickAllCellsAround(int posX, int posY)
        {
            for (int i = posX - 1; i < posX + 2; i++)
            {
                for (int j = posY - 1; j < posY + 2; j++)
                {
                    if (i >= 0 && i < x && j >= 0 && j < y)
                    {
                        if (board[i, j] == UNREVEALED)
                        {
                            cellGrid.PerformStep(i, j);
                        }
                    }
                }
            }
        }

        private void FlagAllCellsAround(int posX, int posY)
        {
            for (int i = posX - 1; i < posX + 2; i++)
            {
                for (int j = posY - 1; j < posY + 2; j++)
                {
                    if (i >= 0 && i < x && j >= 0 && j < y)
                    {
                        if (board[i, j] == UNREVEALED)
                        {
                            mainForm.DecreaseMineCount();
                            cellGrid.Flag(i, j);
                        }
                    }
                }
            }
        }

        private List<Point> GetBorders()
        {
            List<Point> borders = new List<Point>();

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (IsBorder(i, j))
                        borders.Add(new Point(i, j));
                }
            }

            return borders;
        }
        private void Separate(List<Point> border)
        {
            List<Point> borders = border;

            while (borders.Count != 0)
            {

                List<Point> listTemp = new List<Point>();

                listTemp.Add(borders[0]);
                borders.RemoveAt(0);

                bool found = false;
                bool foundLast = false;
                Point temp = new Point(-1, -1);
                int i = 0;

                while (!foundLast)
                {
                    while (!found && i < listTemp.Count)
                    {
                        foreach (Point b in borders)
                        {
                            if (listTemp[i].X >= b.X - 1 && listTemp[i].X <= b.X + 1)
                            {
                                if (listTemp[i].Y >= b.Y - 1 && listTemp[i].Y <= b.Y + 1)
                                {
                                    if (HasCellInCommon(listTemp[i], b))
                                    {
                                        temp = b;
                                        found = true;
                                        break;
                                    }
                                }
                            }
                        }
                        i++;
                    }
                    if (i == listTemp.Count && !found)
                    {
                        foundLast = true;
                    }
                    else
                    {
                        if (found)
                        {
                            listTemp.Add(temp);
                            borders.Remove(temp);
                            i = 0;
                            found = false;
                        }
                    }
                }

                separated.Add(listTemp);
            }
            //Random rnd = new Random();
            //foreach (List<Point> pL in separated)
            //{
            //    Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            //    foreach (Point p in pL)
            //    {
            //        cellGrid.ColorCell(randomColor, p.X, p.Y);
            //    }

            //    //MessageBox.Show("First in this section: " + pL[0].ToString());
            //    //MessageBox.Show("Last in this section: " + pL[pL.Count-1].ToString());

            //    randomColor = Color.Empty;
            //}

            //MessageBox.Show(separated.Count.ToString());
        }

        private bool HasCellInCommon(Point a, Point b)
        {
            //Position of the two cells could be vertical, horizontal, or 2types of diagonal

            //Check vertical

            if (a.X == b.X)
            {
                if (a.X - 1 >= 0)
                {
                    if (board[a.X - 1, a.Y] >= 1 && board[a.X - 1, a.Y] < 9)
                        return true;
                    if (board[b.X - 1, b.Y] >= 1 && board[b.X - 1, b.Y] < 9)
                        return true;
                }
                if (a.X + 1 < x)
                {
                    if (board[a.X + 1, a.Y] >= 1 && board[a.X + 1, a.Y] < 9)
                        return true;
                    if (board[b.X + 1, b.Y] >= 1 && board[b.X + 1, b.Y] < 9)
                        return true;
                }
            }
            else
            {
                //Check Horizontal
                if (a.Y == b.Y)
                {
                    if (a.Y - 1 >= 0)
                    {
                        if (board[a.X, a.Y - 1] >= 1 && board[a.X, a.Y - 1] < 9)
                            return true;
                        if (board[b.X, b.Y - 1] >= 1 && board[b.X, b.Y - 1] < 9)
                            return true;
                    }
                    if (a.Y + 1 < y)
                    {
                        if (board[a.X, a.Y + 1] >= 1 && board[a.X, a.Y + 1] < 9)
                            return true;
                        if (board[b.X, b.Y + 1] >= 1 && board[b.X, b.Y + 1] < 9)
                            return true;
                    }
                }
                else
                //diagonal
                {
                    //2 case
                    if (board[a.X, b.Y] >= 0 && board[a.X, b.Y] < 9)
                        return true;
                    if (board[b.X, a.Y] >= 0 && board[b.X, a.Y] < 9)
                        return true;
                }
            }

            return false;
        }

        private bool IsBorder(int posX, int posY)
        {
            //If have valued cell around then it's a border

            if (board[posX, posY] == UNREVEALED)
            {
                for (int i = posX - 1; i < posX + 2; i++)
                {
                    for (int j = posY - 1; j < posY + 2; j++)
                    {
                        if (i >= 0 && i < x && j >= 0 && j < y)
                        {
                            if (board[i, j] >= 1 && board[i, j] < 9)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void HitAllRemainingCells(bool flag)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if(board[i, j] == UNREVEALED)
                    {
                        if (flag)
                        {
                            cellGrid.Flag(i, j);
                        }
                        else
                        {
                            cellGrid.PerformStep(i, j);
                        }
                    }
                }
            }
        }
    }
}
