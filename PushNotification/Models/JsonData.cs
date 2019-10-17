using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace PushNotification.Models
{
    public class JsonData
    {
        [JsonProperty(PropertyName = "to")]
        public string To { get; set; }

        [JsonProperty(PropertyName = "notification")]
        public NotificationInfo Notification { get; set; }

        [JsonProperty(PropertyName = "data")]
        public Data Data { get; set; }
    }

    public class NotificationInfo
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }
    }

    public class Data
    {
        [JsonProperty(PropertyName = "icon_url")]
        public string IconUrl { get; set; }
    }
}