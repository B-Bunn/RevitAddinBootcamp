using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BDB
{
    public partial class FilterSelectionForm : System.Windows.Forms.Form
    {
        public List<ParameterFilterElement> SelectedFilters { get; private set; }

        public FilterSelectionForm(List<ParameterFilterElement> filters)
        {
            InitializeComponent();
            PopulateFilterList(filters);
        }

        private void PopulateFilterList(List<ParameterFilterElement> filters)
        {
            listViewFilters.Items.Clear(); // Clear any existing items

            foreach (var filter in filters)
            {
                // Use the FilterItem class for displaying
                listViewFilters.Items.Add(new ListViewItem { Text = filter.Name, Tag = filter });
            }
        }

        private void OkButton_Click(object sender, System.EventArgs e)
        {
            SelectedFilters = new List<ParameterFilterElement>();

            foreach (ListViewItem item in listViewFilters.CheckedItems)
            {
                if (item.Tag is ParameterFilterElement filter)
                {
                    SelectedFilters.Add(filter); // Add the filter to the selected list
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CheckAllButton_Click(object sender, System.EventArgs e)
        {
            foreach (ListViewItem item in listViewFilters.Items)
            {
                item.Checked = true;
            }
        }

        private void ListViewFilters_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            // Optionally handle selection changes here
        }

        private void listViewFilters_SelectedIndexChanged_1(object sender, System.EventArgs e)
        {

        }
    }
}
