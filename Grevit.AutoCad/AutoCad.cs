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


    class Command
    {
        private static Dictionary<string, ObjectId> existing_objects;
        private static Dictionary<string, ObjectId> created_objects;


        [CommandMethod("GREVIT")]
        public static void start()
        {
            Grevit.Serialization.Client gs = new Grevit.Serialization.Client();

            System.Windows.Forms.DialogResult dr = gs.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.Cancel) return;

            List<Grevit.Types.Component> stalled = new List<Grevit.Types.Component>();

            existing_objects = getExistingObjectIDs(gs.componentCollection);
            created_objects = new Dictionary<string, ObjectId>();

            foreach (Grevit.Types.Component c in gs.componentCollection.Items)
            {

                if (!c.stalledForReference)
                {
                    if (c.GetType() == typeof(Grevit.Types.Column)) Create((Grevit.Types.Column)c);
                    if (c.GetType() == typeof(Grevit.Types.Wall)) Create((Grevit.Types.Wall)c);
                    if (c.GetType() == typeof(Grevit.Types.Line)) Create((Grevit.Types.Line)c);
                    if (c.GetType() == typeof(Grevit.Types.Spline)) Create((Grevit.Types.Spline)c);
                    if (c.GetType() == typeof(Grevit.Types.Room)) Create((Grevit.Types.Room)c);
                    if (c.GetType() == typeof(Grevit.Types.Arc)) Create((Grevit.Types.Arc)c);
                    if (c.GetType() == typeof(Grevit.Types.PLine)) Create((Grevit.Types.PLine)c);
                    if (c.GetType() == typeof(Grevit.Types.DrawingPoint)) Create((Grevit.Types.DrawingPoint)c);
                    if (c.GetType() == typeof(Grevit.Types.Slab)) Create((Grevit.Types.Slab)c); 
                }
                else stalled.Add(c);
            }



            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            try
            {
                foreach (Grevit.Types.Component c in stalled)
                {
                    if (c.GetType() == typeof(Grevit.Types.Door))
                    {
                        Grevit.Types.Door door = (Grevit.Types.Door)c;
                        foreach (KeyValuePair<string, ObjectId> kvp in Command.created_objects)
                        {
                            if (kvp.Key == door.referenceGID)
                            {
                                Wall wall = (Wall)tr.GetObject(kvp.Value, OpenMode.ForRead);
                                Create(db, door, wall, tr, ms);
                            }
                        }
                    }
                }
                tr.Commit();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                ed.WriteMessage(e.Message);
                tr.Abort();
            }

            finally
            {
                tr.Dispose();
            }
        }

        public static void Create(Grevit.Types.Column c)
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = db.TransactionManager.StartTransaction();

            DictionaryMemberStyle mst = new DictionaryMemberStyle(db);

            try
            {
                Member member = new Member();
                member.MemberType = MemberType.Column;


                    if (Command.existing_objects.ContainsKey(c.GID))
                    {
                        member = (Member)tr.GetObject(Command.existing_objects[c.GID], OpenMode.ForWrite);
                    }
                    
                else
                {

                    member.SetDatabaseDefaults(db);
                    member.SetToStandard(db);


                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    ms.AppendEntity(member);
                    tr.AddNewlyCreatedDBObject(member, true);
                }
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                if (c.TypeOrLayer != "") { if (lt.Has(c.TypeOrLayer)) member.LayerId = lt[c.TypeOrLayer]; }
                if (mst.Has(c.FamilyOrStyle, tr)) member.StyleId = mst.GetAt(c.FamilyOrStyle);
                member.Set(GrevitPtoPoint3d(c.location), GrevitPtoPoint3d(c.locationTop));

                writeProperties(member, c.parameters, tr);
                AddXData(c, member);
                storeID(c, member.Id);
                tr.Commit();
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                tr.Abort();
            }

            finally
            {
                tr.Dispose();
            }

        }

        public static Point3d GrevitPtoPoint3d(Grevit.Types.Point p)
        {
            return new Point3d(p.x, p.y, p.z);
        }

        public static Point2d GrevitPtoPoint2d(Grevit.Types.Point p)
        {
            return new Point2d(p.x, p.y);
        }


        public static void Create(Grevit.Types.Wall w)
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            DictionaryWallStyle ws = new DictionaryWallStyle(db);
            try
            {
                Wall wall = new Wall();
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

                bool newEnt = false;

                if (Command.existing_objects.ContainsKey(w.GID)) wall = (Wall)tr.GetObject(Command.existing_objects[w.GID], OpenMode.ForWrite);
                else
                {
                    wall.SetDatabaseDefaults(db);
                    wall.SetToStandard(db);
                    newEnt = true;
                    wall.JustificationType = WallJustificationType.Center;
                }

                if (w.TypeOrLayer != "") { if (lt.Has(w.TypeOrLayer)) wall.LayerId = lt[w.TypeOrLayer]; }
                if (ws.Has(w.FamilyOrStyle, tr)) wall.StyleId = ws.GetAt(w.FamilyOrStyle);

                

                if (w.from != null && w.to != null)
                {
                    wall.Set(GrevitPtoPoint3d(w.from), GrevitPtoPoint3d(w.to), Vector3d.ZAxis);
                }
                else
                {
                    if (w.curve.GetType() == typeof(Grevit.Types.PLine))
                    {
                        Grevit.Types.PLine pline = (Grevit.Types.PLine)w.curve;
                        for (int i = 0; i < pline.points.Count; i++)
                        {
                            if (i == pline.points.Count - 1)
                            {
                                if (pline.closed)
                                {
                                    w.from = pline.points[i];
                                    w.to = pline.points[0];
                                    Create(w);
                                }
                            }
                            else
                            {
                                w.from = pline.points[i];
                                w.to = pline.points[i + 1];
                                Create(w);
                            }
                        }


                    }
                    else if (w.curve.GetType() == typeof(Grevit.Types.Line))
                    {
                        Grevit.Types.Line baseline = (Grevit.Types.Line)w.curve;
                        wall.Set(GrevitPtoPoint3d(baseline.from), GrevitPtoPoint3d(baseline.to), Vector3d.ZAxis);
                    }
                    else if (w.curve.GetType() == typeof(Grevit.Types.Arc))
                    {
                        Grevit.Types.Arc baseline = (Grevit.Types.Arc)w.curve;
                        CircularArc3d arc = new CircularArc3d(GrevitPtoPoint3d(baseline.center), Vector3d.ZAxis, Vector3d.ZAxis, baseline.radius, baseline.start, baseline.end);
                        wall.Set(arc, Vector3d.ZAxis);
                    }
                    else if (w.curve.GetType() == typeof(Grevit.Types.Curve3Points))
                    {
                        Grevit.Types.Curve3Points baseline = (Grevit.Types.Curve3Points)w.curve;
                        wall.Set(GrevitPtoPoint3d(baseline.a), GrevitPtoPoint3d(baseline.b), GrevitPtoPoint3d(baseline.c), Vector3d.ZAxis);
                    }
                }


                wall.BaseHeight = w.height;

                if (newEnt)
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    AddXData(w, wall);
                    ms.AppendEntity(wall);
                    tr.AddNewlyCreatedDBObject(wall, true);
                    storeID(w, wall.Id);
                }
                writeProperties(wall, w.parameters, tr);
                tr.Commit();
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                ed.WriteMessage(e.Message);
                tr.Abort();
            }

            finally
            {
                tr.Dispose();
            }

        }


        public static void Create(Grevit.Types.Line l)
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

            try
            {
                Line line = new Line(GrevitPtoPoint3d(l.from), GrevitPtoPoint3d(l.to));


                line.SetDatabaseDefaults(db);

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                AddXData(l, line);
                if (lt.Has(l.TypeOrLayer)) line.LayerId = lt[l.TypeOrLayer];
                ms.AppendEntity(line);
                tr.AddNewlyCreatedDBObject(line, true);


                writeProperties(line, l.parameters, tr);
                storeID(l, line.Id);
                tr.Commit();

            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                ed.WriteMessage(e.Message);
                tr.Abort();
            }

            finally
            {
                tr.Dispose();
            }

        }

        public static void Create(Grevit.Types.Spline s)
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

            try
            {
                Point3dCollection points = new Point3dCollection();
                foreach (Grevit.Types.Point p in s.controlPoints) points.Add(GrevitPtoPoint3d(p));
                DoubleCollection dc = new DoubleCollection();
                foreach (double dbl in s.weights) dc.Add(dbl);
                DoubleCollection dcc = new DoubleCollection();
                foreach (double dbl in s.knots) dcc.Add(dbl);
                Spline sp = new Spline(s.degree, s.isRational, s.isClosed, s.isPeriodic, points,dcc, dc, 0, 0);
                sp.SetDatabaseDefaults(db);

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                AddXData(s, sp);
                if (lt.Has(s.TypeOrLayer)) sp.LayerId = lt[s.TypeOrLayer];
                ms.AppendEntity(sp);
                tr.AddNewlyCreatedDBObject(sp, true);


                writeProperties(sp, s.parameters, tr);
                storeID(s, sp.Id);
                tr.Commit();

            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                ed.WriteMessage(e.Message);
                tr.Abort();
            }

            finally
            {
                tr.Dispose();
            }

        }

        public static void Create(Grevit.Types.Arc a)
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;



            try
            {
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                Arc arc = new Arc(GrevitPtoPoint3d(a.center), a.radius, a.start, a.end);

                arc.SetDatabaseDefaults(db);
                if (lt.Has(a.TypeOrLayer)) arc.LayerId = lt[a.TypeOrLayer];

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                ms.AppendEntity(arc);
                tr.AddNewlyCreatedDBObject(arc, true);
                writeProperties(arc, a.parameters, tr);
                storeID(a, arc.Id);
                tr.Commit();
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                ed.WriteMessage(e.Message);
                tr.Abort();
            }

            finally
            {
                tr.Dispose();
            }

        }

        public static void Create(Grevit.Types.PLine a)
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;



            try
            {
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                Point3dCollection pc = new Point3dCollection();
                foreach (Grevit.Types.Point p in a.points) pc.Add(GrevitPtoPoint3d(p));
                Polyline3d pp = new Polyline3d(Poly3dType.SimplePoly, pc, a.closed);
                pp.SetDatabaseDefaults(db);
                if (lt.Has(a.TypeOrLayer)) pp.LayerId = lt[a.TypeOrLayer];

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                ms.AppendEntity(pp);
                tr.AddNewlyCreatedDBObject(pp, true);
                writeProperties(pp, a.parameters, tr);
                storeID(a, pp.Id);
                tr.Commit();
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                ed.WriteMessage(e.Message);
                tr.Abort();
            }

            finally
            {
                tr.Dispose();
            }

        }

        public static void Create(Grevit.Types.Room r)
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            DictionarySpaceStyle ss = new DictionarySpaceStyle(db);



            try
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                Polyline acPoly = new Polyline();
                acPoly.SetDatabaseDefaults();

                int i = 0;
                foreach (Grevit.Types.Point p in r.points)
                {
                    acPoly.AddVertexAt(i, new Point2d(p.x, p.y), 0, 0, 0);
                    i++;
                }
                acPoly.Closed = true;
                ms.AppendEntity(acPoly);
                tr.AddNewlyCreatedDBObject(acPoly, true);


                Autodesk.Aec.Geometry.Profile myProfile = Autodesk.Aec.Geometry.Profile.CreateFromEntity(acPoly, ed.CurrentUserCoordinateSystem);

                Space space;


                bool newEnt = false;

                if (Command.existing_objects.ContainsKey(r.GID))
                {
                    space = (Space)tr.GetObject(Command.existing_objects[r.GID], OpenMode.ForWrite);
                }
                else
                {
                    newEnt = true;
                    space = new Space();
                    space.SetDatabaseDefaults(db);
                    space.SetToStandard(db);
                }

                space.Associative = r.associative;
                space.Name = r.name;

                space.GeometryType = SpaceGeometryType.TwoD;
                space.Location = new Point3d(0, 0, 0);
                space.SetBaseProfile(myProfile, Matrix3d.Identity);

                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                if (r.TypeOrLayer != "") { if (lt.Has(r.TypeOrLayer)) space.LayerId = lt[r.TypeOrLayer]; }
                if (ss.Has(r.FamilyOrStyle, tr)) space.StyleId = ss.GetAt(r.FamilyOrStyle);

                if (newEnt)
                {
                    ms.AppendEntity(space);
                    tr.AddNewlyCreatedDBObject(space, true);
                }
                AddXData(r, space);
                writeProperties(space, r.parameters, tr);
                storeID(r, space.Id);
                tr.Commit();
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                ed.WriteMessage(e.Message);
                tr.Abort();
            }

            finally
            {
                tr.Dispose();
            }

        }

        public static void Create(Grevit.Types.Slab s)
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
           // DictionarySlabStyle ss = new DictionarySlabStyle(db);


            try
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                
                // Polyline acPoly = new Polyline();
                //acPoly.SetDatabaseDefaults();

                //int i = 0;

                DBObjectCollection objColl = new DBObjectCollection();
                Point3dCollection ptcol = new Point3dCollection();
                foreach (Grevit.Types.Component p in s.surface.outline)
                {

                    parse3dCurve(ptcol, p);


                    //Curve3dCollection collt = parse3dCurve(p);
                    //acPoly.AppendVertex(new PolylineVertex3d(new Point3d(p.x, p.y, p.z)));
                    //acPoly.AddVertexAt(i, new Point2d(p.x, p.y), 0, 0, 0);
                    //i++;
                    
                    //foreach (Curve3d curve in collt)
                    //{ 
                    //    objColl.Add(Autodesk.AutoCAD.DatabaseServices.Curve.CreateFromGeCurve(curve));
                    //}
                }
                ed.WriteMessage(ptcol.Count.ToString());

                Polyline3d face = new Polyline3d(Poly3dType.SimplePoly, ptcol, true);
                objColl.Add(face);
                //Polyline3d face = new Polyline3d();
                //ETC...
                // or from your settings
                // Polyline3d face = new Polyline3d(Poly3dType.SimplePoly, vertices, true);


        


                    DBObjectCollection myRegionColl = new DBObjectCollection();
                    // create a single region
                    Autodesk.AutoCAD.DatabaseServices.Region objreg = new Autodesk.AutoCAD.DatabaseServices.Region();
                    DBObjectCollection objRegions = new DBObjectCollection();
                    try
                    {
                        objRegions = Autodesk.AutoCAD.DatabaseServices.Region.CreateFromCurves(objColl);
                        objreg = objRegions[0] as Autodesk.AutoCAD.DatabaseServices.Region;

                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        // eInvalidInput exception
                        Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Error: unable to create region collection:\n" + ex.Message);

                    }
                





                //acPoly.Closed = true;
                //ms.AppendEntity(acPoly);
                //tr.AddNewlyCreatedDBObject(acPoly, true);

                //atrix3d mm = Matrix3d.Displacement(new Vector3d(0, 0, r.outline[0].z));
                //acPoly.TransformBy(mm);


                //Autodesk.Aec.Geometry.Profile myProfile = Autodesk.Aec.Geometry.Profile.CreateFromEntity(acPoly, ed.CurrentUserCoordinateSystem);

                //Slab slab = new Slab();
                //slab.SetDatabaseDefaults(db);
                //slab.SetToStandard(db);         
                //slab.Location = new Point3d(0, 0, 0);

                //slab.SetBaseProfile(myProfile, Matrix3d.Identity);

                //DBObjectCollection col = acPoly.GetOffsetCurves(0);
                
                
                //DBObjectCollection res = Region.CreateFromCurves(coll);
                
                //Region reg = res[0] as Region;

                Solid3d solid = new Solid3d();
                solid.Extrude(objreg, s.height, s.slope);


                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                if (s.TypeOrLayer != "") { if (lt.Has(s.TypeOrLayer)) solid.LayerId = lt[s.TypeOrLayer]; }
                //if (ss.Has(r.family, tr)) slab.StyleId = ss.GetAt(r.family);


                ms.AppendEntity(solid);
                tr.AddNewlyCreatedDBObject(solid, true);
                writeProperties(solid, s.parameters, tr);
                storeID(s, solid.Id);


                tr.Commit();
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                ed.WriteMessage(e.Message);
                tr.Abort();
            }

            finally
            {
                tr.Dispose();
            }

        }



        private static Curve2dCollection parse2dCurve(Grevit.Types.Component curve)
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
                curveArray.Add(new CircularArc2d(GrevitPtoPoint2d(baseline.center), baseline.radius, baseline.start, baseline.end,Vector2d.XAxis,true));
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
                NurbCurve2d sp = new NurbCurve2d(s.degree, new KnotCollection(), points, dc,s.isPeriodic);     
                curveArray.Add(sp);
            }

            return curveArray;
        }

        private static Curve3dCollection parse3dCurve(Grevit.Types.Component curve)
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

        private static void parse3dCurve(Point3dCollection points, Grevit.Types.Component curve)
        {
            

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


        public static void Create(Grevit.Types.DrawingPoint a)
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;



            try
            {
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                Point3d pp = GrevitPtoPoint3d(a.point);
                DBPoint ppp = new DBPoint(pp);
                ppp.SetDatabaseDefaults(db);

                if (a.TypeOrLayer != "") { if (lt.Has(a.TypeOrLayer)) ppp.LayerId = lt[a.TypeOrLayer]; }
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                ms.AppendEntity(ppp);
                tr.AddNewlyCreatedDBObject(ppp, true);
                writeProperties(ppp, a.parameters, tr);
                storeID(a, ppp.Id);
                tr.Commit();
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                ed.WriteMessage(e.Message);
                tr.Abort();
            }

            finally
            {
                tr.Dispose();
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

        public static void Create(Database db, Grevit.Types.Door door, Wall wall, Transaction tr, BlockTableRecord ms)
        {

            Door d = new Door();
            DictionaryDoorStyle dds = new DictionaryDoorStyle(db);
            bool newEnt = false;
            if (Command.existing_objects.ContainsKey(door.GID))
            {
                d = (Door)tr.GetObject(Command.existing_objects[door.GID], OpenMode.ForWrite);
            }
            else
            {
                d.SetDatabaseDefaults(db);
                d.SetToStandard(db);
                AnchorOpeningBaseToWall w = new AnchorOpeningBaseToWall();
                w.SetToStandard(db);
                w.SubSetDatabaseDefaults(db);
                d.SetAnchor(w);
                newEnt = true;
                w.SetSingleReference(wall.Id, Autodesk.Aec.DatabaseServices.RelationType.OwnedBy);
            }




            Point3d pkt = new Point3d(door.locationPoint.x, door.locationPoint.y + (wall.Width / 2), door.locationPoint.z);
            d.Location = pkt;

            LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (door.TypeOrLayer != "") { if (lt.Has(door.TypeOrLayer)) d.LayerId = lt[door.TypeOrLayer]; }
            if (dds.Has(door.FamilyOrStyle, tr)) d.StyleId = dds.GetAt(door.FamilyOrStyle);


            if (newEnt)
            {
                AddXData(door, d);
                ms.AppendEntity(d);
                tr.AddNewlyCreatedDBObject(d, true);
            }

            writeProperties(d, door.parameters, tr);
            storeID(door, d.Id);
        }

        static void AddRegAppTableRecord(string regAppName)
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

        static void AddXData(Grevit.Types.Component comp, Entity ent)
        {
            AddRegAppTableRecord("Grevit");
            ResultBuffer rbs = new ResultBuffer(new TypedValue(1001, "Grevit"), new TypedValue(1000, comp.GID));
            ent.XData = rbs;
            rbs.Dispose();
        }

        private static void storeID(Grevit.Types.Component c, ObjectId id)
        {
            if (c.GID != null && id != ObjectId.Null && !Command.created_objects.ContainsKey(c.GID)) Command.created_objects.Add(c.GID, id);
        }

        public static void writeProperties(DBObject dbo, List<Grevit.Types.Parameter> parameters, Transaction tr)
        {
            if (parameters != null && parameters.Count > 0)
            {
                ObjectIdCollection source_setIds = PropertyDataServices.GetPropertySets(dbo);

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

    }


}