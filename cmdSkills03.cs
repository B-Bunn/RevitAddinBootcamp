using Autodesk.Revit.DB.Architecture;

namespace RevitAddinBootcamp
{
    [Transaction(TransactionMode.Manual)]
    public class cmdSkills03 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Your Module 03 Skills code goes here
            Building building1 = new Building("Big Office Building", "10 Main Street", 10, 150000);
            //building.Name = "Big Office Building";
            //building.Address = "10 Main Street";
            //building.NumberOfFloors = 10;
            //building.Area = 150000;

            Building building2 = new Building("Fancy Hotel", "15 Main Street", 15, 200000);

            building1.NumberOfFloors = 11;

            List<Building> buildings = new List<Building>();
            buildings.Add(building1);
            buildings.Add(building2);
            buildings.Add(new Building("Hospital", "20 Main Street", 20, 350000));
            buildings.Add(new Building("Giant Store", "30 Main Street", 5, 400000));

            Neighborhood downtown = new Neighborhood("Downtown", "Middletown", "CT", buildings);

            TaskDialog.Show("Test", $"There are {downtown.GetBuildingCount()} buildings in the {downtown.Name} neighborhood.");

            //Rooms
            FilteredElementCollector roomCollector = new FilteredElementCollector(doc);
            roomCollector.OfCategory(BuiltInCategory.OST_Rooms);

            Room curRoom = roomCollector.First() as Room;

            string roomName = curRoom.Name;

            

            if (roomName.Contains("1"))
            {
                TaskDialog.Show("room", "Found it");
            }

            Location roomLocation = curRoom.Location;
            LocationPoint roomLocPT = curRoom.Location as LocationPoint;
            XYZ roomPoint = roomLocPT.Point;

            //TaskDialog.Show("Test", $"{roomLocPT.Point}, {roomLocation}");

            return Result.Succeeded;
        }
    }
 }
