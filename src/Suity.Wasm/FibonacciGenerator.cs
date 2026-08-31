namespace Suity.Wasm;

public class FibonacciGenerator
{
    private readonly List<long> _sequence = new();

    public IReadOnlyList<long> Sequence => _sequence;

    public void Generate(int count)
    {
        _sequence.Clear();

        if (count <= 0) return;
        _sequence.Add(0);
        if (count == 1) return;
        _sequence.Add(1);

        for (int i = 2; i < count; i++)
        {
            _sequence.Add(_sequence[i - 1] + _sequence[i - 2]);
        }
    }

    public string GetSequenceString()
    {
        return string.Join(", ", _sequence);
    }
}
