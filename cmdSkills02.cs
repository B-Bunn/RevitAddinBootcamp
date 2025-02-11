using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.Exceptions;
using System.Windows.Controls;

namespace RevitAddinBootcamp
{
    [Transaction(TransactionMode.Manual)]
    public class cmdSkills02 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Your Module 02 Skills code goes here

            //pick single element
            Reference pickRef = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "CLICK THE THING!!!");
            Element pickElement = doc.GetElement(pickRef);

            //pick multiple elements
            //pickelements returns an iList not a real list needs casted to an actual list
            List<Element> pickList = uidoc.Selection.PickElementsByRectangle("CLICK THE THINGS!!").ToList();

            TaskDialog.Show("Test", $"YOU CLICKED {pickList.Count} THINGS!!");

            //filter selected elements for lines

            List<CurveElement> allCurves = new List<CurveElement>();
            foreach (Element elem in pickList)
            {
                if (elem is CurveElement)
                {
                    allCurves.Add(elem as CurveElement);
                }
            }

            TaskDialog.Show("result", $"{allCurves}");

            List<CurveElement> modelCurves = new List<CurveElement>();
            foreach (Element elem2 in pickList)
            {
                if (elem2 is CurveElement)
                {
                    //CurveElement curveElem = elem2 as CurveElement;
                    CurveElement curveElem = (CurveElement)elem2;

                    if (curveElem.CurveElementType == CurveElementType.ModelCurve) ;
                    {
                        modelCurves.Add(curveElem);
                    }
                }
            }

            foreach (CurveElement currentCurve in modelCurves)
            {
                Curve curve = currentCurve.GeometryCurve;
                XYZ startPoint = curve.GetEndPoint(0);
                XYZ endPoint = curve.GetEndPoint(1);

                GraphicsStyle curStyle = currentCurve.LineStyle as GraphicsStyle;

                Debug.Print(curStyle.Name);
            }

            //Transaction t = new Transaction(doc);
            //t.Start("Creating a Wall");

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create a Wall");
                //Create wall
                Level newLevel = Level.Create(doc, 20);

                CurveElement curveElem = modelCurves[0];
                Curve curCurve = curveElem.GeometryCurve;

                Curve curCurve2 = modelCurves[1].GeometryCurve;

                Wall newWall = Wall.Create(doc, curCurve, newLevel.Id, false);

                //create walls with types
                FilteredElementCollector wallTypes = new FilteredElementCollector(doc);
                wallTypes.OfCategory(BuiltInCategory.OST_Walls);
                wallTypes.WhereElementIsElementType();



                Wall newWall2 = Wall.Create(doc, curCurve2, wallTypes.FirstElementId(), newLevel.Id, 20, 0, false, false);

                FilteredElementCollector systemCollector = new FilteredElementCollector(doc);
                systemCollector.OfClass(typeof(MEPSystemType));

                //Create Duct

                MEPSystemType ductSystem = GetSystemTypeByName(doc, "Supply Air");
                //foreach (MEPSystemType systemType in systemCollector)

                //{
                //    if (systemType.Name == "Supply Air")
                //    {
                //        ductSystem = systemType;
                //    }
                //}


                FilteredElementCollector ductCollector = new FilteredElementCollector(doc);
                ductCollector.OfClass(typeof(DuctType));

                Curve curCurve3 = modelCurves[2].GeometryCurve;
                Duct newDuct = Duct.Create(doc, ductSystem.Id, ductCollector.FirstElementId(), newLevel.Id, curCurve3.GetEndPoint(0), curCurve3.GetEndPoint(1));

                //Create Pipe

                MEPSystemType pipeSystem = GetSystemTypeByName(doc, "Domestic Hot Water");
                //foreach (MEPSystemType systemType in systemCollector)

                //{
                //    if (systemType.Name == "Domestic Hot Water")
                //    {
                //        pipeSystem = systemType;
                //    }
                //}


                FilteredElementCollector pipeCollector = new FilteredElementCollector(doc);
                pipeCollector.OfClass(typeof(PipeType));

                Curve curCurve4 = modelCurves[3].GeometryCurve;
                Pipe newPipe = Pipe.Create(doc, pipeSystem.Id, pipeCollector.FirstElementId(), newLevel.Id, curCurve4.GetEndPoint(0), curCurve4.GetEndPoint(1));


                //switch statement
                int numberValue = 5;
                string numberAsString = "";

                switch (numberValue)
                {
                    case 0:
                        numberAsString = "0";
                        break;
                    case 1:
                        numberAsString = "1";
                        break;
                    case 2:
                        numberAsString = "2";
                        break;
                    case 3:
                        numberAsString = "3";
                        break;
                    case 4:
                        numberAsString = "4";
                        break;
                    case 5:
                        numberAsString = "5";
                        break;
                    case 6:
                        numberAsString = "6";
                        break;
                    default:
                        numberAsString = "99";
                        break;


                        t.Commit();
                }


                return Result.Succeeded;
            }
        }

        internal string MyFirstMethod()
        {
            return "this is my first method";
        }

        internal void MySecondMethod()
        {
            Debug.Print("this is my second method");
        }

        internal string MyThirdMethod(string input)
        {
            string returnString = $"this is my third method: {input}";
            return returnString;
        }

        internal MEPSystemType GetSystemTypeByName(Document doc, string name)
        {
            FilteredElementCollector systemCollector = new FilteredElementCollector(doc);
            systemCollector.OfClass(typeof(MEPSystemType));

            //Create Duct

            foreach (MEPSystemType systemType in systemCollector)

            {
                if (systemType.Name == name)
                {
                    return systemType;
                }
            }
            return null;
        }

       
    }
}

