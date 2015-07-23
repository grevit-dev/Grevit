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
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Diagnostics;
using System.Xml;
using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using Grevit.Types;
using System.Windows.Media.Imaging;
using Grevit.Revit;

namespace Grevit.Revit
{
    public static class ComponentExtension
    {
        /// <summary>
        /// Invoke the Components Create Method
        /// </summary>
        /// <param name="component"></param>
        public static void Build(this Grevit.Types.Component component, bool useReferenceElement)
        {
            // Create a new transaction
            Transaction transaction = new Transaction(GrevitCommand.document, "GrevitCreate");
            transaction.Start();

            // Get the components type
            Type type = component.GetType();

            // Get the Create extension Method using reflection
            IEnumerable<System.Reflection.MethodInfo> methods = Grevit.Reflection.Utilities.GetExtensionMethods(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Assembly, type);

            // Check all extensions methods (should only be Create() anyway)
            foreach (System.Reflection.MethodInfo method in methods)
            {
                // get the methods parameters
                object[] parameters = new object[method.GetParameters().Length];

                // As it is an extension method, the first parameter is the component itself
                parameters[0] = component;



                #region usingReferenceElement

                // if we should use a reference element to invoke the Create method
                // and parameter length equals 2
                // get the components referenceGID, see if it has been created already
                // use this element as a parameter to invoke Create(Element element)
                if (useReferenceElement && parameters.Length == 2)
                { 
                    // Get the components reference GID
                    System.Reflection.PropertyInfo propertyReferenceGID = type.GetProperty("referenceGID");

                    // Return if there is no reference GID property
                    if (propertyReferenceGID == null) return;

                    // Get the referene GID string value                    
                    string referenceGID = (string)propertyReferenceGID.GetValue(component);
                    
                    // If the reference has been created already, get 
                    // the Element from the document and apply it as parameter two
                    if (GrevitCommand.created_Elements.ContainsKey(referenceGID))
                    { 
                        Element referenceElement = GrevitCommand.document.GetElement(GrevitCommand.created_Elements[referenceGID]);
                        parameters[1] = referenceElement;
                    }
                }

                #endregion



                // If the create method exists
                if (method != null && method.Name.EndsWith("Create"))
                {
                    // Invoke the Create Method without parameters
                    Element createdElement = (Element)method.Invoke(component, parameters);

                    // If the return value is valud set the parameters
                    if (createdElement != null)
                    {
                        component.SetParameters(createdElement);
                        component.StoreGID(createdElement.Id);
                    }
                }
            }

            // commit and dispose the transaction
            transaction.Commit();
            transaction.Dispose();

        }

    }
}
