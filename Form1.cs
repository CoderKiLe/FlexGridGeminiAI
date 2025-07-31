using FlexGridGeminiAI.Data;
using FlexGridGeminiAI.Helpers;
using FlexGridGeminiAI.Interface;
using FlexGridGeminiAI.Services;
using FlexGridGeminiAI.Views.Forms;
using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;

namespace FlexGridGeminiAI
{
    public partial class Form1 : Form
    {
        private readonly IApiKeyService _geminiKeyService = new GeminiApiKeyService();
        private readonly IApiKeyService _groqKeyService = new GroqApiKeyService();
        #region Fields

        private IDataSource _dataSource = new DataSource();
        private readonly IPromptGenerator _generateAiPrompt = new BuildAIProompt();
        private IAIModel _geminiModel;
        private IAIModel _groqModel;
        private string _sqlQuery;
        private ModelFactory mf = new ModelFactory();
        private const string TableListQuery = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'";
        #endregion

        public Form1()
        {
            InitializeComponent();
            InitializeDashBoard();
            responseTextBoxC1.TabStop = false; // Prevents the response text box from being focused when the form loads

            this.Shown += Form1_Shown;
        }

        #region UI Initialization
        private void InitializeDashBoard()
        {
            // Temporarily disconnect events during setup
            dataTableDd.SelectedIndexChanged -= dataTableDd_SelectedIndexChanged;
            dataSourceDd.SelectedIndexChanged -= dataSourceDd_SelectedIndexChanged;

            _geminiModel = mf.CreateGeminiModel();
            _groqModel = mf.CreateGroqModel();

            loadingIconPictureBox.Visible = false;

            _dataSource.SetDataSource(BuildSelectedQuerySource("C1NWind.db"));
            dataTableDd.DataSource = _dataSource.GetDataTables(TableListQuery);
            _sqlQuery = BuildSelectQuery("Appointees");

            LoadComboBoxes();
            InitGridView();
            InitLoadingIcon();

            // Reconnect events after setup
            dataSourceDd.SelectedIndexChanged += dataSourceDd_SelectedIndexChanged;
            dataTableDd.SelectedIndexChanged += dataTableDd_SelectedIndexChanged;
        }

        /// <summary>
        /// Sets the initial position of the loading icon
        /// </summary>
        private void InitLoadingIcon()
        {
            int centerX = (responseSidePanel.Width - loadingIconPictureBox.Width) / 2;
            int centerY = (responseSidePanel.Height - loadingIconPictureBox.Height) / 2;

            loadingIconPictureBox.Location = new Point(centerX, centerY);
        }

        /// <summary>
        /// Loadthe combobox data dropdown
        /// </summary>
        private void LoadComboBoxes()
        {
            var dataSrc = DataBaseFileLocator.GetDabatBaseFiles(Environment.
                GetFolderPath(Environment.SpecialFolder.Personal) + @"\ComponentOne Samples\Common");
            dataSourceDd.DataSource = dataSrc;
            dataSourceDd.SelectedItem = "Products";
            dataTableDd.SelectedItem = "NORTHWND.db";
            aiModelComboBox.DataSource = new List<IAIModel>
            {
                _geminiModel,
                _groqModel
            };
            aiModelComboBox.DisplayMember = "Name";

        }

        /// <summary>
        /// Initialiaed the component one flexgrid
        /// </summary>
        private void InitGridView()
        {
            var dataTable = _dataSource.GetRows(_sqlQuery);

            flexGridC1.DataSource = dataTable;
            flexGridC1.AllowPinning = C1.Win.FlexGrid.AllowPinning.ColumnRange;
            flexGridC1.ShowFilterIcon = C1.Win.FlexGrid.FilterIconVisibility.Always;
            flexGridC1.AllowFiltering = true;
        }

        #endregion

        #region Event Handling

        //Check API keys on form load
        private void Form1_Shown(object sender, EventArgs e)
        {
            if (!_geminiKeyService.ApiKeyExists() && !_groqKeyService.ApiKeyExists())
            {
                MessageBox.Show("Please set at least one API key in the settings.");
            }
        }

        private async void submitPromptBtn_Click(object sender, EventArgs e)
        {
            await ApplyAIPromptAsync();
        }

