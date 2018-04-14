using Android.App;
using Android.Content;
using Android.Preferences;

namespace nl.pleduc.TsumTsumHeartTracker
{
    public static class Settings
    {
        private static ISharedPreferences _sharedPreferences;
        public static ISharedPreferences SharedPreferences
        {
            get
            {
                if (_sharedPreferences == null)
                {
                    _sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                }
                return _sharedPreferences;
            }
        }

        public static string AppCode
        {
            get
            {
                return SharedPreferences.GetString("preferences_cloud_appcode", string.Empty);
            }
            set
            {
                SharedPreferences.Edit().PutString("preferences_cloud_appcode", value).Commit();
            }
        }

        public static bool WifiToggle
        {
            get
            {
                return SharedPreferences.GetBoolean("preferences_wifi_toggle_enabled", false);
            }
            set
            {
                SharedPreferences.Edit().PutBoolean("preferences_wifi_toggle_enabled", value).Commit();
            }
        }
        public static int WifiInterval
        {
            get
            {
                return SharedPreferences.GetInt("preferences_wifi_toggle_interval", 0);
            }
            set
            {
                SharedPreferences.Edit().PutInt("preferences_wifi_toggle_interval", value).Commit();
            }
        }

        public static int GetInt(string key, int defaultValue)
        {
            return SharedPreferences.GetInt(key, defaultValue);
        }

        public static void SetInt(string key, int value)
        {
            SharedPreferences.Edit().PutInt(key, value).Commit();
        }
    }
}