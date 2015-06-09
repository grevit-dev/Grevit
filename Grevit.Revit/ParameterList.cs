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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Grevit.Revit
{
    public partial class ParameterList : Form
    {
        public ParameterList()
        {
            InitializeComponent();
        }


        private void copyID_Click(object sender, EventArgs e)
        {
            if (parameters.SelectedItems.Count == 1)
            {
                string elementId = parameters.SelectedItems[0].SubItems[0].Text;
                Clipboard.SetText(elementId);
            }
        }

        private void copyName_Click(object sender, EventArgs e)
        {
            if (parameters.SelectedItems.Count == 1)
            {
                string name = parameters.SelectedItems[0].SubItems[1].Text;
                Clipboard.SetText(name);
            }
        }


    }
}
