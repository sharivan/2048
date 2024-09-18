using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logic
{
    public class Logic2048
    {
        const float FOUR_ODD = 0.1F;

        public delegate void ResetEvent();
        public delegate void SpawnEvent(Entry entry, int row, int col);
        public delegate void MoveEvent(Entry entry, int srcRow, int srcCol, int dstRow, int dstCol);
        public delegate void FusionEvent(Entry entry, int srcRow, int srcCol, int dstRow, int dstCol);
        public delegate void MoveEndEvent(bool valid);
        public delegate void GameOverEvent();

        private readonly int rows;
        private readonly int cols;

        private readonly Random rng;
        private readonly Entry[,] entries;

        public event ResetEvent OnReset;
        public event SpawnEvent OnSpawn;
        public event MoveEvent OnMove;
        public event FusionEvent OnFusion;
        public event MoveEndEvent OnMoveEnd;
        public event GameOverEvent OnGameOver;

        public Logic2048() :
            this(4, 4)
        {
        }

        public Logic2048(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;

            rng = new Random();
            entries = new Entry[rows, cols];
        }

        private bool IsEmpty(int row, int col) => entries[row, col] == null;

        private bool IsFull()
        {
            for (int row = 0; row < rows; row++)
                for (int col = 0; col < cols; col++)
                    if (IsEmpty(row, col))
                        return false;

            return true;
        }

        private Entry SpawnNumber()
        {
            if (IsFull())
                return null;

            int value = rng.Next(100);
            if (value < (int) (100 * FOUR_ODD))
                value = 4;
            else
                value = 2;

            int row = -1;
            int col = -1;
            while (row == -1 || col == -1)
            {
                row = rng.Next(0, rows);
                col = rng.Next(0, cols);

                if (!IsEmpty(row, col))
                {
                    row = -1;
                    col = -1;
                }
            }

            var result = new Entry(row, col, value);
            entries[row, col] = result;

            OnSpawn(result, row, col);

            return result;
        }

        private bool IsValidMove(Direction direction) => false;

        private void Clear()
        {
            for (int row = 0; row < rows; row++)
                for (int col = 0; col < cols; col++)
                    entries[row, col] = null;
        }
        
        public void Reset()
        {
            Clear();

            OnReset();

            SpawnNumber();
            SpawnNumber();
        }

        public void Move(Direction direction)
        {
            switch (direction)
            {
                case Direction.LEFT:
                    MoveLeft();
                    break;

                case Direction.UP:
                    MoveUp();
                    break;

                case Direction.RIGHT:
                    MoveRight();
                    break;

                case Direction.DOWN:
                    MoveDown();
                    break;
            }
        }

        public void MoveLeft()
        {
            bool wasMoved = false;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 1;  col < cols; col++)
                {
                    Entry entry = entries[row, col];

                    if (entry != null)
                    {
                        int oldNumber = entry.Number;
                        int newNumber = oldNumber;
                        int srcCol = col;
                        int dstCol = col - 1;

                        while (true)
                        {
                            Entry nextEntry = entries[row, dstCol];
                            if (nextEntry != null)
                            {
                                if (nextEntry.Number != entry.Number)
                                    dstCol++;
                                else
                                    newNumber = 2 * oldNumber;

                                break;
                            }

                            dstCol--;
                            if (dstCol < 0)
                            {
                                dstCol = 0;
                                break;
                            }
                        }

                        if (srcCol != dstCol)
                        {
                            wasMoved = true;

                            entry = new Entry(row, dstCol, newNumber);
                            entries[row, dstCol] = entry;
                            entries[row, srcCol] = null;

                            if (oldNumber < newNumber)
                                OnFusion(entry, row, srcCol, row, dstCol);
                            else
                                OnMove(entry, row, srcCol, row, dstCol);
                        }
                    }
                }
            }

            OnMoveEnd(wasMoved);

            if (wasMoved)
            {
                Entry newEntry = SpawnNumber();
                if (newEntry == null)
                    OnGameOver();
            }
        }

        public void MoveUp()
        {
            bool wasMoved = false;

            for (int col = 0; col < cols; col++)
            {
                for (int row = 1; row < rows; row++)
                {
                    Entry entry = entries[row, col];

                    if (entry != null)
                    {
                        int oldNumber = entry.Number;
                        int newNumber = oldNumber;
                        int srcRow = row;
                        int dstRow = row - 1;

                        while (true)
                        {
                            Entry nextEntry = entries[dstRow, col];
                            if (nextEntry != null)
                            {
                                if (nextEntry.Number != entry.Number)
                                    dstRow++;
                                else
                                    newNumber = 2 * oldNumber;

                                break;
                            }

                            dstRow--;
                            if (dstRow < 0)
                            {
                                dstRow = 0;
                                break;
                            }
                        }

                        if (srcRow != dstRow)
                        {
                            wasMoved = true;

                            entry = new Entry(dstRow, col, newNumber);
                            entries[dstRow, col] = entry;
                            entries[srcRow, col] = null;

                            if (oldNumber < newNumber)
                                OnFusion(entry, srcRow, col, dstRow, col);
                            else
                                OnMove(entry, srcRow, col, dstRow, col);
                        }
                    }
                }
            }

            OnMoveEnd(wasMoved);

            if (wasMoved)
            {
                Entry newEntry = SpawnNumber();
                if (newEntry == null)
                    OnGameOver();
            }
        }

        public void MoveRight()
        {
            bool wasMoved = false;

            for (int row = 0; row < rows; row++)
            {
                for (int col = cols - 2; col >= 0; col--)
                {
                    Entry entry = entries[row, col];

                    if (entry != null)
                    {
                        int oldNumber = entry.Number;
                        int newNumber = oldNumber;
                        int srcCol = col;
                        int dstCol = col + 1;

                        while (true)
                        {
                            Entry nextEntry = entries[row, dstCol];
                            if (nextEntry != null)
                            {
                                if (nextEntry.Number != entry.Number)
                                    dstCol--;
                                else
                                    newNumber = 2 * oldNumber;

                                break;
                            }

                            dstCol++;
                            if (dstCol >= cols)
                            {
                                dstCol = cols - 1;
                                break;
                            }
                        }

                        if (srcCol != dstCol)
                        {
                            wasMoved = true;

                            entry = new Entry(row, dstCol, newNumber);
                            entries[row, dstCol] = entry;
                            entries[row, srcCol] = null;

                            if (oldNumber < newNumber)
                                OnFusion(entry, row, srcCol, row, dstCol);
                            else
                                OnMove(entry, row, srcCol, row, dstCol);
                        }
                    }
                }
            }

            OnMoveEnd(wasMoved);

            if (wasMoved)
            {
                Entry newEntry = SpawnNumber();
                if (newEntry == null)
                    OnGameOver();
            }
        }

        public void MoveDown()
        {
            bool wasMoved = false;

            for (int col = 0; col < cols; col++)
            {
                for (int row = rows - 2; row >= 0; row--)
                {
                    Entry entry = entries[row, col];

                    if (entry != null)
                    {
                        int oldNumber = entry.Number;
                        int newNumber = oldNumber;
                        int srcRow = row;
                        int dstRow = row + 1;

                        while (true)
                        {
                            Entry nextEntry = entries[dstRow, col];
                            if (nextEntry != null)
                            {
                                if (nextEntry.Number != entry.Number)
                                    dstRow--;
                                else
                                    newNumber = 2 * oldNumber;

                                break;
                            }

                            dstRow++;
                            if (dstRow >= cols)
                            {
                                dstRow = cols - 1;
                                break;
                            }
                        }

                        if (srcRow != dstRow)
                        {
                            wasMoved = true;

                            entry = new Entry(dstRow, col, newNumber);
                            entries[dstRow, col] = entry;
                            entries[srcRow, col] = null;

                            if (oldNumber < newNumber)
                                OnFusion(entry, srcRow, col, dstRow, col);
                            else
                                OnMove(entry, srcRow, col, dstRow, col);
                        }
                    }
                }
            }

            OnMoveEnd(wasMoved);

            if (wasMoved)
            {
                Entry newEntry = SpawnNumber();
                if (newEntry == null)
                    OnGameOver();
            }
        }
    }
}
