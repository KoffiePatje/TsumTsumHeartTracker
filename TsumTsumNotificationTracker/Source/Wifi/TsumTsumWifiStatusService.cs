using Android.App;
using Android.Content;
using Android.OS;

namespace nl.pleduc.TsumTsumHeartTracker
{
    public class TsumTsumWifiStatusService : Service
    {
        //private Timer timer;

        public override void OnCreate()
        {
            //Handler handler = new Handler();

            //base.OnCreate();

            //timer = new Timer();
            //timer.Interval = 1000; // 1 sec for testing //1000 * 600; // 600 seconds = 10 min
            //timer.Elapsed += HandleTimerElapsed;

            //Intent i = new Intent(Application.Context,  )
        }

        //private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        //{
        //}

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }

//    public class TsumTsumWifiStatusNotification
//    {
//        public static TsumTsumWifiStatusNotification Instance
//        {
//            get
//            {
//                if (instance == null)
//                    instance = new TsumTsumWifiStatusNotification();
//                return instance;
//            }
//        }
//        private static TsumTsumWifiStatusNotification instance;



//        public TsumTsumWifiStatusNotification()
//        {
//            Notification notification = new Notification();
//        }
//    }

//    //[BroadcastReceiver]
//    //public class BackgroundReceiver : BroadcastReceiver
//    //{
//    //    public override void OnReceive(Context context, Intent intent)
//    //    {
//    //        PowerManager pm = (PowerManager)context.GetSystemService(context.powerservice);
//    //        PowerManager.WakeLock wakelock = pm.NewWakeLock()
//    //    }
//    //}
//}