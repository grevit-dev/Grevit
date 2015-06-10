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

        public static Curve2dCollection To2dCurve(this Grevit.Types.Curve3Points curve)
        {
            Curve2dCollection curveArray = new Curve2dCollection();

            if (curve.GetType() == typeof(Grevit.Types.Line))
            {
                Grevit.Types.Line baseline = (Grevit.Types.Line)curve;
                curveArray.Add(new Line2d(GrevitPtoPoint2d(baseline.from), GrevitPtoPoint2d(baseline.to)));
            }
            else if (curve.GetType() == typeof(Grevit.Types.Arc))
            {
                Grevit.Types.Arc baseline = (Grevit.Types.Arc)curve;
                curveArray.Add(new CircularArc2d(GrevitPtoPoint2d(baseline.center), baseline.radius, baseline.start, baseline.end, Vector2d.XAxis, true));
            }
            else if (curve.GetType() == typeof(Grevit.Types.Curve3Points))
            {
                Grevit.Types.Curve3Points baseline = (Grevit.Types.Curve3Points)curve;
                curveArray.Add(new CircularArc2d(GrevitPtoPoint2d(baseline.a), GrevitPtoPoint2d(baseline.c), GrevitPtoPoint2d(baseline.b)));
            }
            else if (curve.GetType() == typeof(Grevit.Types.PLine))
            {
                Grevit.Types.PLine baseline = (Grevit.Types.PLine)curve;
                for (int i = 0; i < baseline.points.Count - 1; i++)
                {
                    curveArray.Add(new Line2d(GrevitPtoPoint2d(baseline.points[i]), GrevitPtoPoint2d(baseline.points[i + 1])));
                }
            }
            else if (curve.GetType() == typeof(Grevit.Types.Spline))
            {
                Grevit.Types.Spline s = (Grevit.Types.Spline)curve;
                Point2dCollection points = new Point2dCollection();
                foreach (Grevit.Types.Point p in s.controlPoints) points.Add(GrevitPtoPoint2d(p));
                DoubleCollection dc = new DoubleCollection();
                foreach (double dbl in s.weights) dc.Add(dbl);
                NurbCurve2d sp = new NurbCurve2d(s.degree, new KnotCollection(), points, dc, s.isPeriodic);
                curveArray.Add(sp);
            }

            return curveArray;
        }

        public static Curve3dCollection To3dCurve(this Grevit.Types.Curve3Points curve)
        {
            Curve3dCollection curveArray = new Curve3dCollection();

            if (curve.GetType() == typeof(Grevit.Types.Line))
            {
                Grevit.Types.Line baseline = (Grevit.Types.Line)curve;
                curveArray.Add(new Line3d(GrevitPtoPoint3d(baseline.from), GrevitPtoPoint3d(baseline.to)));
            }
            else if (curve.GetType() == typeof(Grevit.Types.Arc))
            {
                Grevit.Types.Arc baseline = (Grevit.Types.Arc)curve;
                curveArray.Add(new Arc(GrevitPtoPoint3d(baseline.center), baseline.radius, baseline.start, baseline.end).GetGeCurve());
            }
            else if (curve.GetType() == typeof(Grevit.Types.Curve3Points))
            {
                Grevit.Types.Curve3Points baseline = (Grevit.Types.Curve3Points)curve;
                curveArray.Add(new CircularArc3d(GrevitPtoPoint3d(baseline.a), GrevitPtoPoint3d(baseline.c), GrevitPtoPoint3d(baseline.b)));
            }
            else if (curve.GetType() == typeof(Grevit.Types.PLine))
            {
                Grevit.Types.PLine baseline = (Grevit.Types.PLine)curve;
                for (int i = 0; i < baseline.points.Count - 1; i++)
                {
                    curveArray.Add(new Line3d(GrevitPtoPoint3d(baseline.points[i]), GrevitPtoPoint3d(baseline.points[i + 1])));
                }
            }
            else if (curve.GetType() == typeof(Grevit.Types.Spline))
            {
                Grevit.Types.Spline s = (Grevit.Types.Spline)curve;
                Point3dCollection points = new Point3dCollection();
                foreach (Grevit.Types.Point p in s.controlPoints) points.Add(GrevitPtoPoint3d(p));
                DoubleCollection dc = new DoubleCollection();
                foreach (double dbl in s.weights) dc.Add(dbl);
                Spline sp = new Spline(s.degree, s.isRational, s.isClosed, s.isPeriodic, points, new DoubleCollection(), dc, 0, 0);
                curveArray.Add(sp.GetGeCurve());
            }

            return curveArray;
        }

        public static void SetParameters(this DBObject dbobject, List<Grevit.Types.Parameter> parameters)
        {
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
        }

        public static void StoreGID(this Grevit.Types.Component c, ObjectId id)
        {
            if (c.GID != null && id != ObjectId.Null && !Command.created_objects.ContainsKey(c.GID)) Command.created_objects.Add(c.GID, id);
        }

        public static Point3dCollection To3dPointCollection(this Grevit.Types.Curve3Points curve)
        {
            Point3dCollection points = new Point3dCollection();

            if (curve.GetType() == typeof(Grevit.Types.Line))
            {
                Grevit.Types.Line baseline = (Grevit.Types.Line)curve;
                Point3d p1 = GrevitPtoPoint3d(baseline.from);
                Point3d p2 = GrevitPtoPoint3d(baseline.to);

                if (!points.Contains(p1)) points.Add(p1);
                if (!points.Contains(p2)) points.Add(p2);
            }
            else if (curve.GetType() == typeof(Grevit.Types.Arc))
            {

            }
            else if (curve.GetType() == typeof(Grevit.Types.Curve3Points))
            {
                Grevit.Types.Curve3Points baseline = (Grevit.Types.Curve3Points)curve;
                Point3d p1 = GrevitPtoPoint3d(baseline.a);
                Point3d p2 = GrevitPtoPoint3d(baseline.b);
                Point3d p3 = GrevitPtoPoint3d(baseline.c);

                if (!points.Contains(p1)) points.Add(p1);
                if (!points.Contains(p2)) points.Add(p2);
                if (!points.Contains(p3)) points.Add(p3);
            }
            else if (curve.GetType() == typeof(Grevit.Types.PLine))
            {
                Grevit.Types.PLine baseline = (Grevit.Types.PLine)curve;
                for (int i = 0; i < baseline.points.Count; i++)
                {
                    Point3d p1 = GrevitPtoPoint3d(baseline.points[i]);
                    if (!points.Contains(p1)) points.Add(p1);
                }
            }
            else if (curve.GetType() == typeof(Grevit.Types.Spline))
            {

                Grevit.Types.Spline s = (Grevit.Types.Spline)curve;

                foreach (Grevit.Types.Point p in s.controlPoints)
                {
                    Point3d p1 = GrevitPtoPoint3d(p);
                    if (!points.Contains(p1)) points.Add(p1);
                }

            }


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

        public static void AddXData(this Grevit.Types.Component comp, Entity ent)
        {
            AddRegAppTableRecord("Grevit");
            ResultBuffer rbs = new ResultBuffer(new TypedValue(1001, "Grevit"), new TypedValue(1000, comp.GID));
            ent.XData = rbs;
            rbs.Dispose();
        }

        public static void AddRegAppTableRecord(string regAppName)
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            Transaction tr = doc.TransactionManager.StartTransaction();
            using (tr)
            {
                RegAppTable rat = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead, false);

                if (!rat.Has(regAppName))
                {
                    rat.UpgradeOpen();
                    RegAppTableRecord ratr = new RegAppTableRecord();
                    ratr.Name = regAppName;
                    rat.Add(ratr);
                    tr.AddNewlyCreatedDBObject(ratr, true);
                }
                tr.Commit();
            }

        }

        /// <summary>
        /// Invoke the Components Create Method
        /// </summary>
        /// <param name="component"></param>
        public static void Build(this Grevit.Types.Component component, bool useReferenceElement)
        {
            // Create a new transaction
            Transaction transaction = Grevit.AutoCad.Command.Database.TransactionManager.StartTransaction();
            using (transaction)
            {

                // Get the components type
                Type type = component.GetType();

                // Get the Create extension Method using reflection
                IEnumerable<System.Reflection.MethodInfo> methods = Grevit.Reflection.Utilities.GetExtensionMethods(component.GetType().Assembly, type);

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
                        Entity createdElement = (Entity)method.Invoke(component, parameters);

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
            }

        }
    }
}
