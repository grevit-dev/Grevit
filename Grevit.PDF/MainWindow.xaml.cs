//
//  Grevit - Create Autodesk Revit (R) Models in McNeel's Rhino Grassopper 3D (R)
//  For more Information visit grevit.net or food4rhino.com/project/grevit
//  Copyright (C) 2015
//  Authors: Maximilian Thumfart,
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Grevit;

namespace Grevit.PDF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            double conversion = 1;
            double.TryParse(this.conversion.Text, out conversion);

            protocol.Items.Add("Preparing to send.");

            string filename = System.IO.Path.GetTempPath() + "GrevitTempFile.skp";
            if (System.IO.File.Exists(filename))
            {
                string host = server.Text;
                int port = 8002;
                int.TryParse(this.port.Text, out port);
                int timeout = 10000;

                protocol.Items.Add("Converting Model Content.");

                var components = ParsePDF.ParseDocument(filename, conversion);
                if (components != null)
                {
                    protocol.Items.Add(String.Format("{0} Components found.", components.Items.Count));
                    protocol.Items.Add("Sending Components to localhost.");


                    Grevit.Client.Utilities.Send(components, host, port, timeout);


                    //this.DialogResult = true;
                    this.Close();
                }
                else
                    protocol.Items.Add("Error: Could not convert Model.");
            }
            else
                protocol.Items.Add(String.Format("Error: {0} not found. Aborting.", filename));
        }

    }
}
