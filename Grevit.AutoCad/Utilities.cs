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
using Autodesk.Aec.PropertyData.DatabaseServices;

namespace Grevit.AutoCad
{
    public static class Utilities
    {

       // Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
       // Transaction tr = db.TransactionManager.StartTransaction();
       // Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        //BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
       // BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

        public static Point3d ToPoint3d(this Grevit.Types.Point p)
        {
            return new Point3d(p.x, p.y, p.z);
        }

        public static Point2d ToPoint2d(this Grevit.Types.Point p)
        {
            return new Point2d(p.x, p.y);
        }

        public static Curve2dCollection To2dCurve(this Grevit.Types.Component curve)
        {
            Curve2dCollection curveArray = new Curve2dCollection();

            if (curve.GetType() == typeof(Grevit.Types.Line))
            {
                Grevit.Types.Line baseline = (Grevit.Types.Line)curve;
                curveArray.Add(new Line2d(baseline.from.ToPoint2d(), baseline.to.ToPoint2d()));
            }
            else if (curve.GetType() == typeof(Grevit.Types.Arc))
            {
                Grevit.Types.Arc baseline = (Grevit.Types.Arc)curve;
                curveArray.Add(new CircularArc2d(baseline.center.ToPoint2d(), baseline.radius, baseline.start, baseline.end, Vector2d.XAxis, true));
            }
            else if (curve.GetType() == typeof(Grevit.Types.Curve3Points))
            {
                Grevit.Types.Curve3Points baseline = (Grevit.Types.Curve3Points)curve;
                curveArray.Add(new CircularArc2d(baseline.a.ToPoint2d(), baseline.c.ToPoint2d(), baseline.b.ToPoint2d()));
            }
            else if (curve.GetType() == typeof(Grevit.Types.PLine))
            {
                Grevit.Types.PLine baseline = (Grevit.Types.PLine)curve;
                for (int i = 0; i < baseline.points.Count - 1; i++)
                {
                    curveArray.Add(new Line2d(baseline.points[i].ToPoint2d(), baseline.points[i + 1].ToPoint2d()));
                }
            }
            else if (curve.GetType() == typeof(Grevit.Types.Spline))
            {
                Grevit.Types.Spline s = (Grevit.Types.Spline)curve;
                Point2dCollection points = new Point2dCollection();
                foreach (Grevit.Types.Point p in s.controlPoints) points.Add(p.ToPoint2d());
                DoubleCollection dc = new DoubleCollection();
                foreach (double dbl in s.weights) dc.Add(dbl);
                NurbCurve2d sp = new NurbCurve2d(s.degree, new KnotCollection(), points, dc, s.isPeriodic);
                curveArray.Add(sp);
            }

            return curveArray;
        }

        public static Curve3dCollection To3dCurve(this Grevit.Types.Component curve)
        {
            Curve3dCollection curveArray = new Curve3dCollection();

            if (curve.GetType() == typeof(Grevit.Types.Line))
            {
                Grevit.Types.Line baseline = (Grevit.Types.Line)curve;
                curveArray.Add(new Line3d(baseline.from.ToPoint3d(), baseline.to.ToPoint3d()));
            }
            else if (curve.GetType() == typeof(Grevit.Types.Arc))
            {
                Grevit.Types.Arc baseline = (Grevit.Types.Arc)curve;
                curveArray.Add(new Arc(baseline.center.ToPoint3d(), baseline.radius, baseline.start, baseline.end).GetGeCurve());
            }
            else if (curve.GetType() == typeof(Grevit.Types.Curve3Points))
            {
                Grevit.Types.Curve3Points baseline = (Grevit.Types.Curve3Points)curve;
                curveArray.Add(new CircularArc3d(baseline.a.ToPoint3d(), baseline.c.ToPoint3d(), baseline.b.ToPoint3d()));
            }
            else if (curve.GetType() == typeof(Grevit.Types.PLine))
            {
                Grevit.Types.PLine baseline = (Grevit.Types.PLine)curve;
                for (int i = 0; i < baseline.points.Count - 1; i++)
                {
                    curveArray.Add(new Line3d(baseline.points[i].ToPoint3d(), baseline.points[i + 1].ToPoint3d()));
                }
            }
            else if (curve.GetType() == typeof(Grevit.Types.Spline))
            {
                Grevit.Types.Spline s = (Grevit.Types.Spline)curve;
                Point3dCollection points = new Point3dCollection();
                foreach (Grevit.Types.Point p in s.controlPoints) points.Add(p.ToPoint3d());
                DoubleCollection dc = new DoubleCollection();
                foreach (double dbl in s.weights) dc.Add(dbl);
                Spline sp = new Spline(s.degree, s.isRational, s.isClosed, s.isPeriodic, points, new DoubleCollection(), dc, 0, 0);
                curveArray.Add(sp.GetGeCurve());
            }

            return curveArray;
        }

