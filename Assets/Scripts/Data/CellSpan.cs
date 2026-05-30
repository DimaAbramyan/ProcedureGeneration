using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct CellSpan
{
    public ushort xMin;
    public ushort xMax;
    public RoomData room;

    public bool Contains(int x) => x >= xMin && x <= xMax;
}
