using System;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;

class RoadGenerator
{
    private List<(double x, double y, double z)> positions;
    private List<(double x, double y, double z)> tangents;
    private double startHeading;
    private double endHeading;
    private int segmentsPerSection;
    private double roadWidth;
    private double uvScaleY;
    private StringWriter objContent;
    private double totalLength;
    private (double upX, double upY, double upZ) upVector;

    public RoadGenerator(List<(double x, double y, double z)> positions, double startHeading, double endHeading, int segmentsPerSection, double roadWidth, double uvScaleY)
    {
        this.positions = positions;
        this.startHeading = startHeading;
        this.endHeading = endHeading;
        this.segmentsPerSection = segmentsPerSection;
        this.roadWidth = roadWidth;
        this.uvScaleY = uvScaleY;
        this.objContent = new StringWriter();
        this.totalLength = 0;
        this.upVector = (0.0, 1.0, 0.0);
        ComputeTangents();
    }

    private void ComputeTangents()
    {
        tangents = new List<(double x, double y, double z)>();

        // Compute tangent at the start point
        double angleRadians = -startHeading * Math.PI / 180.0; // Invert the yaw
        (double dirX, double dirY, double dirZ) = (Math.Cos(angleRadians), 0.0, Math.Sin(angleRadians));

        // Rotate counterclockwise by 90 degrees around the y-axis
        double tempX = dirX;
        dirX = -dirZ;
        dirZ = tempX;

        double scale = VectorLength(positions[1], positions[0]);
        tangents.Add((dirX * scale, dirY * scale, dirZ * scale));

        // Compute tangents for internal points (unchanged)
        for (int i = 1; i < positions.Count - 1; i++)
        {
            var pPrev = positions[i - 1];
            var pNext = positions[i + 1];
            var m = ((pNext.x - pPrev.x) / 2.0, (pNext.y - pPrev.y) / 2.0, (pNext.z - pPrev.z) / 2.0);
            tangents.Add(m);
        }

        // Compute tangent at the end point
        angleRadians = -endHeading * Math.PI / 180.0; // Invert the yaw
        (dirX, dirY, dirZ) = (Math.Cos(angleRadians), 0.0, Math.Sin(angleRadians));

        // Rotate counterclockwise by 90 degrees around the y-axis
        tempX = dirX;
        dirX = -dirZ;
        dirZ = tempX;

        scale = VectorLength(positions[positions.Count - 1], positions[positions.Count - 2]);
        tangents.Add((dirX * scale, dirY * scale, dirZ * scale));
    }


    public void GenerateOBJ(string outputPath)
    {
        objContent.WriteLine("# OBJ file generated using Hermite splines with custom start/end tangents");
        List<(double x, double y, double z)> leftVertices = new List<(double x, double y, double z)>();
        List<(double x, double y, double z)> rightVertices = new List<(double x, double y, double z)>();
        List<double> textureCoordinates = new List<double>();

        // Generate vertices using Hermite splines
        for (int i = 0; i < positions.Count - 1; i++)
        {
            var p0 = positions[i];
            var p1 = positions[i + 1];
            var m0 = tangents[i];
            var m1 = tangents[i + 1];

            for (int j = 0; j <= segmentsPerSection; j++)
            {
                double t = (double)j / segmentsPerSection;
                var (x, y, z) = HermiteSpline(t, p0, m0, p1, m1);
                var (dx, dy, dz) = HermiteSplineDerivative(t, p0, m0, p1, m1);
                var (xDir, yDir, zDir) = Normalize((dx, dy, dz));

                if (xDir == 0 && yDir == 0 && zDir == 0)
                {
                    xDir = 1.0; yDir = 0.0; zDir = 0.0;
                }

                var perp = CrossProduct((xDir, yDir, zDir), upVector);
                var (px, py, pz) = Normalize(perp);
                double halfWidth = roadWidth / 2;
                leftVertices.Add((x + px * halfWidth, y + py * halfWidth, z + pz * halfWidth));
                rightVertices.Add((x - px * halfWidth, y - py * halfWidth, z - pz * halfWidth));
                textureCoordinates.Add(totalLength * uvScaleY);

                if (j < segmentsPerSection || i < positions.Count - 2)
                {
                    double nextT = t + (1.0 / segmentsPerSection);
                    if (nextT > 1.0) nextT = 1.0;
                    var (nextX, nextY, nextZ) = HermiteSpline(nextT, p0, m0, p1, m1);
                    double dxLength = nextX - x;
                    double dyLength = nextY - y;
                    double dzLength = nextZ - z;
                    totalLength += Math.Sqrt(dxLength * dxLength + dyLength * dyLength + dzLength * dzLength);
                }
            }
        }

        WriteVerticesAndFaces(leftVertices, rightVertices, textureCoordinates);

        // Now create a zip archive containing the OBJ file
        CreateZipArchive(outputPath);
    }

