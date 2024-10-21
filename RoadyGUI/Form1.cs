using Newtonsoft.Json;

namespace RoadyGUI
{
    public partial class RoadyGUI : Form
    {

        private Bot bot;
        public RoadyGUI()
        {
            InitializeComponent();
            bot = new Bot();
            LoadConfig();

        }

        private void LoadConfig()
        {
            string configPath = "config.json"; // Path to your config file

            if (File.Exists(configPath))
            {
                // Read the config file
                var json = File.ReadAllText(configPath);
                dynamic config = JsonConvert.DeserializeObject(json);

                // Populate textboxes with data from the config file
                txtUser.Text = config.Username;
                txtWorld.Text = config.World;
                txtFileName.Text = config.FileName;
                txtCacheFolder.Text = config.FilePath;
                txtRoadWidth.Text = config.RoadWidth;
                txtUvScaling.Text = config.UVScale;
                txtSegments.Text = config.Segments;
                chkDoubleSided.Checked = bool.Parse((string)config.TwoSided);


                bot.UpdateDimensions(float.Parse(txtRoadWidth.Text, System.Globalization.CultureInfo.InvariantCulture), float.Parse(txtUvScaling.Text, System.Globalization.CultureInfo.InvariantCulture), int.Parse(txtSegments.Text, System.Globalization.NumberStyles.Integer), chkDoubleSided.Checked);
                bot.folderPath = txtCacheFolder.Text;
                bot.objFileName = txtFileName.Text;
            }
            else
            {
                MessageBox.Show("Config file not found.");
            }
        }

        private void SaveConfig()
        {
            var config = new
            {
                Username = txtUser.Text,
                World = txtWorld.Text,
                FileName = txtFileName.Text,
                FilePath = txtCacheFolder.Text,
                RoadWidth = txtRoadWidth.Text,
                UVScale = txtUvScaling.Text,
                Segments = txtSegments.Text,
                TwoSided = chkDoubleSided.Checked.ToString().ToLower() // convert boolean to string
            };

            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText("config.json", json);
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
                SaveConfig();
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
                SaveConfig();
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
            bot.UpdateDimensions(float.Parse(txtRoadWidth.Text, System.Globalization.CultureInfo.InvariantCulture), float.Parse(txtUvScaling.Text, System.Globalization.CultureInfo.InvariantCulture), int.Parse(txtSegments.Text, System.Globalization.NumberStyles.Integer), chkDoubleSided.Checked);
            SaveConfig();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //bot.PlaceObjectsAlongSpline(3, "gr-streetlamp-round01");
        }


    }
}
