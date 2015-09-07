using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for win_subject.xaml
    /// </summary>
    public partial class win_subject : Window
    {
        public win_subject()
        {
            InitializeComponent();
        }

        private double LastHeight, LastWidth;
        private System.Windows.WindowState LastState;
        
        //Gets rid of borders with F11 key press
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            { 
                if (WindowStyle != WindowStyle.None)
                {
                    LastHeight = Height;
                    LastWidth = Width;
                    LastState = WindowState;

                    Width = 800;
                    Height = 600;
                    
                    WindowStyle = WindowStyle.None;
                    ResizeMode = System.Windows.ResizeMode.NoResize;
                    
                }
                else
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = LastState; ;
                    ResizeMode = ResizeMode.CanResizeWithGrip;
                    Topmost = false;
                    Width = LastWidth;
                    Height = LastHeight;
                }
            }
        }

        //Only hides VCS subject window when X is pressed
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            this.Hide();
        }

        


    }
}
