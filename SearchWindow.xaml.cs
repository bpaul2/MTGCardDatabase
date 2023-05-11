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
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private readonly APIClient _client;
        private bool _firstSearch;
        private readonly string _endpoint = "https://api.scryfall.com/";
        private Card _card;
        // Layer ordering for image boxes
        private int _topImgOrder = 1;
        private int _botImgOrder = 0;

        public SearchWindow()
        {
            InitializeComponent();
            _client = new APIClient(_endpoint);
            _firstSearch = true;
            _card = new Card();
        }
        /// <summary>
        /// Calls client to peform card search
        /// </summary>
        private void performSearch()
        {
            try
            {
                // Get card info
                string name = SearchText.Text;
                _card = _client.SearchCard(name);

                // Update window
                SetCardTxt(_card.DescFront());

                // Update image boxes
                ImageBox1.Source = GetCardImage(_card.ImgSource1);
                ImageBox2.Source = _card.DoubleSided ? GetCardImage(_card.ImgSource2) : null;
            }
            catch (FailedSearchException e)
            {
                // Display error message
                SetCardTxt(e.Message);
                // Reset image boxes
                ImageBox1.Source = null;
                ImageBox2.Source = null;
            }
        }

        private BitmapImage GetCardImage(string uri)
        {
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(uri);
            myBitmapImage.DecodePixelWidth = 146;
            myBitmapImage.EndInit();
            return myBitmapImage;
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

        private void MouseEnterImg1(object sender, MouseEventArgs e)
        {
            SwapImgOrdering(ImageBox1, ImageBox2);
            SetCardTxt(_card.DescFront());
        }

        private void MouseEnterImg2(object sender, MouseEventArgs e)
        {
            SwapImgOrdering(ImageBox2, ImageBox1);
            SetCardTxt(_card.DescBack());
        }

        private void SwapImgOrdering(Image imgTop, Image imgBot)
        {
            Panel.SetZIndex(imgTop, _topImgOrder);
            Panel.SetZIndex(imgBot, -_botImgOrder);
        }

        private void SetCardTxt(string txt)
        {
            CardInfo.Text = txt;
        }
    }
    public class FailedSearchException : Exception
    {
        //public FailedSearchException() { }
        public FailedSearchException(string message) : base(message) { }
        //public FailedSearchException(string message, Exception inner) : base(message, inner) { }
    }
    public class Card
    {
        public string Name { get; set; }
        public string Set { get; set; }
        public bool DoubleSided { get; set; }
        public string SideName1 { get; set; }
        public string SideName2 { get; set; }
        public string Type1 { get; set; }
        public string Type2 { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public string Flavor1 { get; set; }
        public string Flavor2 { get; set; }
        public string ImgSource1 { get; set; }
        public string ImgSource2 { get; set; }

        public Card()
        {
            ImgSource1 = null;
            ImgSource2 = null;
        }

        public string DescFront()
        {
            return SideName1 + "\n" + Type1 + "\n" + Text1 + "\n" + Flavor1 + "\n";
        }

        public string DescBack()
        {
            return SideName2 + "\n" + Type2 + "\n" + Text2 + "\n" + Flavor2 + "\n";
        }

        public override string ToString()
        {
            return Name + "\n" + Set + "\n" + DescFront() + DescBack();
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

            Console.WriteLine(result.ToString());

            if ((string)result["object"] == "card")
            {
                Card card = new Card();
                card.Name = (string)result["name"];
                card.Set = (string)result["set_name"];
                // Check if card is double-sided
                if (result.ContainsKey("card_faces"))
                {
                    card.DoubleSided = true;
                    // Frontside
                    card.SideName1 = (string)result["card_faces"][0]["name"];
                    card.Type1 = (string)result["card_faces"][0]["type_line"];
                    card.Text1 = (string)result["card_faces"][0]["oracle_text"];
                    card.Flavor1 = (string)result["card_faces"][0]["flavor_text"];
                    card.ImgSource1 = (string)result["card_faces"][0]["image_uris"]["small"];
                    // Backside
                    card.SideName2 = (string)result["card_faces"][1]["name"];
                    card.Type2 = (string)result["card_faces"][1]["type_line"];
                    card.Text2 = (string)result["card_faces"][1]["oracle_text"];
                    card.Flavor2 = (string)result["card_faces"][1]["flavor_text"];
                    card.ImgSource2 = (string)result["card_faces"][1]["image_uris"]["small"];
                }
                else
                {
                    card.DoubleSided = false;
                    card.SideName1 = card.Name;
                    card.Type1 = (string)result["type_line"];
                    card.Text1 = (string)result["oracle_text"];
                    card.Flavor1 = (string)result["flavor_text"];
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
