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
namespace Grevit.AutoCad
{
    public static class CreateExtension
    {
        public static DBObject Create(this Grevit.Types.DrawingPoint a, Transaction tr)
        {
            try
            {
                LayerTable lt = (LayerTable)tr.GetObject(Command.Database.LayerTableId, OpenMode.ForRead);
                Point3d pp = a.point.ToPoint3d();
                DBPoint ppp = new DBPoint(pp);
                ppp.SetDatabaseDefaults(Command.Database);

                if (a.TypeOrLayer != "") { if (lt.Has(a.TypeOrLayer)) ppp.LayerId = lt[a.TypeOrLayer]; }
                BlockTable bt = (BlockTable)tr.GetObject(Command.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                ms.AppendEntity(ppp);
                tr.AddNewlyCreatedDBObject(ppp, true);
                return ppp;
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
            }


            return null;

        }

        public static DBObject Create(this Grevit.Types.Door door, Transaction tr, Wall wall)
        {
            BlockTable bt = (BlockTable)tr.GetObject(Command.Database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            Door d = new Door();
            DictionaryDoorStyle dds = new DictionaryDoorStyle(Command.Database);
            bool newEnt = false;
            if (Command.existing_objects.ContainsKey(door.GID))
            {
                d = (Door)tr.GetObject(Command.existing_objects[door.GID], OpenMode.ForWrite);
            }
            else
            {
                d.SetDatabaseDefaults(Command.Database);
                d.SetToStandard(Command.Database);
                AnchorOpeningBaseToWall w = new AnchorOpeningBaseToWall();
                w.SetToStandard(Command.Database);
                w.SubSetDatabaseDefaults(Command.Database);
                d.SetAnchor(w);
                newEnt = true;
                w.SetSingleReference(wall.Id, Autodesk.Aec.DatabaseServices.RelationType.OwnedBy);
            }




            Point3d pkt = new Point3d(door.locationPoint.x, door.locationPoint.y + (wall.Width / 2), door.locationPoint.z);
            d.Location = pkt;

            LayerTable lt = (LayerTable)tr.GetObject(Command.Database.LayerTableId, OpenMode.ForRead);
            if (door.TypeOrLayer != "") { if (lt.Has(door.TypeOrLayer)) d.LayerId = lt[door.TypeOrLayer]; }
            if (dds.Has(door.FamilyOrStyle, tr)) d.StyleId = dds.GetAt(door.FamilyOrStyle);


            if (newEnt)
            {
                ms.AppendEntity(d);
                tr.AddNewlyCreatedDBObject(d, true);
                ms.Dispose();
            }

            return d;
        }

        public static DBObject Create(this Grevit.Types.Wall w, Transaction tr, Grevit.Types.Point from = null, Grevit.Types.Point to = null)
        {
            DictionaryWallStyle ws = new DictionaryWallStyle(Command.Database);
            try
            {
                if (from == null && to == null && w.curve.GetType() == typeof(Grevit.Types.PLine))
                {
                    Grevit.Types.PLine pline = (Grevit.Types.PLine)w.curve;
                    for (int i = 0; i < pline.points.Count; i++)
                    {
                        if (i == pline.points.Count - 1)
                        {
                            if (pline.closed)
                            {
                                w.Create(tr, pline.points[i], pline.points[0]);
                            }
                        }
                        else
                        {
                            w.Create(tr, pline.points[i], pline.points[i + 1]);
                        }
                    }
                }
                else
                {


                    Wall wall = new Wall();
                    LayerTable lt = (LayerTable)tr.GetObject(Command.Database.LayerTableId, OpenMode.ForRead);

                    bool newEnt = false;

                    if (Command.existing_objects.ContainsKey(w.GID)) wall = (Wall)tr.GetObject(Command.existing_objects[w.GID], OpenMode.ForWrite);
                    else
                    {
                        wall.SetDatabaseDefaults(Command.Database);
                        wall.SetToStandard(Command.Database);
                        newEnt = true;
                        wall.JustificationType = WallJustificationType.Center;
                    }

                    if (w.TypeOrLayer != "") { if (lt.Has(w.TypeOrLayer)) wall.LayerId = lt[w.TypeOrLayer]; }
                    if (ws.Has(w.FamilyOrStyle, tr)) wall.StyleId = ws.GetAt(w.FamilyOrStyle);



                    if (from != null && to != null)
                    {
                        wall.Set(from.ToPoint3d(), to.ToPoint3d(), Vector3d.ZAxis);
                    }
                    else
                    {
                        if (w.curve.GetType() == typeof(Grevit.Types.Line))
                        {
                            Grevit.Types.Line baseline = (Grevit.Types.Line)w.curve;
                            wall.Set(baseline.from.ToPoint3d(), baseline.to.ToPoint3d(), Vector3d.ZAxis);
                        }
                        else if (w.curve.GetType() == typeof(Grevit.Types.Arc))
                        {
                            Grevit.Types.Arc baseline = (Grevit.Types.Arc)w.curve;
                            CircularArc3d arc = new CircularArc3d(baseline.center.ToPoint3d(), Vector3d.ZAxis, Vector3d.ZAxis, baseline.radius, baseline.start, baseline.end);
                            wall.Set(arc, Vector3d.ZAxis);
                        }
                        else if (w.curve.GetType() == typeof(Grevit.Types.Curve3Points))
                        {
                            Grevit.Types.Curve3Points baseline = (Grevit.Types.Curve3Points)w.curve;
                            wall.Set(baseline.a.ToPoint3d(), baseline.b.ToPoint3d(), baseline.c.ToPoint3d(), Vector3d.ZAxis);
                        }
                    }


                    wall.BaseHeight = w.height;

                    if (newEnt)
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(Command.Database.BlockTableId, OpenMode.ForRead);
                        BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                        ms.AppendEntity(wall);
                        tr.AddNewlyCreatedDBObject(wall, true);

                    }

                    return wall;
                }
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {

            }

            return null;

        }


        public static DBObject Create(this Grevit.Types.Line l, Transaction tr)
        {
            LayerTable lt = (LayerTable)tr.GetObject(Command.Database.LayerTableId, OpenMode.ForRead);

            try
            {
                Line line = new Line(l.from.ToPoint3d(), l.to.ToPoint3d());


                line.SetDatabaseDefaults(Command.Database);

                BlockTable bt = (BlockTable)tr.GetObject(Command.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                
                if (lt.Has(l.TypeOrLayer)) line.LayerId = lt[l.TypeOrLayer];
                ms.AppendEntity(line);
                tr.AddNewlyCreatedDBObject(line, true);
                return line;
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {

            }

            return null;

        }

        public static DBObject Create(this Grevit.Types.Spline s, Transaction tr)
        {
            LayerTable lt = (LayerTable)tr.GetObject(Command.Database.LayerTableId, OpenMode.ForRead);

            try
            {
                Point3dCollection points = new Point3dCollection();
                foreach (Grevit.Types.Point p in s.controlPoints) points.Add(p.ToPoint3d());
                DoubleCollection dc = new DoubleCollection();
                foreach (double dbl in s.weights) dc.Add(dbl);
                DoubleCollection dcc = new DoubleCollection();
                foreach (double dbl in s.knots) dcc.Add(dbl);
                Spline sp = new Spline(s.degree, s.isRational, s.isClosed, s.isPeriodic, points, dcc, dc, 0, 0);
                sp.SetDatabaseDefaults(Command.Database);

                BlockTable bt = (BlockTable)tr.GetObject(Command.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                if (lt.Has(s.TypeOrLayer)) sp.LayerId = lt[s.TypeOrLayer];
                ms.AppendEntity(sp);
                tr.AddNewlyCreatedDBObject(sp, true);
                ms.Dispose();
                return sp;
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
            }

            return null;

        }

        public static DBObject Create(this Grevit.Types.Arc a, Transaction tr)
        {

            try
            {
                LayerTable lt = (LayerTable)tr.GetObject(Command.Database.LayerTableId, OpenMode.ForRead);
                Arc arc = new Arc(a.center.ToPoint3d(), a.radius, a.start, a.end);

                arc.SetDatabaseDefaults(Command.Database);
                if (lt.Has(a.TypeOrLayer)) arc.LayerId = lt[a.TypeOrLayer];

                BlockTable bt = (BlockTable)tr.GetObject(Command.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                ms.AppendEntity(arc);
                tr.AddNewlyCreatedDBObject(arc, true);
                ms.Dispose();
                return arc;
             }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
            }

            return null;
        }

        public static DBObject Create(this Grevit.Types.PLine a, Transaction tr)
        {
            try
            {
                LayerTable lt = (LayerTable)tr.GetObject(Command.Database.LayerTableId, OpenMode.ForRead);
                Point3dCollection pc = new Point3dCollection();
                foreach (Grevit.Types.Point p in a.points) pc.Add(p.ToPoint3d());
                Polyline3d pp = new Polyline3d(Poly3dType.SimplePoly, pc, a.closed);
                pp.SetDatabaseDefaults(Command.Database);
                if (lt.Has(a.TypeOrLayer)) pp.LayerId = lt[a.TypeOrLayer];

                BlockTable bt = (BlockTable)tr.GetObject(Command.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                ms.AppendEntity(pp);
                tr.AddNewlyCreatedDBObject(pp, true);
                return pp;
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
            }

            return null;
        }

        public static DBObject Create(this Grevit.Types.Room r, Transaction tr)
        {
            DictionarySpaceStyle ss = new DictionarySpaceStyle(Command.Database);



            try
            {
                BlockTable bt = (BlockTable)tr.GetObject(Command.Database.BlockTableId, OpenMode.ForRead);
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


                Autodesk.Aec.Geometry.Profile myProfile = Autodesk.Aec.Geometry.Profile.CreateFromEntity(acPoly, Command.Editor.CurrentUserCoordinateSystem);

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
                    space.SetDatabaseDefaults(Command.Database);
                    space.SetToStandard(Command.Database);
                }

                space.Associative = r.associative;
                space.Name = r.name;

                space.GeometryType = SpaceGeometryType.TwoD;
                space.Location = new Point3d(0, 0, 0);
                space.SetBaseProfile(myProfile, Matrix3d.Identity);

                LayerTable lt = (LayerTable)tr.GetObject(Command.Database.LayerTableId, OpenMode.ForRead);
                if (r.TypeOrLayer != "") { if (lt.Has(r.TypeOrLayer)) space.LayerId = lt[r.TypeOrLayer]; }
                if (ss.Has(r.FamilyOrStyle, tr)) space.StyleId = ss.GetAt(r.FamilyOrStyle);

                if (newEnt)
                {
                    ms.AppendEntity(space);
                    tr.AddNewlyCreatedDBObject(space, true);
                    ms.Dispose();
                }
                return space;

            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
            }

            return null;
        }

        public static DBObject Create(this Grevit.Types.Slab s, Transaction tr)
        {
            try
            {
                BlockTable bt = (BlockTable)tr.GetObject(Command.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                // Polyline acPoly = new Polyline();
                //acPoly.SetDatabaseDefaults();

                //int i = 0;

                DBObjectCollection objColl = new DBObjectCollection();
                Point3dCollection ptcol = new Point3dCollection();
                foreach (Grevit.Types.Loop loop in s.surface.profile)
                {
                    foreach (Grevit.Types.Component p in loop.outline)
                        ptcol = p.To3dPointCollection();


                    //Curve3dCollection collt = parse3dCurve(p);
                    //acPoly.AppendVertex(new PolylineVertex3d(new Point3d(p.x, p.y, p.z)));
                    //acPoly.AddVertexAt(i, new Point2d(p.x, p.y), 0, 0, 0);
                    //i++;

                    //foreach (Curve3d curve in collt)
                    //{ 
                    //    objColl.Add(Autodesk.AutoCAD.DatabaseServices.Curve.CreateFromGeCurve(curve));
                    //}
                }

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


                LayerTable lt = (LayerTable)tr.GetObject(Command.Database.LayerTableId, OpenMode.ForRead);
                if (s.TypeOrLayer != "") { if (lt.Has(s.TypeOrLayer)) solid.LayerId = lt[s.TypeOrLayer]; }
                //if (ss.Has(r.family, tr)) slab.StyleId = ss.GetAt(r.family);


                ms.AppendEntity(solid);
                tr.AddNewlyCreatedDBObject(solid, true);
                return solid;
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
            }

            return null;
        }

        public static DBObject Create(this Grevit.Types.Column c, Transaction tr)
        {
            DictionaryMemberStyle mst = new DictionaryMemberStyle(Command.Database);

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

                    member.SetDatabaseDefaults(Command.Database);
                    member.SetToStandard(Command.Database);


                    BlockTable bt = (BlockTable)tr.GetObject(Command.Database.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    ms.AppendEntity(member);
                    tr.AddNewlyCreatedDBObject(member, true);
                }

                LayerTable lt = (LayerTable)tr.GetObject(Command.Database.LayerTableId, OpenMode.ForRead);
                if (c.TypeOrLayer != "") { if (lt.Has(c.TypeOrLayer)) member.LayerId = lt[c.TypeOrLayer]; }
                if (mst.Has(c.FamilyOrStyle, tr)) member.StyleId = mst.GetAt(c.FamilyOrStyle);
                member.Set(c.location.ToPoint3d(), c.locationTop.ToPoint3d());

                return member;
            }

            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
            }


            return null;
        }




    }
}
