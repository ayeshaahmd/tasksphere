using SE_Project.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SE_Project.Pages
{
    /// <summary>
    /// Timeline page for viewing recent project activity and performing quick project actions.
    /// </summary>
    public class Timeline : UserControl
    {
        private const int ContentWidth = 860;

        private readonly DBHelper db;
        private readonly FlowLayoutPanel timelinePanel;
        private readonly Label emptyLabel;
        private readonly TextBox titleTextBox;
        private readonly TextBox descriptionTextBox;
        private readonly Dictionary<string, int> likeCounts;
        private readonly Dictionary<string, int> dislikeCounts;

        public Timeline()
        {
            db = new DBHelper();
            BackColor = Color.White;
            Dock = DockStyle.Fill;

            titleTextBox = CreateTextBox(new Point(0, 0), new Size(280, 26), string.Empty);
            descriptionTextBox = CreateTextBox(new Point(0, 0), new Size(360, 26), string.Empty);
            emptyLabel = CreateEmptyLabel();
            timelinePanel = CreateTimelinePanel();
            likeCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            dislikeCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            Controls.Add(CreateMainLayout());
            Load += Timeline_Load;
        }

        /// <summary>
        /// Handles the page load event and populates the timeline.
        /// </summary>
        private void Timeline_Load(object sender, EventArgs e)
        {
            LoadTimeline();
        }

        /// <summary>
        /// Loads the project timeline items and renders each row.
        /// </summary>
        private void LoadTimeline()
        {
            timelinePanel.Controls.Clear();
            List<ProjectItem> items = db.GetProjectTimelineItems();

            if (items.Count == 0)
            {
                timelinePanel.Controls.Add(emptyLabel);
                return;
            }

            foreach (ProjectItem item in items)
            {
                timelinePanel.Controls.Add(CreateTimelineRow(item));
            }
        }

        /// <summary>
        /// Adds a new project from the timeline quick-add panel.
        /// </summary>
        private void AddButton_Click(object sender, EventArgs e)
        {
            string result = db.AddProject(titleTextBox.Text, descriptionTextBox.Text);
            MessageBox.Show(result);

            if (!result.Contains("successfully"))
            {
                return;
            }

            titleTextBox.Clear();
            descriptionTextBox.Clear();
            titleTextBox.Focus();
            LoadTimeline();
        }

        private Control CreateMainLayout()
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Color.White;
            layout.ColumnCount = 1;
            layout.RowCount = 4;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Padding = new Padding(24);

            layout.Controls.Add(CreateHeaderPanel(), 0, 0);
            layout.Controls.Add(CreateQuickAddPanel(), 0, 1);
            layout.Controls.Add(CreateDivider(), 0, 2);
            layout.Controls.Add(timelinePanel, 0, 3);

            return layout;
        }

        private Panel CreateHeaderPanel()
        {
            Panel header = new Panel();
            header.Dock = DockStyle.Fill;

            Label title = CreateLabel("Timeline", new Point(0, 0), 28F, FontStyle.Bold, Color.FromArgb(0, 118, 212));
            Label subtitle = CreateLabel("Recent project activity from all lists.", new Point(0, 44), 10F, FontStyle.Regular, Color.Gray);

            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            return header;
        }

        private Panel CreateDivider()
        {
            Panel divider = new Panel();
            divider.Dock = DockStyle.Fill;
            divider.Height = 1;
            divider.BackColor = Color.FromArgb(230, 230, 230);
            return divider;
        }

        private Panel CreateQuickAddPanel()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;

            titleTextBox.Location = new Point(0, 16);
            descriptionTextBox.Location = new Point(296, 16);

            Label titleLabel = CreateLabel("Title", new Point(0, 46), 9F, FontStyle.Regular, Color.Gray);
            Label descriptionLabel = CreateLabel("Description", new Point(296, 46), 9F, FontStyle.Regular, Color.Gray);

            Button addButton = new Button();
            addButton.Text = "Add Project";
            addButton.Font = CreateFont(9F, FontStyle.Bold);
            addButton.Size = new Size(120, 32);
            addButton.Location = new Point(704, 16);
            addButton.BackColor = Color.FromArgb(0, 118, 212);
            addButton.ForeColor = Color.White;
            addButton.FlatStyle = FlatStyle.Flat;
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += AddButton_Click;

            panel.Controls.Add(titleTextBox);
            panel.Controls.Add(descriptionTextBox);
            panel.Controls.Add(addButton);
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(descriptionLabel);
            return panel;
        }

        private FlowLayoutPanel CreateTimelinePanel()
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.TopDown;
            panel.WrapContents = false;
            panel.AutoScroll = true;
            panel.Padding = new Padding(0, 16, 0, 16);
            panel.BackColor = Color.WhiteSmoke;
            return panel;
        }

        private Panel CreateTimelineRowContainer()
        {
            Panel row = new Panel();
            row.Width = ContentWidth;
            row.BackColor = Color.White;
            row.Padding = new Padding(16);
            row.Margin = new Padding(0, 0, 0, 12);
            row.BorderStyle = BorderStyle.FixedSingle;
            row.AutoSize = true;
            row.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            return row;
        }

        private FlowLayoutPanel CreateTimelineActionPanel(int yOffset)
        {
            FlowLayoutPanel actions = new FlowLayoutPanel();
            actions.Location = new Point(28, yOffset);
            actions.AutoSize = true;
            actions.FlowDirection = FlowDirection.LeftToRight;
            actions.WrapContents = false;
            actions.Margin = new Padding(0, 8, 0, 0);
            return actions;
        }

        private Label CreateReactionCountLabel(string projectName, bool isLike)
        {
            int count = GetReactionCount(projectName, isLike);
            Label label = CreateLabel(count.ToString(), new Point(0, 0), 8.5F, FontStyle.Regular, Color.Gray);
            label.AutoSize = true;
            label.Margin = new Padding(4, 6, 12, 0);
            return label;
        }

        private Button CreateReactionButton(string text, string projectName, Label countLabel, bool isLike)
        {
            Button button = CreateActionButton(text);
            button.Click += delegate
            {
                UpdateReactionCount(projectName, countLabel, isLike);
            };
            return button;
        }

        private int GetReactionCount(string projectName, bool isLike)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                return 0;
            }

            Dictionary<string, int> counts = isLike ? likeCounts : dislikeCounts;
            return counts.TryGetValue(projectName, out int value) ? value : 0;
        }

        private void UpdateReactionCount(string projectName, Label countLabel, bool isLike)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                return;
            }

            Dictionary<string, int> counts = isLike ? likeCounts : dislikeCounts;
            int current = counts.TryGetValue(projectName, out int value) ? value : 0;
            current++;
            counts[projectName] = current;
            countLabel.Text = current.ToString();
        }

        private Label CreateEmptyLabel()
        {
            Label label = CreateLabel("No timeline activity yet. Add or update a project to start the timeline.", new Point(0, 0), 11F, FontStyle.Regular, Color.Gray);
            label.AutoSize = true;
            label.Margin = new Padding(16);
            return label;
        }

        private Control CreateTimelineRow(ProjectItem item)
        {
            Panel row = CreateTimelineRowContainer();
            string status = db.GetProjectStatus(item.Name);

            row.Controls.Add(CreateMarker(status));
            row.Controls.Add(CreateRowLabel(item.Name, new Point(28, 6), 12F, FontStyle.Bold, Color.Black));
            row.Controls.Add(CreateRowLabel(status, new Point(28, 32), 8.5F, FontStyle.Bold, Color.FromArgb(0, 118, 212)));
            row.Controls.Add(CreateRowLabel("Started: " + FormatDate(item.CreatedUtc), new Point(28, 52), 8.5F, FontStyle.Regular, Color.Gray));
            row.Controls.Add(CreateRowLabel("Last update: " + FormatDate(item.UpdatedUtc), new Point(28, 68), 8.5F, FontStyle.Regular, Color.Gray));

            string detailsText = string.IsNullOrWhiteSpace(item.Description) ? "No description provided." : item.Description;
            Label details = CreateRowLabel(detailsText, new Point(28, 88), 9.5F, FontStyle.Regular, Color.DimGray);
            details.MaximumSize = new Size(ContentWidth - 220, 0);
            details.AutoSize = true;
            Size detailsSize = TextRenderer.MeasureText(detailsText, details.Font, new Size(ContentWidth - 220, 0), TextFormatFlags.WordBreak);
            details.Size = new Size(ContentWidth - 220, detailsSize.Height);
            row.Controls.Add(details);

            FlowLayoutPanel actions = CreateTimelineActionPanel(details.Bottom + 16);
            actions.Controls.Add(CreateEditButton(item.Name));
            actions.Controls.Add(CreateDeleteButton(item.Name));
            actions.Controls.Add(CreateStatusComboBox(item.Name));

            Label likeLabel = CreateReactionCountLabel(item.Name, true);
            actions.Controls.Add(CreateReactionButton("Like", item.Name, likeLabel, true));
            actions.Controls.Add(likeLabel);

            Label unlikeLabel = CreateReactionCountLabel(item.Name, false);
            actions.Controls.Add(CreateReactionButton("Unlike", item.Name, unlikeLabel, false));
            actions.Controls.Add(unlikeLabel);

            row.Controls.Add(actions);
            row.Height = actions.Bottom + 16;
            return row;
        }

        private void ShowEditProjectDialog(string projectName)
        {
            Form editForm = CreateEditForm();
            TextBox editTitleTextBox = CreateTextBox(new Point(24, 48), new Size(312, 24), projectName);
            TextBox editDescriptionTextBox = CreateTextBox(new Point(24, 112), new Size(312, 58), db.GetProjectDescription(projectName));
            editDescriptionTextBox.Multiline = true;

            Button saveButton = new Button();
            saveButton.Text = "Save";
            saveButton.Font = CreateFont(9F, FontStyle.Bold);
            saveButton.Location = new Point(132, 188);
            saveButton.Size = new Size(96, 30);
            saveButton.Click += delegate
            {
                SaveProjectEdits(editForm, projectName, editTitleTextBox.Text, editDescriptionTextBox.Text);
            };

            editForm.Controls.Add(CreateDialogLabel("New title", new Point(24, 22)));
            editForm.Controls.Add(editTitleTextBox);
            editForm.Controls.Add(CreateDialogLabel("New description", new Point(24, 86)));
            editForm.Controls.Add(editDescriptionTextBox);
            editForm.Controls.Add(saveButton);
            editForm.ShowDialog(this);
        }

        private void SaveProjectEdits(Form editForm, string originalProjectName, string newTitle, string newDescription)
        {
            string result = db.UpdateProject(originalProjectName, newTitle, newDescription);
            MessageBox.Show(result);

            if (!result.Contains("successfully"))
            {
                return;
            }

            editForm.Close();
            LoadTimeline();
        }

        private void DeleteProjectFromTimeline(string projectName)
        {
            DialogResult confirm = MessageBox.Show(
                "Delete this project from every list?",
                "Delete Project",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            string result = db.DeleteProject(projectName);
            MessageBox.Show(result);

            if (result.Contains("successfully"))
            {
                LoadTimeline();
            }
        }

        private void UpdateProjectStatusFromTimeline(string projectName, string status)
        {
            string result = db.SetProjectStatus(projectName, string.Empty, status);
            MessageBox.Show(result);
            LoadTimeline();
        }

        private Button CreateEditButton(string projectName)
        {
            Button button = CreateActionButton("Edit");
            button.Click += delegate { ShowEditProjectDialog(projectName); };
            return button;
        }

        private Button CreateDeleteButton(string projectName)
        {
            Button button = CreateActionButton("Delete");
            button.Click += delegate { DeleteProjectFromTimeline(projectName); };
            return button;
        }

        private ComboBox CreateStatusComboBox(string projectName)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Font = CreateFont(8.5F, FontStyle.Regular);
            comboBox.Items.AddRange(new object[] { "To Do", "InProgress", "Completed" });
            comboBox.Width = 120;
            comboBox.SelectedIndexChanged += delegate
            {
                if (comboBox.SelectedItem != null)
                {
                    UpdateProjectStatusFromTimeline(projectName, comboBox.SelectedItem.ToString());
                }
            };
            return comboBox;
        }

        private Button CreateActionButton(string text)
        {
            Button button = new Button();
            button.Text = text;
            button.Font = CreateFont(8.5F, FontStyle.Bold);
            button.Size = new Size(72, 26);
            button.BackColor = Color.WhiteSmoke;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            return button;
        }

        private Label CreateRowLabel(string text, Point location, float size, FontStyle style, Color color)
        {
            Label label = CreateLabel(text, location, size, style, color);
            label.MaximumSize = new Size(ContentWidth - 120, 0);
            label.AutoSize = true;
            return label;
        }

        private Form CreateEditForm()
        {
            Form form = new Form();
            form.Text = "Edit Project";
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.ClientSize = new Size(360, 242);
            return form;
        }

        private Label CreateDialogLabel(string text, Point location)
        {
            return CreateLabel(text, location, 10F, FontStyle.Bold, Color.Black);
        }

        private TextBox CreateTextBox(Point location, Size size, string text)
        {
            TextBox textBox = new TextBox();
            textBox.Text = text;
            textBox.Font = CreateFont(10F, FontStyle.Regular);
            textBox.Location = location;
            textBox.Size = size;
            return textBox;
        }

        private Label CreateLabel(string text, Point location, float size, FontStyle style, Color color)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = CreateFont(size, style);
            label.ForeColor = color;
            label.Location = location;
            label.AutoSize = true;
            return label;
        }

        private Font CreateFont(float size, FontStyle style)
        {
            return new Font("Century Gothic", size, style);
        }

        private Panel CreateMarker(string action)
        {
            Panel marker = new Panel();
            marker.BackColor = GetActionColor(action);
            marker.Location = new Point(0, 8);
            marker.Size = new Size(16, 16);
            return marker;
        }

        private Color GetActionColor(string action)
        {
            if (string.Equals(action, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                return Color.ForestGreen;
            }
            if (string.Equals(action, "Deleted", StringComparison.OrdinalIgnoreCase))
            {
                return Color.Firebrick;
            }
            if (string.Equals(action, "In Progress", StringComparison.OrdinalIgnoreCase))
            {
                return Color.DarkOrange;
            }
            return Color.FromArgb(0, 118, 212);
        }

        /// <summary>
        /// Formats a stored UTC timestamp into a readable local date string.
        /// </summary>
        private string FormatDate(string createdUtc)
        {
            DateTime created;
            if (!DateTime.TryParse(createdUtc, out created))
            {
                return "Unknown time";
            }
            return created.ToLocalTime().ToString("MMM d, yyyy h:mm tt");
        }
    }
}
