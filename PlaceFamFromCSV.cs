using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.IO;

public class PlaceFamFromCSV
{
    public static void PlaceFamilyFromCSV(Document doc, string csvFilePath)
    {
        // Step 1: Read the CSV and store the data
        List<RoomFamilyTypeQuantity> csvData = ReadCSVFile(csvFilePath);

        // Step 2: Collect all rooms in the project
        List<Room> rooms = CollectAllRooms(doc);
        bool fail = false;
        // Step 3: Iterate over CSV data and place families
        foreach (var cSV
            in csvData)
        {
            // Find matching rooms by name
            foreach (Room room in rooms)
            {
               
                if (room.Name.Contains(cSV.RoomName))
                {
                    // Get the FamilySymbol (Family Type) based on family name and type
                    FamilySymbol familySymbol = GetFamilySymbol(doc, cSV.FamilyName, cSV.TypeName);

                    // Place the family the specified number of times (Quantity)
                    PlaceFamilyInRoom(doc, room, familySymbol, cSV.Quantity);
                }
                else
                {
                    fail = true;

                }
            }
        }
        if (fail == true)
        {
            TaskDialog.Show("test", "Room not match");
        }
    }


    private static List<Room> CollectAllRooms(Document doc)
    {
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(SpatialElement)); // Rooms are derived from SpatialElement

        List<Room> rooms = new List<Room>();

        foreach (Element element in collector)
        {
            if (element is Room room)
            {
                rooms.Add(room);
            }
        }

        return rooms;
    }

    private static List<RoomFamilyTypeQuantity> ReadCSVFile(string filePath)
    {
        List<RoomFamilyTypeQuantity> data = new List<RoomFamilyTypeQuantity>();

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip header if any (adjust based on your CSV format)
                    string[] columns = line.Split(',');

                    if (columns.Length >= 4)
                    {
                        string roomName = columns[0].Trim();
                        string familyName = columns[1].Trim();
                        string typeName = columns[2].Trim();

                        // Get the quantity value and trim any extra spaces
                        string quantityStr = columns[3].Trim();

                        // Debug: Check what's being read from the CSV
                        TaskDialog.Show("Debug1", $"Room: {roomName}, Family: {familyName}, Type: {typeName}, Quantity: {quantityStr}");

                        // Try to parse the Quantity
                        int quantity = 0;
                        if (string.IsNullOrEmpty(quantityStr) || !int.TryParse(quantityStr, out quantity))
                        {
                            // If parsing fails or the quantity is empty, show an error and skip this row
                            //TaskDialog.Show("CSV Error", $"Invalid quantity for room: {roomName}. Provided value: {quantityStr}");
                            continue; // Skip this line
                        }

                        // Add the parsed data to the list
                        data.Add(new RoomFamilyTypeQuantity(roomName, familyName, typeName, quantity));
                    }
                }
            }
        }
        catch (IOException e)
        {
            TaskDialog.Show("Error", "Failed to read the CSV file: " + e.Message);
        }

        return data;
    }

    private static FamilySymbol GetFamilySymbol(Document doc, string familyName, string typeName)
    {
        // Find the family by name
        FilteredElementCollector familyCollector = new FilteredElementCollector(doc);
        familyCollector.OfClass(typeof(FamilySymbol));

        foreach (FamilySymbol symbol in familyCollector)
        {
            if (symbol.Family.Name == familyName && symbol.Name == typeName)
            {
                return symbol;
            }
        }

        throw new Exception($"Family {familyName} with type {typeName} not found in the project.");
    }

    private static void PlaceFamilyInRoom(Document doc, Room room, FamilySymbol familySymbol, int quantity)
    {
        TaskDialog.Show("test", "attempting to place family");
        // Ensure the family symbol is activated before placement
        if (!familySymbol.IsActive)
        {
            familySymbol.Activate();
            doc.Regenerate();
        }

        // Get the room's center point (location)
        LocationPoint roomLocation = room.Location as LocationPoint;
        if (roomLocation != null)
        {
            XYZ roomCenter = roomLocation.Point;

            // Place the family multiple times (Quantity)
            using (Transaction transaction = new Transaction(doc, "Place Family in Room"))
            {
                transaction.Start();

                for (int i = 0; i < quantity; i++)
                {
                    // Place the family instance at the room's center
                    doc.Create.NewFamilyInstance(roomCenter, familySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                }

                transaction.Commit();
            }
        }
        else
        {
            TaskDialog.Show("test", "Loc null");
        }
    }

    // Helper class to store CSV data
    public class RoomFamilyTypeQuantity
    {
        public string RoomName { get; }
        public string FamilyName { get; }
        public string TypeName { get; }
        public int Quantity { get; }

        public RoomFamilyTypeQuantity(string roomName, string familyName, string typeName, int quantity)
        {
            RoomName = roomName;
            FamilyName = familyName;
            TypeName = typeName;
            Quantity = quantity;
        }
    }
}
