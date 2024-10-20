using System;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;

class RoadGeneratorRWX
{
    private List<(double x, double y, double z)> positions;
    private List<(double x, double y, double z)> tangents;
    private double startHeading;
    private double endHeading;
    private int segmentsPerSection;
    private double roadWidth;
    private double uvScaleY;
    private StringWriter rwxContent;
    private double totalLength;
    private (double upX, double upY, double upZ) upVector;
    private bool visibleFromBottom;

    public RoadGeneratorRWX(List<(double x, double y, double z)> positions, double startHeading, double endHeading, int segmentsPerSection, double roadWidth, double uvScaleY, bool visibleFromBottom)
    {
        if (positions == null || positions.Count < 2)
            throw new ArgumentException("At least two positions are required to generate a road.");

        this.positions = positions;
        this.startHeading = startHeading;
        this.endHeading = endHeading;
        this.segmentsPerSection = segmentsPerSection;
        this.roadWidth = roadWidth;
        this.uvScaleY = uvScaleY;
        this.visibleFromBottom = visibleFromBottom;
        this.rwxContent = new StringWriter();
        this.totalLength = 0;
        this.upVector = (0.0, 1.0, 0.0);
        ComputeTangents();
    }

    private void ComputeTangents()
    {
        tangents = new List<(double x, double y, double z)>();

        // Compute tangent at the start point
        double angleRadians = -startHeading * Math.PI / 180.0;
        (double dirX, double dirY, double dirZ) = (Math.Cos(angleRadians), 0.0, Math.Sin(angleRadians));

        double tempX = dirX;
        dirX = -dirZ;
        dirZ = tempX;

        double scale = VectorLength(positions[1], positions[0]);
        tangents.Add((dirX * scale, dirY * scale, dirZ * scale));

        // Compute tangents for internal points
        for (int i = 1; i < positions.Count - 1; i++)
        {
            var pPrev = positions[i - 1];
            var pNext = positions[i + 1];
            var m = ((pNext.x - pPrev.x) / 2.0, (pNext.y - pPrev.y) / 2.0, (pNext.z - pPrev.z) / 2.0);
            tangents.Add(m);
        }

        // Compute tangent at the end point
        angleRadians = -endHeading * Math.PI / 180.0;
        (dirX, dirY, dirZ) = (Math.Cos(angleRadians), 0.0, Math.Sin(angleRadians));

        tempX = dirX;
        dirX = -dirZ;
        dirZ = tempX;

        scale = VectorLength(positions[positions.Count - 1], positions[positions.Count - 2]);
        tangents.Add((dirX * scale, dirY * scale, dirZ * scale));
    }

    public void GenerateRWX(string outputPath)
    {
        // RWX Header
        rwxContent.WriteLine("ModelBegin");
        rwxContent.WriteLine("  ClumpBegin #Layer: Object");
        rwxContent.WriteLine("    Diffuse 1.0");
        rwxContent.WriteLine("    Color 1.0 1.0 1.0");
        rwxContent.WriteLine("    Texture Tile31");
        rwxContent.WriteLine("    LightSampling Vertex");

        List<(double x, double y, double z)> leftVertices = new List<(double x, double y, double z)>();
        List<(double x, double y, double z)> rightVertices = new List<(double x, double y, double z)>();
        List<double> textureCoordinates = new List<double>();

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

        // Optionally write bottom faces
        if (visibleFromBottom)
        {
            WriteBottomFaces(leftVertices, rightVertices, textureCoordinates);
        }

        rwxContent.WriteLine("  ClumpEnd");
        rwxContent.WriteLine("ModelEnd");

        // Create a zip archive containing the RWX file
        CreateZipArchive(outputPath);
    }

    private void WriteVerticesAndFaces(List<(double x, double y, double z)> leftVertices, List<(double x, double y, double z)> rightVertices, List<double> textureCoordinates)
    {
        var (pivotX, pivotY, pivotZ) = positions[0];

        for (int i = 0; i < leftVertices.Count; i++)
        {
            var (lx, ly, lz) = leftVertices[i];
            var (rx, ry, rz) = rightVertices[i];

            lx -= pivotX; ly -= pivotY; lz -= pivotZ;
            rx -= pivotX; ry -= pivotY; rz -= pivotZ;

            double uvY = textureCoordinates[i];

            rwxContent.WriteLine($"    Vertex {lx:F6} {ly:F6} {lz:F6} UV 0 {uvY:F4}");
            rwxContent.WriteLine($"    Vertex {rx:F6} {ry:F6} {rz:F6} UV 1.0 {uvY:F4}");
        }

        rwxContent.WriteLine("#texend Object");

        for (int i = 1; i < leftVertices.Count; i++)
        {
            int v1 = i * 2;
            int v2 = v1 - 1;
            int v3 = v1 + 1;
            int v4 = v1 + 2;

            // **Corrected Winding Order for Top Faces: {v1, v2, v3, v4}**
            rwxContent.WriteLine($"    Quad {v1} {v2} {v3} {v4}");
        }
    }

    private void WriteBottomFaces(List<(double x, double y, double z)> leftVertices, List<(double x, double y, double z)> rightVertices, List<double> textureCoordinates)
    {
        rwxContent.WriteLine("# Bottom faces");

        for (int i = 1; i < leftVertices.Count; i++)
        {
            int v1 = i * 2;
            int v2 = v1 - 1;
            int v3 = v1 + 1;
            int v4 = v1 + 2;

            // **Corrected Winding Order for Bottom Faces: {v2, v1, v4, v3}**
            // This ensures normals point downwards
            rwxContent.WriteLine($"    Quad {v2} {v1} {v4} {v3} # Bottom side");
        }
    }

    private void CreateZipArchive(string outputPath)
    {
        string rwxFileName = Path.GetFileNameWithoutExtension(outputPath) + ".rwx";
        using (MemoryStream rwxStream = new MemoryStream())
        {
            using (StreamWriter writer = new StreamWriter(rwxStream))
            {
                writer.Write(rwxContent.ToString());
                writer.Flush();
                rwxStream.Position = 0;

                string zipPath = Path.ChangeExtension(outputPath, ".zip");
                using (FileStream zipToCreate = new FileStream(zipPath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
                    {
                        ZipArchiveEntry rwxEntry = archive.CreateEntry(rwxFileName);
                        using (Stream entryStream = rwxEntry.Open())
                        {
                            rwxStream.CopyTo(entryStream);
                        }
                    }
                }
            }
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
