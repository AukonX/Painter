using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using Microsoft.Win32;

namespace Painter
{
    public static class Stat
    {
        public static bool ColorSelected = false;
        public static bool ImageEdited = false;
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Color selectedColor = Color.FromRgb(0, 0, 0);

        List<Line> lines = new List<Line>();
        Point currentPoint = new Point();
        int drawingType = 1;
        int arrowOrientation = 0;
        int clickCount = 0;
        bool wasClickedPrior = false;
        Ellipse tempElipseLines = new Ellipse();
        Ellipse end1 = new Ellipse();
        Ellipse end2 = new Ellipse();
        int removed_end = 0;
        Line chosenLine = new Line();

        string filePath;

        /*DrawingType guide:
            1 - freedraw
            2 - point
            3 - line
            4 - edit line
            5 - ellipse
            6 - polyline
            7 - rectangle
            8 - polygon
            9 - plus
            10 - trapez - TODO
            11 - strzałka (w wybraną stronę)
            12 - gumka
            13 - dodaj obraz
            14 - edytuj obraz
        */

        #region Clicks
        private void button_freeDraw_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 1;
        }
        private void button_pointDraw_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 2;
        }
        private void button_lineDraw_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 3;
        }
        private void editLine_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 4;
        }
        private void drawEllipse_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 5;
        }
        private void draw_polyline_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 6;
        }
        private void drawRectangle_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 7;
        }
        private void drawPolygon_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 8;
        }
        private void drawPlus_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 9;
        }
        private void drawTrapezoid_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 10;
        }
        private void drawArrowUp_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 11;
            arrowOrientation = 0;
        }
        private void drawArrowRight_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 11;
            arrowOrientation = 1;
        }
        private void drawArrowDown_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 11;
            arrowOrientation = 2;
        }
        private void drawArrowLeft_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 11;
            arrowOrientation = 3;
        }
        private void Button_eraser_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 12;
        }
        private void addImage_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 13;
        }
        private void editImage_Click(object sender, RoutedEventArgs e)
        {
            drawingType = 14;
        }

        #endregion

        #region Usefull
        private double calculate_Dictance(double x1, double y1, double x2, double y2)
        {
            double distance = 0;
            
            distance = Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2) * (y1 - y2));

            return distance;
        }
        #endregion

        private void Surface_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                currentPoint = e.GetPosition(this);
            }
        }

        private void Surface_MouseMove(object sender, MouseEventArgs e)
        {
            Brush brushColor = new SolidColorBrush(selectedColor);
            if (e.LeftButton == MouseButtonState.Pressed && drawingType == 1)
            {
                Line line = new Line();
                line.Stroke = brushColor;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(this).X;
                line.Y2 = e.GetPosition(this).Y;

                currentPoint = e.GetPosition(this);

                Surface.Children.Add(line);
            }
        }

        private void Surface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Brush brushColor = new SolidColorBrush(selectedColor);
            //Refactor to Switch
            switch (drawingType)
            {
                case 2:
                    Ellipse ellipse = new Ellipse();

                    ellipse.Width = 6;
                    ellipse.Height = 6;
                    ellipse.Fill = brushColor;

                    Canvas.SetTop(ellipse, e.GetPosition(this).Y - ellipse.Height / 2);
                    Canvas.SetLeft(ellipse, e.GetPosition(this).X - ellipse.Width / 2);

                    Surface.Children.Add(ellipse);
                    break;
                case 3:
                    if (wasClickedPrior)
                    {
                        if (currentPoint != e.GetPosition(this))
                        {
                            Line line = new Line();
                            line.Stroke = brushColor;

                            line.X1 = currentPoint.X;
                            line.Y1 = currentPoint.Y;

                            line.X2 = e.GetPosition(this).X;
                            line.Y2 = e.GetPosition(this).Y;
                            Surface.Children.Remove(tempElipseLines);
                            lines.Add(line);
                            Surface.Children.Add(line);
                            wasClickedPrior = false;
                        }
                    }
                    else
                    {
                        currentPoint = e.GetPosition(this);

                        tempElipseLines.Width = 6;
                        tempElipseLines.Height = 6;
                        tempElipseLines.Fill = brushColor;

                        Canvas.SetTop(tempElipseLines, e.GetPosition(this).Y - tempElipseLines.Height / 2);
                        Canvas.SetLeft(tempElipseLines, e.GetPosition(this).X - tempElipseLines.Width / 2);

                        Surface.Children.Add(tempElipseLines);


                        wasClickedPrior = true;
                    }
                    break;
                case 4:
                    if(lines.Count > 0)
                    {
                        double clickPositionX = e.GetPosition(this).X;
                        double clickPositionY = e.GetPosition(this).Y;

                        double side_a, side_b, side_c, half_circ, field, distance;

                        List<double> distances = new List<double>();


                        switch (clickCount)
                        {
                            case 0:
                                foreach (Line temp in lines)
                                {
                                    side_a = calculate_Dictance(temp.X1, temp.Y1, temp.X2, temp.Y2);
                                    side_b = calculate_Dictance(temp.X1, temp.Y1, clickPositionX, clickPositionY);
                                    side_c = calculate_Dictance(clickPositionX, clickPositionY, temp.X2, temp.Y2);

                                    half_circ = (side_a + side_b + side_c) / 2;
                                    double temporary = (half_circ - side_a);
                                    double tempora = (half_circ - side_b);
                                    double temporar = (half_circ - side_c);


                                    field = Math.Sqrt((half_circ - side_a) * (half_circ - side_b) * (half_circ - side_c) * half_circ);

                                    distance = field * 2 / side_a;
                                    distances.Add(distance);

                                    //if (distance <= 30)
                                    //{
                                    //    chosenLine = temp;
                                    //   break;
                                    //}

                                    //Obliczenie wysokości trójkąta (końce odcinka i kliknięcie)
                                    //Jeżeli wysokość <= 10 --> wybieramy
                                }

                                if (distances.Min() < 15)
                                {
                                    chosenLine = lines.ElementAt(distances.IndexOf(distances.Min()));
                                }
                                else
                                {
                                    break;
                                }


                                end1.Height = 6;
                                end1.Width = 6;
                                end1.Fill = brushColor;

                                Canvas.SetTop(end1, chosenLine.Y1 - end1.Height / 2);
                                Canvas.SetLeft(end1, chosenLine.X1 - end1.Width / 2);

                                Surface.Children.Add(end1);

                                end2.Height = 6;
                                end2.Width = 6;
                                end2.Fill = brushColor;

                                Canvas.SetTop(end2, chosenLine.Y2 - end2.Height / 2);
                                Canvas.SetLeft(end2, chosenLine.X2 - end2.Width / 2);

                                Surface.Children.Add(end2);

                                clickCount = 1;
                                break;
                            case 1:
                                //MessageBox.Show($"{Surface.Children.IndexOf(end1)}");
                                double dist_end1 = calculate_Dictance(clickPositionX, clickPositionY, chosenLine.X1, chosenLine.Y1);
                                double dist_end2 = calculate_Dictance(clickPositionX, clickPositionY, chosenLine.X2, chosenLine.Y2);
                                if (dist_end1 < 10 || dist_end2 < 10)
                                {
                                    if (dist_end1 > dist_end2)
                                    {
                                        Surface.Children.Remove(end2);
                                        removed_end = 2;
                                    }
                                    else
                                    {
                                        Surface.Children.Remove(end1);
                                        removed_end = 1;
                                    }
                                    clickCount = 2;
                                }
                                break;
                            case 2:
                                Surface.Children.Remove(chosenLine);
                                lines.Remove(chosenLine);

                                Line line = new Line();

                                line.Stroke = brushColor;

                                if (removed_end == 1)
                                {
                                    line.X1 = chosenLine.X2;
                                    line.Y1 = chosenLine.Y2;
                                }
                                else
                                {
                                    line.X1 = chosenLine.X1;
                                    line.Y1 = chosenLine.Y1;
                                }
                                line.X2 = clickPositionX;
                                line.Y2 = clickPositionY;

                                lines.Add(line);
                                Surface.Children.Add(line);

                                if (Surface.Children.IndexOf(end1) != -1)
                                {
                                    Surface.Children.Remove(end1);
                                }
                                else
                                {
                                    Surface.Children.Remove(end2);
                                }

                                removed_end = 0;
                                clickCount = 0;
                                break;
                        }
                    }
                    break;
                case 5:
                    Ellipse ellip = new Ellipse();

                    ellip.Width = 100;
                    ellip.Height = 70;
                    ellip.Stroke = brushColor;

                    Canvas.SetTop(ellip, e.GetPosition(this).Y - ellip.Height / 2);
                    Canvas.SetLeft(ellip, e.GetPosition(this).X - ellip.Width / 2);

                    Surface.Children.Add(ellip);

                    break;
                case 6:
                    if(!wasClickedPrior)
                    {
                        currentPoint = e.GetPosition(this);

                        tempElipseLines.Width = 6;
                        tempElipseLines.Height = 6;
                        tempElipseLines.Fill = brushColor;

                        Canvas.SetTop(tempElipseLines, e.GetPosition(this).Y - tempElipseLines.Height / 2);
                        Canvas.SetLeft(tempElipseLines, e.GetPosition(this).X - tempElipseLines.Width / 2);

                        Surface.Children.Add(tempElipseLines);


                        wasClickedPrior = true;
                    }
                    else
                    {
                        Line line = new Line();
                        line.Stroke = brushColor;

                        line.X1 = currentPoint.X;
                        line.Y1 = currentPoint.Y;

                        line.X2 = e.GetPosition(this).X;
                        line.Y2 = e.GetPosition(this).Y;
                        Surface.Children.Remove(tempElipseLines);
                        Surface.Children.Add(line);
                        //wasClickedPrior = false;
                    }
                    break;
                case 7:
                    Rectangle rectangle = new Rectangle();

                    rectangle.Stroke = brushColor;
                    rectangle.Width = 80;
                    rectangle.Height = 60;

                    Canvas.SetTop(rectangle, e.GetPosition(this).Y - rectangle.Height / 2);
                    Canvas.SetLeft(rectangle, e.GetPosition(this).X - rectangle.Width / 2);

                    Surface.Children.Add(rectangle);
                    break;
                case 8:
                    Polygon polygon = new Polygon();

                    double mouseX = e.GetPosition(this).X;
                    double mouseY = e.GetPosition(this).Y;

                    double PolySize = 20;

                    Point point1 = new Point(mouseX - PolySize, mouseY + 2 * PolySize);
                    Point point2 = new Point(mouseX + PolySize, mouseY + 2 * PolySize);
                    Point point3 = new Point(mouseX + 2 * PolySize, mouseY);
                    Point point4 = new Point(mouseX + PolySize, mouseY - 2 * PolySize);
                    Point point5 = new Point(mouseX - PolySize, mouseY - 2 * PolySize);
                    Point point6 = new Point(mouseX - 2 * PolySize, mouseY);

                    System.Windows.Media.PointCollection points = new System.Windows.Media.PointCollection();
                    points.Add(point1);
                    points.Add(point2);
                    points.Add(point3);
                    points.Add(point4);
                    points.Add(point5);
                    points.Add(point6);

                    polygon.Points = points;
                    
                    polygon.Stroke = brushColor;

                    Surface.Children.Add(polygon);
                    break;
                case 9:
                    Polygon plus = new Polygon();

                    double mX = e.GetPosition(this).X;
                    double mY = e.GetPosition(this).Y;

                    double PlusSize = 10;

                    Point p1 = new Point(mX - PlusSize, mY + 5 * PlusSize);
                    Point p2 = new Point(mX + PlusSize, mY + 5 * PlusSize);
                    Point p3 = new Point(mX + PlusSize, mY + PlusSize);
                    Point p4 = new Point(mX + 5 * PlusSize, mY + PlusSize);
                    Point p5 = new Point(mX + 5 * PlusSize, mY - PlusSize);
                    Point p6 = new Point(mX + PlusSize, mY - PlusSize);
                    Point p7 = new Point(mX + PlusSize, mY - 5 * PlusSize);
                    Point p8 = new Point(mX - PlusSize, mY - 5 * PlusSize);
                    Point p9 = new Point(mX - PlusSize, mY - PlusSize);
                    Point p10 = new Point(mX - 5 * PlusSize, mY - PlusSize);
                    Point p11 = new Point(mX - 5 * PlusSize, mY + PlusSize);
                    Point p12 = new Point(mX - PlusSize, mY + PlusSize);

                    System.Windows.Media.PointCollection plus_points = new System.Windows.Media.PointCollection();
                    plus_points.Add(p1);
                    plus_points.Add(p2);
                    plus_points.Add(p3);
                    plus_points.Add(p4);
                    plus_points.Add(p5);
                    plus_points.Add(p6);
                    plus_points.Add(p7);
                    plus_points.Add(p8);
                    plus_points.Add(p9);
                    plus_points.Add(p10);
                    plus_points.Add(p11);
                    plus_points.Add(p12);

                    plus.Points = plus_points;

                    plus.Stroke = brushColor;

                    Surface.Children.Add(plus);
                    break;
                case 10:
                    Polygon trapezoid = new Polygon();

                    double trapX = e.GetPosition(this).X;
                    double trapY = e.GetPosition(this).Y;

                    double trapSize = 20;
                    trapezoid.Stroke = brushColor;

                    Point TrapPoint1, TrapPoint2, TrapPoint3, TrapPoint4;
                    System.Windows.Media.PointCollection Trap_points = new System.Windows.Media.PointCollection();

                    TrapPoint1 = new Point(trapX - 2 * trapSize, trapY + trapSize);
                    TrapPoint2 = new Point(trapX + 2 * trapSize, trapY + trapSize);
                    TrapPoint3 = new Point(trapX + trapSize, trapY - trapSize);
                    TrapPoint4 = new Point(trapX - trapSize, trapY - trapSize);

                    Trap_points.Add(TrapPoint1);
                    Trap_points.Add(TrapPoint2);
                    Trap_points.Add(TrapPoint3);
                    Trap_points.Add(TrapPoint4);

                    trapezoid.Points = Trap_points;
                    Surface.Children.Add(trapezoid);

                    break;
                case 11:
                    Polygon arrow = new Polygon();

                    double arX = e.GetPosition(this).X;
                    double arY = e.GetPosition(this).Y;

                    double arrowSize = 10;
                    arrow.Stroke = brushColor;

                    Point arrowPoint1, arrowPoint2, arrowPoint3, arrowPoint4, arrowPoint5, arrowPoint6, arrowPoint7;
                    System.Windows.Media.PointCollection arrow_points = new System.Windows.Media.PointCollection();

                    switch (arrowOrientation)
                    {
                        case 0:
                            arrowPoint1 = new Point(arX, arY - 3 * arrowSize);
                            arrowPoint2 = new Point(arX- 3 * arrowSize, arY);
                            arrowPoint3 = new Point(arX - arrowSize, arY);
                            arrowPoint4 = new Point(arX - arrowSize, arY + 3* arrowSize);
                            arrowPoint5 = new Point(arX + arrowSize, arY + 3 * arrowSize);
                            arrowPoint6 = new Point(arX + arrowSize, arY);
                            arrowPoint7 = new Point(arX + 3 * arrowSize, arY);

                            arrow_points.Add(arrowPoint1);
                            arrow_points.Add(arrowPoint2);
                            arrow_points.Add(arrowPoint3);
                            arrow_points.Add(arrowPoint4);
                            arrow_points.Add(arrowPoint5);
                            arrow_points.Add(arrowPoint6);
                            arrow_points.Add(arrowPoint7);
                            break;
                        case 1:
                            arrowPoint1 = new Point(arX + 3 * arrowSize, arY);
                            arrowPoint2 = new Point(arX, arY + 3 * arrowSize);
                            arrowPoint3 = new Point(arX, arY + arrowSize);
                            arrowPoint4 = new Point(arX - 3 * arrowSize, arY + arrowSize);
                            arrowPoint5 = new Point(arX - 3 * arrowSize, arY - arrowSize);
                            arrowPoint6 = new Point(arX, arY - arrowSize);
                            arrowPoint7 = new Point(arX, arY - 3 * arrowSize);

                            arrow_points.Add(arrowPoint1);
                            arrow_points.Add(arrowPoint2);
                            arrow_points.Add(arrowPoint3);
                            arrow_points.Add(arrowPoint4);
                            arrow_points.Add(arrowPoint5);
                            arrow_points.Add(arrowPoint6);
                            arrow_points.Add(arrowPoint7);
                            break;
                        case 2:
                            arrowPoint1 = new Point(arX, arY + 3 * arrowSize);
                            arrowPoint2 = new Point(arX + 3 * arrowSize, arY);
                            arrowPoint3 = new Point(arX + arrowSize, arY);
                            arrowPoint4 = new Point(arX + arrowSize, arY - 3 * arrowSize);
                            arrowPoint5 = new Point(arX - arrowSize, arY - 3 * arrowSize);
                            arrowPoint6 = new Point(arX - arrowSize, arY);
                            arrowPoint7 = new Point(arX - 3 * arrowSize, arY);

                            arrow_points.Add(arrowPoint1);
                            arrow_points.Add(arrowPoint2);
                            arrow_points.Add(arrowPoint3);
                            arrow_points.Add(arrowPoint4);
                            arrow_points.Add(arrowPoint5);
                            arrow_points.Add(arrowPoint6);
                            arrow_points.Add(arrowPoint7);
                            break;
                        case 3:
                            arrowPoint1 = new Point(arX - 3 * arrowSize, arY);
                            arrowPoint2 = new Point(arX, arY - 3 * arrowSize);
                            arrowPoint3 = new Point(arX, arY - arrowSize);
                            arrowPoint4 = new Point(arX + 3 * arrowSize, arY - arrowSize);
                            arrowPoint5 = new Point(arX + 3 * arrowSize, arY + arrowSize);
                            arrowPoint6 = new Point(arX, arY + arrowSize);
                            arrowPoint7 = new Point(arX, arY + 3 * arrowSize);

                            arrow_points.Add(arrowPoint1);
                            arrow_points.Add(arrowPoint2);
                            arrow_points.Add(arrowPoint3);
                            arrow_points.Add(arrowPoint4);
                            arrow_points.Add(arrowPoint5);
                            arrow_points.Add(arrowPoint6);
                            arrow_points.Add(arrowPoint7);
                            break;
                        default:
                            break;
                    }

                    arrow.Points = arrow_points;
                    Surface.Children.Add(arrow);

                    break;
                case 12:
                    var clickedElement = e.Source as FrameworkElement;
                    if(clickedElement != null )
                    {
                        if (Surface.Children.Contains(clickedElement))
                        {
                            Surface.Children.Remove(clickedElement);
                        }
                    }
                    break;
                case 13:

                    double pos_x = e.GetPosition(this).X;
                    double pos_y = e.GetPosition(this).Y;
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Image Files(*.png;*.jpg;*jpeg)|*.png;*.jpg;*jpeg";

                    if (openFileDialog.ShowDialog() == true)
                    {
                        filePath = openFileDialog.FileName;

                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(filePath);
                        bitmapImage.EndInit();

                        Image image = new Image
                        {
                            Source = bitmapImage,
                            Width = bitmapImage.Width / 5,
                            Height = bitmapImage.Height / 5,
                            Name = "Image",
                        };

                        Canvas.SetTop(image, pos_y - image.Height/2);
                        Canvas.SetLeft(image, pos_x - image.Width/2);

                        Surface.Children.Add(image);
                    }
                    break;
                case 14:
                    double click_x = e.GetPosition(this).X;
                    double click_y = e.GetPosition(this).Y;
                    var clickedImage = e.Source as FrameworkElement;
                    if (clickedImage is Image)
                    {
                        EditImageWindow editWindow = new EditImageWindow();
                        editWindow.Path = filePath;
                        editWindow.Image = (Image)clickedImage;
                        editWindow.imageChosen.Source = editWindow.Image.Source;
                        editWindow.ShowDialog();

                        if(Stat.ImageEdited)
                        {
                            Image edited = new Image
                            {
                                Source = editWindow.imageFinal.Source,
                                Width = clickedImage.Width,
                                Height = clickedImage.Height,
                            };
                            Canvas.SetTop(edited, click_y - edited.Height / 2);
                            Canvas.SetLeft(edited, click_x - edited.Width / 2);

                            Surface.Children.Remove(clickedImage);
                            Surface.Children.Add(edited);
                        }

                        editWindow.Close();
                    }
                    break;
                default:
                    break;
            }
        }

        private void colorRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ColorSelectWindow colorSelectWindow = new ColorSelectWindow();

            colorSelectWindow.ShowDialog();

            if (Stat.ColorSelected)
            {
                selectedColor = colorSelectWindow.NewColor;
                Brush brushColor = new SolidColorBrush(colorSelectWindow.NewColor);
                colorRectangle.Fill = brushColor;
                Stat.ColorSelected = false;
            }

            colorSelectWindow.Close();
        }

        private void Surface_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(drawingType == 6 && wasClickedPrior)
            {
                wasClickedPrior = false;
            }
        }

        public void SaveToPngFile(Uri path, Canvas surface)
        {
            Transform transform = surface.LayoutTransform;
            surface.LayoutTransform = null;

            Size size = new Size(surface.ActualWidth, surface.ActualHeight);

            surface.Measure(size);
            surface.Arrange(new Rect(size));

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Pbgra32);

            renderTargetBitmap.Render(surface);

            using (FileStream outStr = new FileStream(path.LocalPath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                encoder.Save(outStr);
            }

            surface.LayoutTransform = transform;
        }

        public void SaveToJpegFile(Uri path, Canvas surface)
        {
            Transform transform = surface.LayoutTransform;
            surface.LayoutTransform = null;

            Size size = new Size(surface.ActualWidth, surface.ActualHeight);

            surface.Measure(size);
            surface.Arrange(new Rect(size));

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Pbgra32);

            renderTargetBitmap.Render(surface);

            using (FileStream outStr = new FileStream(path.LocalPath, FileMode.Create))
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                encoder.Save(outStr);
            }

            surface.LayoutTransform = transform;
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image File (*.png)|*.png | Image File (*.jpg)|*.jpg";
            saveFileDialog.FilterIndex = 1;
            if(saveFileDialog.ShowDialog() == true)
            {
                if (saveFileDialog.FilterIndex == 1)
                {
                    Uri newFileUri = new Uri(saveFileDialog.FileName);
                    SaveToPngFile(newFileUri, Surface);
                }
                else
                {
                    Uri newFileUri = new Uri(saveFileDialog.FileName);
                    SaveToJpegFile(newFileUri, Surface);
                }
            }

        }
    }
}
