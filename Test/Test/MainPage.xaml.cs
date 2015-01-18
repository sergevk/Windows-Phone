using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Web.Http.Filters;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.

            // Register on event
            Windows.Networking.Connectivity.NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            //Get LAN identifiers. If 0 - WiFi disconnected
            var wlanIds = Windows.Networking.Connectivity.NetworkInformation.GetLanIdentifiers().ToList();

            if (wlanIds.Count() != 0)
            {
                tbWiFiState.Text = "WiFi state: Connected";
            }
            else
            {
                tbWiFiState.Text = "WiFi state: Disconnected";
            }

            // Update tile and app fields
            var ConnectionData = await PrepareConnectionData(); 
            RefreshTile(ConnectionData);
            RefreshAppData(ConnectionData);

            //var zx = 1;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //iptext.Text = "Hello world!";
            //var networkInfo = Windows.Networking.Connectivity.NetworkInformation;
            
            var iplist = "ip: ";
            var hostList = Windows.Networking.Connectivity.NetworkInformation.GetHostNames().ToList();
            var connprof = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            var connps = Windows.Networking.Connectivity.NetworkInformation.GetConnectionProfiles().ToList();
            //var conssid = connps[2].WlanConnectionProfileDetails.GetConnectedSsid();
            //var conwwan = connps[1].WwanConnectionProfileDetails.HomeProviderId;
            //var adid = Windows.Networking.Connectivity.NetworkInformation.GetLanIdentifiers().ToList();

            //Get LAN identifiers. If 0 - no connections via WiFi
            //var wlanIds = Windows.Networking.Connectivity.NetworkInformation.GetLanIdentifiers().ToList();
            /*
            if (wlanIds.Count() != 0)
            {
                tbWiFiState.Text = "WiFi state: Connected";
            }
            else
            {
                tbWiFiState.Text = "WiFi state: Disconnected";
            }
            */

            foreach (var host in hostList) {
                
                //string ip = host.DisplayName;
                if (host.IPInformation != null)
                {
                    string ip = host.DisplayName;
                    string mask = host.IPInformation.PrefixLength.ToString();
                    iplist += ip + "; mask " + mask;
                }
            }
            //var ipText = getipaddress();
            iptext.Text = iplist;

        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {
        }

        private async void btRefresh_Click(object sender, RoutedEventArgs e)
        {
            var ConnectionData = await PrepareConnectionData();
            RefreshTile(ConnectionData);
            RefreshAppData(ConnectionData);

            var zx = 0;
            //tbConnectionProfile.Text = "Profile: " + connectionData[1];
            //tbInternalIP.Text = "Internal IP: " + connectionData[2];
            //tbExternalIP.Text = "External IP: " + connectionData[3];
            
            //string externalIPURI = "http://ifconfig.me/ip";
            //HttpClient htClient = new HttpClient();
            //string ResponceResult = await htClient.GetStringAsync(new Uri(externalIPURI));
            //HttpResponseMessage response = await htClient.GetAsync(externalIPURI);

            //string externalIPURI = "http://ifconfig.me/ip";
            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(externalIPURI);
            //request.BeginGetResponse(GetAvatarImageCallback, request);

            //var tmp = System.Net.NetworkCredential
        }   

        private async Task<string> GetExternalIP()
        {

            int unixTimestamp = (int)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; //Need for generate unique URI. It's disable cache.
            string externalIPURI = "http://curlmyip.com/&temp="+unixTimestamp.ToString();
            //string externalIPURI = "http://ifconfig.me/ip";
            HttpClient htClient = new HttpClient();
            
            try
            {
                string responseBodyAsText;
                //tbOutputText.Text = "";
                //tbStatusText.Text = "Waiting for response ...";
                //tbExternalIP.Text = "External IP: Retriving data...";

                HttpResponseMessage response = await htClient.GetAsync(externalIPURI);
                response.EnsureSuccessStatusCode();

                //tbStatusText.Text = response.StatusCode + " " + response.ReasonPhrase + Environment.NewLine;
                responseBodyAsText = await response.Content.ReadAsStringAsync();
                //responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine); // Insert new lines
                //tbOutputText.Text = responseBodyAsText;
                return responseBodyAsText;
            }
            catch (HttpRequestException hre)
            {
                //tbStatusText.Text = hre.ToString();
                return "hre-" + hre.ToString();
            }
            catch (Exception ex)
            {
                // For debugging
                //tbStatusText.Text = ex.ToString();
                return "ex-" + ex.ToString();
            }
                        
        }

        private async Task<string[]> PrepareConnectionData()
        {
            //Initialize data
            string[] connectionData = new string[4];

            //Get LAN identifiers. If 0 - WiFi disconnected
            var wlanIds = Windows.Networking.Connectivity.NetworkInformation.GetLanIdentifiers().ToList();
            if (wlanIds.Count() > 0)
            {
                // Get all hostnames (include IPs)
                var hostnameList = Windows.Networking.Connectivity.NetworkInformation.GetHostNames().ToList();
                // Get all connection profiles 
                var profilelist = Windows.Networking.Connectivity.NetworkInformation.GetConnectionProfiles().ToList();

                // Set NetworkID
                var wlanNetworkID = wlanIds[0].NetworkAdapterId.ToString();

                // Detect SSID
                //connectionData[0] = "-";
                foreach (var connectionProfile in profilelist)
                {
                    if ((wlanNetworkID == connectionProfile.NetworkAdapter.NetworkAdapterId.ToString()) && 
                        (connectionProfile.GetNetworkConnectivityLevel() != 0))
                    {
                        connectionData[0] = connectionProfile.WlanConnectionProfileDetails.GetConnectedSsid();
                        connectionData[1] = connectionProfile.GetNetworkConnectivityLevel().ToString();
                    }
                }                       

                // Detect current internal IP
                connectionData[2] = "-";
                foreach (var host in hostnameList)
                {
                    if (host.IPInformation != null)
                    {
                        if (wlanNetworkID == host.IPInformation.NetworkAdapter.NetworkAdapterId.ToString())
                        {
                            connectionData[2] = host.DisplayName.ToString();
                        }
                    }
                }

                // Get external IP
                connectionData[3] = await GetExternalIP();
            }
            else
            {                
                connectionData[0] = "Disconnected";
                connectionData[1] = "-";
                connectionData[2] = "-";
                connectionData[3] = "-";

            }
                        
            //Get all hostnames (include IPs)
            //var hostnameList = Windows.Networking.Connectivity.NetworkInformation.GetHostNames().ToList();

            //Get Internet connection profile
            /*
            var connectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile != null)
            {
                connectionData[1] = connectionProfile.ProfileName;
            }
            */
            //Detect current internal IP
            /*
            connectionData[2] = "-";
            foreach (var host in hostnameList)
            {
                if (host.IPInformation != null)
                {
                    string netAdapterID = host.IPInformation.NetworkAdapter.NetworkAdapterId.ToString();
                    if (netAdapterID == connectionProfile.NetworkAdapter.NetworkAdapterId.ToString())
                    {
                        connectionData[2] = host.DisplayName.ToString();
                    }
                }
            }
            */
            // Detect public IP
            //connectionData[3] = await GetExternalIP();

            // var zx = 0;
            return connectionData;
        }

        private void RefreshAppData(string[] ConnectionData)
        {
            // Update data inside application
            if (ConnectionData[0] == "Disconnected")
            {
                tbWiFiState.Text = "WiFi state: Disconnected";
            }
            else {
                tbWiFiState.Text = "WiFi state: Connected";
            }
            tbConnectionProfile.Text = "Access: " + ConnectionData[1];
            tbInternalIP.Text = "Internal IP: " + ConnectionData[2];
            tbExternalIP.Text = "External IP: " + ConnectionData[3];       
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            var ConnectionData = await PrepareConnectionData();
            RefreshTile(ConnectionData);
            //RefreshAppData(ConnectionData);
        }

        private void RefreshTile(string[] ConnectionData)
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text01);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = ConnectionData[0];
            tileTextAttributes[1].InnerText = "Access: " + ConnectionData[1]; //+ connectionProfile.ProfileName;
            tileTextAttributes[2].InnerText = "Private: " + ConnectionData[2]; // Inetrnal IP address;
            tileTextAttributes[3].InnerText = "Public: " + ConnectionData[3]; // External IP

            XmlDocument WideTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text01);
            XmlNodeList WideTileTextAttributes = WideTileXml.GetElementsByTagName("text");
            WideTileTextAttributes[0].InnerText = ConnectionData[0];
            WideTileTextAttributes[1].InnerText = "Access: " + ConnectionData[1]; //+ connectionProfile.ProfileName;
            WideTileTextAttributes[2].InnerText = "Private: " + ConnectionData[2]; // Inetrnal IP address;
            WideTileTextAttributes[3].InnerText = "Public: " + ConnectionData[3]; // External IP

            IXmlNode node = tileXml.ImportNode(WideTileXml.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            TileNotification tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
          }
    }
}
