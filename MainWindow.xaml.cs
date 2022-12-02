using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Proj3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Chromatic chromatic;
        Bezier bezier;

        public MainWindow()
        {
            InitializeComponent();

            chromatic = new Chromatic();
            chromatic.Initialize(chromaticCanvas);

            bezier = new Bezier();
            bezier.Initialize(bezierCanvas);
        }

        private void ChromaticBackgroundButtonClick(object sender, RoutedEventArgs e)
        {
            if(ChromaticBackgroundButton.Content.ToString() == "Background ON")
            {
                chromaticCanvas.Children.Remove(chromatic.chromaticDiagram);
                ChromaticBackgroundButton.Content = "Background OFF";
            }
            else
            {
                chromaticCanvas.Children.Add(chromatic.chromaticDiagram);
                ChromaticBackgroundButton.Content = "Background ON";
            }
        }

        private void bezierPointsTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void bezierPointsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = ((TextBox)sender).Text;
            if (s.Length > 0 && bezier != null)
            {
                bezier.bezierPoints = Int32.Parse(s);
                bezier.ResetCurve(bezierCanvas);
            }
        }

        private void bezierCanvas_MouseDown(object sender, MouseButtonEventArgs e) => bezier.DrawCurve(bezierCanvas);

        private void bezierCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            bezier.DrawCurve(bezierCanvas);
        }
    }
}
