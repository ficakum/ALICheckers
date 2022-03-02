using System;

namespace ALICheckers
{
    enum Piece 
    {
        Blocked = ' ',
        Empty = '_',
        WhitePawn = 'o',
        BlackPawn = 'x',
        WhiteKing = 'O',
        BlackKing = 'X'
    }

    enum Color
    {
        None = 0,
        White = 1,
        Black = -1
    }

    static class PieceExtensions
    {
        public static bool IsWhite(this Piece piece)
        {
            return piece == Piece.WhitePawn || piece == Piece.WhiteKing;
        }

        public static bool IsBlack(this Piece piece)
        {
            return piece == Piece.BlackPawn || piece == Piece.BlackKing;
        }

        public static bool IsKing(this Piece piece)
        {
            return piece == Piece.WhiteKing || piece == Piece.BlackKing;
        }

        public static bool IsPawn(this Piece piece)
        {
            return piece == Piece.WhitePawn || piece == Piece.BlackPawn;
        }

        public static bool IsPiece(this Piece piece)
        {
            return piece != Piece.Blocked && piece != Piece.Empty;
        }

        // Maybe better to have everything use this instead?
        public static Color GetColor(this Piece piece)
        {
            if(piece.IsWhite())
                return Color.White;
            else if(piece.IsBlack())
                return Color.Black;
            else
                return Color.None;
        }

        // White goes down, increasing the y coordinate, black does the opposite.
        public static int GetDirection(this Piece piece)
        {
            return piece.IsWhite()? 1 : -1;
        }

        // No need to check for kings, otherwise check if signs match.
        public static bool IsValidDirection(this Piece piece, int deltaY)
        {
            return (piece.IsKing() ||
                    ((piece.GetDirection() < 0) == (deltaY < 0)));
        }
    }
}
