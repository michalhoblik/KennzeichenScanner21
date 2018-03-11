using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Views;
using KennzeichenScanner21.Droid.Helpers;
using KennzeichenScanner21.Droid.Renderers;
using KennzeichenScanner21.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Platform.Android;

[assembly: Xamarin.Forms.ExportRenderer(typeof(SmartCameraPage), typeof(SmartCameraPageRenderer))]
namespace KennzeichenScanner21.Droid.Renderers
{
    public class SmartCameraPageRenderer : PageRenderer, TextureView.ISurfaceTextureListener, Android.Hardware.Camera.IAutoFocusCallback, Android.Hardware.Camera.IPictureCallback, Android.Hardware.Camera.IShutterCallback
    {
        private const int PICTURE_SIZE_MAX_WIDTH = 1280;
        private const int PICTURE_GALLERY_WIDTH = 1024;
        private const int PREVIEW_SIZE_MAX_WIDTH = 640;

        private const int PickImageId = 1000;

        private Android.Hardware.Camera _camera;
        private bool _flashOn;
        private float _mDist;

        private Android.Widget.Button _takePhotoButton;
        private Android.Widget.Button _toggleFlashButton;
        private Android.Widget.Button _backCameraButton;
        private Android.Widget.Button _libraryButton;
        private View _view;
        private Android.App.Activity _activity;

        private TextureView _textureView;
        private SurfaceTexture _surfaceTexture;

        private int _displayOrientation;
        private int _layoutOrientation;

        private CameraOrientationListener _orientationListener;

        public SmartCameraPageRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null && e.NewElement != null)
            {
                SetupUserInterface();
                SetupEventHandlers();
                AddView(_view);
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            _view.Measure(msw, msh);
            _view.Layout(0, 0, r - l, b - t);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            var newParameters = _camera.GetParameters();
            var action = e.Action;

            if (e.PointerCount > 1)
            {
                if (action == MotionEventActions.PointerDown)
                {
                    _mDist = GetFingerSpacing(e);
                }
                else if (action == MotionEventActions.Move && newParameters.IsZoomSupported)
                {
                    _camera.CancelAutoFocus();
                    HandleZoom(e, newParameters);
                }
            }

            return true;
        }

        private void HandleZoom(MotionEvent e, Android.Hardware.Camera.Parameters parameters)
        {
            var maxZoom = parameters.MaxZoom;
            var zoom = parameters.Zoom;
            var newDist = GetFingerSpacing(e);

            if (newDist > _mDist)
            {
                //zoom in
                if (zoom < maxZoom) zoom++;
            }
            else if (newDist < _mDist)
            {
                //zoom out
                if (zoom > 0) zoom--;
            }

            _mDist = newDist;
            parameters.Zoom = zoom;
            _camera.SetParameters(parameters);
        }

        private static float GetFingerSpacing(MotionEvent e)
        {
            var x = e.GetX(0) - e.GetX(1);
            var y = e.GetY(0) - e.GetY(1);
            return (float)Math.Sqrt(x * x + y * y);
        }

        private void SetupUserInterface()
        {
            _activity = Context as Android.App.Activity;
            _view = _activity?.LayoutInflater.Inflate(Resource.Layout.CameraLayout, this, false);

            if (_view != null) _textureView = _view.FindViewById<TextureView>(Resource.Id.textureView);
            _textureView.SurfaceTextureListener = this;
        }

        private void SetupEventHandlers()
        {
            _orientationListener = new CameraOrientationListener(_activity);
            if (_orientationListener.CanDetectOrientation()) _orientationListener.Enable();

            _takePhotoButton = _view.FindViewById<Android.Widget.Button>(Resource.Id.takePhotoButton);
            _takePhotoButton.Click += TakePhotoButtonTapped;

            _backCameraButton = _view.FindViewById<Android.Widget.Button>(Resource.Id.backCameraButton);
            _backCameraButton.Click += BackCameraButtonTappedAsync;

            _toggleFlashButton = _view.FindViewById<Android.Widget.Button>(Resource.Id.toggleFlashButton);
            _toggleFlashButton.Click += ToggleFlashButtonTapped;

            _libraryButton = _view.FindViewById<Android.Widget.Button>(Resource.Id.fromLibraryButton);
            _libraryButton.Click += _libraryButton_Click;
        }

        private void _libraryButton_Click(object sender, EventArgs e)
        {
            var libraryIntent = new Intent();
            libraryIntent.SetType("image/*");
            libraryIntent.SetAction(Intent.ActionGetContent);

            MainActivity.Instance.ConfigureActivityResultCallback(ImageChooserCallback);
            MainActivity.Instance.StartActivityForResult(Intent.CreateChooser(libraryIntent, "Bild auswählen"), PickImageId);
        }

        private void ImageChooserCallback(int requestCode, Android.App.Result resultCode, Intent data)
        {
            if (requestCode == PickImageId && resultCode == Android.App.Result.Ok && data != null)
            {
                try
                {
                    var pictureData = DecodeUri(data.Data);
                    ReformatAndSearchKennzeichen(pictureData, DateTime.Now.ToString());
                }
                catch (Exception)
                {
                    Element.Navigation.PopModalAsync();
                }
            }
        }