        public static void SetParameters(this Grevit.Types.Component component, DBObject dbobject)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {

                List<Grevit.Types.Parameter> parameters = component.parameters;

                if (parameters != null && parameters.Count > 0)
                {

                    ObjectIdCollection source_setIds = PropertyDataServices.GetPropertySets(dbobject);

                    foreach (Grevit.Types.Parameter param in parameters)
                    {
                        foreach (ObjectId source_id in source_setIds)
                        {
                            PropertySet source_pset = (PropertySet)tr.GetObject(source_id, OpenMode.ForWrite, false, false);
                            PropertySetDataCollection allProps = source_pset.PropertySetData;
                            foreach (PropertySetData ii in allProps)
                            {
                                int id = ii.Id;
                                string name = source_pset.PropertyIdToName(id);
                                object value = source_pset.GetAt(id);

                                if (name == param.name)
                                {
                                    if (value.GetType() == typeof(string)) source_pset.SetAt(id, param.value);
                                    if (value.GetType() == typeof(double)) source_pset.SetAt(id, param.value);
                                    if (value.GetType() == typeof(int)) source_pset.SetAt(id, param.value);
                                    if (value.GetType() == typeof(bool)) source_pset.SetAt(id, param.value);
                                }

                            }

                        }
                    }
                }

                tr.Commit();
            }
            

        }

        public static void StoreGID(this Grevit.Types.Component c, ObjectId id)
        {
            if (c.GID != null && id != ObjectId.Null && !Command.created_objects.ContainsKey(c.GID)) Command.created_objects.Add(c.GID, id);
        }

        public static Point3dCollection To3dPointCollection(this Grevit.Types.Component curve)
        {
            Point3dCollection points = new Point3dCollection();

            if (curve.GetType() == typeof(Grevit.Types.Line))
            {
                Grevit.Types.Line baseline = (Grevit.Types.Line)curve;
                Point3d p1 = baseline.from.ToPoint3d();
                Point3d p2 = baseline.to.ToPoint3d();

                if (!points.Contains(p1)) points.Add(p1);
                if (!points.Contains(p2)) points.Add(p2);
            }
            else if (curve.GetType() == typeof(Grevit.Types.Arc))
            {

            }
            else if (curve.GetType() == typeof(Grevit.Types.Curve3Points))
            {
                Grevit.Types.Curve3Points baseline = (Grevit.Types.Curve3Points)curve;
                Point3d p1 = baseline.a.ToPoint3d();
                Point3d p2 = baseline.b.ToPoint3d();
                Point3d p3 = baseline.c.ToPoint3d();

                if (!points.Contains(p1)) points.Add(p1);
                if (!points.Contains(p2)) points.Add(p2);
                if (!points.Contains(p3)) points.Add(p3);
            }
            else if (curve.GetType() == typeof(Grevit.Types.PLine))
            {
                Grevit.Types.PLine baseline = (Grevit.Types.PLine)curve;
                for (int i = 0; i < baseline.points.Count; i++)
                {
                    Point3d p1 = baseline.points[i].ToPoint3d();
                    if (!points.Contains(p1)) points.Add(p1);
                }
            }
            else if (curve.GetType() == typeof(Grevit.Types.Spline))
            {

                Grevit.Types.Spline s = (Grevit.Types.Spline)curve;

                foreach (Grevit.Types.Point p in s.controlPoints)
                {
                    Point3d p1 = p.ToPoint3d();
                    if (!points.Contains(p1)) points.Add(p1);
                }

            }

            return points;
        }

        public static Dictionary<string, ObjectId> getExistingObjectIDs(Grevit.Types.ComponentCollection cs)
        {
            Dictionary<string, ObjectId> existingObjects = new Dictionary<string, ObjectId>();
            if (cs.update)
            {
                Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
                Transaction tr = db.TransactionManager.StartTransaction();
                using (tr)
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);



                    List<string> banned = new List<string>();

                    foreach (ObjectId id in ms)
                    {
                        Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                        ResultBuffer rb = ent.XData;

                        if (rb != null)
                        {
                            bool hasGrevitId = false;
                            string GrevitId = "";

                            foreach (TypedValue tv in rb)
                            {
                                if (tv.TypeCode == 1001 && tv.Value.ToString() == "Grevit") hasGrevitId = true;
                                if (tv.TypeCode == 1000 && tv.Value != null) GrevitId = tv.Value.ToString();
                            }
                            rb.Dispose();

                            if (hasGrevitId && GrevitId.Length > 0)
                            {
                                foreach (Grevit.Types.Component c in cs.Items)
                                {
                                    if (c.GID == GrevitId)
                                    {
                                        if (!existingObjects.ContainsKey(c.GID))
                                        {
                                            if (!banned.Contains(c.GID)) existingObjects.Add(c.GID, ent.Id);
                                        }
                                        else
                                        {
                                            existingObjects.Remove(c.GID);
                                            banned.Add(c.GID);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(existingObjects.Count.ToString());
            }

            return existingObjects;
        }

        public static void AddXData(this DBObject dbobject, Grevit.Types.Component comp, Transaction tr)
        {
            AddRegAppTableRecord("Grevit",tr);

            Entity ent = (Entity)tr.GetObject(dbobject.Id, OpenMode.ForWrite);

            ResultBuffer rbs = new ResultBuffer(new TypedValue(1001, "Grevit"), new TypedValue(1000, comp.GID));
            ent.XData = rbs;
            rbs.Dispose();

            ent.Dispose();

        }

        public static void AddRegAppTableRecord(string regAppName, Transaction tr)
        {
            RegAppTable rat = (RegAppTable)tr.GetObject(Command.Database.RegAppTableId, OpenMode.ForRead, false);

            if (!rat.Has(regAppName))
            {
                rat.UpgradeOpen();
                RegAppTableRecord ratr = new RegAppTableRecord();
                ratr.Name = regAppName;
                rat.Add(ratr);
                tr.AddNewlyCreatedDBObject(ratr, true);
            }

        }


    }
}
