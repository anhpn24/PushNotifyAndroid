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

namespace PushNotification.Common
{
    public class Constant
    {
        public static string URL = "https://fcm.googleapis.com/fcm/send";
        public static string keyAPI = "AIzaSyClQdXQ2wqV_KPd1Yqum45-eXfTVhQEHVs";
    }
    
    public enum Units
    {
        Imperial,
        Metric
    }    
}