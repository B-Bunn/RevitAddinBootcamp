using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace RAB_Module02_Skills
{
    [Transaction(TransactionMode.Manual)]
    public class RAB_Module02_Skills : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // This is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // This is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Getting the UIDocument for the current selection
            UIDocument uidoc = uiapp.ActiveUIDocument;

            // Prompt the user to select elements by rectangle
            IList<Element> pickList = uidoc.Selection.PickElementsByRectangle("Select Elements");

            // Defines list type|name of list = creates new list(empty)
            List<CurveElement> allCurves = new List<CurveElement>();
            foreach (Element elem in pickList)
            {
                if (elem is CurveElement)
                {
                    allCurves.Add(elem as CurveElement);
                }
            }

            List<CurveElement> modelCurves = new List<CurveElement>();
            foreach (Element elem in pickList)
            {
                if (elem is CurveElement curve)
                    modelCurves.Add(curve);
                //if (elem is CurveElement)
                //{
                //    CurveElement curveElem = elem as CurveElement;
                //    if (curveElem.CurveElementType == CurveElementType.ModelCurve)
                //    {
                //        modelCurves.Add(elem as CurveElement);
                //    }
                //}
            }

            Parameter levelParameter = doc.ActiveView.LookupParameter("Associated Level");

            //bool bool1 = My1st.BoolUtils.GetBooleanValueFromParameter(levelParameter);

            using (Transaction trans = new Transaction(doc, "Create Walls, Ducts, and Pipes"))
            {
                trans.Start();

                // Create a new Level
                Level newLevel = Level.Create(doc, 20);

                foreach (CurveElement currentCurve in modelCurves)
                {
                    Curve curve = currentCurve.GeometryCurve;
                    GraphicsStyle curveGS = currentCurve.LineStyle as GraphicsStyle;

                    if (curveGS != null)
                    {
                        WallType wallType1 = GetWallTypeByName(doc, "Storefront");
                        WallType wallType2 = GetWallTypeByName(doc, "Exterior - Brick on CMU");
                        DuctType ductType = GetDuctByName(doc, "Default duct");
                        PipeType pipeType = GetPipeByName(doc, "Default pipe");

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

        internal WallType GetWallTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));

            foreach (WallType curType in collector)
            {
                if (curType.Name == typeName)
                    return curType;
            }

            return null;
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
