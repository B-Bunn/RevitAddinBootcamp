using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Form = System.Windows.Forms.Form; // Add this line to resolve ambiguity

namespace BDB
{

    [Transaction(TransactionMode.Manual)]
    public class FilterSelectionByType : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Collect all selected elements in the active view
            List<Element> selectedElements = uiDoc.Selection.GetElementIds()
                .Select(id => doc.GetElement(id))
                .ToList();

            // Group elements by Category, Family, and Type
            var groupedElements = selectedElements
                .GroupBy(e => new ElementGroupKey { Category = e.Category?.Name ?? "Unknown", Family = GetFamilyName(e), Type = e.Name })
                .OrderBy(g => g.Key.Category)
                .ThenBy(g => g.Key.Family)
                .ThenBy(g => g.Key.Type)
                .ToList();

            // Show selection UI
            using (SelectionFilterForm form = new SelectionFilterForm(groupedElements))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Apply selection
                    IList<ElementId> selectedIds = form.GetSelectedElementIds();
                    if (selectedIds.Count > 0)
                    {
                        uiDoc.Selection.SetElementIds(selectedIds);
                    }
                    else
                    {
                        TaskDialog.Show("Selection Filter", "No elements were selected.");
                    }
                }
            }

            return Result.Succeeded;
        }

        private string GetFamilyName(Element e)
        {
            FamilyInstance fi = e as FamilyInstance;
            return fi?.Symbol?.Family?.Name ?? "Unknown";
        }

        public class ElementGroupKey
        {
            public string Category { get; set; }
            public string Family { get; set; }
            public string Type { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is ElementGroupKey other)
                {
                    return Category == other.Category && Family == other.Family && Type == other.Type;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (Category, Family, Type).GetHashCode();
            }
        }
    }

    // Windows Form UI for selection
    public class SelectionFilterForm : Form
    {
        private TreeView treeView;
        private Button btnApply;
        private Button btnCheckAll; // Add Check All button
        private Button btnUncheckAll; // Add Uncheck All button
        private Button btnExpandAll; // Add Expand All button
        private Button btnCollapseAll; // Add Collapse All button
        private List<IGrouping<FilterSelectionByType.ElementGroupKey, Element>> groupedElements; // Use the fully qualified name

        private bool isExpanded = false; // Track the expand/collapse state
        private bool isChecked = false; // Track the check/uncheck state

        public SelectionFilterForm(List<IGrouping<FilterSelectionByType.ElementGroupKey, Element>> elements) // Use the fully qualified name
        {
            this.groupedElements = elements;
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Selection Filter";
            this.Size = new System.Drawing.Size(500, 700);
            this.StartPosition = FormStartPosition.CenterParent; // Center the form in the parent

            treeView = new TreeView { Dock = DockStyle.Top, Height = 525, CheckBoxes = true };
            treeView.AfterCheck += TreeView_AfterCheck; // Add event handler for AfterCheck event
            btnApply = new Button { Text = "Apply Selection", Dock = DockStyle.Bottom };
            btnCheckAll = new Button { Text = "Check All", Dock = DockStyle.Bottom }; // Initialize Check All button
            btnUncheckAll = new Button { Text = "Uncheck All", Dock = DockStyle.Bottom }; // Initialize Uncheck All button
            btnExpandAll = new Button { Text = "Expand All", Dock = DockStyle.Bottom }; // Initialize Expand All button
            btnCollapseAll = new Button { Text = "Collapse All", Dock = DockStyle.Bottom }; // Initialize Collapse All button

            // Populate tree view
            foreach (var group in groupedElements)
            {
                TreeNode categoryNode = treeView.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == group.Key.Category);
                if (categoryNode == null)
                {
                    categoryNode = new TreeNode(group.Key.Category);
                    treeView.Nodes.Add(categoryNode);
                }

                TreeNode familyNode = categoryNode.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == group.Key.Family);
                if (familyNode == null)
                {
                    familyNode = new TreeNode(group.Key.Family);
                    categoryNode.Nodes.Add(familyNode);
                }

                TreeNode typeNode = new TreeNode($"{group.Key.Type} ({group.Count()})") { Tag = group.Key.Type };
                familyNode.Nodes.Add(typeNode);
            }

            btnApply.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); }; // Close the form when Apply is clicked
            btnCheckAll.Click += (s, e) => CheckAllItems(); // Add event handler for Check All button
            btnUncheckAll.Click += (s, e) => UncheckAllItems(); // Add event handler for Uncheck All button
            btnExpandAll.Click += (s, e) => ExpandAllNodes(); // Add event handler for Expand All button
            btnCollapseAll.Click += (s, e) => CollapseAllNodes(); // Add event handler for Collapse All button

            this.Controls.Add(treeView);
            this.Controls.Add(btnApply);
            this.Controls.Add(btnCheckAll); // Add Check All button to the form
            this.Controls.Add(btnUncheckAll); // Add Uncheck All button to the form
            this.Controls.Add(btnExpandAll); // Add Expand All button to the form
            this.Controls.Add(btnCollapseAll); // Add Collapse All button to the form

            this.KeyPreview = true; // Enable key preview to capture key events
            this.KeyDown += new KeyEventHandler(SelectionFilterForm_KeyDown); // Add key event handler
        }

        private void TreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Ensure the event is not triggered recursively
            treeView.AfterCheck -= TreeView_AfterCheck;

            // Check or uncheck all child nodes
            CheckAllChildNodes(e.Node, e.Node.Checked);

            // Reattach the event handler
            treeView.AfterCheck += TreeView_AfterCheck;
        }

        private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
        {
            foreach (TreeNode node in treeNode.Nodes)
            {
                node.Checked = nodeChecked;
                CheckAllChildNodes(node, nodeChecked);
            }
        }

        private void SelectionFilterForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnApply.PerformClick(); // Simulate click on Apply button when Enter is pressed
            }
            else if (e.KeyCode == Keys.A)
            {
                btnApply.PerformClick(); // Simulate click on Apply button when Enter is pressed
            }
            else if (e.KeyCode == Keys.E)
            {
                if (isExpanded)
                {
                    CollapseAllNodes(); // Collapse all nodes if currently expanded
                }
                else
                {
                    ExpandAllNodes(); // Expand all nodes if currently collapsed
                }
                isExpanded = !isExpanded; // Toggle the expand/collapse state
            }
            else if (e.KeyCode == Keys.C)
            {
                if (isChecked)
                {
                    UncheckAllItems(); // Uncheck all items if currently checked
                }
                else
                {
                    CheckAllItems(); // Check all items if currently unchecked
                }
                isChecked = !isChecked; // Toggle the check/uncheck state
            }
        }

        private void CheckAllItems()
        {
            foreach (TreeNode node in treeView.Nodes)
            {
                CheckAllNodes(node);
            }
        }

        private void CheckAllNodes(TreeNode treeNode)
        {
            treeNode.Checked = true;
            foreach (TreeNode node in treeNode.Nodes)
            {
                CheckAllNodes(node);
            }
        }

        private void UncheckAllItems()
        {
            foreach (TreeNode node in treeView.Nodes)
            {
                UncheckAllNodes(node);
            }
        }

        private void UncheckAllNodes(TreeNode treeNode)
        {
            treeNode.Checked = false;
            foreach (TreeNode node in treeNode.Nodes)
            {
                UncheckAllNodes(node);
            }
        }

        private void ExpandAllNodes()
        {
            foreach (TreeNode node in treeView.Nodes)
            {
                node.ExpandAll();
            }
        }

        private void CollapseAllNodes()
        {
            foreach (TreeNode node in treeView.Nodes)
            {
                node.Collapse();
            }
        }

        public IList<ElementId> GetSelectedElementIds()
        {
            List<ElementId> selectedIds = new List<ElementId>();

            foreach (TreeNode categoryNode in treeView.Nodes)
            {
                foreach (TreeNode familyNode in categoryNode.Nodes)
                {
                    foreach (TreeNode typeNode in familyNode.Nodes)
                    {
                        if (typeNode.Checked)
                        {
                            var group = groupedElements.First(g => g.Key.Category == categoryNode.Text && g.Key.Family == familyNode.Text && g.Key.Type == (string)typeNode.Tag);
                            selectedIds.AddRange(group.Select(e => e.Id));
                        }
                    }
                }
            }

            return selectedIds;
        }
    }
}