        public Bitmap DecodeUri(Android.Net.Uri uri)
        {
            using (var stream2 = Context.ContentResolver.OpenInputStream(uri))
            {
                var options = new BitmapFactory.Options { InJustDecodeBounds = true, InScaled = true };
                using (var stream = Context.ContentResolver.OpenInputStream(uri))
                {
                    BitmapFactory.DecodeStream(stream, null, options);

                    var widthTemp = options.OutWidth;
                    var heightTemp = options.OutHeight;
                    var scale = 1;

                    while (true)
                    {
                        if (widthTemp / 2 < PICTURE_GALLERY_WIDTH || heightTemp / 2 < PICTURE_GALLERY_WIDTH) break;
                        widthTemp /= 2;
                        heightTemp /= 2;
                        scale *= 2;
                    }

                    var options2 = new BitmapFactory.Options { InSampleSize = scale, InMutable = true };
                    return BitmapFactory.DecodeStream(stream2, null, options2);
                }
            }
        }

        #region SurfaceMethods

        public async void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            try
            {
                _camera = Android.Hardware.Camera.Open((int)CameraFacing.Back);
                determineDisplayOrientation();
                setupCamera();

                _textureView.LayoutParameters = new Android.Widget.FrameLayout.LayoutParams(width, height);
                _surfaceTexture = surface;
                _camera.SetPreviewTexture(_surfaceTexture);
                _camera.StartPreview();
            }
            catch (Exception)
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(_activity);

                builder.SetMessage("Die Kamera kann nicht geöffnet werden.");
                builder.SetTitle("Fehler");

                AlertDialog dialog = builder.Create();

