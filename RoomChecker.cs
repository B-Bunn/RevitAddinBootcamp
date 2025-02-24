using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Architecture;

[Transaction(TransactionMode.Manual)]
public class CheckPipesInRoomsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIApplication uiApp = commandData.Application;
        UIDocument uidoc = uiApp.ActiveUIDocument;
        Document doc = uidoc.Document;

        try
        {
            FilteredElementCollector roomCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType();

            List<string> roomsWithPipes = new List<string>();

            foreach (Room room in roomCollector)
            {
                if (room == null || room.Location == null) continue;

                BoundingBoxXYZ roomBB = room.get_BoundingBox(null);
                Outline outline = new Outline(roomBB.Min, roomBB.Max);
                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

                FilteredElementCollector pipeCollector = new FilteredElementCollector(doc)
                    .OfClass(typeof(Pipe))
                    .WherePasses(filter);

                if (pipeCollector.GetElementCount() > 0)
                {
                    roomsWithPipes.Add(room.Name);
                }
            }

            string resultMessage = roomsWithPipes.Count > 0
                ? $"Rooms containing pipes:\n{string.Join("\n", roomsWithPipes)}"
                : "No rooms contain pipes.";

            TaskDialog.Show("Room Pipe Check", resultMessage);

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}
