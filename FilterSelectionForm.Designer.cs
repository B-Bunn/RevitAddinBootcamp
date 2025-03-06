using System.Windows.Forms;


namespace BDB
{
    partial class FilterSelectionForm
    {
        private System.ComponentModel.IContainer components = null;
        private ListView listViewFilters;
        private Button okButton;
        private Button cancelButton;
        private Button checkAllButton;


        private void InitializeComponent()
        {
            listViewFilters = new ListView();
            okButton = new Button();
            cancelButton = new Button();
            checkAllButton = new Button();
            SuspendLayout();
            // 
            // listViewFilters
            // 
            listViewFilters.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewFilters.CheckBoxes = true;
            listViewFilters.Location = new System.Drawing.Point(12, 12);
            listViewFilters.Name = "listViewFilters";
            listViewFilters.Size = new Size(422, 208);
            listViewFilters.TabIndex = 3;
            listViewFilters.UseCompatibleStateImageBehavior = false;
            listViewFilters.View = System.Windows.Forms.View.List;
            listViewFilters.SelectedIndexChanged += listViewFilters_SelectedIndexChanged_1;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            okButton.Location = new System.Drawing.Point(358, 232);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.TabIndex = 1;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += OkButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.Location = new System.Drawing.Point(277, 232);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += CancelButton_Click;
            // 
            // checkAllButton
            // 
            checkAllButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            checkAllButton.Location = new System.Drawing.Point(12, 232);
            checkAllButton.Name = "checkAllButton";
            checkAllButton.Size = new Size(75, 23);
            checkAllButton.TabIndex = 4;
            checkAllButton.Text = "Check All";
            checkAllButton.UseVisualStyleBackColor = true;
            checkAllButton.Click += CheckAllButton_Click;
            // 
            // FilterSelectionForm
            // 
            ClientSize = new Size(445, 261);
            Controls.Add(checkAllButton);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(listViewFilters);
            Name = "FilterSelectionForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Select Filters";
            TopMost = true;
            ResumeLayout(false);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
