using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VpNet;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace RoadyGUI
{
    public class Bot
    {
        private VirtualParadiseClient client;
        private Avatar owner;
        private Dictionary<int, NodeLocation> nodes;
        public bool sessionInProgress { get; private set; } = false;
        private bool doubleSided = false;
        private int placedNodes = 0;
        private int firstNodeId = 0;
        private VpObject? previewObj = null;

        public string folderPath { get; set; } = " ";
        public string objFileName { get; set; } = " ";

        private string objFilePath;

        private double startHeading = 0.0f;
        private double endHeading = 0.0f;

        public float uvScaleY { get; set; } = 1.0f;
        public int segmentsPerSection { get; set; } = 20;

        public float roadWidth { get; set; } = 1.0f;

        public Bot()
        {
            client = new VirtualParadiseClient();
            client.ObjectCreated += Client_ObjectCreated;
            client.ObjectChanged += Client_ObjectChanged;
            client.ObjectDeleted += Client_ObjectDeleted;

            nodes = new Dictionary<int, NodeLocation>();
        }

        private async void Client_ObjectDeleted(VirtualParadiseClient sender, ObjectDeleteArgs args)
        {
            if (nodes.ContainsKey(args.Object.Id))
            {
                nodes.Remove(args.Object.Id);
                placedNodes--;

                if (placedNodes >= 1)
                {
                    GenerateRoad(new VpNet.Vector3(0, 0, 0));  // Default origin
                    await UpdatePreviewObject(new VpNet.Vector3(0, 0, 0));  // Default origin
                }
            }
        }

        public async Task<bool> Connect(string username, string password, string botName, string worldName, double x, double y, double z)
        {
            World world = new World { Name = worldName };
            client.Configuration = new VirtualParadiseClientConfiguration
            {
                ApplicationName = "Tomsbot",
                ApplicationVersion = "0.0.1",
                BotName = botName,
                UserName = username,
                World = world
            };

            try
            {
                await client.LoginAndEnterAsync(password, true);
                client.UpdateAvatar(x, y, z);
                return true; // Connection successful
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed: " + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; // Connection failed
            }
        }

        private async void Client_ObjectChanged(VirtualParadiseClient sender, ObjectChangeArgs args)
        {
            if (nodes.TryGetValue(args.Object.Id, out NodeLocation value))
            {
                NodeLocation updatedNode = new NodeLocation
                {
                    position = args.Object.Position,
                    heading = AxisAngleToYaw(args.Object.Rotation.X, args.Object.Rotation.Y, args.Object.Rotation.Z, args.Object.Angle)
                };
                nodes[args.Object.Id] = updatedNode;

                client.ConsoleMessage(args.Avatar, "[Roady]", "New Pos x: " + args.Object.Position.X + ", y: " + args.Object.Position.Y + ", z: " + args.Object.Position.Z + " heading: " + updatedNode.heading);

                if (placedNodes >= 1)
                {
                    GenerateRoad(new VpNet.Vector3(0, 0, 0));  // Default origin
                    await UpdatePreviewObject(new VpNet.Vector3(0, 0, 0));  // Default origin
                }
            }
        }

        private async void Client_ObjectCreated(VirtualParadiseClient sender, ObjectCreateArgs args)
        {
            if (sessionInProgress && args.Object.Owner == client.CurrentUser.Id)
            {
                if (args.Object.Description == "roadnode")
                {
                    double hdg = AxisAngleToYaw(args.Object.Rotation.X, args.Object.Rotation.Y, args.Object.Rotation.Z, args.Object.Angle);
                    if (double.IsNaN(hdg))
                    {
                        hdg = 0.0f; // Ensure there's always a valid heading
                    }

                    NodeLocation node = new NodeLocation
                    {
                        position = args.Object.Position,
                        heading = hdg
                    };

                    nodes.Add(args.Object.Id, node);
                    placedNodes++;
                    client.ConsoleMessage(args.Avatar, "[Roady]", "Road Node Added");

                    // Trigger road generation on any node placement
                    if (placedNodes >= 1)
                    {
                        // Ensure the endHeading is always updated based on the last node's heading
                        if (placedNodes > 1)
                        {
                            endHeading = node.heading;
                        }

                        // Trigger the road generation immediately after adding the node
                        GenerateRoad(new VpNet.Vector3(0, 0, 0)); // Use default origin or set offset
                        await UpdatePreviewObject(new VpNet.Vector3(0, 0, 0)); // Update preview object position
                    }
                }
            }
        }

        public async Task<bool> StartRoadSession()
        {
            if (!sessionInProgress)
            {
                var avatars = client.Avatars;
                owner = avatars.FirstOrDefault(avatar => avatar.User.Id == client.CurrentUser.Id);
                sessionInProgress = true;
                client.ConsoleMessage(owner, "[Roady]", "Road Design Session Started");

                VpObject v = new VpObject
                {
                    Model = "terrain.rwx",
                    Description = "roadnode",
                    Position = owner.Location.Position,
                    Rotation = new VpNet.Vector3(0, 1, 0),  // Initial orientation, you can adjust this
                    Angle = 0.0f  // Make sure to set the angle to 0 or any default initial heading
                };

                int id = await client.AddObjectAsync(v);
                NodeLocation nodeLocation = new NodeLocation
                {
                    position = v.Position,
                    heading = 0.0f // Ensure initial heading is correctly set
                };
                nodes.Add(id, nodeLocation);
                firstNodeId = id;
                startHeading = 0.0f;  // Set initial heading for the road
                return true;
            }
            return false;
        }

        public async Task UpdatePreviewObject(VpNet.Vector3 originOffset)
        {
            if (previewObj == null)
            {
                int previewObjId = await client.AddObjectAsync(new VpObject
                {
                    Model = objFileName,
                    Position = new VpNet.Vector3(nodes[firstNodeId].position.X + originOffset.X,
                                                 nodes[firstNodeId].position.Y + originOffset.Y,
                                                 nodes[firstNodeId].position.Z + originOffset.Z)
                });
                previewObj = await client.GetObjectAsync(previewObjId);
                Console.WriteLine($"Spawned preview object at {previewObj.Position}");
            }
            else
            {
                // Move the preview object to the new origin and change the model
                previewObj.Position = new VpNet.Vector3(nodes[firstNodeId].position.X + originOffset.X,
                                                       nodes[firstNodeId].position.Y + originOffset.Y,
                                                       nodes[firstNodeId].position.Z + originOffset.Z);
                previewObj.Model = objFileName + "tmp";
                await client.ChangeObjectAsync(previewObj);
                Console.WriteLine($"Updated preview object to model: {previewObj.Model}");

                // Insert a delay to give the server time to process the change
                await Task.Delay(500); // Delay for 500 milliseconds (adjust as needed)

                previewObj.Model = objFileName;
                await client.ChangeObjectAsync(previewObj);
                Console.WriteLine($"Updated preview object to model: {objFileName}");
            }
        }

        public void GenerateRoad(VpNet.Vector3 originOffset)
        {
            objFilePath = Path.Combine(folderPath, objFileName + ".rwx");
            List<(double x, double y, double z)> positions = new List<(double x, double y, double z)>();

            var sortedNodes = nodes.OrderBy(k => k.Key).ToList();
            var firstNode = sortedNodes.First();
            var firstNodePosition = firstNode.Value.position;

            // Force initialize the heading and ensure road generation logic kicks in
            if (sortedNodes.Count == 1)
            {
                positions.Add((originOffset.X, originOffset.Y, originOffset.Z));  // Place at the origin
                startHeading = 0.0f;  // Set default heading to ensure road generation
            }
            else
            {
                foreach (var kvp in sortedNodes)
                {
                    double offsetX = kvp.Value.position.X - firstNodePosition.X + originOffset.X;
                    double offsetY = kvp.Value.position.Y - firstNodePosition.Y + originOffset.Y;
                    double offsetZ = kvp.Value.position.Z - firstNodePosition.Z + originOffset.Z;

                    if (kvp.Key == firstNode.Key)
                    {
                        positions.Add((originOffset.X, originOffset.Y, originOffset.Z));
                        startHeading = kvp.Value.heading; // Ensure startHeading is properly set
                    }
                    else if (kvp.Key == sortedNodes.Last().Key)
                    {
                        // Ensure endHeading is properly set for the last node
                        positions.Add((offsetX, offsetY, offsetZ));
                        endHeading = kvp.Value.heading;
                    }
                    else
                    {
                        positions.Add((offsetX, offsetY, offsetZ));
                        Console.WriteLine($"ID = {kvp.Key}, Location (Offset) = ({offsetX}, {offsetY}, {offsetZ})");
                    }
                }
            }

            RoadGeneratorRWX generator = new RoadGeneratorRWX(positions, startHeading, endHeading, segmentsPerSection, roadWidth, uvScaleY, doubleSided);
            generator.GenerateRWX(objFilePath);
        }

        public async void ClearNodes()
        {
            sessionInProgress = false;
            placedNodes = 0;
            firstNodeId = 0;
            startHeading = 0.0f;
            endHeading = 0.0f;
            await client.DeleteObjectAsync(previewObj.Id);
            previewObj = null;
            foreach (var node in nodes)
            {
                await client.DeleteObjectAsync(node.Key);
            }
            nodes.Clear();
        }

        public async void UpdateDimensions(float width, float uvScale, int segments, bool twoSided)
        {
            roadWidth = width;
            uvScaleY = uvScale;
            segmentsPerSection = segments;
            doubleSided = twoSided;

            if (sessionInProgress)
            {
                if (placedNodes >= 1)
                {
                    GenerateRoad(new VpNet.Vector3(0, 0, 0));  // Default origin
                    await UpdatePreviewObject(new VpNet.Vector3(0, 0, 0));  // Default origin
                }
            }
        }

        public static double AxisAngleToYaw(double x, double y, double z, double theta)
        {
            double mag = Math.Sqrt(x * x + y * y + z * z);
            if (mag > 0)
            {
                x /= mag;
                y /= mag;
                z /= mag;
            }

            if (Math.Abs(x) < 1e-6 && Math.Abs(z) < 1e-6 && Math.Abs(y - 1) < 1e-6)
            {
                return theta * (180.0 / Math.PI);
            }
            else if (Math.Abs(x) < 1e-6 && Math.Abs(z) < 1e-6 && Math.Abs(y + 1) < 1e-6)
            {
                return -theta * (180.0 / Math.PI);
            }

            double cosTheta = Math.Cos(theta);
            double sinTheta = Math.Sin(theta);
            double R11 = cosTheta + (1 - cosTheta) * x * x;
            double R21 = (1 - cosTheta) * y * x + sinTheta * z;

            double yaw = Math.Atan2(R21, R11);
            return yaw * (180.0 / Math.PI);
        }
    }
}