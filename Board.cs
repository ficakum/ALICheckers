using System;
using System.Collections.Generic;

namespace ALICheckers
{
    enum MoveType
    {
        Invalid,
        Normal,
        Capture
    }

    class Board 
    {
        Piece[,] board;
        int size;
        // Maybe rename to Player
        Color playing;
        const int PieceRowCount = 3;

        public Board(int size)
        {
            this.board = new Piece[size,size];
            this.size = size;
            this.playing = Color.Black;
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            for (int y = 0; y < this.size; y++) {
                for (int x = 0; x < this.size; x++) {
                    // Valid squares
                    if ((y+x) % 2 == 1) {
                        // White piece rows
                        if (y < PieceRowCount) {
                            this.board[y,x] = Piece.WhitePawn;
                        } 
                        // Black piece rows
                        else if (size - y <= PieceRowCount) {
                            this.board[y,x] = Piece.BlackPawn;
                        } 
                        // Empty rows
                        else {
                            this.board[y,x] = Piece.Empty;
                        }
                    } 
                    // Non-valid squares
                    else {
                        this.board[y,x] = Piece.Blocked;
                    }
                }
            }
        }

        // Temporary name?
        public Piece GetPiece((int,int) position) 
        {
            return this.board[position.Item1, position.Item2];
        }

        private MoveType GetMoveType((int y, int x) delta)
        {
            (int y, int x) deltaAbs = (Math.Abs(delta.y), Math.Abs(delta.x));
            if (deltaAbs.y == 1 && deltaAbs.x == 1) {
                return MoveType.Normal;
            }
            else if (deltaAbs.y == 2 && deltaAbs.y == 2) {
                return MoveType.Capture;
            }
            else {
                return MoveType.Invalid;
            }
        }

        public bool IsMoveValid((int y,int x) start, (int y,int x) end) 
        {
            if(IsInBounds(start) && IsInBounds(end)) {
                Piece endPiece = this.GetPiece(end);
                Piece startPiece = this.GetPiece(start);
                if (endPiece == Piece.Empty && startPiece.GetColor() == this.playing) {
                    (int y, int x) delta = (end.y - start.y, end.x - start.x);

                    if (startPiece.IsKing() || (startPiece.IsPawn() && startPiece.IsValidDirection(delta.y))) {
                        MoveType type = GetMoveType(delta);
                        // Regular move
                        if (type == MoveType.Normal) {
                            return true;
                        }
                        // Capture move
                        else if (type == MoveType.Capture) {
                            Piece capturedPiece = this.GetPiece((start.y + delta.y/2, start.x + delta.x/2));
                            if (capturedPiece.IsPiece() && startPiece.GetColor() != capturedPiece.GetColor()) {
                               return true;
                            }
                        }
                    }
                }
            }
            return false; 
        }

        // NOTE: Should return a new state later on.
        public bool MakeMove((int y, int x) start, (int y, int x) end)
        {
            if (IsMoveValid(start, end)) {
                (int y, int x) delta = (end.y - start.y, end.x - start.x);
                MoveType type = GetMoveType(delta);
                
                // Remove captured piece if it was a capture
                if (type == MoveType.Capture) {
                    this.board[start.y + delta.y/2, start.x + delta.x/2] = Piece.Empty;
                }

                Piece movingPiece = GetPiece(start);
                this.board[start.y, start.x] = Piece.Empty;
                this.board[end.y, end.x] = movingPiece;

                // Switch to the opposing color
                this.playing = this.playing == Color.Black? Color.White : Color.Black;

                return true;
            }
            else {
                return false;
            }
        }

        public bool IsInBounds((int y, int x) pos)
        {
            return pos.y >= 0 && pos.y < size && 
                   pos.x >= 0 && pos.x < size;
        }


        // NOTE: Doesn't do multiple captures in one move atm.
        public IEnumerable<((int y, int x) start, (int y, int x) end)> GetAllMoves()
        {
            // Scan for the current player's pieces
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    Piece piece = GetPiece((y,x));
                    // Maybe split this off into a separate function?
                    if(piece.GetColor() == this.playing) {
                        (int y, int x) start = (y, x);
                        // Goes in both y directions for kings, only one otherwise
                        for (int dy = -1; dy <= 1; dy += 2) {
                            if (!piece.IsValidDirection(dy))
                                continue;
                            // Both x directions for everything
                            for (int dx = -1; dx <= 1; dx += 2) {
                                (int y, int x) normalMove = (y+dy, x+dx);
                                (int y, int x) captureMove = (y+dy*2, x+dx*2);

                                if(IsMoveValid(start, normalMove))
                                    yield return (start, normalMove);
                                if(IsMoveValid(start, captureMove))
                                    yield return (start, captureMove);
                            }
                        }
                    }
                }
            }
        }

        override public string ToString()
        {
            string res = "";
            for (int y = 0; y < this.size; y++) {
                for (int x = 0; x < this.size; x++) {
                    res += (char)this.board[y,x];
                }
                res += '\n';
            }
            return res;
        }
    }
}
