using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using Android.Net.Wifi;

namespace nl.pleduc.TsumTsumHeartTracker
{
    [Activity(Label = "TsumTsumHeartTracker", MainLauncher = true, Icon = "@drawable/icon")]
    [MetaData("android.app.searchable", Resource = "@xml/searchable")]
    public class MainActivity : Activity /*, SearchView.IOnQueryTextListener*/
    {
        private const string VERSION = "1.0";
        private const string TAG = "TsumTsumHeartTracker";

        private Spinner sortingSpinner;
        private Toolbar toolbar;

        private TsumTsumNotifcationBroadcastReceiver tsumTsumBroadcastReceiver;
        private TsumTsumDatabaseManager tsumTsumDatabaseManager;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            sortingSpinner = FindViewById<Spinner>(Resource.Id.spinner1);

            tsumTsumDatabaseManager = new TsumTsumDatabaseManager();
            tsumTsumBroadcastReceiver = new TsumTsumNotifcationBroadcastReceiver();

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = GetString(Resource.String.ApplicationNameShort);

            sortingSpinner.ItemSelected += delegate
            {
                Settings.SetInt("sortingType", sortingSpinner.SelectedItemPosition);
                UpdateHeartView();
            };

            StartNotificationServiceIfNotRunning();

            if (!IsNotificationPermissionSet())
            {
                CreateAlert(
                    "No Permission",
                    "The required permissions are not set, do you want to set the setting now?",
                    "Yes",
                    (object sender, DialogClickEventArgs args) => { StartActivityForResult(new Intent("android.settings.ACTION_NOTIFICATION_LISTENER_SETTINGS"), 0); }, // BEFORE: Android.Provider.Settings.ActionNotificationListenerSettings
                    "No",
                    (object sender, DialogClickEventArgs args) => { } // Deliberately empty!
                );
            }

            CheckVersion(null, () =>
            {
                CreateAlert(
                    "New version available",
                    "A new version of the app is available, do you want to download it now?",
                    "Yes",
                    (object sender, DialogClickEventArgs args) => { StartActivity(new Intent(Intent.ActionView, global::Android.Net.Uri.Parse("http://tsumtsumhearttracker.pleduc.nl/download/newest/nl.pleduc.TsumTsumNotificationTracker-Signed.apk"))); },
                    "No",
                    (object sender, DialogClickEventArgs args) => { } // Deliberately empty!
                );
            });

            Notification.Builder notificationBuilder = new Notification.Builder(this);
            notificationBuilder.SetContentTitle("Test");
            notificationBuilder.SetContentText("Test2");
            notificationBuilder.SetSmallIcon(Resource.Drawable.Icon);
            Notification notification = notificationBuilder.Build();

            
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnResume()
        {
            base.OnResume();

            StartNotificationServiceIfNotRunning();
            UpdateHeartView();

            Log.Info(TAG, $"Appcode {Settings.AppCode}, WifiToggle {Settings.WifiToggle}, WifiInterval {Settings.WifiInterval}");
        }

        protected override void OnPause()
        {
            base.OnPause();

            StartNotificationServiceIfNotRunning();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.actionbar_menu_main, menu);

            SearchManager searchManager = (SearchManager)GetSystemService(Context.SearchService);
            SearchView searchView = (SearchView)menu.FindItem(Resource.Id.menu_search).ActionView;
            searchView.QueryTextChange += (s, e) => { Log.Info(TAG, e.NewText); e.Handled = true; };
            searchView.QueryTextSubmit += (s, e) => { Log.Info(TAG, e.Query); e.Handled = true; };
            //searchView.SetSearchableInfo(searchManager.GetSearchableInfo(ComponentName));
            //searchView.SetOnQueryTextListener(this);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Toast.MakeText(this, "Action selected: " + item.TitleFormatted, ToastLength.Short).Show();

