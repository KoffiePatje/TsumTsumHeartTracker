using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Service.Notification;

namespace nl.pleduc.TsumTsumHeartTracker
{
    [Service(Label = "TsumTsumHeartTracker", Name = "nl.pleduc.TsumTsumHeartTracker.TsumTsumNotificationListener", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new string[] { "android.service.notification.NotificationListenerService" })]
    public class TsumTsumNotificationListener : NotificationListenerService
    {
        private const string TAG = "TsumTsumNotificationService";

        private const string TargetPackageName = "com.linecorp.LGTMTMG"; // TsumTsum
        private const string TargetAppName = "TsumTsum";

        private TsumTsumDatabaseManager tsumTsumDatabaseManager;

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Info(TAG, "OnCreate");

            tsumTsumDatabaseManager = new TsumTsumDatabaseManager();

            StartService(new Intent(this, typeof(TsumTsumNotificationListener)));
        }

#if __ANDROID_21__
        public override void OnListenerConnected()
        {
            base.OnListenerConnected();
            Log.Info(TAG, "OnListenerConnected");
        }
#endif

#if __ANDROID_25__
        public override void OnListenerDisconnected()
        {
            base.OnListenerDisconnected();
            Log.Info(TAG, "OnListenerDisconnected");
        }
#endif

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);
            return StartCommandResult.Sticky;
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
#if __ANDROID_21__ // In previous versions this function was marked abstract, and thus the base call was not possible
            base.OnNotificationPosted(sbn);
#endif
            Log.Info(TAG, "OnNotificationPosted");

            if (IsTsumTsumNotification(sbn))
            {
                string senderName;
                if (TryGetTsumTsumHeartSender(sbn, out senderName))
                {
                    Log.Info(TAG, $"Succesfully identified sender as \"{senderName}\"");

                    TsumTsumSender sender = tsumTsumDatabaseManager.RetrieveOrCreateSenderByName(senderName);
                    if (sender != null)
                    {
                        sender.HeartCount++;
                        sender.LastReceiveTimestamp = DateTime.Now;
                        tsumTsumDatabaseManager.UpdateOrInsertSender(sender);

                        SendNotification(senderName, () => { Log.Info(TAG, $"Sender: {sender} send!"); }, () => { Log.Info(TAG, $"Sender: {sender} failed!"); });

                        // Send a broadcast, if the application is running it can pick up the broadcast message and update the view
                        Intent intent = new Intent(this, Java.Lang.Class.FromType(typeof(TsumTsumNotifcationBroadcastReceiver))); //new Intent("nl.pleduc.TsumTsumHeartTracker");
                        intent.PutExtra("sender_database_updated", senderName);
                        Android.Support.V4.Content.LocalBroadcastManager.GetInstance(this).SendBroadcast(intent);

                        // If we have more than a set number of TsumTsum notifications, remove the oldest!
                        StatusBarNotification[] sbns = GetActiveNotifications();
                        List<StatusBarNotification> tsumTsumNotificationList = new List<StatusBarNotification>();
                        for (int i = 0; i < sbns.Length; i++)
                        {
                            if (IsTsumTsumNotification(sbns[i]))
                            {
                                tsumTsumNotificationList.Add(sbns[i]);
                            }
                        }

                        // Delete notifications in a FIFO way
                        Log.Info(TAG, $"Notification count {tsumTsumNotificationList.Count}");
                        if (tsumTsumNotificationList.Count > 5) {
                            tsumTsumNotificationList.Sort((a, b) => { return b.Notification.When.CompareTo(a.Notification.When); });
                            //Android.Support.V4.App.NotificationManagerCompat notificationManager = Android.Support.V4.App.NotificationManagerCompat.From(this);

                            Log.Info(TAG, "Canceling Notification(s)");
                            while(tsumTsumNotificationList.Count > 5)
                            {
                                int idxToRemove = tsumTsumNotificationList.Count - 1;
                                StatusBarNotification notificationToRemove = tsumTsumNotificationList[idxToRemove];
                                CancelNotification(notificationToRemove.PackageName, notificationToRemove.Tag, notificationToRemove.Id);
                                //CancelNotification(notificationToRemove.Key);
                                tsumTsumNotificationList.RemoveAt(idxToRemove);
                            }
                        }
                    }
                }
                else
                {
                    Log.Error(TAG, "Unable to extract the sender's name from the notification, possibly another type of TsumTsum notification!");
                }
            }
            else
            {
                Log.Info(TAG, $"Not a {TargetAppName} notification, skipping it!");
            }
        }

