using System;
using System.Diagnostics;

namespace ALICheckersLogic
{
    class Program
    {
        static void Main(string[] args)
        {
            Board b = new Board(8);
            bool cpuMove = false;
            bool cpuOnly = false;
            if (args.Length >= 1 && args[0] == "cpu")
                cpuOnly = true;

            while (!b.IsFinished())
            {
                Console.WriteLine("Playing: " + (cpuMove ? "White" : "Black"));
                Console.WriteLine(b);

                if (!cpuOnly && !cpuMove)
                {
                    Console.WriteLine("Moves: ");
                    foreach (var m in b.GetAllMoves())
                        Console.WriteLine(m);
                    var move = ReadMove();
                    b = b.NextState(move.start, move.end);
                }
                else
                {
                    var minmax = b.Minmax();
                    Console.WriteLine("Best score: " + minmax.bestScore);
                    b = minmax.bestChild;
                }
                cpuMove = !cpuMove;
            }
            Console.WriteLine(b);
        }

        static (int y, int x) ReadPos()
        {
            (int y, int x) pos;
            string[] inputArr = Console.ReadLine().Split();
            int.TryParse(inputArr[0], out pos.y);
            int.TryParse(inputArr[1], out pos.x);
            return pos;
        }

        static ((int y, int x) start, (int y, int x) end) ReadMove()
        {
            Console.Write("From: ");
            var start = ReadPos();
            Console.Write("To: ");
            var end = ReadPos();
            return (start, end);
        }
    }
}
