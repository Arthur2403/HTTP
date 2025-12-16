using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ImageSearchWpf
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _http = new HttpClient();


        private const string GOOGLE_API_KEY = "AIzaSyAuDX2Vyo05s5DqRGDNcltEXTmaujszFF8";
        private const string GOOGLE_CX = "70a7ae3f6ab0247d5";


        public MainWindow()
        {
            InitializeComponent();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QueryTextBox.Text)) return;


            ImagesPanel.Children.Clear();


            if (GoogleCheck.IsChecked == true)
                await LoadImage("Google", await SearchGoogleImage(QueryTextBox.Text));


            if (DuckCheck.IsChecked == true)
                await LoadImage("DuckDuckGo", await SearchDuckImage(QueryTextBox.Text));


            if (WikiCheck.IsChecked == true)
                await LoadImage("Wikipedia", await SearchWikiImage(QueryTextBox.Text));
        }

        private async Task<string?> SearchGoogleImage(string q)
        {
            var url = $"https://www.googleapis.com/customsearch/v1?key={GOOGLE_API_KEY}&cx={GOOGLE_CX}&searchType=image&num=1&q={Uri.EscapeDataString(q)}";
            var r = await _http.GetFromJsonAsync<GoogleImageResponse>(url);
            return r?.Items?[0]?.Link;
        }

        private async Task<string?> SearchDuckImage(string q)
        {
            var url = $"https://api.duckduckgo.com/?q={Uri.EscapeDataString(q)}&format=json";
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);


            if (doc.RootElement.TryGetProperty("Image", out var img))
                return img.GetString();


            return null;
        }

        private async Task<string?> SearchWikiImage(string q)
        {
            var url = $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(q)}";
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);


            if (doc.RootElement.TryGetProperty("thumbnail", out var thumb))
                return thumb.GetProperty("source").GetString();


            return null;
        }
        private async Task LoadImage(string source, string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;


            var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            stack.Children.Add(new TextBlock { Text = source, FontWeight = FontWeights.Bold });


            var img = new Image { Height = 200, Margin = new Thickness(0, 5, 0, 0) };
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(url);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();


            img.Source = bitmap;
            img.MouseLeftButtonUp += (_, __) =>
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });


            stack.Children.Add(img);
            ImagesPanel.Children.Add(stack);
            await Task.CompletedTask;
        }
    }
    public class GoogleImageResponse
    {
        public GoogleImageItem[]? Items { get; set; }
    }


    public class GoogleImageItem
    {
        public string Link { get; set; } = "";
    }
}