#if !(__ANDROID_API_21__) 
        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            // Empty (deliberatly)
        }
#endif

        private bool IsTsumTsumNotification(StatusBarNotification sbn)
        {
            return sbn.PackageName == TargetPackageName && (sbn.Notification != null && sbn.Notification.Extras != null && sbn.Notification.TickerText != null && sbn.Notification.TickerText.ToString() == TargetAppName);
        }

        private bool TryGetTsumTsumHeartSender(StatusBarNotification sbn, out string senderName)
        {
            if (sbn.Notification != null && sbn.Notification.Extras != null)
            {
                string notificationText = sbn.Notification.Extras.GetCharSequence("android.text").ToString();
                if (!string.IsNullOrEmpty(notificationText))
                {
                    senderName = notificationText.Substring(0, notificationText.IndexOf(" send you a Heart as a gift!"));
                    return true;
                }
                else
                {
                    Log.Error(TAG, "No text found for the current notification!");
                }
            }

            senderName = "Unknown";
            return false;
        }

        public void SendNotification(string senderName, Action onNotificationSendSuccess, Action onNotificationSendFailed)
        {
            if (!TsumTsumWebHelper.CanPerformWebRequest(this)) { return; }
            if (string.IsNullOrEmpty(senderName)) { return; }

            TsumTsumWebHelper.PerformPostRequest(
                $"requestType=notification&appcode=ttht_vTOz_598b7075b7141&senderName={senderName}",
                (result) =>
                {
                    Boolean succeeded = result[TsumTsumWebHelper.DATA_KEY];
                    if (succeeded)
                    {
                        onNotificationSendSuccess();
                    }
                    else
                    {
                        onNotificationSendFailed();
                    }
                },
                onNotificationSendFailed
            );
        }

#if __ANDROID_API_21__
        private void LogStausBarNotification(StatusBarNotification sbn)
        {
            Log.Info(TAG, string.Format("[StatusBarNotification] GroupKey: {0}, Id: {1}, Key: {2}, Notification: {3}, PackageName: {4}, Tag: {5}, ToString: {6}",
                sbn.GroupKey,
                sbn.Id,
                sbn.Key,
                sbn.Notification,
                sbn.PackageName,
                sbn.Tag,
                sbn.ToString()));
        }

        private void LogNotification(Notification notification)
        {
            Bundle extras = notification.Extras;
            string bundleTitle = extras != null ? extras.GetString("android.title") : "unavailable";
            string bundleText = extras != null ? extras.GetCharSequence("android.text").ToString() : "unavailable";

            Log.Info(TAG, string.Format("[Notification] Category: {0}, Number: {1}, TicketText.ToString: {2}, bundleTitle: {3}, bundleText: {4}",
                notification.Category,
                notification.Number,
                notification.TickerText != null ? notification.TickerText.ToString() : "unavailable",
                bundleTitle ?? "null",
                bundleText ?? "null"));
        }
#else
        private void LogStausBarNotification(StatusBarNotification sbn)
        {
            Log.Info(TAG, string.Format("[StatusBarNotification] GroupKey: {0}, Id: {1}, Key: {2}, Notification: {3}, PackageName: {4}, Tag: {5}, ToString: {6}",
                "Unavailable (API)",
                sbn.Id,
                "Unavailable (API)",
                sbn.Notification,
                sbn.PackageName,
                sbn.Tag,
                sbn.ToString()));
        }

        private void LogNotification(Notification notification)
        {
            Bundle extras = notification.Extras;
            string bundleTitle = extras != null ? extras.GetString("android.title") : "unavailable";
            string bundleText = extras != null ? extras.GetCharSequence("android.text").ToString() : "unavailable";

            Log.Info(TAG, string.Format("[Notification] Category: {0}, Number: {1}, TicketText.ToString: {2}, bundleTitle: {3}, bundleText: {4}",
                "Unavailable (API)",
                notification.Number,
                notification.TickerText != null ? notification.TickerText.ToString() : "unavailable",
                bundleTitle ?? "null",
                bundleText ?? "null"));
        }
#endif
    }
}