using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;

using Android.Gms.Common;
using Firebase.Messaging;
using Firebase.Iid;
using Android.Util;
using System.Threading.Tasks;
using System.Net.Http;
using static Newtonsoft.Json.JsonConvert;
using PushNotification.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using PushNotification.Common;

namespace PushNotification
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        static readonly string TAG = "MainActivity";

        internal static readonly string CHANNEL_ID = "my_notification_channel";
        internal static readonly int NOTIFICATION_ID = 180;

        TextView msgText;
        TextView weaText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            //// Set our view from the "main" layout resource
            //SetContentView(Resource.Layout.activity_main);

            SetContentView(Resource.Layout.activity_main);
            msgText = FindViewById<TextView>(Resource.Id.msgText);
            weaText = FindViewById<TextView>(Resource.Id.weatherText);

            if (Intent.Extras != null)
            {
                foreach (var key in Intent.Extras.KeySet())
                {
                    var value = Intent.Extras.GetString(key);
                    Log.Debug(TAG, "Key: {0} Value: {1}", key, value);
                }
            }

            IsPlayServicesAvailable();
            CreateNotificationChannel();

            // Get Token
            var logTokenButton = FindViewById<Button>(Resource.Id.logTokenButton);
            logTokenButton.Click += delegate {
                Log.Debug(TAG, "InstanceID token: " + FirebaseInstanceId.Instance.Token);
            };

            // Topic message
            var subscribeButton = FindViewById<Button>(Resource.Id.subscribeButton);
            subscribeButton.Click += delegate {
                FirebaseMessaging.Instance.SubscribeToTopic("news");
                Log.Debug(TAG, "Subscribed to remote notifications");
            };

            var getWeatherButton = FindViewById<Button>(Resource.Id.getWeatherButton);
            getWeatherButton.Click += async delegate {
                WeatherRoot weatherRoot = await GetWeather("Ha Noi");
                if (weatherRoot != null)
                {
                    weaText.Text = weatherRoot.DisplayIcon;
                }
                Log.Debug(TAG, SerializeObject(weatherRoot));
            };

            var sendFCMNotification = FindViewById<Button>(Resource.Id.sendFCMNotification);
            sendFCMNotification.Click += async delegate {
                weaText.Text = await SendFCM(FirebaseInstanceId.Instance.Token.ToString());
            };
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    msgText.Text = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                else
                {
                    msgText.Text = "This device is not supported";
                    Finish();
                }
                return false;
            }
            else
            {
                msgText.Text = "Google Play Services is available.";
                return true;
            }
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID,
                                                  "FCM Notifications",
                                                  NotificationImportance.Default)
            {

                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        public async Task<WeatherRoot> GetWeather(string city, Units units = Units.Imperial)
        {
            const string APIKey = "b6fab57a77c3e8b460a4bd78df6539e2";
            const string WeatherCityUri = "http://api.openweathermap.org/data/2.5/weather?q={0}&units={1}&appid={2}";

            using (var client = new HttpClient())
            {
                var url = string.Format(WeatherCityUri, city, units.ToString().ToLower(), APIKey);
                var json = await client.GetStringAsync(url);

                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return DeserializeObject<WeatherRoot>(json);
            }
        }

        private async Task<string> SendFCM(string deviceToken)
        {
            //var deviceToken = "d4dVsaQGjFs:APA91bHVZf2I5F81vJd6vfx_QKJPJgPrfVVsyuus9aIK37NedEtE5FCAtmjfm6PPzQXGhdWiPYiMsph_WMnmbVaNkgeCTQ0mU6NfSbwYBRzWDEXw8D7dglDSkdp5lM2cWdzv0NzUPWj9";
            var titleNoti = "This is Ha Noi's weather, today";
            var bodyNoti = "Welcome to Ha Noi, Viet Nam. We bring for you weather info to help your nice travel";

            HttpClient client = new HttpClient();

            var jsonData = new JsonData
            {
                To = deviceToken,
                Notification = new NotificationInfo { Title = titleNoti, Body = bodyNoti },
                Data = new Data { IconUrl = "http://openweathermap.org/img/wn/10d@2x.png" }
            };

            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(jsonData));
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", string.Format("key={0}", Constant.keyAPI));
            //httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("Authorization", @"key=AIzaSyClQdXQ2wqV_KPd1Yqum45-eXfTVhQEHVs"));
            HttpResponseMessage response = client.PostAsync(Constant.URL, httpContent).Result;

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return "Send Failed";
            }
        }
    }

}