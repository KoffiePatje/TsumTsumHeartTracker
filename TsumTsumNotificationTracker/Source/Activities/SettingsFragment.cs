using Android.App;
using Android.OS;
using Android.Preferences;

namespace nl.pleduc.TsumTsumHeartTracker
{
    public class SettingsFragment : PreferenceFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            AddPreferencesFromResource(Resource.Xml.preferences);
        }
    }
}