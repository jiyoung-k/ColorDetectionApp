using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ColorDetectionApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap image;
        private EventHandler<Color> DetectionColorEvent;
        

        private int TL_X = 0;
        private int TL_Y = 0;
        private int BR_X = 0;
        private int BR_Y = 0;
        private float R = 0;
        private float Min_R = 0;
        private float Max_R = 0;
        private float Sum_R = 0;
        private float G = 0;
        private float Min_G = 0;
        private float Max_G = 0;
        private float Sum_G = 0;
        private float B = 0;
        private float Min_B = 0;
        private float Max_B = 0;
        private float Sum_B = 0;

        private int count = 0;

        public MainWindow()
        {
            InitializeComponent();
            DetectionColorEvent += DetectionColor;
            ColorDetectionAreaCanvas.DrawColorDetectionArea += DrawArea;
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            ColorDetectionImage.Visibility = Visibility.Visible;
            StatusText.Text = "Color Detection Start !";
            StatusText.Visibility = Visibility.Hidden;
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileName.Text = openFileDialog.FileName;
                Bitmap bitmap = new Bitmap(openFileDialog.FileName);
                MemoryStream ms = new MemoryStream();

                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();

                image = new WriteableBitmap(bi);
                ColorDetectionImage.Source = image;
                ms.Close();
                bitmap.Dispose();
            }
        }

        private void SetArea_Click(object sender, RoutedEventArgs e)
        {
            TL_X = Int32.Parse(TLX.Text);
            TL_Y = Int32.Parse(TLY.Text);
            BR_X = Int32.Parse(BRX.Text);
            BR_Y = Int32.Parse(BRY.Text);
            ColorDetectionAreaCanvas.DrawColorDetection(ColorDetectionImage.ActualWidth, ColorDetectionImage.ActualHeight, ColorDetectionAreaCanvas.ActualWidth, ColorDetectionAreaCanvas.ActualHeight, TL_X, TL_Y, BR_X, BR_Y);
            
        }

        private void DrawArea(object sender, EventArgs e)
        {
            ColorDetectionAreaCanvas.InvalidateVisual();
        }

        private void ColorDetection_Click(object sender, RoutedEventArgs e)
        {
            int width = BR_X - TL_X;
            int height = BR_Y - TL_Y;
            
            CroppedBitmap cb = new CroppedBitmap(image, new Int32Rect(TL_X, TL_Y, width, height));

            WriteableBitmap final = new WriteableBitmap(cb);
            GetRGBAvg_lockBitsWithByteArr(final);
        }

        private void ReSet_Click(object sender, RoutedEventArgs e)
        {
            R = 0;
            Min_R = 0;
            Max_R = 0;
            Sum_R = 0;
            G = 0;
            Min_G = 0;
            Max_G = 0;
            Sum_G = 0;
            B = 0;
            Min_B = 0;
            Max_B = 0;
            Sum_B = 0;
            count = 0;

            CurrentR.Text = "";
            CurrentG.Text = "";
            CurrentB.Text = "";

            MinR.Text = "";
            MinG.Text = "";
            MinB.Text = "";

            MaxR.Text = "";
            MaxG.Text = "";
            MaxB.Text = "";

            AvgR.Text = "";
            AvgG.Text = "";
            AvgB.Text = "";
        }

        private void DetectionColor(object sender, Color color)
        {
            count++;

            Sum_R += color.R;
            Sum_G += color.G;
            Sum_B += color.B;

            Min_R = GetMinScore(Min_R, color.R);
            Min_G = GetMinScore(Min_G, color.G);
            Min_B = GetMinScore(Min_B, color.B);

            Max_R = GetMaxScore(Max_R, color.R);
            Max_G = GetMaxScore(Max_G, color.G);
            Max_B = GetMaxScore(Max_B, color.B);

            R = color.R;
            G = color.G;
            B = color.B;

            CurrentR.Text = R.ToString();
            CurrentG.Text = G.ToString();
            CurrentB.Text = B.ToString();

            MinR.Text = Min_R.ToString();
            MinG.Text = Min_G.ToString();
            MinB.Text = Min_B.ToString();


            MaxR.Text = Max_R.ToString();
            MaxG.Text = Max_G.ToString();
            MaxB.Text = Max_B.ToString();

            AvgR.Text = (Sum_R / count).ToString();
            AvgG.Text = (Sum_G / count).ToString();
            AvgB.Text = (Sum_B / count).ToString();

        }

        private void GetRGBAvg_lockBitsWithByteArr(WriteableBitmap detectedImage)
        {
            try
            {
                Bitmap orig;
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create((BitmapSource)detectedImage));
                    enc.Save(outStream);
                    orig = new System.Drawing.Bitmap(outStream);
                }

                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, orig.Width, orig.Height);
                BitmapData bmpData = orig.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                int bytes = bmpData.Stride * orig.Height;
                byte[] rgbaValues = new byte[bytes];
                Marshal.Copy(bmpData.Scan0, rgbaValues, 0, bytes);
                orig.UnlockBits(bmpData);

                int lineStart = 0;

                int r = 0;
                int g = 0;
                int b = 0;

                int total = 0;

                for (int y = 0; y < orig.Height; ++y)
                {
                    int offset = lineStart;

                    for (int x = 0; x < orig.Width; ++x)
                    {
                        b += rgbaValues[offset + 0];
                        g += rgbaValues[offset + 1];
                        r += rgbaValues[offset + 2];

                        offset += 3;
                        total++;
                    }
                    lineStart += bmpData.Stride;
                }

                //return Color.FromArgb(r /= total, g /= total, b /= total);
                DetectionColorEvent?.Invoke(null, Color.FromArgb(r /= total, g /= total, b /= total));
            }
            catch (Exception em)
            {
                System.Windows.Forms.MessageBox.Show("Color Detection Error " + em.Message);
            }

        }

        private float GetMinScore(float current, float detect)
        {
            float min = current;

            if (current > detect)
            {
                min = detect;
            }

            if (current == 0) min = detect;
            return min;
        }

        private float GetMaxScore(float current, float detect)
        {
            float max = current;

            if (current < detect)
            {
                max = detect;
            }

            if (current == 0) max = detect;

            return max;
        }

        private void ColorDetectionImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Point cursor = e.GetPosition(e.Source as IInputElement);
            PointStatus.Text = string.Format("Image 좌표 X:{0} Y:{1}"
                , ((Convert.ToInt32(cursor.X) * 1920) / Convert.ToInt32(ColorDetectionImage.ActualWidth)).ToString()
                , ((Convert.ToInt32(cursor.Y) * 1200) / Convert.ToInt32(ColorDetectionImage.ActualHeight)).ToString()
                );
        }


        private void AutoDetection_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            int width = BR_X - TL_X;
            int height = BR_Y - TL_Y;

            R = 0;
            Min_R = 0;
            Max_R = 0;
            Sum_R = 0;
            G = 0;
            Min_G = 0;
            Max_G = 0;
            Sum_G = 0;
            B = 0;
            Min_B = 0;
            Max_B = 0;
            Sum_B = 0;
            count = 0;

            CurrentR.Text = "";
            CurrentG.Text = "";
            CurrentB.Text = "";

            MinR.Text = "";
            MinG.Text = "";
            MinB.Text = "";

            MaxR.Text = "";
            MaxG.Text = "";
            MaxB.Text = "";

            AvgR.Text = "";
            AvgG.Text = "";
            AvgB.Text = "";

            
            ColorDetectionImage.Visibility = Visibility.Hidden;
            StatusText.Text = "Color Detection Start !";
            StatusText.Visibility = Visibility.Visible;

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StatusText.Text = "Color Detecting ..................... ";
                string folderPath = folderBrowserDialog.SelectedPath;

                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                {
                    string fileName = folderPath + "\\" + fileInfo.Name;
                    try
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            Bitmap bitmap = new Bitmap(fileName);
                            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                            ms.Seek(0, SeekOrigin.Begin);

                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit();
                            bi.StreamSource = ms;
                            bi.CacheOption = BitmapCacheOption.OnLoad;
                            bi.EndInit();
                            WriteableBitmap AutoImage = new WriteableBitmap(bi);
                            CroppedBitmap cb = new CroppedBitmap(AutoImage, new Int32Rect(TL_X, TL_Y, width, height));
                            AutoImage.Freeze();
                            WriteableBitmap final = new WriteableBitmap(cb);
                            bitmap.Dispose();
                            Bitmap orig;
                            using (MemoryStream outStream = new MemoryStream())
                            {
                                BitmapEncoder enc = new BmpBitmapEncoder();
                                enc.Frames.Add(BitmapFrame.Create((BitmapSource)final));
                                enc.Save(outStream);
                                orig = new System.Drawing.Bitmap(outStream);
                            }

                            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, orig.Width, orig.Height);
                            BitmapData bmpData = orig.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                            int bytes = bmpData.Stride * orig.Height;
                            byte[] rgbaValues = new byte[bytes];
                            Marshal.Copy(bmpData.Scan0, rgbaValues, 0, bytes);
                            orig.UnlockBits(bmpData);

                            int lineStart = 0;

                            int r = 0;
                            int g = 0;
                            int b = 0;

                            int total = 0;

                            for (int y = 0; y < orig.Height; ++y)
                            {
                                int offset = lineStart;

                                for (int x = 0; x < orig.Width; ++x)
                                {
                                    b += rgbaValues[offset + 0];
                                    g += rgbaValues[offset + 1];
                                    r += rgbaValues[offset + 2];

                                    offset += 3;
                                    total++;
                                }
                                lineStart += bmpData.Stride;
                            }

                            Color detectionColor = new Color();
                            detectionColor = Color.FromArgb(r /= total, g /= total, b /= total);

                            count++;

                            Sum_R += detectionColor.R;
                            Sum_G += detectionColor.G;
                            Sum_B += detectionColor.B;

                            Min_R = GetMinScore(Min_R, detectionColor.R);
                            Min_G = GetMinScore(Min_G, detectionColor.G);
                            Min_B = GetMinScore(Min_B, detectionColor.B);

                            Max_R = GetMaxScore(Max_R, detectionColor.R);
                            Max_G = GetMaxScore(Max_G, detectionColor.G);
                            Max_B = GetMaxScore(Max_B, detectionColor.B);

                            R = detectionColor.R;
                            G = detectionColor.G;
                            B = detectionColor.B;

                            ms.Close();
                        }
                    }
                    catch (Exception em)
                    {
                        System.Windows.Forms.MessageBox.Show("Color Detection Error " + em.Message);
                    }
                }

                CurrentR.Text = R.ToString();
                CurrentG.Text = G.ToString();
                CurrentB.Text = B.ToString();

                MinR.Text = Min_R.ToString();
                MinG.Text = Min_G.ToString();
                MinB.Text = Min_B.ToString();


                MaxR.Text = Max_R.ToString();
                MaxG.Text = Max_G.ToString();
                MaxB.Text = Max_B.ToString();

                AvgR.Text = (Sum_R / count).ToString();
                AvgG.Text = (Sum_G / count).ToString();
                AvgB.Text = (Sum_B / count).ToString();

                StatusText.Text = "Color Detection Finish !";
            }
        }

        private void CropArea_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            int width = BR_X - TL_X;
            int height = BR_Y - TL_Y;

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = folderBrowserDialog.SelectedPath;
                string cropFolderPath = folderPath + "\\DetectionAreaCrop";

                if (!Directory.Exists(cropFolderPath)){
                    Directory.CreateDirectory(cropFolderPath);
                }


                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                StatusText.Text = "Croping ..................... ";


                foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                {
                    string fileName = folderPath + "\\" + fileInfo.Name;
                    string saveFileName = cropFolderPath + "\\" + fileInfo.Name;
                    try
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            Bitmap bitmap = new Bitmap(fileName);
                            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                            ms.Seek(0, SeekOrigin.Begin);

                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit();
                            bi.StreamSource = ms;
                            bi.CacheOption = BitmapCacheOption.OnLoad;
                            bi.EndInit();
                            WriteableBitmap AutoImage = new WriteableBitmap(bi);
                            CroppedBitmap cb = new CroppedBitmap(AutoImage, new Int32Rect(TL_X, TL_Y, width, height));
                            AutoImage.Freeze();
                            WriteableBitmap final = new WriteableBitmap(cb);
                            bitmap.Dispose();


                            ImageCodecInfo myImageCodecInfo;
                            Encoder myEncoder;
                            EncoderParameter myEncoderParameter;
                            EncoderParameters myEncoderParameters;
                            myImageCodecInfo = GetEncoderInfo("image/jpeg");
                            myEncoder = Encoder.Quality;
                            myEncoderParameters = new EncoderParameters(1);
                            myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                            myEncoderParameters.Param[0] = myEncoderParameter;

                            Bitmap orig;
                            using (MemoryStream outStream = new MemoryStream())
                            {
                                BitmapEncoder enc = new BmpBitmapEncoder();
                                enc.Frames.Add(BitmapFrame.Create((BitmapSource)final));
                                enc.Save(outStream);
                                orig = new System.Drawing.Bitmap(outStream);

                                orig.Save(saveFileName, myImageCodecInfo, myEncoderParameters);
                                orig.Dispose();
                            }

                            ms.Close();
                        }
                    }
                    catch (Exception em)
                    {
                        System.Windows.Forms.MessageBox.Show("Crop Area Error " + em.Message);
                    }
                }

                StatusText.Text = "Crop Area Finish !";
            }
        }

        private ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int i;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (i = 0; i < encoders.Length; ++i)
            {
                if (encoders[i].MimeType == mimeType)
                    return encoders[i];
            }
            return null;
        }
    }
}