            switch (item.ItemId)
            {
                case Resource.Id.menu_refresh:
                    UpdateHeartView();
                    WifiManager wm = (WifiManager)GetSystemService(Context.WifiService);
                    wm.SetWifiEnabled(!wm.IsWifiEnabled);
                    break;
                case Resource.Id.menu_reset:
                    CreateAlert(
                        "Reset Data",
                        "Are you sure you want to reset all tracking data?",
                        "Yes",
                        (object sender, DialogClickEventArgs args) => { tsumTsumDatabaseManager.ClearDatabase(); UpdateHeartView(); },
                        "No",
                        (object sender, DialogClickEventArgs args) => { } // Deliberately empty!
                    );
                    break;
                case Resource.Id.menu_settings:
                    StartActivity(new Intent(this, Java.Lang.Class.FromType(typeof(SettingsActivity))));
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void UpdateHeartView()
        {
            TableLayout tableLayout = FindViewById<TableLayout>(Resource.Id.tableLayout1);
            tableLayout.RemoveAllViews();

            List<TsumTsumSender> tableList;

            int sortingType = Settings.GetInt("sortingType", 0);
            switch (sortingType)
            {
                case 0: // Alphabetic
                    tableList = tsumTsumDatabaseManager.PerformRawQuery<TsumTsumSender>($"SELECT * FROM Senders ORDER BY SenderName COLLATE NOCASE ASC, HeartCount DESC");
                    break;
                case 1: // Heartcount
                    tableList = tsumTsumDatabaseManager.PerformRawQuery<TsumTsumSender>($"SELECT * FROM Senders ORDER BY HeartCount DESC, SenderName COLLATE NOCASE ASC");
                    break;
                case 2: // Time (TODO)
                    tableList = tsumTsumDatabaseManager.PerformRawQuery<TsumTsumSender>($"SELECT * FROM Senders ORDER BY LastReceiveTimestamp DESC, SenderName COLLATE NOCASE ASC");
                    break;
                default:
                    throw new NotImplementedException();
            }

            tableLayout.AddView(CreateRow("#", "Name", "Total", "Last Received", true), 0);

            for (int i = 0; i < tableList.Count; i++)
            {
                TsumTsumSender sender = tableList[i];

                TableRow row = CreateRow(
                    (i + 1).ToString(),
                    sender.SenderName,
                    sender.HeartCount.ToString(),
                    string.Format("{0} {1}", sender.LastReceiveTimestamp.ToString("t"), sender.LastReceiveTimestamp.ToString("d"))
                );


                tableLayout.AddView(row, i + 1);
            }
        }

        private TableRow CreateRow(string indexField, string senderNameField, string hearthCountField, string timeField, bool isHeader = false)
        {
            TableRow row = new TableRow(this);
            TableRow.LayoutParams layoutParams = new TableRow.LayoutParams(TableRow.LayoutParams.MatchParent);
            row.LayoutParameters = layoutParams;

            TextView numberTextView = new TextView(this);
            TextView usernameTextView = new TextView(this);
            TextView heartCountTextView = new TextView(this);
            TextView receiveTimeTextView = new TextView(this);

            numberTextView.SetPadding(0, 2, 10, 2);
            usernameTextView.SetPadding(3, 2, 3, 2);
            heartCountTextView.SetPadding(3, 2, 3, 2);
            receiveTimeTextView.SetPadding(3, 2, 0, 2);

            heartCountTextView.SetMinWidth(35);

            numberTextView.SetText(indexField, TextView.BufferType.Normal);
            usernameTextView.SetText(senderNameField, TextView.BufferType.Normal);
            heartCountTextView.SetText(hearthCountField, TextView.BufferType.Normal);
            receiveTimeTextView.SetText(timeField, TextView.BufferType.Normal);

            if (isHeader)
            {
                numberTextView.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                usernameTextView.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                heartCountTextView.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                receiveTimeTextView.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);

                numberTextView.SetTextSize(ComplexUnitType.Sp, (numberTextView.TextSize * 1.25f) / Resources.DisplayMetrics.Density);
                usernameTextView.SetTextSize(ComplexUnitType.Sp, (usernameTextView.TextSize * 1.25f) / Resources.DisplayMetrics.Density);
                heartCountTextView.SetTextSize(ComplexUnitType.Sp, (heartCountTextView.TextSize * 1.25f) / Resources.DisplayMetrics.Density);
                receiveTimeTextView.SetTextSize(ComplexUnitType.Sp, (receiveTimeTextView.TextSize * 1.25f) / Resources.DisplayMetrics.Density);
            }

            numberTextView.LayoutParameters = new TableRow.LayoutParams(TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent);
            usernameTextView.LayoutParameters = new TableRow.LayoutParams(TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent, 0.6f);
            heartCountTextView.LayoutParameters = new TableRow.LayoutParams(TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent, 0.4f);
            receiveTimeTextView.LayoutParameters = new TableRow.LayoutParams(TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent);

            receiveTimeTextView.TextAlignment = TextAlignment.ViewEnd;

            row.AddView(numberTextView);
            row.AddView(usernameTextView);
            row.AddView(heartCountTextView);
            row.AddView(receiveTimeTextView);

            return row;
        }

