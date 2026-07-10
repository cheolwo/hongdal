using Android.App;
using Android.Content.PM;
using Android.Gms.Tasks;
using Android.OS;
using DriverApp.Platforms.Android;
using Firebase.Messaging;

#pragma warning disable CA1416, CS0618

namespace DriverApp;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize
                           | ConfigChanges.Orientation
                           | ConfigChanges.UiMode
                           | ConfigChanges.ScreenLayout
                           | ConfigChanges.SmallestScreenSize
                           | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private const int PostNotificationsRequestCode = 3107;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        RequestPostNotificationsPermissionIfNeeded();
        FirebaseMessaging.Instance.GetToken().AddOnCompleteListener(new FcmTokenCompleteListener());
    }

    private void RequestPostNotificationsPermissionIfNeeded()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
        {
            return;
        }

        if (CheckSelfPermission(Android.Manifest.Permission.PostNotifications) == Permission.Granted)
        {
            return;
        }

        RequestPermissions([Android.Manifest.Permission.PostNotifications], PostNotificationsRequestCode);
    }

    private sealed class FcmTokenCompleteListener : Java.Lang.Object, IOnCompleteListener
    {
        public void OnComplete(global::Android.Gms.Tasks.Task task)
        {
            if (!task.IsSuccessful)
            {
                return;
            }

            var token = task.Result?.ToString();
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            _ = HongdalFirebaseMessagingService.RegisterTokenAsync(token);
        }
    }
}

#pragma warning restore CA1416, CS0618
