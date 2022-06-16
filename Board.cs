using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ALICheckersLogic
{
    public enum MoveType
    {
        Invalid,
        Normal,
        Capture
    }

    public class Board
    {
        Piece[,] board;
        int size;

        // Maybe rename enum to Player
        public Color playing;

        // For the midstates where we're only allowed to continue capturing
        bool captureMode = false;
        public ((int y, int x) start, (int y, int x) end) lastMove = ((-1, -1), (-1, -1));
        public Board prevBoard = null;

        const int PieceRowCount = 3;
        const string PositionChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        static Dictionary<string, ((int y, int x) start, (int y, int x) end)> cache = new Dictionary<string, ((int y, int x) start, (int y, int x) end)>();
        public bool useCache = true;

        private static Random rng = new Random();

        public string LastMoveString
        {
            get
            {
                if (lastMove == ((-1, -1), (-1, -1)))
                    return "Start";
                else
                    return $"{PositionChars[lastMove.start.x]}{lastMove.start.y} -> {PositionChars[lastMove.end.x]}{lastMove.end.y}";
            }
        }

        public Piece this[int y, int x]
        {
            get { return board[y, x]; }
            // set { board[y, x] = value; }
        }

        public Board(int size)
        {
            this.board = new Piece[size, size];
            this.size = size;
            this.playing = Color.Black;
            InitializeBoard();
        }

        public Board(Board other)
        {
            this.size = other.size;
            this.board = new Piece[size, size];
            this.playing = other.playing;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    this.board[y, x] = other.board[y, x];
                }
            }
        }

        private void InitializeBoard()
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Valid squares
                    if ((y + x) % 2 == 1)
                    {
                        // White piece rows
                        if (y < PieceRowCount)
                        {
                            board[y, x] = Piece.WhitePawn;
                        }
                        // Black piece rows
                        else if (size - y <= PieceRowCount)
                        {
                            board[y, x] = Piece.BlackPawn;
                        }
                        // Empty rows
                        else
                        {
                            board[y, x] = Piece.Empty;
                        }
                    }
                    // Non-valid squares
                    else
                    {
                        board[y, x] = Piece.Blocked;
                    }
                }
            }
        }

        public Board Clone()
        {
            return new Board(this);
        }

        public Piece GetPiece((int y, int x) position)
        {
            return board[position.y, position.x];
        }

        public MoveType GetMoveType((int y, int x) delta)
        {
            (int y, int x) deltaAbs = (Math.Abs(delta.y), Math.Abs(delta.x));
            if (deltaAbs.y == 1 && deltaAbs.x == 1)
            {
                return MoveType.Normal;
            }
            else if (deltaAbs.y == 2 && deltaAbs.y == 2)
            {
                return MoveType.Capture;
            }
            else
            {
                return MoveType.Invalid;
            }
        }

        public MoveType GetMoveType(((int y, int x) start, (int y, int x) end) move)
        {
            return GetMoveType((move.end.y - move.start.y, move.end.x - move.end.y));
        }

        // Probably better to keep a counter rather than scaning every time
        public int GetScore()
        {
            // Piece count
            int blackPieceScore = 0;
            int whitePieceScore = 0;
            // How close they are to promoting pawns
            int blackPositionScore = 0;
            int whitePositionScore = 0;

            int boardHalf = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    switch (GetPiece((y, x)))
                    {
                        case Piece.BlackPawn:
                            blackPieceScore += 3;
                            // Reward pawns for being closer to becoming a king
                            blackPositionScore += size - y - 1;
                            break;
                        case Piece.BlackKing:
                            blackPieceScore += 5;
                            blackPositionScore += (y <= boardHalf ? y : y - 1 - boardHalf) * 2;
                            break;
                        case Piece.WhitePawn:
                            whitePieceScore += 3;
                            whitePositionScore += y;
                            break;
                        case Piece.WhiteKing:
                            whitePieceScore += 5;
                            whitePositionScore += (y <= boardHalf ? y : y - 1 - boardHalf) * 2;
                            break;
                    }
                }
            }
            return (blackPieceScore - whitePieceScore) * 1000 +
                    (blackPositionScore - whitePositionScore) * 10 +
                    rng.Next(0, 10);
        }

        // Change later to return who won?
        public bool IsFinished()
        {
            if (GetAllMoves().Count() == 0)
                return true;
            else
                return false;
        }

        public bool IsMoveValid((int y, int x) start, (int y, int x) end, bool isHuman = false)
        {
            if (IsInBounds(start) && IsInBounds(end))
            {
                Piece endPiece = GetPiece(end);
                Piece startPiece = GetPiece(start);
                if (endPiece == Piece.Empty && startPiece.GetColor() == playing)
                {
                    (int y, int x) delta = (end.y - start.y, end.x - start.x);

                    if (startPiece.IsKing() || startPiece.IsPawn() && startPiece.IsValidDirection(delta.y))
                    {
                        MoveType type = GetMoveType(delta);

                        // Regular move
                        if (type == MoveType.Normal)
                        {
                            if (isHuman)
                            {
                                var pieceCaptureMoves = GetAllMovesByType(MoveType.Capture).ToList();
                                if (pieceCaptureMoves.Count() == 0)
                                    return true;
                                else
                                    return false;
                            }
                            else return true;
                        }
                        // Capture move
                        else if (type == MoveType.Capture)
                        {
                            Piece capturedPiece = GetPiece((start.y + delta.y / 2, start.x + delta.x / 2));
                            if (capturedPiece.IsPiece() && startPiece.GetColor() != capturedPiece.GetColor())
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool MakeMove((int y, int x) start, (int y, int x) end, bool isHuman = false)
        {
            if (IsMoveValid(start, end, isHuman))
            {
                (int y, int x) delta = (end.y - start.y, end.x - start.x);
                MoveType type = GetMoveType(delta);

                // Remove captured piece if it was a capture
                if (type == MoveType.Capture)
                {
                    board[start.y + delta.y / 2, start.x + delta.x / 2] = Piece.Empty;
                    captureMode = true;
                }
                else
                {
                    // Switch to the opposing color
                    playing = playing == Color.Black ? Color.White : Color.Black;
                }

                Piece movingPiece = GetPiece(start);
                board[start.y, start.x] = Piece.Empty;

                // Handle promotion, or just move the piece to the destination
                if (end.y == 0 && movingPiece == Piece.BlackPawn)
                    board[end.y, end.x] = Piece.BlackKing;
                else if (end.y == size - 1 && movingPiece == Piece.WhitePawn)
                    board[end.y, end.x] = Piece.WhiteKing;
                else
                    board[end.y, end.x] = movingPiece;

                lastMove = (start, end);

                // Check if the capturing can continue or not
                if (captureMode)
                {
                    var pieceCaptureMoves = GetPieceMovesByType(lastMove.end, MoveType.Capture).ToList();
                    if (pieceCaptureMoves.Count() == 0)
                    {
                        captureMode = false;
                        playing = playing == Color.Black ? Color.White : Color.Black;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public Board NextState((int y, int x) start, (int y, int x) end, bool isHuman = false)
        {
            Board next = Clone();
            next.prevBoard = this;
            if (next.MakeMove(start, end, isHuman)) {
                return next;
            } else
                return null;
        }

        public bool IsInBounds((int y, int x) pos)
        {
            return pos.y >= 0 && pos.y < size &&
                   pos.x >= 0 && pos.x < size;
        }

        public IEnumerable<((int y, int x) start, (int y, int x) end)> GetPieceMovesByType((int y, int x) piecePos, MoveType type)
        {
            Piece piece = GetPiece(piecePos);
            if (piece.GetColor() == playing)
            {
                // Goes in both y directions for kings, only one otherwise
                for (int dy = -1; dy <= 1; dy += 2)
                {
                    if (!piece.IsValidDirection(dy))
                        continue;
                    // Both x directions for everything
                    for (int dx = -1; dx <= 1; dx += 2)
                    {
                        (int y, int x) normalMove = (piecePos.y + dy, piecePos.x + dx);
                        (int y, int x) captureMove = (piecePos.y + dy * 2, piecePos.x + dx * 2);

                        if (type == MoveType.Normal && IsMoveValid(piecePos, normalMove))
                            yield return (piecePos, normalMove);
                        if (type == MoveType.Capture && IsMoveValid(piecePos, captureMove))
                            yield return (piecePos, captureMove);
                    }
                }
            }
        }

        public IEnumerable<((int y, int x) start, (int y, int x) end)> GetAllMovesByType(MoveType type)
        {
            // Scan for the current player's pieces
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    foreach (var pieceMove in GetPieceMovesByType((y, x), type))
                    {
                        yield return pieceMove;
                    }
                }
            }
        }

        // Forces capture moves if they exist, otherwise shows normal moves
        public List<((int y, int x) start, (int y, int x) end)> GetAllMoves()
        {
            // Last move was a capture move, and they have more pieces they can capture
            if (captureMode)
            {
                return GetPieceMovesByType(lastMove.end, MoveType.Capture).ToList();
            }

            var captureMoves = GetAllMovesByType(MoveType.Capture).ToList();
            if (captureMoves.Count() != 0)
                return captureMoves;
            else
                return GetAllMovesByType(MoveType.Normal).ToList();
        }


        public (int bestScore, Board bestChild) Minmax(int depth = 9)
        {
            var boardStr = this.ToString();
            if (useCache && cache.ContainsKey(boardStr))
            {
                var move = cache[boardStr];
                var newBoard = this.NextState(move.start, move.end);
                return (newBoard.GetScore(), newBoard);
            }

            (int bestScore, Board bestChild) res;
            if (playing == Color.Black)
                res = Minmax(depth, -100000, +100000);
            else
                res = Minmax(depth, +100000, -100000);

            if (useCache)
                cache[boardStr] = res.bestChild.lastMove;

            return res;
        }

        private (int bestScore, Board bestChild) Minmax(int depth, int bestOverallScore, int limit)
        {
            var minOrMax = playing == Color.Black ? (Func<int, int, int>)Math.Max : Math.Min;
            var minOrMaxInv = playing == Color.Black ? (Func<int, int, int>)Math.Min : Math.Max;
            int worstScore = minOrMaxInv(100000, -100000);

            var allMoves = GetAllMoves();
            // In case of a loss don't go further.
            // Doesn't call IsFinished to avoid calling GetAllMoves twice
            // for no reason.
            if (allMoves.Count() == 0)
                return (worstScore, this);

            if (depth == 0)
            {
                return (GetScore(), this);
            }
            else
            {
                int bestScore = worstScore;
                Board bestChild = null;

                foreach (var move in allMoves)
                {
                    Board child = NextState(move.start, move.end);
                    var childMinmax = child.playing == playing ?
                            child.Minmax(depth - 1, bestOverallScore, limit) :
                            child.Minmax(depth - 1, limit, bestOverallScore);
                    if (minOrMax(childMinmax.bestScore, bestScore) == childMinmax.bestScore)
                    {
                        bestScore = childMinmax.bestScore;
                        bestChild = child;

                        if (minOrMax(bestScore, bestOverallScore) == bestScore)
                        {
                            bestOverallScore = bestScore;
                            if (minOrMax(bestOverallScore, limit) == bestOverallScore)
                                break;
                        }
                    }
                }

                return (bestScore, bestChild);
            }
        }

        override public string ToString()
        {
            string res = "";
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    res += (char)board[y, x];
                }
                res += '\n';
            }
            return res;
        }

        // JSONSerializer can't save valuetuples, have to convert to/from MoveShim class.
        private class MoveShim
        {
            public int X1 { get; set; }
            public int Y1 { get; set; }
            public int X2 { get; set; }
            public int Y2 { get; set; }
        }

        public static void LoadCache(string path)
        {
            Board.cache = JsonSerializer.Deserialize<Dictionary<string, MoveShim>>(File.ReadAllText(path)).ToDictionary(kvp => kvp.Key, kvp => {
                var move = kvp.Value;
                return (
                    (move.Y1, move.X1),
                    (move.Y2, move.X2)
                );
            });
        }
        
        public static void SaveCache(string path)
        {
            var savableCache = Board.cache.ToDictionary(kvp => kvp.Key, kvp => {
                var start = kvp.Value.start;
                var end = kvp.Value.end;
                return new MoveShim
                {
                    X1 = start.x,
                    Y1 = start.y,
                    X2 = end.x,
                    Y2 = end.y,
                };
            });

            File.WriteAllText(path, JsonSerializer.Serialize(savableCache));
        }
    }
}
