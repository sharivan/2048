using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logic
{
    public class Entry
    {
        public int Row { get; }

        public int Col { get; }

        public int Number { get; }

        public Entry() : this(0, -1, -1) { }

        public Entry(int number) : this(number, -1, -1) { }

        public Entry(int row, int col, int number)
        {
            this.Row = row;
            this.Col = col;
            this.Number = number;
        }
    }
}
