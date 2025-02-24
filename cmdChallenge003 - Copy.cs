using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using My1st;
using RevitAddinBootcamp.Common;

namespace RevitAddinBootcamp
{
    [Transaction(TransactionMode.Manual)]
    public class cmdChallenge003 : IExternalCommand
    {
        


         public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string dbPath = My1st.StringUtils.GetDebugFilePath();
            CSVUtils.DeleteCsvFile(dbPath);
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            string csvPath = @"C:\Users\BlakeB\Desktop\RAB Challange 3\RAB_Module 03_Furniture List2.csv"; // Change this path

            List<string> lines = My1st.CSVUtils.ReadStringsFromCsv(csvPath);


            // Read CSV data
            Dictionary<string, List<FamilyPlacementInfo>> roomData = ReadCSV(csvPath);
            if (roomData.Count == 0)
            {
                TaskDialog.Show("Error", "No valid data found in CSV.");
                return Result.Failed;
            }
            CSVUtils.WriteStringToCsv(dbPath, "Step 1");

            using (Transaction trans = new Transaction(doc, "Insert Families into Rooms"))
            {
                trans.Start();

                foreach (var entry in roomData)
                {
                    string roomName = entry.Key;
                    Room room = GetRoomByName(doc, roomName);

                    if (room == null)
                    {
                        TaskDialog.Show("Error", $"Room '{roomName}' not found.");
                        continue;
                    }

                    XYZ roomCenter = GetRoomCenter(room);

                    foreach (var item in entry.Value)
                    {
                        FamilySymbol familySymbol = Utils.GetFamilySymbolByName(doc, item.FamilyName, item.TypeName);
                        if (familySymbol == null)
                        {
                            TaskDialog.Show("Error", $"Family '{item.FamilyName}' of type '{item.TypeName}' not found.");
                            continue;
                        }

                        if (!familySymbol.IsActive)
                        {
                            familySymbol.Activate();
                            doc.Regenerate();
                        }

                        for (int i = 0; i < item.Quantity; i++)
                        {
                            XYZ placementPoint = GetOffsetPoint(roomCenter, i);
                            doc.Create.NewFamilyInstance(placementPoint, familySymbol, StructuralType.NonStructural);
                        }
                    }
                }

                trans.Commit();
            }

            TaskDialog.Show("Success", "Families placed successfully.");
            return Result.Succeeded;
        }

        private Dictionary<string, List<FamilyPlacementInfo>> ReadCSV(string filePath)
        {
            Dictionary<string, List<FamilyPlacementInfo>> data = new Dictionary<string, List<FamilyPlacementInfo>>();

            if (!File.Exists(filePath))
            {
                TaskDialog.Show("Error", $"CSV file not found: {filePath}");
                return data;
            }

            foreach (var line in File.ReadLines(filePath).Skip(1))
            {
                var values = line.Split(',').Select(v => v.Trim()).ToArray();
                if (values.Length < 4) continue;

                string room = values[0];
                string family = values[1];
                string type = values[2];
                if (!int.TryParse(values[3], out int quantity)) continue;

                if (!data.ContainsKey(room))
                    data[room] = new List<FamilyPlacementInfo>();

                data[room].Add(new FamilyPlacementInfo(family, type, quantity));
            }

            return data;
        }

        private Room GetRoomByName(Document doc, string roomName)
        {
            // Trim spaces to avoid mismatches
            roomName = "Classroom";

            // Collect all rooms in the document (ignoring phase)
            var rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(r => r.Name.Trim().Equals(roomName, StringComparison.OrdinalIgnoreCase))
                .ToList();


            return rooms.First();
        }


        private XYZ GetRoomCenter(Room room)
        {
            BoundingBoxXYZ bbox = room.get_BoundingBox(null);
            return (bbox.Min + bbox.Max) / 2;
        }

        private XYZ GetOffsetPoint(XYZ basePoint, int index)
        {
            return basePoint + new XYZ(0.5 * index, 0, 0);
        }

        private class FamilyPlacementInfo
        {
            public string FamilyName { get; }
            public string TypeName { get; }
            public int Quantity { get; }

            public FamilyPlacementInfo(string familyName, string typeName, int quantity)
            {
                FamilyName = familyName;
                TypeName = typeName;
                Quantity = quantity;
            }
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnChallenge03";
            string buttonTitle = "Module\r03";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Module03,
                Properties.Resources.Module03,
                "Module 03 Challenge");

            return myButtonData.Data;
        }
    }

}
