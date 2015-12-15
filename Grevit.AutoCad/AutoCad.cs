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
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices; 
using Autodesk.AutoCAD.EditorInput; 
using Autodesk.AutoCAD.Runtime; 
using Autodesk.AutoCAD.Geometry; 
using Autodesk.Aec.Structural.DatabaseServices;
using Autodesk.Aec.Arch.DatabaseServices;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Runtime.Serialization;
using Autodesk.Aec.PropertyData.DatabaseServices;


[assembly: CommandClass(typeof(Grevit.AutoCad.Command))]
namespace Grevit.AutoCad
{
    public class Command
    {
        public static Dictionary<string, ObjectId> existing_objects;
        public static Dictionary<string, ObjectId> created_objects;
        public static Document Document;
        public static Database Database;
        public static Editor Editor;

        [CommandMethod("GREVIT")]
        public static void Start()
        {
            Command.Document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Command.Database = Document.Database;
            Command.Editor = Command.Document.Editor;
            //Grevit.Serialization.Client grevitClientDialog = new Grevit.Serialization.Client();

            // Create new Grevit Client sending existing Families 
            Grevit.Client.ClientWindow grevitClientDialog = new Grevit.Client.ClientWindow();
            //Grevit.Serialization.Client grevitClientDialog = new Grevit.Serialization.Client(document.GetFamilies());

            // Show Client Dialog
            grevitClientDialog.ShowWindow();

           // if (grevitClientDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            List<Grevit.Types.Component> stalled = new List<Grevit.Types.Component>();

            existing_objects = Utilities.getExistingObjectIDs(grevitClientDialog.componentCollection);
            created_objects = new Dictionary<string, ObjectId>();

            foreach (Grevit.Types.Component component in grevitClientDialog.componentCollection.Items)
            {
                if (!component.stalledForReference)
                    component.Build(false);
                else
                    stalled.Add(component);
            }



            try
            {
                foreach (Grevit.Types.Component c in stalled)  c.Build(true);               
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                //ed.WriteMessage(e.Message);
            }

        }

    }


}