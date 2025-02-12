using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddinBootcamp
{
    public class DataCollector
    {
        internal static WallType GetWallTypeByName(Document doc, string typeName)
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
