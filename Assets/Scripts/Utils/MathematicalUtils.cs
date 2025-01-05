using Unity.Collections;
using Unity.Mathematics;

public static class MathematicalUtils
{
    /// <summary>
    /// Arranges units: first one at the center (targetPosition), then subsequent units in concentric rings.
    /// Each ring is placed to avoid overlap, with a margin around each unit and each ring.
    /// </summary>
    /// <param name="targetPosition">The center position where the first unit goes.</param>
    /// <param name="count">Total number of units to place (including the center one).</param>
    /// <param name="unitSize">The diameter of each unit's footprint.</param>
    /// <param name="allocator">Memory allocator for the NativeArray.</param>
    /// <returns>A NativeArray of float3 positions for the units.</returns>
    public static NativeArray<float3> ArrangeUnitsInRings(float3 targetPosition, int count, float unitSize, Allocator allocator = Allocator.Temp)
    {
        if (count <= 0)
            return new NativeArray<float3>(0, allocator);

        // We'll store all positions
        var positions = new NativeArray<float3>(count, allocator);

        // Place the first unit at the center
        positions[0] = targetPosition;
        if (count == 1)
            return positions; // Only one unit requested

        int unitsPlaced = 1;
        int unitsLeft = count - 1;

        // Margins and spacing calculations
        float ringMargin = unitSize * 0.5f;    // Margin added between rings
        float unitSpacing = unitSize * 1.5f;   // Each unit on the ring needs unitSize + 0.2*unitSize = 1.2 * unitSize of circumference space

        // Calculate the first ring radius:
        // first ring radius = center unit radius + ring unit radius + ring margin = 0.5*unitSize + 0.5*unitSize + 0.5*unitSize = 1.5 * unitSize
        float ringRadius = 1.5f * unitSize;

        // Place remaining units in rings
        while (unitsLeft > 0)
        {
            float circumference = 2f * math.PI * ringRadius;
            int unitsOnThisRing = (int)math.floor(circumference / unitSpacing);

            // Ensure at least one unit can be placed
            if (unitsOnThisRing < 1)
                unitsOnThisRing = 1;

            // If we have fewer units left than fits on the ring, adjust
            if (unitsOnThisRing > unitsLeft)
                unitsOnThisRing = unitsLeft;

            float angleStep = 2f * math.PI / unitsOnThisRing;

            // Place units evenly on this ring
            for (int i = 0; i < unitsOnThisRing; i++)
            {
                float angle = i * angleStep;
                float x = targetPosition.x + ringRadius * math.cos(angle);
                float z = targetPosition.z + ringRadius * math.sin(angle);
                // Keep the same Y as the target position
                positions[unitsPlaced] = new float3(x, targetPosition.y, z);
                unitsPlaced++;
                unitsLeft--;
            }

            // Increase radius for the next ring:
            // next ring radius increment = unitSize + ringMargin
            ringRadius += (unitSize + ringMargin);
        }

        return positions;
    }
}