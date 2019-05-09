using System.Collections.Generic;

public interface IMapGenPass
{
    string getPassName();
    string getPassDesc();
    void run(IMapInfo mapData);
    IEnumerable<string> getReqMapParams();
    IEnumerable<string> getMapDataRead();
    IEnumerable<string> getPrereqPasses();
    IEnumerable<string> getMapDataWritten();
}
