using System;
// Window/XAML stuff
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
// API interation
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MTGDB_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private APIClient client;
        private Boolean firstSearch;
        public MainWindow()
        {
            InitializeComponent();
            client = new APIClient("https://api.scryfall.com/");
            firstSearch = true;
        }
        /// <summary>
        /// Calls client to peform search
        /// </summary>
        private void performSearch()
        {
            // Get card info
            string name = SearchText.Text;
            Card card = new Card();
            card = client.SearchCard(name);

            // Update window
            CardInfo.Text = card.name;

            // Update image
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(card.imgSource);
            myBitmapImage.DecodePixelWidth = 146;
            myBitmapImage.EndInit();
            ImageBox.Source = myBitmapImage;
        }
        /// <summary>
        /// Handles 'Get Card' button cick
        /// </summary>
        private void GetCardClick(object sender, RoutedEventArgs e)
        {
            performSearch();
        }
        /// <summary>
        /// Handles search entry
        /// </summary>
        private void GetSearchEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                performSearch();
            }   
        }
        /// <summary>
        /// Handles clicking on 
        /// </summary>
        private void GetSearchClick(object sender, MouseButtonEventArgs e)
        {
            if (firstSearch)
            {
                SearchText.Text = "";
                firstSearch = false;
            }
        }
    }
    public class Card
    {
        public string name { get; set; }
        public string imgSource { get; set; }
        public Card()
        {
            name = null;
            imgSource = null;
        }
    }
    public class APIClient
    {
        protected readonly string endpoint;
        public APIClient(string endpoint) { this.endpoint = endpoint; }
        
        /// <summary>
        /// Perform search query for singular card
        /// </summary>
        public Card SearchCard(string name)
        {
            string parameters = "cards/named?fuzzy=" + name;
            JObject result = GetQuery(parameters);
            Card card = new Card
            {
                name = (string)result["name"],
                imgSource = (string)result["image_uris"]["small"]
            };
            return card;
        }
        /// <summary>
        /// Perform query on API
        /// </summary>
        private JObject GetQuery(string parameters)
        {
            using (var httpClient = new HttpClient())
            {
                var query = endpoint + parameters;

                var response = httpClient.GetAsync(query).Result;

                return JObject.Parse(response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
