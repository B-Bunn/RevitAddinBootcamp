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
    public class cmdChallenge03 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            string dBPath = My1st.StringUtils.GetDebugFilePath();
            CSVUtils.DeleteCsvFile(dBPath);


            string csvPath = @"C:\Users\BlakeB\Desktop\RAB Challange 3\RAB_Module 03_Furniture List2.csv"; // Change this path


            Dictionary<string, List<FamilyPlacementInfo>> roomData = ReadCSV(csvPath);

            List<Room> rooms = CollectAllRooms(doc, dBPath);



            PlaceFamFromCSV.PlaceFamilyFromCSV(doc, csvPath);




            return Result.Succeeded;

        }

        public static List<Room> CollectMatchedRooms(List<Room> allRooms, HashSet<string> csvRoomNames)
        {
            List<Room> matchedRooms = new List<Room>();

            // Compare each room name in the project against the CSV list
            foreach (Room room in allRooms)
            {
                if (csvRoomNames.Contains(room.Name))
                {
                    matchedRooms.Add(room);
                }
            }

            return matchedRooms;
        }

        public static List<Room> CollectAllRooms(Document doc, string dBPath)
        {
            // Create a filter to collect all rooms in the document
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(SpatialElement)); // Rooms are derived from SpatialElement

            // Create a list to store rooms
            List<Room> rooms = new List<Room>();

            // Iterate through each element and collect rooms
            foreach (Element element in collector)
            {
                if (element is Room room)
                {
                    // Add the room to the list
                    rooms.Add(room);
                    string nameParam = element.LookupParameter("Name").AsValueString();
                    My1st.CSVUtils.WriteStringToCSV(dBPath, $"{room.Name}-{nameParam}");
                }
                string roomDisplay = string.Join("\n", rooms);

            }

            // Return the list of rooms
            return rooms;
        }

        private Dictionary<string, List<FamilyPlacementInfo>> ReadCSV(string filePath)
        {
            Dictionary<string, List<FamilyPlacementInfo>> roomData = new Dictionary<string, List<FamilyPlacementInfo>>();

            if (!File.Exists(filePath))
            {
                TaskDialog.Show("Error", $"CSV file not found: {filePath}");
                return roomData;
            }

            List<string> debugMessages = new List<string>(); // Store debug messages

            foreach (var line in File.ReadLines(filePath).Skip(1)) // Skip header row
            {
                var values = line.Split(',').Select(v => v.Trim()).ToArray();
                if (values.Length < 4) continue; // Ensure all columns exist

                string roomName = values[0];
                string familyName = values[1];
                string typeName = values[2];

                if (!int.TryParse(values[3], out int quantity)) continue; // Ensure quantity is a valid number

                if (!roomData.ContainsKey(roomName))
                    roomData[roomName] = new List<FamilyPlacementInfo>();

                roomData[roomName].Add(new FamilyPlacementInfo(familyName, typeName, quantity));

                // Add to debug messages
                debugMessages.Add($"Room: {roomName}, Family: {familyName}, Type: {typeName}, Qty: {quantity}");
            }

            // Display all CSV data in a TaskDialog
            string debugText = string.Join("\n", debugMessages);
            TaskDialog.Show("CSV Data", debugText.Length > 0 ? debugText : "No valid data found.");

            return roomData;
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
