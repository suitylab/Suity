using System.Collections.Generic;
using System.Linq;

namespace Suity.Rex.Mapping;

internal interface IRexMappingGenericObjectGetter
{
    object GetObject(RexMapper mapper);

    IEnumerable<object> GetObjects(RexMapper mapper);
}

internal class RexMappingGenericObjectGetter<T> : IRexMappingGenericObjectGetter where T : class
{
    public object GetObject(RexMapper mapper)
    {
        return mapper.Get<T>();
    }

    public IEnumerable<object> GetObjects(RexMapper mapper)
    {
        return mapper.GetMany<T>().OfType<object>();
    }
}