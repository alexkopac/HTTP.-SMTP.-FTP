using System;
using System.Net.Http;
using System.Windows.Forms;

namespace HttpClientApp
{
    public class MainForm : Form
    {
        private readonly TextBox txtUri;
        private readonly Button btnSend;
        private readonly TextBox txtResult;
        private static readonly HttpClient httpClient = new HttpClient();

        public MainForm()
        {
            Text = "HTTP Клієнт";
            Width = 800;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            txtUri = new TextBox
            {
                Left = 15,
                Top = 15,
                Width = 630,
                Text = "http://localhost:8080/",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnSend = new Button
            {
                Left = 655,
                Top = 13,
                Width = 115,
                Height = 25,
                Text = "Надіслати",
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSend.Click += BtnSend_Click;

            txtResult = new TextBox
            {
                Left = 15,
                Top = 50,
                Width = 755,
                Height = 495,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.Add(txtUri);
            Controls.Add(btnSend);
            Controls.Add(txtResult);
        }

        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            string rawUri = txtUri.Text.Trim();

            if (!Uri.TryCreate(rawUri, UriKind.Absolute, out Uri? validUri) ||
                (validUri.Scheme != Uri.UriSchemeHttp && validUri.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show(
                    "Введіть коректну HTTP або HTTPS URI-адресу (наприклад, http://localhost:8080/ або https://example.com).",
                    "Помилка URI",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            btnSend.Enabled = false;
            txtResult.Text = "Виконання запиту...";

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(validUri);
                response.EnsureSuccessStatusCode();

                string htmlContent = await response.Content.ReadAsStringAsync();
                txtResult.Text = htmlContent;
            }
            catch (HttpRequestException ex)
            {
                txtResult.Clear();
                MessageBox.Show(
                    $"Помилка HTTP-запиту: {ex.Message}",
                    "Помилка мережі",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                txtResult.Clear();
                MessageBox.Show(
                    $"Сталася помилка: {ex.Message}",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnSend.Enabled = true;
            }
        }

        [STAThread]
        public static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
