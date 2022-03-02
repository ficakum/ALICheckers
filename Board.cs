using System;

namespace ALICheckers
{
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

        public bool IsMoveValid((int y,int x) start, (int y,int x) end) 
        {
            Piece endPiece = this.GetPiece(end);
            Piece startPiece = this.GetPiece(start);
            if (endPiece == Piece.Empty && startPiece.IsPiece()) {
                (int y, int x) delta = (end.y - start.y, end.x - start.x);

                if (startPiece.IsKing() || (startPiece.IsPawn() && startPiece.IsValidDirection(delta.y))) {
                    (int y, int x) deltaAbs = (Math.Abs(delta.y), Math.Abs(delta.x));
                    // Regular move
                    if (deltaAbs.y == 1 && deltaAbs.x == 1) {
                        return true;
                    }
                    // Capture move
                    else if (deltaAbs.y == 2 && deltaAbs.y == 2) {
                        Piece capturedPiece = this.GetPiece((start.y + delta.y, start.x + delta.x));
                        if (capturedPiece.IsPiece() && startPiece.GetColor() == capturedPiece.GetColor()) {
                           return true;
                        }
                    }
                }
            }
            return false; 
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
