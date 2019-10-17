using System;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Util;
using Firebase.Messaging;

using System.Collections.Generic;
using Android.Support.V4.App;
using Android.Graphics;
using static Newtonsoft.Json.JsonConvert;
using Java.Net;
using Java.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PushNotification
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "MyFirebaseMsgService";
        public override void OnMessageReceived(RemoteMessage message)
        {
            var notify = message.GetNotification();            
            Log.Debug(TAG, "[Title + Content]: " + notify.Body); // Log

            SendNotificationAsync(notify, message.Data);
        }

        async void SendNotificationAsync(object body, IDictionary<string, string> data)
        {
            var obj = (RemoteMessage.Notification)body;
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            foreach (var key in data.Keys)
            {
                intent.PutExtra(key, data[key]);
            }

            var pendingIntent = PendingIntent.GetActivity(this,
                                                          MainActivity.NOTIFICATION_ID,
                                                          intent,
                                                          PendingIntentFlags.OneShot);

            Bitmap bitmap = null;
            //bitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.ic_user);

            using (var client = new HttpClient())
            {
                var inputStream = await client.GetStreamAsync(data["icon_url"]);
                bitmap = BitmapFactory.DecodeStream(inputStream);
            }

            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                                      .SetSmallIcon(Resource.Drawable.ic_user)
                                      .SetContentTitle(obj.Title)      
                                      .SetContentText(obj.Body)
                                      .SetWhen(DateTime.UtcNow.Ticks)                                      
                                      .SetAutoCancel(true)
                                      .SetLargeIcon(bitmap)
                                      .SetStyle(new NotificationCompat.BigTextStyle().SetBigContentTitle(obj.Title))
                                      .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NOTIFICATION_ID, notificationBuilder.Build());
        }
    }
}