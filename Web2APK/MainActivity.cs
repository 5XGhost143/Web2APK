using Android.App;
using Android.OS;
using Android.Webkit;
using Android.Views;

namespace Web2APK
{
    [Activity(Label = "@string/app_name", MainLauncher = true,
              Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen",
              ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : Activity
    {
        WebView webView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            string appName = GetString(Resource.String.app_name);
            string appUrl = GetString(Resource.String.app_url);
            bool enableFadein = Resources.GetBoolean(Resource.Boolean.enable_fadein);
            bool onlyVertical = Resources.GetBoolean(Resource.Boolean.only_vertical);
            bool onlyHorizontal = Resources.GetBoolean(Resource.Boolean.only_horizontal);
            int fadeinDuration = Resources.GetInteger(Resource.Integer.fadein_duration);

            if (onlyVertical && onlyHorizontal)
            {
                FinishAffinity();
                return;
            }

            if (onlyVertical)
                RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            else if (onlyHorizontal)
                RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
            else
                RequestedOrientation = Android.Content.PM.ScreenOrientation.Unspecified;

            this.Title = appName;

            webView = new WebView(this);
            var webSettings = webView.Settings;
            webSettings.JavaScriptEnabled = true;
            webSettings.DomStorageEnabled = true;
            webSettings.LoadWithOverviewMode = true;
            webSettings.UseWideViewPort = true;
            webSettings.CacheMode = CacheModes.Default;
            webSettings.UserAgentString = "Mozilla/5.0 (Linux; Android) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.5938.132 Mobile Safari/537.36";
            webSettings.BuiltInZoomControls = false;
            webSettings.DisplayZoomControls = false;
            webSettings.SetSupportZoom(false);

            webView.SetWebViewClient(new WebViewClient());
            webView.LoadUrl(appUrl);

            if (onlyVertical || onlyHorizontal)
            {
                webView.SetOnTouchListener(new CustomTouchListener());
            }

            var layoutParams = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent
            );

            SetContentView(webView, layoutParams);

            SetFullScreen();

            if (enableFadein)
            {
                webView.Alpha = 0f;
                webView.Post(() =>
                {
                    webView.Animate()
                           .Alpha(1f)
                           .SetDuration(fadeinDuration)
                           .Start();
                });
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetFullScreen();
        }

        private void SetFullScreen()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R) // Android 11+
            {
                Window.SetDecorFitsSystemWindows(false);
                var controller = Window.InsetsController;
                if (controller != null)
                {
                    controller.Hide(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());
                    controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                }
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) // Android 4.4+ (last version that uses this should be 10)
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                    SystemUiFlags.Fullscreen |
                    SystemUiFlags.HideNavigation |
                    SystemUiFlags.ImmersiveSticky |
                    SystemUiFlags.LayoutStable |
                    SystemUiFlags.LayoutHideNavigation |
                    SystemUiFlags.LayoutFullscreen
                );
            }
            else
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.Fullscreen; // this is just here to be sure that it works
            }
        }

        public override void OnBackPressed()
        {
            if (webView.CanGoBack())
                webView.GoBack();
            else
                base.OnBackPressed();
        }
    }

    public class CustomTouchListener : Java.Lang.Object, View.IOnTouchListener
    {
        public bool OnTouch(View v, MotionEvent e)
        {
            if (e.ActionMasked == MotionEventActions.Move)
                v.Parent?.RequestDisallowInterceptTouchEvent(true);

            return false;
        }
    }
}
