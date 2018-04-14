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

namespace nl.pleduc.TsumTsumHeartTracker
{
    [Activity(Label = "SettingsActivity")]
    public class SettingsActivity : Activity
    {
        private Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Settings);

            // Create your application here
            FragmentManager.BeginTransaction().Replace(Resource.Id.context, new SettingsFragment()).Commit();

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = GetString(Resource.String.ApplicationNameShort);
            ActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.actionbar_menu_settings, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnNavigateUp()
        {
            Finish();
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Toast.MakeText(this, "Action selected: " + item.TitleFormatted, ToastLength.Short).Show();

            //switch (item.ItemId)
            //{

            //}

            return base.OnOptionsItemSelected(item);
        }
    }
}