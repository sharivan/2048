using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logic
{
    public class Entry
    {
        private int row;
        private int col;
        private int number;

        public int Row
        {
            get
            {
                return row;
            }
        }

        public int Col
        {
            get
            {
                return col;
            }
        }

        public int Number
        {
            get
            {
                return number;
            }
        }

        public Entry() : this(0, -1, -1) { }

        public Entry(int number) : this(number, -1, -1) { }

        public Entry(int row, int col, int number)
        {
            this.row = row;
            this.col = col;
            this.number = number;
        }
    }
}
