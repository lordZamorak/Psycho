namespace Psycho.Cache
{
    public sealed class PsychoMapLandscape
    {
        public int RegionX;
        public int RegionY;
        public bool Osrs;
        public int[,,] Heights = new int[4, 64, 64];
        public byte[,,] RenderFlags = new byte[4, 64, 64];
        public byte[,,] OverlayIds = new byte[4, 64, 64];
        public byte[,,] OverlayShapes = new byte[4, 64, 64];
        public byte[,,] OverlayRotations = new byte[4, 64, 64];
        public byte[,,] UnderlayIds = new byte[4, 64, 64];
    }
}
