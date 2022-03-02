using System;

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
        const int PieceRowCount = 3;

        public Board(int size)
        {
            this.board = new Piece[size,size];
            this.size = size;
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
            Piece endPiece = this.GetPiece(end);
            Piece startPiece = this.GetPiece(start);
            if (endPiece == Piece.Empty && startPiece.IsPiece()) {
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
                return true;
            }
            else {
                return false;
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
