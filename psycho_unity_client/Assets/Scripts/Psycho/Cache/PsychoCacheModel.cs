namespace Psycho.Cache
{
    public enum PsychoCacheModelFormat
    {
        Old,
        New525,
        New622
    }

    public sealed class PsychoCacheModel
    {
        public int ModelId;
        public PsychoCacheModelFormat Format;
        public int[] VertexX;
        public int[] VertexY;
        public int[] VertexZ;
        public int[] TriangleA;
        public int[] TriangleB;
        public int[] TriangleC;
        public int[] FaceColors;
        public int[] FaceAlpha;

        public int VertexCount => VertexX == null ? 0 : VertexX.Length;

        public int FaceCount => TriangleA == null ? 0 : TriangleA.Length;
    }
}
