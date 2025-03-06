using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
//using B_R_A.UI; 

namespace BDB
{
    [Transaction(TransactionMode.Manual)]
    public class DeleteFiltersCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                // Get all filters in the project
                var filterCollector = new FilteredElementCollector(doc)
                    .OfClass(typeof(ParameterFilterElement))
                    .Cast<ParameterFilterElement>()
                    .ToList();

                // Display filters to user with a form
                using (var form = new FilterSelectionForm(filterCollector))
                {
                    if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        // Delete selected filters
                        using (Transaction trans = new Transaction(doc, "Delete Filters"))
                        {
                            trans.Start();
                            foreach (var filter in form.SelectedFilters)
                            {
                                doc.Delete(filter.Id);
                            }
                            trans.Commit();
                        }
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
