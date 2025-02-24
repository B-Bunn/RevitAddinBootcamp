using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.Exceptions;
using System.Windows.Controls;

namespace RevitAddinBootcamp
{
    [Transaction(TransactionMode.Manual)]
    public class cmdChallenge02 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;


            List<Element> pickList = uidoc.Selection.PickElementsByRectangle("CLICK THE THINGS!!").ToList();

            List<CurveElement> allCurves = new List<CurveElement>();
            foreach (Element elem in pickList)
            {
                if (elem is CurveElement)
                {
                    allCurves.Add(elem as CurveElement);
                }
            }

            using (Transaction trans = new Transaction(doc, "Create Walls, Ducts, and Pipes"))
            {
                trans.Start();

                // Create a new Level
                Level newLevel = Level.Create(doc, 20);


                // Get level
                View curView = doc.ActiveView;
                Parameter levelParam = curView.LookupParameter("Associated Level");
                //Parameter levelParam2 = curView.get_Parameter(BuiltInParameter.ASSOCIATED_LEVEL);
                string levelName = levelParam.AsString();
                ElementId levelId = levelParam.AsElementId();

                Level currentLevel = GetLevelByName(doc, levelName);

                foreach (CurveElement currentCurve in allCurves)
                {
                    Curve curve = currentCurve.GeometryCurve;
                    GraphicsStyle curveGS = currentCurve.LineStyle as GraphicsStyle;

                    if (curveGS != null)
                    {
                        WallType wallType1 = DataCollector.GetWallTypeByName(doc, "Storefront");
                        WallType wallType2 = DataCollector.GetWallTypeByName(doc, "Exterior - Brick on CMU");
                        DuctType ductType = GetDuctByName(doc, "Default");
                        PipeType pipeType = GetPipeByName(doc, "Default");

                        MEPSystemType ductSystemType = GetMEPSystemType(doc, "Supply Air");
                        MEPSystemType pipeSystemType = GetMEPSystemType(doc, "Domestic Cold Water");

                        //TaskDialog.Show("Graphics Style", $"Current curve GraphicsStyle.Name: {curveGS.Name}");

                        switch (curveGS.Name)
                        {
                            case "A-GLAZ":
                                Wall.Create(doc, curve, wallType1.Id, newLevel.Id, 20, 0, false, false);
                                break;

                            case "A-WALL":
                                Wall.Create(doc, curve, wallType2.Id, newLevel.Id, 20, 0, false, false);
                                break;

                            case "M-DUCT":
                                Duct.Create(doc, ductSystemType.Id, ductType.Id, newLevel.Id, curve.GetEndPoint(0), curve.GetEndPoint(1));
                                break;

                            case "P-PIPE":
                                Pipe.Create(doc, pipeSystemType.Id, pipeType.Id, newLevel.Id, curve.GetEndPoint(0), curve.GetEndPoint(1));
                                break;

                            default:
                                //TaskDialog.Show("Warning", "No action taken for this line style.");
                                break;
                        }
                    }
                    else
                    {
                        //TaskDialog.Show("Error", "Curve does not have a valid GraphicsStyle.");
                        trans.RollBack();
                        return Result.Failed;
                    }                  
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnChallenge02";
            string buttonTitle = "Module\r02";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Module02,
                Properties.Resources.Module02,
                "Module 02 Challenge");

            return myButtonData.Data;
        }

        
        internal DuctType GetDuctByName(Document doc, string typeName)
        {
            FilteredElementCollector ductCollector = new FilteredElementCollector(doc);
            ductCollector.OfClass(typeof(DuctType));

            foreach (DuctType curDuctType in ductCollector)
            {
                if (curDuctType.Name == typeName)
                {
                    return curDuctType;
                }
            }

            return null;
        }

        internal PipeType GetPipeByName(Document doc, string typeName)
        {
            FilteredElementCollector pipeCollector = new FilteredElementCollector(doc);
            pipeCollector.OfClass(typeof(PipeType));

            foreach (PipeType curPipeType in pipeCollector)
            {
                if (curPipeType.Name == typeName)
                {
                    return curPipeType;
                }
            }

            return null;
        }

        internal Level GetLevelByName(Document doc, string levelName)
        {
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            levelCollector.OfCategory(BuiltInCategory.OST_Levels);
            levelCollector.WhereElementIsNotElementType();

            foreach (Level curLevel in levelCollector)
            {
                if (curLevel.Name == levelName)
                {
                    return curLevel;
                }
            }

            return null;
        }

        internal MEPSystemType GetMEPSystemType(Document doc, string typeName)
        {
            FilteredElementCollector mepCollector = new FilteredElementCollector(doc);
            mepCollector.OfClass(typeof(MEPSystemType));

            foreach (MEPSystemType curType in mepCollector)
            {
                if (curType.Name == typeName)
                {
                    return curType;
                }
            }

            return null;
        }
    }
}
