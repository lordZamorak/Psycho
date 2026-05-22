using System.Collections.Generic;

namespace Psycho.Cache
{
    public sealed class PsychoMapObjects
    {
        public int RegionX;
        public int RegionY;
        public bool Osrs;
        public List<PsychoMapObjectPlacement> Placements = new List<PsychoMapObjectPlacement>();
    }

    public sealed class PsychoMapObjectPlacement
    {
        public int ObjectId;
        public int LocalX;
        public int LocalY;
        public int WorldX;
        public int WorldY;
        public int Plane;
        public int Type;
        public int Orientation;
    }
}
