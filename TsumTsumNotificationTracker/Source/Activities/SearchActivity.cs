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
using Android.Util;

namespace nl.pleduc.TsumTsumHeartTracker
{
    [Activity(Label = "SearchActivity")]
    [IntentFilter(new string[] { "android.intent.action.SEARCH" })]
    public class SearchActivity : Activity
    {
        public const string TAG = "TsumTsumSearchActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            handleIntent(Intent);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            handleIntent(intent);
        }

        private void handleIntent(Intent intent)
        {
            if (Intent.ActionSearch.Equals(intent.Action))
            {
                string query = intent.GetStringExtra(SearchManager.Query);
                Log.Info(TAG, query);
            }
        }
    }
}