        private void StartNotificationServiceIfNotRunning()
        {
            if (!IsNotificationServiceRunning())
            {
                Log.Info(TAG, "Notification service is not running, trying to start service!");
                StartService(new Intent(this, typeof(TsumTsumNotificationListener)));
            }
            else
            {
                Log.Info(TAG, "Notification service is running!");
            }
        }

        private bool IsNotificationServiceRunning()
        {
            ActivityManager activityManager = GetSystemService(ActivityService) as ActivityManager;
            IList<ActivityManager.RunningServiceInfo> runningServices = activityManager.GetRunningServices(int.MaxValue);

            for (int i = 0; i < runningServices.Count; i++)
            {
                if (runningServices[i].Service.ClassName == typeof(TsumTsumNotificationListener).FullName)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsNotificationPermissionSet()
        {
            ComponentName cn = new ComponentName(this, Java.Lang.Class.FromType(typeof(TsumTsumNotificationListener)));
            string flat = Android.Provider.Settings.Secure.GetString(this.ContentResolver, "enabled_notification_listeners");
            return (flat != null && flat.Contains(cn.FlattenToString()));
        }

        private int internalNotificationId = 0;
        private void SendNotification(string title, string text, int resourceIconId)
        {
            Notification.Builder notificationBuilder = new Notification.Builder(this);
            notificationBuilder.SetContentTitle(title);
            notificationBuilder.SetContentText(text);
            notificationBuilder.SetSmallIcon(resourceIconId);

            Notification notification = notificationBuilder.Build();

            NotificationManager notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            notificationManager.Notify(++internalNotificationId, notification);
        }

        private void CreateAlert(string title, string message, string positiveText, EventHandler<DialogClickEventArgs> positiveCallback, string negativeText, EventHandler<DialogClickEventArgs> negativeCallback, int iconId = Resource.Drawable.Icon)
        {
            AlertDialog.Builder builder;
            builder = new AlertDialog.Builder(this);

            builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetPositiveButton(positiveText, positiveCallback);
            builder.SetNegativeButton(negativeText, negativeCallback);
            builder.SetIcon(Resource.Drawable.Icon);
            builder.Show();
        }

        private void HandleDatabaseUpdatedEvent(object sender, EventArgs args)
        {
            Log.Debug(TAG, "Database updated event received, updating view!");
            UpdateHeartView();
        }

        private void RunCallback(Action callback, bool runOnUIThread)
        {

        }

        public void CheckAppCode(string appcode, Action onAppcodeVerified, Action onAppcodeDenied, bool runCallbackOnUIThread = false)
        {
            if (!TsumTsumWebHelper.CanPerformWebRequest(this)) { return; }

            TsumTsumWebHelper.PerformPostRequest(
                $"requestType=verify&appcode={appcode}",
                (result) =>
                {
                    Boolean succeeded = result[TsumTsumWebHelper.DATA_KEY];
                    if (succeeded)
                    {
                        RunCallback(onAppcodeVerified, runCallbackOnUIThread);
                    }
                    else
                    {
                        RunCallback(onAppcodeDenied, runCallbackOnUIThread);
                    }
                },
                () => { RunCallback(onAppcodeDenied, runCallbackOnUIThread); }
            );
        }

        public void CheckVersion(Action onVersionMatch, Action onVersionMismatch, bool runCallbackOnUIThread = false)
        {
            if (!TsumTsumWebHelper.CanPerformWebRequest(this)) { return; }

            TsumTsumWebHelper.PerformPostRequest(
                "requestType=version",
                (result) =>
                {
                    string retrievedVersion = result[TsumTsumWebHelper.DATA_KEY];
                    if (!string.IsNullOrEmpty(retrievedVersion))
                    {
                        if (VERSION == retrievedVersion)
                        {
                            RunCallback(onVersionMatch, runCallbackOnUIThread);
                        }
                        else
                        {
                            RunCallback(onVersionMismatch, runCallbackOnUIThread);
                        }
                    }
                    else
                    {
                        Log.Error(TAG, $"Empty or null result from ({TsumTsumWebHelper.DATA_KEY}), from json: {result.ToString()}");
                    }
                },
                () => { RunCallback(onVersionMismatch, runCallbackOnUIThread); }
            );
        }
    }
}

