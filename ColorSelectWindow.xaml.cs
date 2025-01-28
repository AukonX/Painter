using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Painter
{
    /// <summary>
    /// Interaction logic for ColorSelectWindow.xaml
    /// </summary>
    public partial class ColorSelectWindow : Window
    {
        public ColorSelectWindow()
        {
            InitializeComponent();
        }

        Color _newColor;
        public Color NewColor { get; set; }

        private void buttonViewColor_Click(object sender, RoutedEventArgs e)
        {
            if (Int32.Parse(textBoxRedValue.Text) > 255 || Int32.Parse(textBoxRedValue.Text) < 0
                || Int32.Parse(textBoxGreenValue.Text) > 255 || Int32.Parse(textBoxGreenValue.Text) < 0 
                || Int32.Parse(textBoxBlueValue.Text) > 255 ||Int32.Parse(textBoxBlueValue.Text) < 0)
            {
                MessageBox.Show("Podaj wartości z odpowiedniego przedziału");
                return;
            }

            Byte blue = Byte.Parse(textBoxBlueValue.Text);
            Byte red = Byte.Parse(textBoxRedValue.Text);
            byte green = Byte.Parse(textBoxGreenValue.Text);

            convertToHSV(Int32.Parse(textBoxRedValue.Text), Int32.Parse(textBoxGreenValue.Text), Int32.Parse(textBoxBlueValue.Text));

            NewColor = Color.FromRgb(red, green, blue);
            viewColor.Fill = new SolidColorBrush(NewColor);
        }

        private void buttonAccept_Click(object sender, RoutedEventArgs e)
        {
            if (Int32.Parse(textBoxRedValue.Text) > 255 || Int32.Parse(textBoxRedValue.Text) < 0
                || Int32.Parse(textBoxGreenValue.Text) > 255 || Int32.Parse(textBoxGreenValue.Text) < 0
                || Int32.Parse(textBoxBlueValue.Text) > 255 || Int32.Parse(textBoxBlueValue.Text) < 0)
            {
                MessageBox.Show("Podaj wartości z odpowiedniego przedziału");
                return;
            }
            Byte blue = Byte.Parse(textBoxBlueValue.Text);
            Byte red = Byte.Parse(textBoxRedValue.Text);
            byte green = Byte.Parse(textBoxGreenValue.Text);

            NewColor = Color.FromRgb(red, green, blue);

            Stat.ColorSelected = true;
            Hide();
        }

        private void convertToHSV(int r, int g, int b)
        {
            double R = r / 255.0;
            double G = g / 255.0;
            double B = b / 255.0;

            double m_max = Math.Max(R, Math.Max(G, B));  
            double m_min = Math.Min(R, Math.Min(G, B));

            double delta = m_max - m_min;

            double v = m_max;
            double s = (m_max == 0) ? 0 : 1 - (m_min / m_max);
            double h = 0;

            if(delta != 0)
            {
                if(m_max == R)
                {
                    h = ((G - B) / delta) % 6;
                }
                else if (m_max == G)
                {
                    h = ((B - R) / delta) + 2;
                }
                else
                {
                    h = ((R - G) / delta) + 4;
                }

                h *= 60;

                if(h < 0)
                {
                    h += 360;
                }
            }

            textBoxHue.Text = Math.Round(h, 2).ToString();
            textBoxSaturation.Text = Math.Round(s * 100, 2).ToString();
            textBoxValue.Text = Math.Round(v * 100, 2).ToString();
        }
    }
}
