using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Emgu;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using static System.Net.Mime.MediaTypeNames;

namespace Painter
{
    /// <summary>
    /// Interaction logic for EditImageWindow.xaml
    /// </summary>
    public partial class EditImageWindow : Window
    {
        public EditImageWindow()
        {
            InitializeComponent();
        }

        private System.Windows.Controls.Image _image;
        public System.Windows.Controls.Image Image { get { return _image; } set { _image = value; } }

        private string _path;
        public string Path { get; set; }

        private void sobelButton_Click(object sender, RoutedEventArgs e)
        {
            Stat.ImageEdited = true;
            Sobel();
        }

        private void Sobel()
        {
            Mat mat = new Mat();
            mat = ToMat((BitmapSource)Image.Source);

            Image<Bgr, byte> img = mat.ToImage<Bgr, byte>();
            Image<Gray, byte> grayimg = img.Convert<Gray, byte>();
            Image<Gray, Single> sobeledImage = grayimg.Sobel(0, 1, 3).Add(grayimg.Sobel(1, 0 , 3)).AbsDiff(new Gray(0.0));

            Bitmap bitmapa =  sobeledImage.ToBitmap();

            ImageSource imageSource = ToImageSource(bitmapa);

            imageFinal.Source = imageSource;

            //return sobeledImage;
        }

        private void Matrix(double[,] fmatrix)
        {
            WriteableBitmap image = new WriteableBitmap((BitmapSource)Image.Source);

            int width = image.PixelWidth;
            int height = image.PixelHeight;

            int stride = width * 4; // Każdy piksel zajmuje 4 bajty (ARGB)

            byte[] pixels = new byte[height * stride];
            image.CopyPixels(pixels, stride, 0);

            byte[] resultPixels = new byte[height * stride];

            int filterSize = fmatrix.GetLength(0);
            int filterOffset = filterSize / 2;

            for (int y = filterOffset; y < height - filterOffset; y++)
            {
                for (int x = filterOffset; x < width - filterOffset; x++)
                {
                    double red = 0, green = 0, blue = 0;

                    for (int i = 0; i < filterSize; i++)
                    {
                        for (int j = 0; j < filterSize; j++)
                        {
                            int pixelIndex = ((y + i - filterOffset) * stride) + ((x + j - filterOffset) * 4);

                            red += pixels[pixelIndex + 2] * fmatrix[i, j];
                            green += pixels[pixelIndex + 1] * fmatrix[i, j];
                            blue += pixels[pixelIndex] * fmatrix[i, j];
                        }
                    }

                    int resultIndex = (y * stride) + (x * 4);
                    resultPixels[resultIndex + 2] = Clamp((int)red);
                    resultPixels[resultIndex + 1] = Clamp((int)green);
                    resultPixels[resultIndex] = Clamp((int)blue);
                    resultPixels[resultIndex + 3] = 255; // Alpha

                }
            }

            image.WritePixels(new Int32Rect(0, 0, width, height), resultPixels, stride, 0);

            imageFinal.Source = image;
        }

        private Mat ToMat(BitmapSource source)
        {
            if (source.Format == PixelFormats.Bgr32)
            {
                Mat result = new Mat();
                result.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 4);
                source.CopyPixels(Int32Rect.Empty, result.DataPointer, result.Step * result.Rows, result.Step);
                return result;
            }
            return null;
        }

        static ImageSource ToImageSource(Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return bitmapSource;
        }

        private void matrixButton_Click(object sender, RoutedEventArgs e)
        {
            Stat.ImageEdited = true;
            double[,] filterMatrix = {
                { 2, 7, 2 },
                { -1, 4, -1},
                { 1, -11, 1 }
            };

            Matrix(filterMatrix);
        }

        private byte Clamp(int value)
        {
            return (byte)(value < 0 ? 0 : (value > 255 ? 255 : value));
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
