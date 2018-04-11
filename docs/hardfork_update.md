Hardfork update
====

You need to read [this article](https://github.com/pjc0247/minichain/blob/master/docs/customize_chain.md) first.

```cs
private static readonly int M2_Hardfork_BlockNo = 5000;

private static double M1_RewardFunction(int blockNo) 
{
    return 100000 - blockNo;
}
private static double M2_RewardFunction(int blockNo)
{
    return (100000 - blockNo) * 0.75;
}
public static double CalcBlockReward(int blockNo)
{
    if (blockNo >= M2_Hardfork_BlockNo)
        return M2_RewardFunction(blockNo);
    else 
        return M1_RewardFunction(blockNo);
}
```
