using System;
using System.Diagnostics;

namespace ALICheckers
{
    class Program
    {
        static void Main(string[] args)
        {
            Board b = new Board(8);
            Console.Write(b);
            Debug.Assert(b.IsMoveValid((2,1), (3,0)));
            Debug.Assert(!b.IsMoveValid((2,1), (3,1)));
            Debug.Assert(!b.IsMoveValid((2,3), (4,5)));
            Console.WriteLine("Tests passed");

            while(true) {
               var move = ReadMove(); 
               Console.WriteLine(b.MakeMove(move.start, move.end));
               Console.Write(b);
            }
        }

        static (int y, int x) ReadPos()
        {
            (int y, int x) pos;
            string[] inputArr = Console.ReadLine().Split();
            Int32.TryParse(inputArr[0], out pos.y);
            Int32.TryParse(inputArr[1], out pos.x);
            return pos;
        }

        static ((int y, int x) start, (int y, int x) end) ReadMove()
        {
            Console.Write("From: ");
            var start = ReadPos();
            Console.Write("To:   ");
            var end = ReadPos();
            return (start, end);
        }
    }
}
