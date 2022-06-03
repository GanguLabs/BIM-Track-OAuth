using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BIM_Track_OAuth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // client configuration
        const string clientID = "<Client_ID>";
        const string clientSecret = "<Client_Secret>";
        const string authorizationEndpoint = "https://auth.bimtrackapp.co/connect/authorize";
        const string tokenEndpoint = "https://auth.bimtrackapp.co/connect/token";
        const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

        public MainWindow()
        {
            InitializeComponent();
        }

        // ref http://stackoverflow.com/a/3978040
        public static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            // Generates state and PKCE values.
            string state = randomDataBase64url(32);
            string code_verifier = randomDataBase64url(32);
            string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Creates a redirect URI using an available port on the loopback address.
            string BT_RedirectURI = string.Format("http://{0}:{1}", IPAddress.Loopback, GetRandomUnusedPort());
            output("redirect URI: " + BT_RedirectURI);

            // Creates an HttpListener to listen for requests on that redirect URI.
            string httpRedirectURI = BT_RedirectURI + "/";
            var http = new HttpListener();
            http.Prefixes.Add(httpRedirectURI);
            output("Listening..");
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?response_type=code&scope=BIMTrack_Api%20openid&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationEndpoint,
                System.Uri.EscapeDataString(BT_RedirectURI),
                clientID,
                state,
                code_challenge,
                code_challenge_method);

            // Opens request in the browser.
            System.Diagnostics.Process.Start(authorizationRequest);

            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Brings this app back to the foreground.
            this.Activate();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                output(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return;
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                output("Malformed authorization response. " + context.Request.QueryString);
                return;
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state)
            {
                output(String.Format("Received request with invalid state ({0})", incoming_state));
                return;
            }
            output("Authorization code: " + code);

            // Starts the code exchange at the Token Endpoint.
            performCodeExchange(code, code_verifier, BT_RedirectURI);
        }

        async void performCodeExchange(string code, string code_verifier, string redirectURI)
        {
            output("Exchanging code for tokens...");

            // builds the  request
            string tokenRequestURI = tokenEndpoint;
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
                code,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                code_verifier,
                clientSecret
                );

            // sends the request
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();

            try
            {
                // gets the response
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
                {
                    // reads response body
                    string responseText = await reader.ReadToEndAsync();
                    output(responseText);

                    // converts to dictionary
                    Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                    string access_token = tokenEndpointDecoded["access_token"];
                    getHubs_HttpWebRequest("https://api.bimtrackapp.co/v3/hubs", access_token);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        output("HTTP: " + response.StatusCode);
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            string responseText = await reader.ReadToEndAsync();
                            output(responseText);
                        }
                    }

                }
            }
        }

        async void getHubs_HttpWebRequest(string tokenRequestURI, string accessToken)
        {
            // This used inbuilt System.Web.httpWebRequest library to send API Calls. Below is anothe function that uses RestSharp with is supported by PostMan

            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
            tokenRequest.Method = "GET";
            tokenRequest.Accept = "Accept=application/json,text/json,application/xml,text/xml";
            tokenRequest.Headers.Add("Authorization", "Bearer " + accessToken);
            WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
            using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
            {
                // reads response body
                string responseText = await reader.ReadToEndAsync();

                // convert JSON Response to List of Hubs
                List<BT_Hub> responseDecoded = JsonConvert.DeserializeObject<List<BT_Hub>>(responseText);

                output(responseDecoded[0].Name);

            }


        }

        //async void getHubs_RestClient(string tokenRequestURI, string accessToken)
        //{
        //    // this is a RestSharp implementation of the function - PostMan supports RestSharp for code snippet export
        //    var client = new RestClient(tokenRequestURI);
        //    client.Timeout = -1;
        //    var request = new RestRequest(Method.GET);
        //    request.AddHeader("Accept", "application/json, text/json, application/xml, text/xml");
        //    request.AddHeader("Authorization", "Bearer " + accessToken);
        //    IRestResponse response = client.Execute(request);
        //    Console.WriteLine(response.Content);

        //    string responseText = response.Content;
        //    List<BT_Hub> responseDecoded = JsonConvert.DeserializeObject<List<BT_Hub>>(responseText);
        //}


        /// <summary>
        /// Appends the given string to the on-screen log, and the debug console.
        /// </summary>
        /// <param name="output">string to be appended</param>
        public void output(string output)
        {
            textBoxOutput.Text = textBoxOutput.Text + output + Environment.NewLine;
            Console.WriteLine(output);
        }

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string randomDataBase64url(uint length)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return base64urlencodeNoPadding(bytes);
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        public static byte[] sha256(string inputStirng)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            SHA256Managed sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string base64urlencodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }

    }
}
