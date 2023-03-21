using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace myapp
{
    public partial class Form1 : Form
    {
        private Timer countdownTimer;
        private int remainingTime = 300000; // 5 minutes in milliseconds
        private Label lblCountdown;

        public Form1()
        {
            InitializeComponent();

            // Create a new Panel control and set its properties
            Panel panel = new Panel();
            panel.Name = "panel";
            panel.Dock = DockStyle.None;
            panel.BackColor = Color.White;
            panel.Width = 300;
            panel.Height = 100;
            panel.Anchor = AnchorStyles.None;
            panel.Top = (this.ClientSize.Height - panel.Height) / 2;
            panel.Left = (this.ClientSize.Width - panel.Width) / 2;

            // Create a new Label control and set its properties
            lblCountdown = new Label();
            lblCountdown.Name = "lblCountdown";
            lblCountdown.Text = "05:00.00";
            lblCountdown.Font = new Font("Microsoft Sans Serif", 36F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            lblCountdown.TextAlign = ContentAlignment.MiddleCenter;
            lblCountdown.Anchor = AnchorStyles.None;
            lblCountdown.AutoSize = true;
            lblCountdown.Top = (panel.Height - lblCountdown.Height) / 2;
            lblCountdown.Left = (panel.Width - lblCountdown.Width) / 2;

            // Add the Label control to the panel's Controls collection
            panel.Controls.Add(lblCountdown);

            // Add the Panel control to the form's Controls collection
            this.Controls.Add(panel);

            // Set the form's properties
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Padding = new Padding(0);

            // Create a new Timer control and set its properties
            countdownTimer = new Timer();
            countdownTimer.Interval = 10; // update every 10 milliseconds
            countdownTimer.Tick += new EventHandler(countdownTimer_Tick);
            countdownTimer.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            System.Diagnostics.Process.Start("shutdown", "/r /t 0 /f");
        }

        private void countdownTimer_Tick(object sender, EventArgs e)
        {
            remainingTime -= countdownTimer.Interval;
            TimeSpan ts = TimeSpan.FromMilliseconds(remainingTime);
            lblCountdown.Text = string.Format("{0:D2}:{1:D2}.{2:D2}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            if (remainingTime <= 0)
            {
                countdownTimer.Enabled = false;
                MessageBox.Show("The action is being executed now!");
                button1_Click(null, null); // Execute the action
            }
        }
    }

}



