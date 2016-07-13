using System;
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
using Newtonsoft.Json;
using System.Net.Http;
using MeetingPoint.Classes;
using Windows.UI.Popups;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MeetingPoint
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        HttpClient client;
        HttpRequestMessage requestMessage;

        public Login()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            progBarPrijava.IsIndeterminate = true;

            if (await Task.Run(() => NetworkInterface.GetIsNetworkAvailable()))
            {
                //create new httpClient and request message
                client = new HttpClient();
                requestMessage = new HttpRequestMessage(HttpMethod.Post, "censored");
                //add required headers
                requestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Add("censored", "censored");

                //create object with user input credentials
                Dictionary<string, string> credentials = new Dictionary<string, string>();
                credentials.Add("censored", tbxUsername.Text);
                credentials.Add("censored", tbxPassword.Password);

                //put object in proper form for sending and set it as requestMessage content
                var content = new FormUrlEncodedContent(credentials);
                requestMessage.Content = content;

                //if response is recieved, read it as string, then create object from it, else show exception message
                //if responseObject's Success property is set to true, then show property ConferenceName,
                //if not show Error
                try
                {
                    HttpResponseMessage response = await client.SendAsync(requestMessage);
                    response.EnsureSuccessStatusCode();
                    string responseString = await response.Content.ReadAsStringAsync();
                    ResponseObject responseObject = JsonConvert.DeserializeObject<ResponseObject>(responseString);
                    if (responseObject.Success)
                    {
                        this.Frame.Navigate(typeof(MainPage), responseObject);
                    }
                    else
                    {
                        MessageDialog loginFailedMsg = new MessageDialog(responseObject.Error);
                        await loginFailedMsg.ShowAsync();
                    }
                }
                catch(HttpRequestException hre)
                {
                    MessageDialog msgNotSucessfull = new MessageDialog(hre.Message.ToString());
                    await msgNotSucessfull.ShowAsync();
                }
            }
            else
            {
                MessageDialog msg = new MessageDialog("Check intenet connection");
                await msg.ShowAsync();
            }
            
            progBarPrijava.IsIndeterminate = false;
        }
    }
}
