using System;
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
        private APIClient _client;
        private Boolean _firstSearch;
        private readonly string _endpoint = "https://api.scryfall.com/";
        public MainWindow()
        {
            InitializeComponent();
            _client = new APIClient(_endpoint);
            _firstSearch = true;
        }
        /// <summary>
        /// Calls client to peform search
        /// </summary>
        private void performSearch()
        {
            try
            {
                // Get card info
                string name = SearchText.Text;
                Card card = new Card();
                card = _client.SearchCard(name);

                // Update window
                CardInfo.Text = card.Name;

                // Update images
                ImageBox1.Source = SetImage(card.ImgSource1);
                ImageBox2.Source = SetImage(card.ImgSource2);

                Console.WriteLine(ImageBox1.Source);
                Console.WriteLine(ImageBox2.Source);
            }
            catch (FailedSearchException e)
            {
                // Display why search failed
                CardInfo.Text = e.Message;
                // Reset images
                ImageBox1.Source = null;
                ImageBox2.Source = null;
            }
        }
        private BitmapImage SetImage(string uri)
        {
            if (uri != null)
            {
                BitmapImage myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri(uri);
                myBitmapImage.DecodePixelWidth = 146;
                myBitmapImage.EndInit();
                return myBitmapImage;
            }
            else
            {
                Console.WriteLine("UhOh");
                return null;
            }
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
            if (_firstSearch)
            {
                SearchText.Text = "";
                _firstSearch = false;
            }
        }
    }
    public class FailedSearchException : Exception
    {
        public FailedSearchException() { }
        public FailedSearchException(string message) : base(message) { }
        public FailedSearchException(string message, Exception inner) : base(message, inner) { }
    }
    public class Card
    {
        public string Name { get; set; }
        public string ImgSource1 { get; set; }
        public string ImgSource2 { get; set; }
        public Card()
        {
            Name = null;
            ImgSource1 = null;
            ImgSource2 = null;
        }
    }
    public class APIClient
    {
        private readonly string _endpoint;
        public APIClient(string endpoint) { _endpoint = endpoint; }
        
        /// <summary>
        /// Perform search query for singular card
        /// </summary>
        public Card SearchCard(string name)
        {
            string parameters = "cards/named?fuzzy=" + name;
            JObject result = GetQuery(parameters);

            //Console.WriteLine(result.ToString());

            if ((string)result["object"] == "card")
            {
                Card card = new Card();
                card.Name = (string)result["name"];
                // Check if card is double-sided
                if (result.ContainsKey("card_faces"))
                {
                    card.ImgSource1 = (string)result["card_faces"][0]["image_uris"]["small"];
                    card.ImgSource2 = (string)result["card_faces"][1]["image_uris"]["small"];
                }
                else
                {
                    card.ImgSource1 = (string)result["image_uris"]["small"];
                }

                return card;
            }
            else
            {
                throw new FailedSearchException((string)result["details"]);
            }
        }
        /// <summary>
        /// Perform query on API
        /// </summary>
        private JObject GetQuery(string parameters)
        {
            using (var httpClient = new HttpClient())
            {
                var query = _endpoint + parameters;

                var response = httpClient.GetAsync(query).Result;

                return JObject.Parse(response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
