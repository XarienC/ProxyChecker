using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Diagnostics;

namespace ProxyChecker
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://cracked.to/Matt101");
        }

        private void HD_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/HDzzzz/ProxyCheckerWPF");
        }
    }
}
