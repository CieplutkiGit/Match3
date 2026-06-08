namespace Match3.Model
{
    public interface IGrid<T>
    {
        int Width { get; }
        int Height { get; }
        T Get(int x, int y);
        void Set(int x, int y, T value);
        bool IsValidPosition(int x, int y);
    }
}
