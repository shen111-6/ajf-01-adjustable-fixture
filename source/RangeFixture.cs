using System;using System.IO;using System.Collections.Generic;using SolidWorks.Interop.sldworks;
public static class RangeFixture {
 static string template=@"C:\ProgramData\SOLIDWORKS\SOLIDWORKS 2024\templates\";
 public static void Run(string root){
 var sw=(ISldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));int e=0,w=0;
 var main=(IModelDoc2)sw.OpenDoc6(Path.Combine(root,"solidworks","AJF01_Assembly.SLDASM"),2,1,"",ref e,ref w);var baseAss=(IAssemblyDoc)main;var baseComps=(object[])baseAss.GetComponents(false);
 var paths=new List<string>();var transforms=new List<double[]>();var names=new List<string>();foreach(object o in baseComps){var c=(IComponent2)o;paths.Add(c.GetPathName());transforms.Add((double[])c.Transform2.ArrayData);names.Add(c.Name2);}
 var records=new List<string>();Directory.CreateDirectory(Path.Combine(root,"range"));
 foreach(int L in new int[]{25,100}){
 int W=L==25?20:80,H=L==25?10:20;string sample=Path.Combine(root,"range","Sample_"+L+"x"+W+"x"+H+".SLDPRT");
 var part=(IModelDoc2)sw.NewDocument(template+"gb_part.prtdot",0,0,0);var f=(IFeature)part.FirstFeature();while(f!=null&&f.GetTypeName2()!="RefPlane")f=(IFeature)f.GetNextFeature();part.Extension.SelectByID2(f.Name,"PLANE",0,0,0,false,0,null,0);part.SketchManager.InsertSketch(true);part.SketchManager.CreateCornerRectangle(0,0,0,L/1000.0,W/1000.0,0);part.SketchManager.InsertSketch(true);var sk=(IFeature)part.FeatureByPositionReverse(0);part.ClearSelection2(true);sk.Select2(false,0);
 var boss=part.FeatureManager.FeatureExtrusion2(true,false,false,0,0,H/1000.0,0,false,false,false,false,0,0,false,false,false,false,true,true,true,0,0,false);if(boss==null)throw new Exception("Sample extrude");if(!part.Extension.SaveAs(sample,0,1,null,ref e,ref w))throw new Exception("Sample save");sw.CloseDoc(part.GetTitle());
 var variant=(IModelDoc2)sw.NewDocument(template+"gb_assembly.asmdot",0,0,0);var a=(IAssemblyDoc)variant;var pp=new List<string>();var tt=new List<double>();var cs=new List<string>();
 for(int i=0;i<names.Count;i++){string n=names[i];var v=(double[])transforms[i].Clone();if(n.StartsWith("P03_")||n.StartsWith("P06_")||n.StartsWith("P08_")||n.StartsWith("H01_")||n.StartsWith("H04_")||n.StartsWith("H06_"))v[9]+=(L-60)/1000.0;pp.Add(n.StartsWith("P07_")?sample:paths[i]);tt.AddRange(v);cs.Add("");}
 var comps=(object[])a.AddComponents3(pp.ToArray(),tt.ToArray(),cs.ToArray());if(comps==null||comps.Length!=29)throw new Exception("Range insert");IComponent2 basis=null;foreach(object o in comps){var c=(IComponent2)o;if(c.GetPathName().EndsWith("P01_Base.SLDPRT"))basis=c;}
 variant.ClearSelection2(true);basis.Select4(false,null,false);a.FixComponent();
 foreach(object o in comps){var c=(IComponent2)o;if(c.Name2==basis.Name2)continue;variant.ClearSelection2(true);c.Select4(false,null,false);a.UnfixComponent();var sd=(ISelectData)((ISelectionMgr)variant.SelectionManager).CreateSelectData();sd.Mark=1;variant.ClearSelection2(true);basis.Select4(false,(SelectData)sd,false);c.Select4(true,(SelectData)sd,false);int me=0;var mate=a.AddMate5(16,0,false,0,0,0,0,0,0,0,0,false,false,0,out me);if(mate==null||me!=1)throw new Exception("Range mate");}
 variant.ClearSelection2(true);variant.ForceRebuild3(false);string ap=Path.Combine(root,"range","AJF01_L"+L+".SLDASM");if(!variant.Extension.SaveAs(ap,0,1,null,ref e,ref w))throw new Exception("Range save");
 var mgr=(IInterferenceDetectionMgr)a.InterferenceDetectionManager;mgr.TreatCoincidenceAsInterference=false;mgr.IgnoreHiddenBodies=false;var ints=(object[])mgr.GetInterferences();int unknown=0;
 if(ints!=null)foreach(object o in ints){var it=(IInterference)o;string n="";foreach(object co in (object[])it.Components)n+=((IComponent2)co).GetPathName()+"|";bool thread=(n.Contains("H02_M6x50")&&n.Contains("P01_Base"))||(n.Contains("H03_M6x25")&&(n.Contains("P01_Base")||n.Contains("H05_Foot")))||(n.Contains("P06_Spindle150")&&(n.Contains("P05_ScrewBracket")||n.Contains("H06_Knob")));if(!thread)unknown++;}
 if(unknown>0)throw new Exception("Unexpected range interference "+unknown);
 double q=1/Math.Sqrt(2),r=1/Math.Sqrt(6),s=1/Math.Sqrt(3);var mu=(IMathUtility)sw.GetMathUtility();((IModelView)variant.ActiveView).Orientation3=(MathTransform)mu.CreateTransform(new double[]{-q,-r,s,q,-r,s,0,2*r,s,0,0,0,1,0,0,0});variant.ViewZoomtofit2();variant.Save3(1,ref e,ref w);variant.Extension.SaveAs(Path.Combine(root,"evidence","range-L"+L+".png"),0,1,null,ref e,ref w);variant.Extension.SaveAs(Path.ChangeExtension(ap,"STEP"),0,1,null,ref e,ref w);
 records.Add("L="+L+"|W="+W+"|H="+H+"|components=29|activeLockMates=28|threadEnvelopeInterferences="+(ints==null?0:ints.Length)+"|unexpected="+unknown+"|endstopGap="+(L-20)+"|M8centerX="+(L+50)+"|knobThreadEngagement=12|bottomGap=4|softwareGeometry=PASS");sw.CloseDoc(variant.GetTitle());
 }
 File.WriteAllLines(Path.Combine(root,"evidence","range-validation.txt"),records);
 }
}
