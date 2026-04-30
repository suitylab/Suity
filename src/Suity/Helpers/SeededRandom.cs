using System;

namespace Suity.Helpers;

/// <summary>
/// Provides random number generation with a seed for reproducibility.
/// </summary>
public class SeededRandom
{
    private int _seed;
    private readonly RandomWELL _rnd;

    public SeededRandom()
    {
        _seed = DateTime.UtcNow.Ticks.GetHashCode();
        _rnd = new RandomWELL(_seed);
    }

    public SeededRandom(int seed)
    {
        _seed = seed;
        _rnd = new RandomWELL(_seed);
    }

    public int Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            _rnd.srand(value);
        }
    }

    public float NextFloat()
    {
        return _rnd.frand2();
    }

    public int NextInt()
    {
        return _rnd.rand();
    }

    public float Range(float min, float max)
    {
        return _rnd.Range(min, max);
    }

    public int Range(int min, int max)
    {
        int value = _rnd.Range(min, max);

        return value;
    }
}