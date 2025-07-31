using FlexGridGeminiAI.Helpers;
using FlexGridGeminiAI.Interface;
using FlexGridGeminiAI.Services;
using System.Diagnostics;
using System.Security.Cryptography;

namespace FlexGridGeminiAI.Views.Forms
{
    public partial class APIKeyInputForm : Form
    {
        private readonly IApiKeyService _geminiKeyService = new GeminiApiKeyService();
        private readonly IApiKeyService _groqKeyService = new GroqApiKeyService();
        public APIKeyInputForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            
            groqApiInputTextBox.PasswordChar = '\0';
            apiInputTextBox.PasswordChar = '\0';

            SetTextBoxDisplayGemin();
            SetTextBoxDisplayGroq();
        }
        
        private void SetTextBoxDisplayGemin()
        {
            if (!_geminiKeyService.ApiKeyExists())
            {
                apiInputTextBox.UseSystemPasswordChar = false;
                apiInputTextBox.Value = "Enter Your Gemini Key";
            }
            else
            {
                apiInputTextBox.UseSystemPasswordChar = true;
                apiInputTextBox.PasswordChar = '\0';
                string? key = _geminiKeyService.GetApiKey();
                if (key != null) apiInputTextBox.Text = key;
                return;
            }
        }
        
        private void SetTextBoxDisplayGroq()
        {
            if (!_groqKeyService.ApiKeyExists())
            {
                groqApiInputTextBox.UseSystemPasswordChar = false;
                groqApiInputTextBox.Value = "Enter Your Groq API key";
            }
            else
            {
                groqApiInputTextBox.UseSystemPasswordChar = true;
                groqApiInputTextBox.PasswordChar = '\0';
                string? key = _groqKeyService.GetApiKey();
                if (key != null) groqApiInputTextBox.Text = key;
                return;

            }

        }
        
        private void c1Button1_Click(object sender, EventArgs e)
        {
            // call the set enviromet method from here
            this.DialogResult = DialogResult.OK;

            if (apiInputTextBox.Text != null && groqApiInputTextBox.Text != null)
            {
                _geminiKeyService.SetApiKey(apiInputTextBox.Text);
                _groqKeyService.SetApiKey(groqApiInputTextBox.Text);
                this.Close();
            }
            else
            {
                MessageBox.Show("Pass the API Key");
            }
        }
        
        private void geminiAPIlink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://aistudio.google.com/app/apikey",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occured: {ex}");
            }

        }
        
        private void groqAPIlink_LinkClicked(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://console.groq.com/keys",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
            }
        }
        
        private void c1PictureBox1_Click(object sender, EventArgs e)
        {
            apiInputTextBox.PasswordChar = '\0';
            apiInputTextBox.UseSystemPasswordChar = !apiInputTextBox.UseSystemPasswordChar;
        }
        
        private void c1PictureBox2_Click(object sender, EventArgs e)
        {
            apiInputTextBox.PasswordChar = '\0';
            groqApiInputTextBox.UseSystemPasswordChar = !groqApiInputTextBox.UseSystemPasswordChar;
        }
        
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
