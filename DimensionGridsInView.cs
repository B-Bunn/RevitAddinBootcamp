using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BDB
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionGridsInView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                // Ensure the active view is a 2D plan view.
                View activeView = doc.ActiveView;
                if (!(activeView is ViewPlan))
                {
                    TaskDialog.Show("Error", "This command only works in a 2D plan view.");
                    return Result.Failed;
                }

                // Get all grids in the current view.
                List<Grid> grids = GetGridsInView(doc);
                if (grids.Count < 2)
                {
                    TaskDialog.Show("Error", "At least two grids are required to create dimensions.");
                    return Result.Failed;
                }

                // Sort grids alphanumerically by their name
                grids = SortGridsByName(grids);

                // Identify the first vertical and horizontal grids
                Grid firstHorizontalGrid = grids.FirstOrDefault(g => IsHorizontal(g));
                Grid firstVerticalGrid = grids.FirstOrDefault(g => !IsHorizontal(g));

                if (firstHorizontalGrid == null || firstVerticalGrid == null)
                {
                    TaskDialog.Show("Error", "At least one horizontal and one vertical grid are required.");
                    return Result.Failed;
                }

                // Create dimensions between each grid and the next grid in the sorted list.
                using (Transaction trans = new Transaction(doc, "Auto Dimension Grids"))
                {
                    trans.Start();
                    CreateDimensions(doc, activeView, grids, firstHorizontalGrid, firstVerticalGrid);
                    trans.Commit();
                }

                TaskDialog.Show("Success", "Dimensions created successfully.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        // Get all grids in the current view
        private List<Grid> GetGridsInView(Document doc)
        {
            return new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfClass(typeof(Grid))
                .Cast<Grid>()
                .ToList();
        }

        // Sort grids alphanumerically by their name
        private List<Grid> SortGridsByName(List<Grid> grids)
        {
            return grids.OrderBy(grid => grid.Name, new AlphanumericComparer()).ToList();
        }

        // Check if a grid is horizontal
        private bool IsHorizontal(Grid grid)
        {
            Curve curve = grid.Curve;
            XYZ direction = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
            return Math.Abs(direction.Y) > Math.Abs(direction.X);
        }

        // Create dimensions between each grid and the next grid in the sorted list
        private void CreateDimensions(Document doc, View view, List<Grid> grids, Grid firstHorizontalGrid, Grid firstVerticalGrid)
        {
            XYZ firstHorizontalPoint = firstHorizontalGrid.Curve.GetEndPoint(0);
            XYZ firstVerticalPoint = firstVerticalGrid.Curve.GetEndPoint(0);

            for (int i = 0; i < grids.Count - 1; i++)
            {
                Grid grid1 = grids[i];
                Grid grid2 = grids[i + 1];

                if (IsHorizontal(grid1))
                {
                    CreateAlignedDimension(doc, view, grid1, grid2, firstHorizontalPoint.Y, true);
                }
                else
                {
                    CreateAlignedDimension(doc, view, grid1, grid2, firstVerticalPoint.X, false);
                }
            }
        }

        // Create an aligned dimension between two grids
        private void CreateAlignedDimension(Document doc, View view, Grid grid1, Grid grid2, double matchingValue, bool isHorizontal)
        {
            Curve curve1 = grid1.Curve;
            Curve curve2 = grid2.Curve;

            if (curve1 == null || curve2 == null)
            {
                TaskDialog.Show("Error", "One of the grids does not have a valid curve.");
                return;
            }

            XYZ point1 = GetMatchingPoint(curve1, matchingValue, isHorizontal);
            XYZ point2 = GetMatchingPoint(curve2, matchingValue, isHorizontal);

            // Get the crop region boundaries
            BoundingBoxXYZ cropBox = view.CropBox;
            XYZ cropMin = cropBox.Min;
            XYZ cropMax = cropBox.Max;

            // Adjust the dimension line to be within the crop region
            point1 = AdjustPointToCropRegion(point1, cropMin, cropMax);
            point2 = AdjustPointToCropRegion(point2, cropMin, cropMax);

            // Create the reference array
            ReferenceArray referenceArray = new ReferenceArray();
            referenceArray.Append(new Reference(grid1));
            referenceArray.Append(new Reference(grid2));

            // Create the aligned dimension line
            Line dimensionLine = Line.CreateBound(point1, point2);
            Dimension dimension = doc.Create.NewDimension(view, dimensionLine, referenceArray);

            if (dimension == null)
            {
                TaskDialog.Show("Error", "Failed to create dimension.");
            }
        }

        // Get a point on the curve that matches the specified value
        private XYZ GetMatchingPoint(Curve curve, double matchingValue, bool isHorizontal)
        {
            XYZ point1 = curve.GetEndPoint(0);
            XYZ point2 = curve.GetEndPoint(1);

            if (isHorizontal)
            {
                if (Math.Abs(point1.Y - matchingValue) < Math.Abs(point2.Y - matchingValue))
                {
                    return new XYZ(point1.X, matchingValue, point1.Z);
                }
                else
                {
                    return new XYZ(point2.X, matchingValue, point2.Z);
                }
            }
            else
            {
                if (Math.Abs(point1.X - matchingValue) < Math.Abs(point2.X - matchingValue))
                {
                    return new XYZ(matchingValue, point1.Y, point1.Z);
                }
                else
                {
                    return new XYZ(matchingValue, point2.Y, point2.Z);
                }
            }
        }

        // Adjust a point to be within the crop region
        private XYZ AdjustPointToCropRegion(XYZ point, XYZ cropMin, XYZ cropMax)
        {
            double x = Math.Max(cropMin.X, Math.Min(cropMax.X, point.X));
            double y = Math.Max(cropMin.Y, Math.Min(cropMax.Y, point.Y));
            double z = point.Z; // Keep the original Z value
            return new XYZ(x, y, z);
        }

        // Custom alphanumeric comparer
        private class AlphanumericComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == null || y == null)
                {
                    return 0;
                }

                var regex = new Regex(@"(\d+|\D+)");
                var xParts = regex.Matches(x).Cast<Match>().Select(m => m.Value).ToArray();
                var yParts = regex.Matches(y).Cast<Match>().Select(m => m.Value).ToArray();

                for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
                {
                    int xPartInt, yPartInt;
                    bool xIsInt = int.TryParse(xParts[i], out xPartInt);
                    bool yIsInt = int.TryParse(yParts[i], out yPartInt);

                    int result;
                    if (xIsInt && yIsInt)
                    {
                        result = xPartInt.CompareTo(yPartInt);
                    }
                    else
                    {
                        result = string.Compare(xParts[i], yParts[i], StringComparison.OrdinalIgnoreCase);
                    }

                    if (result != 0)
                    {
                        return result;
                    }
                }

                return xParts.Length.CompareTo(yParts.Length);
            }
        }
    }
}