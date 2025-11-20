using GLib;

namespace ModAdvancedGameChanges
{
    public static class Vector3iExtensions
    {
        public static int LengthSquaredWithPenalty(this Vector3i vector)
        {
            return (vector.m_x * vector.m_x) + (vector.m_y * vector.m_y) + (vector.m_z * vector.m_z * 128 * 128);
        }
    }
}
