using System;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;

class TubeGenerator
{
    private List<(double x, double y, double z)> positions;
    private List<(double x, double y, double z)> tangents;
    private double startHeading;
    private double endHeading;
    private int segmentsPerSection;
    private double tubeDiameter;
    private int tubeSegments; // Number of segments around the circumference of the tube
    private double uvScaleY;
    private StringWriter objContent;
    private double totalLength;
    private (double upX, double upY, double upZ) upVector;

    public TubeGenerator(List<(double x, double y, double z)> positions, double startHeading, double endHeading, int segmentsPerSection, double tubeDiameter, double uvScaleY, int tubeSegments = 16)
    {
        this.positions = positions;
        this.startHeading = startHeading;
        this.endHeading = endHeading;
        this.segmentsPerSection = segmentsPerSection;
        this.tubeDiameter = tubeDiameter;
        this.uvScaleY = uvScaleY;
        this.tubeSegments = tubeSegments; // Default is 16 segments around the circumference
        this.objContent = new StringWriter();
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
        scale = VectorLength(positions[positions.Count - 1], positions[positions.Count - 2]);
        tangents.Add((dirX * scale, dirY * scale, dirZ * scale));
    }

    public void GenerateOBJ(string outputPath)
    {
        objContent.WriteLine("# OBJ file generated for a tube");
        List<List<(double x, double y, double z)>> tubeVertices = new List<List<(double x, double y, double z)>>();

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

                // Get the tangent (tube direction) and compute the perpendicular vector
                var perp = CrossProduct((xDir, yDir, zDir), upVector);
                var (px, py, pz) = Normalize(perp);

                // Generate the circle for the tube's cross-section
                List<(double x, double y, double z)> tubeSection = new List<(double x, double y, double z)>();
                double radius = tubeDiameter / 2.0;
                for (int k = 0; k < tubeSegments; k++)
                {
                    double angle = 2.0 * Math.PI * k / tubeSegments;
                    double offsetX = Math.Cos(angle) * radius;
                    double offsetY = Math.Sin(angle) * radius;

                    // Rotate around the tangent vector to create the tube's circular cross-section
                    var circleVertex = RotateAroundAxis((offsetX, offsetY, 0), (px, py, pz), (xDir, yDir, zDir));
                    tubeSection.Add((x + circleVertex.x, y + circleVertex.y, z + circleVertex.z));
                }
                tubeVertices.Add(tubeSection);
            }
        }

        WriteVerticesAndFaces(tubeVertices);
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

    private void WriteVerticesAndFaces(List<List<(double x, double y, double z)>> tubeVertices)
    {
        int vertexIndex = 1;

        // Write vertices and faces for the tube sections
        for (int i = 0; i < tubeVertices.Count - 1; i++)
        {
            var section1 = tubeVertices[i];
            var section2 = tubeVertices[i + 1];

            for (int j = 0; j < tubeSegments; j++)
            {
                var (x1, y1, z1) = section1[j];
                var (x2, y2, z2) = section2[j];
                objContent.WriteLine($"v {x1} {y1} {z1}");
                objContent.WriteLine($"v {x2} {y2} {z2}");

                int nextJ = (j + 1) % tubeSegments;
                var (x3, y3, z3) = section1[nextJ];
                var (x4, y4, z4) = section2[nextJ];
                objContent.WriteLine($"v {x3} {y3} {z3}");
                objContent.WriteLine($"v {x4} {y4} {z4}");

                objContent.WriteLine($"f {vertexIndex} {vertexIndex + 2} {vertexIndex + 3} {vertexIndex + 1}");
                vertexIndex += 4;
            }
        }
    }

    private static (double x, double y, double z) RotateAroundAxis((double x, double y, double z) point, (double x, double y, double z) axis, (double x, double y, double z) direction)
    {
        // Placeholder for proper rotation math. Implement rotation logic here.
        return (point.x, point.y, point.z);
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
