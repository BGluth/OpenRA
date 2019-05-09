using System;
using System.Collections.Generic;

public interface IMapInfo
{
    void writeMapData(string dataKey, object data);
    object getMapData(string dataKey);
    object getParamData(string paramKey);
}
