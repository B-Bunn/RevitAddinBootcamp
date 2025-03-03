using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BDB
{
    [Transaction(TransactionMode.Manual)]
    public class WallDimensioning : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                TaskDialog.Show("Info", "Entered try block.");

                // Ensure the active view is a 2D plan view.
                View activeView = doc.ActiveView;
                if (!(activeView is ViewPlan))
                {
                    TaskDialog.Show("Error", "This command only works in a 2D plan view.");
                    return Result.Failed;
                }
                TaskDialog.Show("Info", "Active view is a 2D plan view.");

                // Get all walls in the current view.
                List<Wall> walls = GetWallsInView(doc);
                TaskDialog.Show("Info", $"Found {walls.Count} walls in the current view.");
                if (walls.Count < 2)
                {
                    TaskDialog.Show("Error", "At least two walls are required to create dimensions.");
                    return Result.Failed;
                }

                // Group walls into vertical and horizontal
                var wallGroups = GroupWallsByOrientation(walls);
                TaskDialog.Show("Info", $"Grouped walls into {wallGroups.Count} orientations.");

                // Create dimensions for each group of parallel walls
                using (Transaction trans = new Transaction(doc, "Auto Dimension Walls"))
                {
                    trans.Start();
                    foreach (var wallGroup in wallGroups.Values)
                    {
                        TaskDialog.Show("Info", $"Creating dimensions for a group of {wallGroup.Count} parallel walls.");
                        CreateDimensionsForWallGroup(doc, activeView, wallGroup);
                    }
                    trans.Commit();
                }

                TaskDialog.Show("Success", "Dimensions created successfully.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", $"An exception occurred: {ex.Message}");
                message = ex.Message;
                return Result.Failed;
            }
        }

        // Get all walls in the current view
        private List<Wall> GetWallsInView(Document doc)
        {
            return new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfClass(typeof(Wall))
                .Cast<Wall>()
                .ToList();
        }

        // Group walls by their orientation into vertical and horizontal
        private Dictionary<string, List<Wall>> GroupWallsByOrientation(List<Wall> walls)
        {
            var verticalWalls = new List<Wall>();
            var horizontalWalls = new List<Wall>();

            foreach (var wall in walls)
            {
                Line wallLine = (wall.Location as LocationCurve).Curve as Line;
                XYZ orientation = wallLine.Direction.Normalize();

                if (Math.Abs(orientation.X) > Math.Abs(orientation.Y))
                {
                    horizontalWalls.Add(wall);
                }
                else
                {
                    verticalWalls.Add(wall);
                }
            }

            return new Dictionary<string, List<Wall>>
            {
                { "Vertical", verticalWalls },
                { "Horizontal", horizontalWalls }
            };
        }

        // Create dimensions for a group of parallel walls
        private void CreateDimensionsForWallGroup(Document doc, View view, List<Wall> walls)
        {
            // Sort walls by their respective value (X for vertical, Y for horizontal)
            if (walls.Count == 0) return;

            Line firstWallLine = (walls[0].Location as LocationCurve).Curve as Line;
            XYZ firstOrientation = firstWallLine.Direction.Normalize();

            if (Math.Abs(firstOrientation.X) > Math.Abs(firstOrientation.Y))
            {
                walls = walls.OrderBy(w => ((w.Location as LocationCurve).Curve as Line).GetEndPoint(0).Y).ToList();
            }
            else
            {
                walls = walls.OrderBy(w => ((w.Location as LocationCurve).Curve as Line).GetEndPoint(0).X).ToList();
            }

            for (int i = 0; i < walls.Count - 1; i++)
            {
                Wall wall1 = walls[i];
                Wall wall2 = walls[i + 1];

                // Get the closest points on the wall cores
                XYZ point1 = GetClosestPointOnWallCore(wall1, wall2);
                XYZ point2 = GetClosestPointOnWallCore(wall2, wall1);
                TaskDialog.Show("Info", $"Closest points on wall cores: Point1 ({point1}), Point2 ({point2})");

                // Check if the distance between points is greater than Revit's tolerance
                double distance = point1.DistanceTo(point2);
                if (distance < 0.001) // Revit's default tolerance for short curves
                {
                    TaskDialog.Show("Warning", "The distance between the walls is too small for Revit's tolerance.");
                    continue;
                }

                // Adjust the dimension line to be within the crop region
                BoundingBoxXYZ cropBox = view.CropBox;
                XYZ cropMin = cropBox.Min;
                XYZ cropMax = cropBox.Max;

                point1 = AdjustPointToCropRegion(point1, cropMin, cropMax);
                point2 = AdjustPointToCropRegion(point2, cropMin, cropMax);
                TaskDialog.Show("Info", $"Adjusted points to crop region: Point1 ({point1}), Point2 ({point2})");

                // Create the reference array
                ReferenceArray referenceArray = new ReferenceArray();
                referenceArray.Append(GetWallReferenceAtPoint(wall1, point1));
                referenceArray.Append(GetWallReferenceAtPoint(wall2, point2));
                TaskDialog.Show("Info", "Created reference array for dimension.");

                // Create the dimension line
                Line dimensionLine = Line.CreateBound(point1, point2);
                Dimension dimension = doc.Create.NewDimension(view, dimensionLine, referenceArray);

                if (dimension == null)
                {
                    TaskDialog.Show("Error", "Failed to create dimension.");
                }
                else
                {
                    TaskDialog.Show("Info", "Dimension created successfully.");
                }
            }
        }

        // Get the closest point on a wall core to another wall core
        private XYZ GetClosestPointOnWallCore(Wall wall1, Wall wall2)
        {
            LocationCurve locCurve1 = wall1.Location as LocationCurve;
            LocationCurve locCurve2 = wall2.Location as LocationCurve;

            Curve curve1 = locCurve1.Curve;
            Curve curve2 = locCurve2.Curve;

            XYZ point1 = curve1.GetEndPoint(0);
            XYZ point2 = curve1.GetEndPoint(1);

            XYZ closestPoint = point1.DistanceTo(curve2.GetEndPoint(0)) < point2.DistanceTo(curve2.GetEndPoint(0)) ? point1 : point2;

            return closestPoint;
        }

        // Adjust a point to be within the crop region
        private XYZ AdjustPointToCropRegion(XYZ point, XYZ cropMin, XYZ cropMax)
        {
            double x = Math.Max(cropMin.X, Math.Min(cropMax.X, point.X));
            double y = Math.Max(cropMin.Y, Math.Min(cropMax.Y, point.Y));
            double z = point.Z; // Keep the original Z value
            return new XYZ(x, y, z);
        }

        // Get a reference for a wall at a specific point
        private Reference GetWallReferenceAtPoint(Wall wall, XYZ point)
        {
            LocationCurve locCurve = wall.Location as LocationCurve;
            Curve curve = locCurve.Curve;
            XYZ closestPoint = curve.Project(point).XYZPoint;
            return new Reference(wall);
        }

        // Custom comparer for XYZ to handle direction comparison
        private class XYZComparer : IEqualityComparer<XYZ>
        {
            public bool Equals(XYZ x, XYZ y)
            {
                return x.IsAlmostEqualTo(y);
            }

            public int GetHashCode(XYZ obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}