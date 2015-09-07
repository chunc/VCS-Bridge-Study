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

//Newly included
using CsvHelper;
using System.Diagnostics;
using System.IO;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for demo1.xaml
    /// </summary>
    public partial class demo1 : Window
    {
        public demo1()
        {
            InitializeComponent();
        }

        public class restaurant
        {
            public string Name { get; set; }
            public string Cuisine { get; set; }
            public double Rating { get; set; }
            public double Reviews { get; set; }
            public double Distance { get; set; }
            public string Price { get; set; }
            public int Trial { get; set; }
            public int Order { get; set; }
        }

        void button1_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            demo2 objWelcome = new demo2();
            objWelcome.win2_textblock.Text = win1_textinput.Text;
            objWelcome.Show();
            Close();
        }

        void readcsv_Click(object sender, RoutedEventArgs e)
        {
            using (var sr = new StreamReader(@"restaurant.csv"))
            {
                var reader = new CsvReader(sr);
                List<restaurant> LISTrecord = new List<restaurant>();

                //CSVReader will now read the whole file into an enumerable
                IEnumerable<restaurant> records = reader.GetRecords<restaurant>();

                int n = 2;
                
                var query =
                    from record in records
                    where record.Trial == n
                    select record;

                //Print Result of Query
                foreach (var g in query.Take(5))
                {
                    //Debug.Print("Name: {0}, Rating: {1}", g.Name, g.Rating);
                    LISTrecord.Add(g);
                }

                Debug.Print(LISTrecord[1].Name);

            }
        }
    }
}
