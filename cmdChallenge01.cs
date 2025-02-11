namespace RevitAddinBootcamp
{
    [Transaction(TransactionMode.Manual)]
    public class cmdChallenge01 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector collector1 = new FilteredElementCollector(doc);
            collector1.OfClass(typeof(ViewFamilyType));

            ViewFamilyType floorPlanVFT = null;

            foreach (ViewFamilyType curVFT in collector1)
            {
                if (curVFT.ViewFamily == ViewFamily.FloorPlan)
                {
                    floorPlanVFT = curVFT;
                }
            }

            ViewFamilyType ceilingPlanVFT = null;
            foreach (ViewFamilyType curVFT in collector1)
            {
                if (curVFT.ViewFamily == ViewFamily.CeilingPlan)
                {
                    ceilingPlanVFT = curVFT;
                }
            }

            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            collector2.OfCategory(BuiltInCategory.OST_TitleBlocks);
            collector2.WhereElementIsElementType();

            Transaction t = new Transaction(doc);
            t.Start("Fizz Buzz");

            // Your Module 01 Challenge code goes here
            int number = 250;

            int startingEl = 0;

            int floorHeight = 15;

            List<int> FizzBuzz_ = new List<int>();

            Level newLevel = Level.Create(doc, floorHeight);

            for (int i = startingEl; i <= number; i++)
            {
                floorHeight += i;
                FizzBuzz_.Add(i);
            }

            foreach (int currentFloorHeight in FizzBuzz_)
            {
                double remainder1 = currentFloorHeight % 3;
                double remainder2 = currentFloorHeight % 5;
                if (remainder1 == 0 && remainder2 == 0)
                {
                    ViewSheet newSheet = ViewSheet.Create(doc, collector2.FirstElementId());
                    newSheet.Name = "FizzBuzz_" + currentFloorHeight;
                }
                else if (remainder1 == 0 && remainder2 != 0)
                {
                    ViewPlan newFloorPlan = ViewPlan.Create(doc, floorPlanVFT.Id, newLevel.Id);
                    newFloorPlan.Name = "FIZZ_" + currentFloorHeight;
                }
                else if (remainder1 != 0 && remainder2 == 0)
                {
                    ViewPlan newCeilingPlan = ViewPlan.Create(doc, ceilingPlanVFT.Id, newLevel.Id);
                    newCeilingPlan.Name = "BUZZ_" + currentFloorHeight;
                }
            }



            t.Commit();





            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnChallenge01";
            string buttonTitle = "Module\r01";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Module01,
                Properties.Resources.Module01,
                "Module 01 Challenge");

            return myButtonData.Data;
        }
    }

}
