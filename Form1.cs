using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Web.WebView2.WinForms;

namespace LumenDesktop;

public sealed class Form1 : Form
{
    private const string CurrentVersion = "1.4.0";
    private readonly WebView2 browser = new() { Dock = DockStyle.Fill };
    private Process? server;

    public Form1()
    {
        Text = "Lumen";
        Width = 1280;
        Height = 820;
        MinimumSize = new Size(760, 520);
        StartPosition = FormStartPosition.CenterScreen;
        Controls.Add(browser);
        Shown += async (_, _) => await StartAsync();
        FormClosing += (_, _) => StopServer();
    }

    private async Task StartAsync()
    {
        var root = AppContext.BaseDirectory;
        var exe = Path.Combine(root, "Lumen.exe");
        if (File.Exists(exe))
            server = Process.Start(new ProcessStartInfo(exe) { WorkingDirectory = root, UseShellExecute = false, CreateNoWindow = true });
        try
        {
            using var http = new HttpClient();
            for (var i = 0; i < 40; i++)
            {
                try { await http.GetAsync("http://127.0.0.1:8765/api/status"); break; }
                catch { await Task.Delay(250); }
            }
            await browser.EnsureCoreWebView2Async();
            browser.CoreWebView2.Settings.IsStatusBarEnabled = false;
            browser.Source = new Uri("http://localhost:8765");
            _ = CheckForUpdateAsync(root);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo iniciar Lumen.\n\n{ex.Message}", "Lumen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private static async Task CheckForUpdateAsync(string root)
    {
        var feed = Environment.GetEnvironmentVariable("LUMEN_UPDATE_URL");
        var envFile = Path.Combine(root, ".env");
        if (string.IsNullOrWhiteSpace(feed) && File.Exists(envFile))
            feed = File.ReadLines(envFile).FirstOrDefault(x => x.StartsWith("LUMEN_UPDATE_URL=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2).ElementAtOrDefault(1)?.Trim();
        if (string.IsNullOrWhiteSpace(feed)) return;
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            using var doc = JsonDocument.Parse(await http.GetStringAsync(feed));
            var rootJson = doc.RootElement;
            var version = rootJson.GetProperty("version").GetString() ?? CurrentVersion;
            var url = rootJson.GetProperty("url").GetString();
            var sha = rootJson.TryGetProperty("sha256", out var hash) ? hash.GetString() : null;
            if (string.IsNullOrWhiteSpace(url) || !Version.TryParse(version, out var remote) || !Version.TryParse(CurrentVersion, out var current) || remote <= current) return;
            var temp = Path.Combine(Path.GetTempPath(), $"Lumen-Setup-{version}.exe");
            await File.WriteAllBytesAsync(temp, await http.GetByteArrayAsync(url));
            if (!string.IsNullOrWhiteSpace(sha))
            {
                await using var stream = File.OpenRead(temp);
                var actual = Convert.ToHexString(await SHA256.HashDataAsync(stream));
                if (!actual.Equals(sha, StringComparison.OrdinalIgnoreCase)) { File.Delete(temp); return; }
            }
            if (MessageBox.Show($"Hay una actualización de Lumen ({version}). ¿Instalarla al cerrar?", "Actualización disponible", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                Process.Start(new ProcessStartInfo(temp, "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART") { UseShellExecute = true });
        }
        catch { }
    }

    private void StopServer()
    {
        try { if (server is { HasExited: false }) server.Kill(entireProcessTree: true); } catch { }
    }
}
