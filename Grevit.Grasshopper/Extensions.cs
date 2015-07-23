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
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using System.Net.Sockets;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using Grevit.Types;

namespace Grevit.GrassHopper
{
    /// <summary>
    /// Grevit Extensions for Rhino and Grasshopper
    /// </summary>
    public static class ComponentExtensions
    {
        public static Grevit.Types.Color ToGrevitColor(this GH_Colour color)
        {
            return
                new Types.Color() { R = color.Value.R, B = color.Value.B, G = color.Value.G };
        }


        /// <summary>
        /// Get Grevit Point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Grevit.Types.Point ToGrevitPoint(this GH_Point point)
        {
            Grevit.Types.Point grevitPoint = new Grevit.Types.Point(point.Value.X, point.Value.Y, point.Value.Z);
            grevitPoint.id = point.ReferenceID.ToString();
            return grevitPoint;
        }

        /// <summary>
        /// Get Grevit Point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Grevit.Types.Point ToGrevitPoint(this Rhino.Geometry.Point3d point)
        {
            return new Grevit.Types.Point(point.X, point.Y, point.Z);
        }

        /// <summary>
        /// Get Grevit Points
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static Tuple<Grevit.Types.Point, Grevit.Types.Point> ToGrevitPoints(this GH_Line curve)
        {

            Grevit.Types.Point p1 = new Grevit.Types.Point()
            {
                x = curve.Value.From.X,
                y = curve.Value.From.Y,
                z = curve.Value.From.Z,
                id = curve.ReferenceID.ToString()
            };

            Grevit.Types.Point p2 = new Grevit.Types.Point()
            {
                x = curve.Value.To.X,
                y = curve.Value.To.Y,
                z = curve.Value.To.Z,
                id = curve.ReferenceID.ToString()
            };

            return new Tuple<Grevit.Types.Point, Grevit.Types.Point>(p1, p2);
        }

        /// <summary>
        /// Get Grevit Curve Component
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static Component ToGrevitCurve(this GH_Curve curve)
        {
            return curve.Value.ToGrevitCurve(curve.ReferenceID.ToString());
        }

        /// <summary>
        /// Get Grevit Curve Component
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="referenceID"></param>
        /// <returns></returns>
        public static Component ToGrevitCurve(this Rhino.Geometry.Curve curve, string referenceID = "")
        {
            if (curve.IsArc(Rhino.RhinoMath.ZeroTolerance))
            {
                Rhino.Geometry.Arc a;
                if (curve.TryGetArc(out a))
                {
                    Curve3Points arc = new Curve3Points();
                    arc.a = curve.PointAtStart.ToGrevitPoint();
                    arc.b = curve.PointAt(0.5).ToGrevitPoint();
                    arc.c = curve.PointAtEnd.ToGrevitPoint();
                    arc.GID = referenceID;

                    return arc;
                }
            }
            else if (curve.IsPolyline())
            {
                Rhino.Geometry.Polyline pline;
                if (curve.TryGetPolyline(out pline))
                {
                    PLine arc = new PLine();
                    arc.points = new List<Grevit.Types.Point>();
                    foreach (Rhino.Geometry.Point3d pkt in pline)
                    {
                        arc.points.Add(pkt.ToGrevitPoint());
                    }
                    arc.closed = pline.IsClosed;
                    arc.GID = referenceID;
                    return arc;
                }
            }
            else if (curve.IsEllipse())
            {

                Curve3Points arc = new Curve3Points();
                arc.a = curve.PointAtStart.ToGrevitPoint();
                arc.b = curve.PointAt(0.5).ToGrevitPoint();
                arc.c = curve.PointAtEnd.ToGrevitPoint();
                arc.GID = referenceID;
                return arc;

            }
            else if (curve.GetType() == typeof(Rhino.Geometry.NurbsCurve))
            {
                Rhino.Geometry.NurbsCurve nc = (Rhino.Geometry.NurbsCurve)curve;
                Grevit.Types.Spline spline = new Grevit.Types.Spline();
                spline.controlPoints = new List<Grevit.Types.Point>();
                spline.weights = new List<double>();
                foreach (Rhino.Geometry.ControlPoint p in nc.Points)
                {
                    spline.controlPoints.Add(p.Location.ToGrevitPoint());
                    spline.weights.Add(p.Weight);
                }
                spline.degree = nc.Degree;
                spline.isClosed = nc.IsClosed;
                spline.isRational = nc.IsRational;
                spline.isPeriodic = nc.IsPeriodic;
                spline.GID = referenceID;
                spline.knots = new List<double>();
                foreach (double dbl in nc.Knots) spline.knots.Add(dbl);

                return spline;

            }
            else
            {
                Line arc = new Line();
                arc.from = curve.PointAtStart.ToGrevitPoint();
                arc.to = curve.PointAtEnd.ToGrevitPoint();
                arc.GID = referenceID;
                return arc;
            }


            return null;
        }

    }
}
