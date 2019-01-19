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
using System.Diagnostics;
using System.Xml;
using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using Grevit.Types;



namespace Grevit.SketchUp
{

    public static class Utilities
    {

        public static ComponentCollection Translate(string filename)
        {

            SketchUpNET.SketchUp skp = new SketchUpNET.SketchUp();
            if (skp.LoadModel(filename))
            {
                Grevit.Types.ComponentCollection components = new ComponentCollection() { Items = new List<Component>() };
                components.scale = 3.28084;
                components.update = true;
                components.delete = false;

                foreach (SketchUpNET.Instance instance in skp.Instances)
                {
                    SketchUpNET.Transform transform = instance.Transformation;
                    transform.Data[12] /= 39.3701;
                    transform.Data[13] /= 39.3701;
                    transform.Data[14] /= 39.3701;

                    SketchUpNET.Component parent = instance.Parent as SketchUpNET.Component;
                    if (parent == null) continue;

                    string elementType = parent.Name.ToLower();
                    string family = parent.Name;
                    string type = parent.Name;

                    if (parent.Description.Contains(";"))
                    {
                        string[] data = parent.Description.Split(';');
                        family = data[0];
                        type = data[1];
                    }

                    if (elementType.Contains("wall"))
                    {
                        foreach (SketchUpNET.Surface surface in parent.Surfaces)
                        {
                            components.Items.Add(new WallProfileBased(family, type, new List<Types.Parameter>(), surface.ToGrevitOutline(transform), "") { GID = instance.Guid });
                        }
                    }
                    else if (elementType.Contains("grid"))
                    {
                        foreach (SketchUpNET.Edge edge in parent.Edges)
                        {
                            components.Items.Add(new Grid(new List<Types.Parameter>(), edge.Start.ToGrevitPoint(transform), edge.End.ToGrevitPoint(transform), parent.Name) { GID = instance.Guid });
                        }
                    }
                    else if (elementType.Contains("line"))
                    {
                        foreach (SketchUpNET.Edge edge in parent.Edges)
                        {
                            components.Items.Add(new RevitLine() { curve = edge.ToGrevitLine(transform), isModelCurve = true, isDetailCurve = false, isRoomBounding = false, parameters = new List<Parameter>(), GID = instance.Guid, FamilyOrStyle = family, TypeOrLayer = type });
                        }
                    }
                    else if (elementType.Contains("floor"))
                    {
                        foreach (SketchUpNET.Surface surface in parent.Surfaces)
                        {
                            Types.Point bottom = transform.GetTransformed(surface.Vertices[0]).ToGrevitPoint();
                            int ctr = surface.Vertices.Count / 2;
                            Types.Point top = transform.GetTransformed(surface.Vertices[ctr]).ToGrevitPoint();



                            components.Items.Add(new Slab()
                            {
                                FamilyOrStyle = family,
                                TypeOrLayer = type,
                                parameters = new List<Types.Parameter>(),
                                structural = true,
                                height = 1,
                                surface =
                                    surface.ToGrevitProfile(transform),
                                bottom = bottom,
                                top = top,
                                slope = top.z - bottom.z,
                                GID = instance.Guid,
                                levelbottom = "",
                            });
                        }
                    }
                    else if (elementType.Contains("column"))
                    {
                        Grevit.Types.Profile profile = null;
                        Grevit.Types.Point top = null;
                        SketchUpNET.Vertex v = new SketchUpNET.Vertex(0, 0, 0);
                        Grevit.Types.Point btm = v.ToGrevitPoint(transform);

                        foreach (SketchUpNET.Surface surface in parent.Surfaces)
                        {

                            if (surface.Normal.Z == 1)
                            {

                                top = new Types.Point(v.ToGrevitPoint(transform).x, v.ToGrevitPoint(transform).y,
                                    surface.Vertices[0].ToGrevitPoint(transform).z);
                            }

                        }

                        components.Items.Add(new Grevit.Types.Column(family, type, new List<Types.Parameter>(), btm, top, "", true)
                        {
                            GID = instance.Guid
                        });
                    }



                }

                return components;
            }
            return null;
        }


    }
    


}


