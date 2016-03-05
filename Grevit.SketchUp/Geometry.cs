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
using Grevit.Types;

namespace Grevit.SketchUp
{
    public static class Geometry
    {

        public static Grevit.Types.Profile ToGrevitProfile(this SketchUpNET.Surface surface, SketchUpNET.Transform t = null)
        {
            Types.Profile profile = new Types.Profile();
            profile.profile = new List<Loop>();

            Loop outerloop = new Loop();

            outerloop.outline = new List<Types.Component>();
            foreach (SketchUpNET.Edge corner in surface.OuterEdges.Edges)
                outerloop.outline.Add(corner.ToGrevitLine(t));

            profile.profile.Add(outerloop);


            foreach (SketchUpNET.Loop skploop in surface.InnerEdges)
            {
                Loop innerloop = new Loop();

                innerloop.outline = new List<Types.Component>();
                foreach (SketchUpNET.Edge corner in skploop.Edges)
                    innerloop.outline.Add(corner.ToGrevitLine(t));

                profile.profile.Add(innerloop);
            }

            return profile;
        }

        public static List<Grevit.Types.Component> ToGrevitOutline(this SketchUpNET.Surface surface, SketchUpNET.Transform t = null)
        {
            List<Grevit.Types.Component> lines = new List<Types.Component>();
            foreach (SketchUpNET.Edge corner in surface.OuterEdges.Edges)
            {
                lines.Add(corner.ToGrevitLine(t));
            }
            return lines;
        }

        public static Grevit.Types.Line ToGrevitLine(this SketchUpNET.Edge corner, SketchUpNET.Transform t = null)
        {
            return new Grevit.Types.Line()
            {
                from = corner.Start.ToGrevitPoint(t),
                to = corner.End.ToGrevitPoint(t)
            };
        }

        public static Grevit.Types.Point ToGrevitPoint(this SketchUpNET.Vertex v, SketchUpNET.Transform t = null)
        {

            if (t != null)
            {
                SketchUpNET.Vertex vertex = t.GetTransformed(v);
                return new Grevit.Types.Point(vertex.X, vertex.Y, vertex.Z);
            }
            else
                return new Grevit.Types.Point(v.X, v.Y, v.Z);
        }
    }
}
