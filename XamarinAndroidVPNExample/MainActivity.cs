using System;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using XamarinAndroidVPNExample.VPNService;

namespace XamarinAndroidVPNExample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const int VPN_REQUEST_CODE = 0x0F;

        private bool _waitingForVPNStart;

        VpnStateReceiver vpnStateReceiver;

        public class VpnStateReceiver : BroadcastReceiver
        {
            MainActivity c;

            public VpnStateReceiver(MainActivity c)
            {
                this.c = c;
            }

            public Action<Intent> Receive { get; set; }

            public override void OnReceive(Context context, Intent intent)
            {
                if (LocalVPNService.BROADCAST_VPN_STATE.Equals(intent.Action))
                {
                    if (intent.GetBooleanExtra("running", false))
                    {
                        c._waitingForVPNStart = false;
                    }
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

           Button btStart = FindViewById<Button>(Resource.Id.btStart);
            btStart.Click += BtStart_Click;

            _waitingForVPNStart = false;

            vpnStateReceiver = new VpnStateReceiver(this);

            LocalBroadcastManager.GetInstance(this).RegisterReceiver(vpnStateReceiver,
                    new IntentFilter(LocalVPNService.BROADCAST_VPN_STATE));
        }

        private void BtStart_Click(object sender, EventArgs e)
        {
            StartVPN();
        }       

        private void StartVPN()
        {
            Intent vpnIntent = VpnService.Prepare(this);
            if (vpnIntent != null)
                StartActivityForResult(vpnIntent, VPN_REQUEST_CODE);
            else
                OnActivityResult(VPN_REQUEST_CODE, Result.Ok, null);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == VPN_REQUEST_CODE && resultCode == Result.Ok)
            {
                _waitingForVPNStart = true;
                StartService(new Intent(this, typeof(LocalVPNService)));
                EnableButton(false);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            EnableButton(!_waitingForVPNStart && !LocalVPNService.IsRunning);
        }

        private void EnableButton(bool enable)
        {
            Button vpnButton = FindViewById<Button>(Resource.Id.btStart);
            if (enable)
            {
                vpnButton.Enabled = true;
                vpnButton.Text = "Start VPN";
            }
            else
            {
                vpnButton.Enabled = false;
                vpnButton.Text = "Stop VPN via VPN settings";
            }
        }
    }
}

