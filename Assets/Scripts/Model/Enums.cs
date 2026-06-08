namespace Match3.Model
{
    public enum PieceColor
    {
        None,
        Red,
        Green,
        Blue,
        Yellow,
        Purple,
        Orange
    }

    public enum PieceType
    {
        Empty,
        Normal,
        Bomb,
        HorizontalLine,
        VerticalLine
    }

    public enum GameState
    {
        Initializing,
        AwaitingInput,
        Animating,
        GameOver
    }
}