    private void CreateZipArchive(string outputPath)
    {
        string objFileName = Path.GetFileNameWithoutExtension(outputPath) + ".obj";
        using (MemoryStream objStream = new MemoryStream())
        {
            using (StreamWriter writer = new StreamWriter(objStream))
            {
                writer.Write(objContent.ToString());
                writer.Flush();
                objStream.Position = 0;

                string zipPath = Path.ChangeExtension(outputPath, ".zip");
                using (FileStream zipToCreate = new FileStream(zipPath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
                    {
                        ZipArchiveEntry objEntry = archive.CreateEntry(objFileName);
                        using (Stream entryStream = objEntry.Open())
                        {
                            objStream.CopyTo(entryStream);
                        }
                    }
                }
            }
        }
    }

    private void WriteVerticesAndFaces(List<(double x, double y, double z)> leftVertices, List<(double x, double y, double z)> rightVertices, List<double> textureCoordinates)
    {
        // The pivot will be the position of the first node (start of the road)
        var (pivotX, pivotY, pivotZ) = positions[0];

        for (int i = 0; i < leftVertices.Count; i++)
        {
            var (lx, ly, lz) = leftVertices[i];
            var (rx, ry, rz) = rightVertices[i];

            // Translate all vertices so that the pivot point is at the first node
            lx -= pivotX; ly -= pivotY; lz -= pivotZ;
            rx -= pivotX; ry -= pivotY; rz -= pivotZ;

            double uvY = textureCoordinates[i];
            objContent.WriteLine($"v {lx} {ly} {lz}");
            objContent.WriteLine($"v {rx} {ry} {rz}");
            objContent.WriteLine($"vt 0 {uvY:F4}");
            objContent.WriteLine($"vt 1 {uvY:F4}");
        }

        int totalVertices = leftVertices.Count * 2;
        for (int i = 0; i < totalVertices - 2; i += 2)
        {
            int v1 = i + 1;
            int v2 = v1 + 1;
            int v3 = v1 + 3;
            int v4 = v1 + 2;
            objContent.WriteLine($"f {v1}/{v1} {v4}/{v4} {v3}/{v3} {v2}/{v2}");
        }
    }


    private static (double x, double y, double z) HermiteSpline(double t, (double x, double y, double z) p0, (double x, double y, double z) m0, (double x, double y, double z) p1, (double x, double y, double z) m1)
    {
        double t2 = t * t;
        double t3 = t2 * t;
        double h00 = 2.0 * t3 - 3.0 * t2 + 1.0;
        double h10 = t3 - 2.0 * t2 + t;
        double h01 = -2.0 * t3 + 3.0 * t2;
        double h11 = t3 - t2;
        return (h00 * p0.x + h10 * m0.x + h01 * p1.x + h11 * m1.x,
                h00 * p0.y + h10 * m0.y + h01 * p1.y + h11 * m1.y,
                h00 * p0.z + h10 * m0.z + h01 * p1.z + h11 * m1.z);
    }

    private static (double x, double y, double z) HermiteSplineDerivative(double t, (double x, double y, double z) p0, (double x, double y, double z) m0, (double x, double y, double z) p1, (double x, double y, double z) m1)
    {
        double t2 = t * t;
        double h00 = 6.0 * t2 - 6.0 * t;
        double h10 = 3.0 * t2 - 4.0 * t + 1.0;
        double h01 = -6.0 * t2 + 6.0 * t;
        double h11 = 3.0 * t2 - 2.0 * t;
        return (h00 * p0.x + h10 * m0.x + h01 * p1.x + h11 * m1.x,
                h00 * p0.y + h10 * m0.y + h01 * p1.y + h11 * m1.y,
                h00 * p0.z + h10 * m0.z + h01 * p1.z + h11 * m1.z);
    }

    private static (double x, double y, double z) Normalize((double x, double y, double z) v)
    {
        double length = Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        if (length == 0) return (0, 0, 0);
        return (v.x / length, v.y / length, v.z / length);
    }

    private static (double x, double y, double z) CrossProduct((double x, double y, double z) a, (double x, double y, double z) b)
    {
        return (a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x);
    }

    private static double VectorLength((double x, double y, double z) a, (double x, double y, double z) b)
    {
        double dx = a.x - b.x;
        double dy = a.y - b.y;
        double dz = a.z - b.z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}

