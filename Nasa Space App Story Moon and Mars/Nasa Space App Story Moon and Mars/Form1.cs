using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nasa_Space_App_Story_Moon_and_Mars
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _bookManager = new BookManager();
            SetupBookLayout();

            // Load initial NASA equipment story collection
            LoadNasaBook();
        }

        private BookManager _bookManager;

        // UI Controls (Can also be created via Visual Studio Designer)
        private Panel pnlBookCover;
        private Label lblBookTitle;
        private PictureBox picEquipment;
        private Label lblPageTitle;
        private RichTextBox txtStory;
        private LinkLabel lnkSource;
        private Button btnPrevious;
        private Button btnNext;
        private Label lblPageNumber;



        private void SetupBookLayout()
        {
            pnlBookCover = new Panel()
            {
                Location = new Point(30, 20),
                Size = new Size(825, 530),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pnlBookCover);

            lblBookTitle = new Label()
            {
                Text = "NASA DIGITAL ARCHIVES: SPACE EQUIPMENT",
                Font = new Font("Georgia", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 43, 76),
                Location = new Point(30, 20),
                AutoSize = true
            };
            pnlBookCover.Controls.Add(lblBookTitle);

            lblPageTitle = new Label()
            {
                Font = new Font("Georgia", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(11, 29, 58),
                Location = new Point(30, 45),
                Size = new Size(760, 35)
            };
            pnlBookCover.Controls.Add(lblPageTitle);

            picEquipment = new PictureBox()
            {
                Location = new Point(30, 90),
                Size = new Size(760, 240),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(240, 240, 245),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlBookCover.Controls.Add(picEquipment);

            txtStory = new RichTextBox()
            {
                Location = new Point(30, 345),
                Size = new Size(760, 145),
                Font = new Font("Calibri", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(50, 50, 50),
                BackColor = Color.White,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            pnlBookCover.Controls.Add(txtStory);

            lnkSource = new LinkLabel()
            {
                Text = "Summary adapted from NASA - read the source",
                Location = new Point(30, 500),
                AutoSize = true,
                Visible = false
            };
            lnkSource.LinkClicked += LnkSource_LinkClicked;
            pnlBookCover.Controls.Add(lnkSource);

            btnPrevious = new Button()
            {
                Text = "< Previous Page",
                Location = new Point(30, 565),
                Size = new Size(130, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnPrevious.Click += BtnPrevious_Click;
            this.Controls.Add(btnPrevious);

            lblPageNumber = new Label()
            {
                Location = new Point(380, 573),
                Size = new Size(130, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblPageNumber);

            btnNext = new Button()
            {
                Text = "Next Page >",
                Location = new Point(725, 565),
                Size = new Size(130, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnNext.Click += BtnNext_Click;
            this.Controls.Add(btnNext);
        }

        private async void LoadNasaBook()
        {
            lblPageTitle.Text = "Querying NASA Archives...";
            txtStory.Text = "Loading mission stories and finding their images in NASA's image library...";
            picEquipment.Image = null;
            btnPrevious.Enabled = false;
            btnNext.Enabled = false;
            lnkSource.Visible = false;

            try
            {
                await _bookManager.InitializeBookAsync();
                if (IsDisposed) return;
                DisplayCurrentPage();
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                lblPageTitle.Text = "Could not load mission stories";
                txtStory.Text = "Check that MissionStories.json is included with the application.\n\n" + ex.Message;
            }
        }

        private void DisplayCurrentPage()
        {
            var page = _bookManager.GetCurrentPage();
            if (page == null) return;

            lblPageTitle.Text = page.Title;
            txtStory.Text = page.Story;
            txtStory.AppendText("\n\n" + (string.IsNullOrEmpty(page.ImageUrl)
                ? "NASA image unavailable. The mission story is still available above."
                : "NASA image: " + page.ImageTitle));
            txtStory.SelectionStart = 0;
            txtStory.ScrollToCaret();
            lnkSource.Tag = page.SourceUrl;
            lnkSource.Visible = !string.IsNullOrWhiteSpace(page.SourceUrl);
            lblPageNumber.Text = $"Page {_bookManager.CurrentIndex + 1} of {_bookManager.TotalPages}";

            try
            {
                picEquipment.CancelAsync();
                picEquipment.Image = null;
                if (!string.IsNullOrEmpty(page.ImageUrl))
                {
                    picEquipment.LoadAsync(page.ImageUrl);
                }
                else
                {
                    picEquipment.Image = null;
                }
            }
            catch (Exception)
            {
                picEquipment.Image = null;
            }

            // Update button states and colors
            btnPrevious.Enabled = _bookManager.HasPrevious;
            btnNext.Enabled = _bookManager.HasNext;

            btnPrevious.BackColor = btnPrevious.Enabled ? Color.FromArgb(200, 210, 225) : Color.FromArgb(235, 235, 235);
            btnNext.BackColor = btnNext.Enabled ? Color.FromArgb(200, 210, 225) : Color.FromArgb(235, 235, 235);
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            _bookManager.NextPage();
            DisplayCurrentPage();
        }

        private void LnkSource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string sourceUrl = lnkSource.Tag as string;
            if (string.IsNullOrWhiteSpace(sourceUrl)) return;

            try
            {
                Process.Start(new ProcessStartInfo(sourceUrl) { UseShellExecute = true });
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Could not open the browser. NASA source:\n" + sourceUrl,
                    "NASA Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnPrevious_Click(object sender, EventArgs e)
        {
            _bookManager.PreviousPage();
            DisplayCurrentPage();
        }
    }
}
