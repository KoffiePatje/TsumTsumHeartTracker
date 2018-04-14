using System;
using System.Text;
using System.IO;
using System.Json;
using System.Net;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Net;
using Android.Util;
using System.Net.Http;

namespace nl.pleduc.TsumTsumHeartTracker
{
    public class TsumTsumWebHelper
    {
        public const string TAG = "TsumTsumWebHelper";
        public const string DATA_KEY = "data";

#if DEBUG || FORCE_DEV
        public const string url = @"http://tsumtsumhearttrackerdev.pleduc.nl/api.php";
#else
        public const string url = @"http://tsumtsumhearttracker.pleduc.nl/api.php";
#endif

        public static bool CanPerformWebRequest(Context context)
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            if (connectivityManager.ActiveNetworkInfo == null)
            {
                Log.Info(TAG, "No internet available, skipping version check");
                return false;
            }

            return true;
        }

        public static async void PerformGetRequest(string requestData, Action<JsonValue> onSuccess, Action onFailed)
        {
            if (string.IsNullOrEmpty(requestData)) throw new InvalidOperationException("getRequest was null or empty");

            try
            {
                JsonValue jsonResult = await AsyncGetRequest(string.Format("{0}?{1}", url, requestData));

                if (jsonResult != null && jsonResult.ContainsKey(DATA_KEY))
                {
                    onSuccess(jsonResult);
                }
                else
                {
                    onFailed();
                }
            }
            catch (WebException exp)
            {
                Log.Error(TAG, $"WebException: {exp.Message}");
            }
            catch (IOException exp)
            {
                Log.Error(TAG, $"IOException: {exp.Message}");
            }
            catch (Exception exp)
            {
                Log.Error(TAG, $"Exception: {exp.Message}");
            }
        }

        public static async void PerformPostRequest(string postRequest, Action<JsonValue> onSuccess, Action onFailed)
        {
            if (string.IsNullOrEmpty(postRequest)) throw new InvalidOperationException("postRequest was null or empty");

            try
            {
                JsonValue jsonResult = await AsyncPostRequest(url, postRequest);

                if (jsonResult != null && jsonResult.ContainsKey(DATA_KEY))
                {
                    onSuccess(jsonResult);
                }
                else
                {
                    onFailed();
                }
            }
            catch (WebException exp)
            {
                Log.Error(TAG, $"WebException: {exp.Message}");
            }
            catch (IOException exp)
            {
                Log.Error(TAG, $"IOException: {exp.Message}");
            }
            catch (Exception exp)
            {
                Log.Error(TAG, $"Exception: {exp.Message}");
            }
        }

        private static async Task<JsonValue> AsyncGetRequest(string url)
        { 
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new global::System.Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    Log.Info(TAG, $"Response: {jsonDoc.ToString()}");

                    return jsonDoc;
                }
            }
        }

        private static async Task<JsonValue> AsyncPostRequest(string url, string postData)
        {
            byte[] data = Encoding.UTF8.GetBytes(postData); // Encoding.ASCII.GetBytes(postData);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new global::System.Uri(url));
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.ContentLength = data.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    Log.Info(TAG, $"Response: {jsonDoc.ToString()}");

                    return jsonDoc;
                }
            }
        }
    }
}