        private async void promptTextBoxC1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.ActiveControl = null;
                await ApplyAIPromptAsync();
                e.Handled = true; // Optional: suppress default Enter behavior
            }
        }
        private void ResponseSidePanel_ResizeEvent(object sender, EventArgs e)
        {
            int centerX = (responseSidePanel.Width - loadingIconPictureBox.Width) / 2;
            int centerY = (responseSidePanel.Height - loadingIconPictureBox.Height) / 2;

            loadingIconPictureBox.Location = new Point(centerX, centerY);
        }

        private void dataSourceDd_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dataSourceDd.SelectedItem is not string selectedTable
                || string.IsNullOrWhiteSpace(selectedTable)) return;

            _dataSource.SetDataSource(BuildSelectedQuerySource(selectedTable));
            dataTableDd.DataSource = _dataSource.GetDataTables(TableListQuery);

            InitGridView();
        }

        /// <summary>
        /// Apply gemini prompt
        /// </summary>
        /// <returns></returns>
        private async Task ApplyAIPromptAsync()

        {
            loadingIconPictureBox.Visible = true;
            sendPictureIcon.Enabled = false;
            settingsPictureIcon.Enabled = false;
            clearPictureIcon.Enabled = false;
            ClearTextFields();

            ModelFactory mfact = new ModelFactory();

            var schema = _dataSource.GetDataTableSchema(_dataSource.GetRows(_sqlQuery));

            var prompt = _generateAiPrompt.GenerateGeminiPrompt(promptTextBoxC1.Text, schema);

            var selectedModel = aiModelComboBox.SelectedItem as IAIModel;

            if (selectedModel.Name == "Gemini")
            {
                selectedModel = mfact.CreateGeminiModel();
            }
            else if (selectedModel.Name == "Groq")
            {
                selectedModel = mfact.CreateGroqModel();
            }
            else
            {
                selectedModel = mfact.CreateGeminiModel();
            }

            try
            {
                var response = await selectedModel.GetResponse(prompt);
                try
                {
                    flexGridC1.FilterDefinition = response;
                }
                catch (Exception)
                {
                    MessageBox.Show(response);
                }
                finally
                {
                    loadingIconPictureBox.Visible = false;
                    responseTextBoxC1.Text = response;
                    sendPictureIcon.Enabled = true;
                    settingsPictureIcon.Enabled = true;
                    clearPictureIcon.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Key Invalid: {ex}");
            }
        }
        private void clearPromptBtn_Click(object sender, EventArgs e)
        {
            promptTextBoxC1.Text = "";
            responseTextBoxC1.Text = "";
            flexGridC1.ClearFilter();
        }
        private void resetApiKeyBtn_Click(object sender, EventArgs e)
        {
            var inputForm = new APIKeyInputForm();
            inputForm.StartPosition = FormStartPosition.CenterParent;
            inputForm.ShowDialog();
        }

        private void dataTableDd_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dataTableDd.SelectedItem is not string selectedTable
                || string.IsNullOrWhiteSpace(selectedTable)) return;



            _sqlQuery = BuildSelectQuery(selectedTable);
            InitGridView();
        }

        private static string BuildSelectQuery(string tableName)
        {
            var sanitized = tableName.Replace("`", "``");
            return $"SELECT * FROM `{sanitized}`";
        }

        private static string BuildSelectedQuerySource(string sourceName)
        {
            return @$"\{sourceName}";
        }

        private void ClearTextFields()
        {
            responseTextBoxC1.Text = "AI response will appear here...";
        }

        private void settingsPictureIcon_Click(object sender, EventArgs e)
        {
            var inputForm = new APIKeyInputForm();
            inputForm.StartPosition = FormStartPosition.CenterParent;
            inputForm.ShowDialog();
        }

        private void clearPictureIcon_Click(object sender, EventArgs e)
        {
            promptTextBoxC1.Text = "";
            responseTextBoxC1.Text = "";
            flexGridC1.ClearFilter();
        }

        private async void sendPictureIcon_Click(object sender, EventArgs e)
        {
            await ApplyAIPromptAsync();
        }

        #endregion


        #region - UX improvement - cursor change on mouse move
        private void i_Icon_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }

        private void dataSourceDd_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }

        private void dataTableDd_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }
        private void settingsPictureIcon_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }
        private void clearPictureIcon_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }

        private void aiModelComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }

        private void sendPictureIcon_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }

        #endregion

    }
}