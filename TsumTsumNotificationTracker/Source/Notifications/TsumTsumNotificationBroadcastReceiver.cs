using System;

using Android.App;
using Android.Content;

namespace nl.pleduc.TsumTsumHeartTracker
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class TsumTsumNotifcationBroadcastReceiver : BroadcastReceiver
    {
        public event EventHandler dataBaseUpdatedEvent;

        public override void OnReceive(Context context, Intent intent)
        {
            string value = intent.GetStringExtra("sender_database_updated");
            if (!string.IsNullOrEmpty(value))
            {
                dataBaseUpdatedEvent(this, EventArgs.Empty);
            }
        }
    }
}