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
            Console.WriteLine("Tests passed");
        }
    }
}