                dialog.Show();
                await Element.Navigation.PopModalAsync();
            }
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            if (_camera == null) return true;
            _orientationListener.Disable();
            _camera.CancelAutoFocus();
            _camera.StopPreview();
            _camera.Release();
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }

        #endregion

        private void ToggleFlashButtonTapped(object sender, EventArgs e)
        {
            _flashOn = !_flashOn;
            if (_flashOn)
            {
                _toggleFlashButton.SetBackgroundResource(Resource.Drawable.FlashButton);
                var newCamparams = _camera.GetParameters();
                newCamparams.FlashMode = Android.Hardware.Camera.Parameters.FlashModeTorch;
                _camera.SetParameters(newCamparams);
            }
            else
            {
                _toggleFlashButton.SetBackgroundResource(Resource.Drawable.NoFlashButton);
                var newParameters = _camera.GetParameters();
                newParameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOff;
                _camera.SetParameters(newParameters);
            }
        }

        private void BackCameraButtonTappedAsync(object sender, EventArgs e)
        {
            Element.Navigation.PopModalAsync();
        }

        private void TakePhotoButtonTapped(object sender, EventArgs e)
        {
            _takePhotoButton.Enabled = false;
            _takePhotoButton.Clickable = false;
            _orientationListener.rememberOrientation();
            _camera.TakePicture(this, null, null, this);
        }

        public void determineDisplayOrientation()
        {
            var cameraInfo = new Android.Hardware.Camera.CameraInfo();
            Android.Hardware.Camera.GetCameraInfo(0, cameraInfo);

            var rotation = _activity.WindowManager.DefaultDisplay.Rotation;
            var degrees = 0;

            switch (rotation)
            {
                case SurfaceOrientation.Rotation0:
                    degrees = 0;
                    break;
                case SurfaceOrientation.Rotation180:
                    degrees = 180;
                    break;
                case SurfaceOrientation.Rotation270:
                    degrees = 270;
                    break;
                case SurfaceOrientation.Rotation90:
                    degrees = 90;
                    break;
            }

            int displayOrientation;

            if (cameraInfo.Facing == CameraFacing.Front)
            {
                displayOrientation = (cameraInfo.Orientation + degrees) % 360;
                displayOrientation = (360 - displayOrientation) % 360;
            }
            else
            {
                displayOrientation = (cameraInfo.Orientation - degrees + 360) % 360;
            }

            _displayOrientation = displayOrientation;
            _layoutOrientation = degrees;

            _camera.SetDisplayOrientation(displayOrientation);
        }

        public void setupCamera()
        {
            var parameters = _camera.GetParameters();

            var bestPreviewSize = determineBestPreviewSize(parameters);
            var bestPictureSize = determineBestPictureSize(parameters);

            parameters.SetPreviewSize(bestPreviewSize.Width, bestPreviewSize.Height);
            parameters.SetPictureSize(bestPictureSize.Width, bestPictureSize.Height);

            // set continous auto-focus (available from API9)
            parameters.FocusMode = parameters.SupportedFocusModes.Contains(Android.Hardware.Camera.Parameters.FocusModeContinuousPicture) ?
                                                                           Android.Hardware.Camera.Parameters.FocusModeContinuousPicture :
                                                                           Android.Hardware.Camera.Parameters.FocusModeFixed;

            _camera.SetParameters(parameters);
        }

        private Android.Hardware.Camera.Size determineBestPreviewSize(Android.Hardware.Camera.Parameters parameters)
        {
            var sizes = parameters.SupportedPreviewSizes;
            return determineBestSize(sizes, PREVIEW_SIZE_MAX_WIDTH);
        }

        private Android.Hardware.Camera.Size determineBestPictureSize(Android.Hardware.Camera.Parameters parameters)
        {
            var sizes = parameters.SupportedPictureSizes;
            return determineBestSize(sizes, PICTURE_SIZE_MAX_WIDTH);
        }

        protected Android.Hardware.Camera.Size determineBestSize(IList<Android.Hardware.Camera.Size> sizes, int widthThreshold)
        {
            Android.Hardware.Camera.Size bestSize = null;

            foreach (Android.Hardware.Camera.Size currentSize in sizes)
            {
                var isDesiredRatio = currentSize.Width / 4 == currentSize.Height / 3;
                var isBetterSize = bestSize == null || currentSize.Width > bestSize.Width;
                var isInBounds = currentSize.Width <= PICTURE_SIZE_MAX_WIDTH;

                if (isDesiredRatio && isInBounds && isBetterSize)
                {
                    bestSize = currentSize;
                }
            }

            return bestSize ?? sizes.First();
        }

        #region Camera_Callbacks

        public void OnAutoFocus(bool success, Android.Hardware.Camera camera)
        {
        }

        public void OnShutter()
        {
        }

        public void OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
        {
            try
            {
                camera.CancelAutoFocus();

                var baseBitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
                var rotation = (_displayOrientation + _orientationListener.getRememberedOrientation() + _layoutOrientation) % 360;
                if (rotation != 0)
                {
                    var oldBitmap = baseBitmap;
                    var matrix = new Matrix();
                    matrix.PostRotate(rotation);
                    baseBitmap = Bitmap.CreateBitmap(baseBitmap, 0, 0, baseBitmap.Width, baseBitmap.Height, matrix, false);

                    oldBitmap.Recycle();
                    oldBitmap.Dispose();
                    oldBitmap = null;
                }

                var bitmap = baseBitmap.Copy(Bitmap.Config.Argb8888, true);
                baseBitmap.Recycle();
                baseBitmap.Dispose();
                baseBitmap = null;

                // get filepath wo das gespeichert ist
                ReformatAndSearchKennzeichen(bitmap, DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                AlertDialog.Builder dialog = new AlertDialog.Builder(MainActivity.Instance);
                AlertDialog alert = dialog.Create();
                alert.SetTitle("Fehler");
                alert.SetMessage($"Fehler aufgetreten:\n{ex.Message}");
                alert.SetButton("OK", (c, ev) =>
                {
                });

                alert.Show();
                Element.Navigation.PopModalAsync();
            }
        }

        private void ReformatAndSearchKennzeichen(Bitmap bitmap, string timestamp)
        {
            var canvas = new Canvas(bitmap);
            var tPaint = new Paint { TextSize = 35, Color = Color.Red };
            tPaint.SetStyle(Paint.Style.Fill);
            tPaint.AntiAlias = true;
            var bPaint = new Paint();

            byte[] imageContent;
            using (var output = new System.IO.MemoryStream())
            {
                canvas.DrawBitmap(bitmap, 0f, 0f, bPaint);

                var width = tPaint.MeasureText(timestamp);
                canvas.DrawText(timestamp, bitmap.Width - width - 25f, bitmap.Height - 25f, tPaint);

                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 50, output);
                imageContent = output.ToArray();

                output.Flush();
                output.Close();
                output.Dispose();

                bitmap.Recycle();
                bitmap.Dispose();
                bitmap = null;
            }

            canvas.Dispose();
            canvas = null;

            bPaint.Dispose();
            bPaint = null;

            tPaint.Dispose();
            tPaint = null;

            var progressDialog = ProgressDialog.Show(MainActivity.Instance, "Bitte warten...", "Die Kennzeichen werden gesucht...", true);
            new System.Threading.Thread(new System.Threading.ThreadStart(delegate 
            {
                //save image
                var dataFolder = System.IO.Path.Combine(MainActivity.Instance.ApplicationInfo.DataDir, "kennzeichen_data");
                if (!System.IO.Directory.Exists(dataFolder))
                {
                    System.IO.Directory.CreateDirectory(dataFolder);
                }

                var imagePath = System.IO.Path.Combine(dataFolder, $"temp_{ Guid.NewGuid()}.jpg");
                System.IO.File.WriteAllBytes(imagePath, imageContent);

                // get kennzeichen when foto saved
                var result = App.KennzeichenRecognizer.Recognize(imagePath, "eu", "de");

                // delete tempfoto
                System.IO.File.Delete(imagePath);

                //HIDE PROGRESS DIALOG 
                _activity.RunOnUiThread(() =>
                {
                    progressDialog.Hide();

                    // invoke event on kennzeichenfound
                    ((SmartCameraPage)Element).RaiseLicenseFoundEvent(result);

                    // go back
                    Element.Navigation.PopModalAsync();
                });
            })).Start();
        }

        #endregion
    }
}