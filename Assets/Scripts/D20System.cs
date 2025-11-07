using UnityEngine;

public static class D20System
{
    public static int RollD20() => Random.Range(1, 21);

    public static int RollDice(int count, int sides)
    {
        int total = 0;
        for (int i = 0; i < count; i++)
            total += Random.Range(1, sides + 1);
        return total;
    }
}
