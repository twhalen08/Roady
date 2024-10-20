namespace RoadyGUI
{
    public partial class RoadyGUI : Form
    {

        private Bot bot;
        public RoadyGUI()
        {
            InitializeComponent();
            bot = new Bot();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            dialog.Description = "Select Model Cache Directory"; // Optional description text

            dialog.RootFolder = Environment.SpecialFolder.Desktop; // Start browsing from Desktop



            if (dialog.ShowDialog() == DialogResult.OK)

            {

                string selectedFolderPath = dialog.SelectedPath;

                // Do something with the selected folder path

                txtCacheFolder.Text = selectedFolderPath;
                bot.folderPath = selectedFolderPath;
                bot.objFileName = txtFileName.Text;
            }

        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            txtUser.Enabled = false;
            txtPass.Enabled = false;
            txtWorld.Enabled = false;
            btnLogin.Enabled = false;

            bool loggedIn = await bot.Connect(txtUser.Text, txtPass.Text, "RoadyGUI", txtWorld.Text, 0f, 0f, 0f);



            if (loggedIn)
            {

            }
            else
            {
                txtUser.Enabled = true;
                txtPass.Enabled = true;
                txtWorld.Enabled = true;
                btnLogin.Enabled = true;
            }
        }

        private async void btnStartRoad_Click(object sender, EventArgs e)
        {
            bot.roadWidth = float.Parse(txtRoadWidth.Text, System.Globalization.CultureInfo.InvariantCulture);
            bot.uvScaleY = float.Parse(txtUvScaling.Text, System.Globalization.CultureInfo.InvariantCulture);
            bot.segmentsPerSection = int.Parse(txtSegments.Text, System.Globalization.NumberStyles.Integer);
            await bot.StartRoadSession();
            btnStartRoad.Enabled = false;
            btnReset.Enabled = true;

        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            btnReset.Enabled = false;
            if (bot.sessionInProgress)
            {
                bot.ClearNodes();
                btnStartRoad.Enabled = true;
            }

        }

        private void btnSetDimensions_Click(object sender, EventArgs e)
        {
            bot.UpdateDimensions(float.Parse(txtRoadWidth.Text, System.Globalization.CultureInfo.InvariantCulture), float.Parse(txtUvScaling.Text, System.Globalization.CultureInfo.InvariantCulture), int.Parse(txtSegments.Text, System.Globalization.NumberStyles.Integer));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //bot.PlaceObjectsAlongSpline(3, "gr-streetlamp-round01");
        }
    }
}
