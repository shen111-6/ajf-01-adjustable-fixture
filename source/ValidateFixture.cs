using System;
using System.IO;
using System.Collections.Generic;
using SolidWorks.Interop.sldworks;
public static class ValidateFixture {
 public static void Presentation(string root){
  ISldWorks sw=(ISldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));int e=0,w=0,act=0;
  var a=(IModelDoc2)sw.OpenDoc6(Path.Combine(root,"solidworks","AJF01_Assembly.SLDASM"),2,1,"",ref e,ref w);sw.ActivateDoc3(a.GetTitle(),false,2,ref act);
  var mu=(IMathUtility)sw.GetMathUtility();double q=1/Math.Sqrt(2),r=1/Math.Sqrt(6),s=1/Math.Sqrt(3);
  ((IModelView)a.ActiveView).Orientation3=(MathTransform)mu.CreateTransform(new double[]{-q,-r,s,q,-r,s,0,2*r,s,0,0,0,1,0,0,0});
  a.ViewZoomtofit2();a.NameView("AJF_Tabletop");a.Save3(1,ref e,ref w);a.Extension.SaveAs(Path.Combine(root,"evidence","solidworks-tabletop.png"),0,1,null,ref e,ref w);sw.CloseDoc(a.GetTitle());
  var d=(IModelDoc2)sw.OpenDoc6(Path.Combine(root,"solidworks","P01_Base.SLDDRW"),3,1,"",ref e,ref w);sw.ActivateDoc3(d.GetTitle(),false,2,ref act);d.ViewZoomtofit2();d.Extension.SaveAs(Path.Combine(root,"evidence","solidworks-base-drawing.png"),0,1,null,ref e,ref w);sw.CloseDoc(d.GetTitle());
 }
 public static void Run(string root){
  ISldWorks sw=(ISldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));sw.Visible=true;
  var log=new List<string>();log.Add("SW="+sw.RevisionNumber());int e=0,w=0;
  var a=(IModelDoc2)sw.OpenDoc6(Path.Combine(root,"solidworks","AJF01_Assembly.SLDASM"),2,1,"",ref e,ref w);if(a==null)throw new Exception("Assembly reopen "+e);
  int act=0;sw.ActivateDoc3(a.GetTitle(),false,2,ref act);
  var comps=(object[])((IAssemblyDoc)a).GetComponents(false);if(comps.Length!=29)throw new Exception("Assembly count");
  log.Add("ASSEMBLY_REOPEN|components=29|openErrors="+e+"|warnings="+w);int bodychecks=0;
  var seen=new HashSet<string>();
  foreach(object o in comps){var c=(IComponent2)o;string path=c.GetPathName();if(!File.Exists(path))throw new Exception("Missing ref "+path);if(!seen.Add(path))continue;
   var m=(IModelDoc2)c.GetModelDoc2();if(m==null)throw new Exception("Unresolved part");var bodies=(object[])((IPartDoc)m).GetBodies2(0,false);if(bodies==null||bodies.Length!=1)throw new Exception("Part body count");
   var box=(double[])((IBody2)bodies[0]).GetBodyBox();log.Add("PART|"+Path.GetFileName(path)+"|bodies=1|box_m="+string.Join(",",box));bodychecks++;
  }
  int locks=0,errors=0;var f=(IFeature)a.FirstFeature();while(f!=null){if(f.GetTypeName2()=="MateGroup"){var sub=(IFeature)f.GetFirstSubFeature();while(sub!=null){if(sub.GetTypeName2()=="MateLock"){bool suppressed=sub.IsSuppressed();if(!suppressed)locks++;bool warning=false;int code=sub.GetErrorCode2(out warning);if(code!=0&&!warning&&!suppressed)errors++;log.Add("MATE|"+sub.Name+"|error="+code+"|warning="+warning+"|suppressed="+suppressed);}sub=(IFeature)sub.GetNextSubFeature();}}f=(IFeature)f.GetNextFeature();}
  if(locks!=28||errors!=0)throw new Exception("Mate check "+locks+" "+errors);log.Add("MATES|lockCount=28|featureErrors=0");
  // Present XY base as a horizontal tabletop in a named model view; only view metadata changes.
  var mu=(IMathUtility)sw.GetMathUtility();double q=1/Math.Sqrt(2),r=1/Math.Sqrt(6),s=1/Math.Sqrt(3);
  ((IModelView)a.ActiveView).Orientation3=(MathTransform)mu.CreateTransform(new double[]{-q,-r,s,q,-r,s,0,2*r,s,0,0,0,1,0,0,0});
  a.ViewZoomtofit2();a.NameView("AJF_Tabletop");a.Save3(1,ref e,ref w);
  if(!a.Extension.SaveAs(Path.Combine(root,"evidence","solidworks-tabletop.png"),0,1,null,ref e,ref w))throw new Exception("Image export");
  sw.CloseDoc(a.GetTitle());
  foreach(string file in Directory.GetFiles(Path.Combine(root,"solidworks"),"*.SLDDRW")){
   var m=(IModelDoc2)sw.OpenDoc6(file,3,1,"",ref e,ref w);if(m==null)throw new Exception("Drawing reopen");
   var d=(IDrawingDoc)m;var view=(IView)d.GetFirstView();int count=0;view=(IView)view.GetNextView();while(view!=null){string model=view.GetReferencedModelName();if(!File.Exists(model))throw new Exception("Drawing reference");count++;view=(IView)view.GetNextView();}
   if(count!=4)throw new Exception("Drawing views "+count);log.Add("DRAWING_REOPEN|"+Path.GetFileName(file)+"|modelViews=4|referencesExist=PASS");
   m.ViewZoomtofit2();if(Path.GetFileName(file)=="P01_Base.SLDDRW")m.Extension.SaveAs(Path.Combine(root,"evidence","solidworks-base-drawing.png"),0,1,null,ref e,ref w);
   sw.CloseDoc(m.GetTitle());
  }
  log.Add("RESULT|PASS|uniquePartTypes="+bodychecks+"|samples=1|motionSimulation=NOT_RUN|FEA=NOT_RUN|physicalTest=NOT_RUN|fullSolidInterference=NOT_RUN");
  File.WriteAllLines(Path.Combine(root,"evidence","native-validation.txt"),log);
 }
}
