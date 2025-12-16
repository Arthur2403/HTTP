using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace GoogleSearchWpf
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


            ResultsList.ItemsSource = null;
            var all = new List<SearchItem>();


            try
            {
                if (GoogleCheck.IsChecked == true)
                    all.AddRange(await SearchGoogle(QueryTextBox.Text));


                if (DuckCheck.IsChecked == true)
                    all.AddRange(await SearchDuckDuckGo(QueryTextBox.Text));


                if (WikiCheck.IsChecked == true)
                    all.AddRange(await SearchWikipedia(QueryTextBox.Text));


                ResultsList.ItemsSource = all;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<SearchItem>> SearchGoogle(string q)
        {
            var url = $"https://www.googleapis.com/customsearch/v1?key={GOOGLE_API_KEY}&cx={GOOGLE_CX}&q={Uri.EscapeDataString(q)}";
            var r = await _http.GetFromJsonAsync<GoogleResponse>(url);


            return r?.Items?.Select(i => new SearchItem
            {
                Source = "Google",
                Title = i.Title,
                Link = i.Link,
                Snippet = i.Snippet
            }).ToList() ?? new List<SearchItem>();
        }

        private async Task<List<SearchItem>> SearchDuckDuckGo(string q)
        {
            var url = $"https://api.duckduckgo.com/?q={Uri.EscapeDataString(q)}&format=json";
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);


            var list = new List<SearchItem>();
            if (doc.RootElement.TryGetProperty("RelatedTopics", out var topics))
            {
                foreach (var t in topics.EnumerateArray())
                {
                    if (t.TryGetProperty("Text", out var text) && t.TryGetProperty("FirstURL", out var link))
                    {
                        list.Add(new SearchItem
                        {
                            Source = "DuckDuckGo",
                            Title = text.GetString() ?? "",
                            Link = link.GetString() ?? "",
                            Snippet = text.GetString() ?? ""
                        });
                    }
                }
            }
            return list;
        }
        private async Task<List<SearchItem>> SearchWikipedia(string q)
        {
            var url = $"https://en.wikipedia.org/w/api.php?action=query&list=search&format=json&srsearch={Uri.EscapeDataString(q)}";
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);


            var list = new List<SearchItem>();
            var results = doc.RootElement.GetProperty("query").GetProperty("search");
            foreach (var r in results.EnumerateArray())
            {
                var title = r.GetProperty("title").GetString() ?? "";
                list.Add(new SearchItem
                {
                    Source = "Wikipedia",
                    Title = title,
                    Snippet = r.GetProperty("snippet").GetString() ?? "",
                    Link = $"https://en.wikipedia.org/wiki/{title.Replace(' ', '_')}"
                });
            }
            return list;
        }


        private void Link_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBlock tb)
                Process.Start(new ProcessStartInfo(tb.Text) { UseShellExecute = true });
        }
    }
    public class SearchItem
    {
        public string Source { get; set; } = "";
        public string Title { get; set; } = "";
        public string Snippet { get; set; } = "";
        public string Link { get; set; } = "";
    }


    public class GoogleResponse
    {
        public List<GoogleItem>? Items { get; set; }
    }


    public class GoogleItem
    {
        public string Title { get; set; } = "";
        public string Link { get; set; } = "";
        public string Snippet { get; set; } = "";
    }
}