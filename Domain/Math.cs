namespace Domain;

internal static class Math
{
    internal static int Modulo(int x, int m)
    {
        return (x % m + m) % m;
    }
}