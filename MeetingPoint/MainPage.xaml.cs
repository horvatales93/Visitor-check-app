using MeetingPoint.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.UI.ViewManagement;
using ZXing.Common;
using ZXing;
using Lumia.Imaging;
using VideoEffects;
using System.Diagnostics;
using Windows.Phone.UI.Input;
using Windows.System.Display;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace MeetingPoint
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HttpClient client;
        HttpRequestMessage requestMessage;
        ResponseObject loginResponse;
        MediaCapture captureManager;
        DisplayRequest displayRequest = new DisplayRequest();
        bool isPreviewing;
        bool isSendingRequest;
        BarcodeReader barCodeReader = new BarcodeReader
        {
            Options = new DecodingOptions
            {
                PossibleFormats = new BarcodeFormat[] { BarcodeFormat.CODE_128,BarcodeFormat.UPC_A },
                TryHarder = true
            }
        };
        private string sucScan;

        public string SucScan
        {
            get { return sucScan; }
            set {
                sucScan = value;
                tblResponse.Text = sucScan;
            }
        }

        //ContinuousAutoFocus autof;

        public MainPage()
        {

            this.InitializeComponent();

            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
        }

        private async void InitializePreview()
        {
                captureManager = new MediaCapture();
                await captureManager.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    PhotoCaptureSource = PhotoCaptureSource.Photo,
                    AudioDeviceId = string.Empty,
                });
                captureManager.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
                StartPreview();
            displayRequest.RequestActive();
        }

        private async void StartPreview()
        {
            btnStopPreview.Icon = new SymbolIcon(Symbol.Stop);
            btnStopPreview.Label = "Stop";
            if (tblResponse!=null)
            {
                tblResponse.Text = "";
            }
            capturePreview.Source = captureManager;
            await captureManager.StartPreviewAsync();
            isPreviewing = true;

            var definition = new LumiaAnalyzerDefinition(ColorMode.Yuv420Sp, 640, AnalyzeBitmap);
            await captureManager.AddEffectAsync(
                MediaStreamType.VideoPreview,
                definition.ActivatableClassId,
                definition.Properties
                );
        }
        private async void StopPreview()
        {
            if (captureManager != null)
            {
                if (isPreviewing)
                {
                    await captureManager.StopPreviewAsync();
                    isPreviewing = false;
                }
                capturePreview.Source = null;
                btnStopPreview.Icon = new SymbolIcon(Symbol.Video);
                btnStopPreview.Label = "Scan";
                captureManager.Dispose();
                displayRequest.RequestRelease();
            }
        }
        void AnalyzeBitmap(Bitmap bitmap, TimeSpan time)
        {
                Result result = barCodeReader.Decode(
                bitmap.Buffers[0].Buffer.ToArray(),
                (int)bitmap.Buffers[0].Pitch,
                (int)bitmap.Dimensions.Height,
                BitmapFormat.Gray8
                );

            var updateUI = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (result != null)
                {
                    //tblResponse.Text = result.Text;  
                    if (!isSendingRequest)
                    {
                        SendRequest(result.Text);
                        isSendingRequest = true;
                    }                            
                }
            }
            );        
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            loginResponse = (e.Parameter as ResponseObject);
            tblImeDogodka.Text = loginResponse.ConferenceName;
        }

        private async void SendRequest(string qrcode)
        {
            prgBarMainPage.IsIndeterminate = true;
            if (await Task.Run(() => NetworkInterface.GetIsNetworkAvailable()))
            {
                //create new httpClient and request message
                client = new HttpClient();
                requestMessage = new HttpRequestMessage(HttpMethod.Post, "censored");
                //add required headers
                requestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Add("censored", "censored");

                //create object with userId and Barcode input
                Dictionary<string, string> credentials = new Dictionary<string, string>();
                credentials.Add("censored", loginResponse.UserId.ToString());
                credentials.Add("censored", qrcode);

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
                    QRResponse responseObject = JsonConvert.DeserializeObject<QRResponse>(responseString);
                    if (responseObject.Success)
                    {
                        tblResponse.Visibility = Visibility.Visible;
                        tblResponse.Text ="Welcome to event\n" + responseObject.ToString();
                        btnStopPreview.Icon = new SymbolIcon(Symbol.Play);
                        btnStopPreview.Label = "Continue";
                    }
                    else
                    {
                        tblResponse.Visibility = Visibility.Visible;
                        tblResponse.Text = responseObject.Error;
                        btnStopPreview.Icon = new SymbolIcon(Symbol.Play);
                        btnStopPreview.Label = "Continue";
                    }

                }
                catch (HttpRequestException hre)
                {
                    MessageDialog msgHre = new MessageDialog("HttpRequest exception:" + hre.Message.ToString());
                    await msgHre.ShowAsync();
                }
                catch (Exception e)
                {
                    MessageDialog msgEx = new MessageDialog("Exception:" + e.Message.ToString());
                    await msgEx.ShowAsync();
                }
            }
            else
            {
                MessageDialog msg = new MessageDialog("Check intenet connection");
                await msg.ShowAsync();
            }
            prgBarMainPage.IsIndeterminate = false;
        }

        private async void btnFocus_Click(object sender, RoutedEventArgs e)
        {
            if (isPreviewing)
            {
                await captureManager.VideoDeviceController.FocusControl.FocusAsync();
            }                   
        }

        private void btnStopPreview_Click(object sender, RoutedEventArgs e)
        {
            if (isPreviewing)
            {
                if (isSendingRequest)
                {
                    tblResponse.Visibility = Visibility.Collapsed;
                    btnStopPreview.Icon = new SymbolIcon(Symbol.Stop);
                    btnStopPreview.Label = "Stop";
                    isSendingRequest = false;
                }
                else
                {
                    StopPreview();
                }               
            }
            else
            {
                InitializePreview();
                
            }           
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (isPreviewing)
            {
                StopPreview();
            }
            this.Frame.Navigate(typeof(Login));
        }

    }
}
