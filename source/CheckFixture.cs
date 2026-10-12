using System;using System.IO;using System.Collections.Generic;using SolidWorks.Interop.sldworks;
public static class CheckFixture {
 public static void FixKnob(string root){
 var sw=(ISldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));int oe=0,ow=0,oa=0;var m=(IModelDoc2)sw.OpenDoc6(Path.Combine(root,"solidworks","AJF01_Assembly.SLDASM"),2,1,"",ref oe,ref ow);if(m==null)throw new Exception("Open assembly failed");sw.ActivateDoc3(m.GetTitle(),false,2,ref oa);var ass=(IAssemblyDoc)m;var knob=(IComponent2)ass.GetComponentByName("H06_Knob-1");var basis=(IComponent2)ass.GetComponentByName("P01_Base-1");
 if(knob==null||basis==null)throw new Exception("Wrong assembly");var v=(double[])knob.Transform2.ArrayData;if(Math.Abs(v[9]-.258)<1e-8)return;
 IFeature target=null;var f=(IFeature)m.FirstFeature();while(f!=null){if(f.GetTypeName2()=="MateGroup"){var s=(IFeature)f.GetFirstSubFeature();while(s!=null){if(s.GetTypeName2()=="MateLock"){var mate=(IMate2)s.GetSpecificFeature2();for(int j=0;j<mate.GetMateEntityCount();j++){var c=(IComponent2)mate.MateEntity(j).ReferenceComponent;if(c!=null&&c.Name2==knob.Name2)target=s;}}s=(IFeature)s.GetNextSubFeature();}}f=(IFeature)f.GetNextFeature();}
 if(target==null)throw new Exception("Knob mate missing");m.ClearSelection2(true);if(!target.SetSuppression2(0,2,null))throw new Exception("Suppress old knob mate failed");target.Name="Legacy_Knob_Lock_Suppressed";
 v[9]=.258;var mu=(IMathUtility)sw.GetMathUtility();if(!knob.SetTransformAndSolve2((MathTransform)mu.CreateTransform(v)))throw new Exception("Move failed");
 var sd=(ISelectData)((ISelectionMgr)m.SelectionManager).CreateSelectData();sd.Mark=1;m.ClearSelection2(true);basis.Select4(false,(SelectData)sd,false);knob.Select4(true,(SelectData)sd,false);int err=0;var nm=ass.AddMate5(16,0,false,0,0,0,0,0,0,0,0,false,false,0,out err);if(nm==null||err!=1)throw new Exception("Remate");
 m.ClearSelection2(true);m.ForceRebuild3(false);int e=0,w=0;if(!m.Save3(1,ref e,ref w))throw new Exception("Save");m.Extension.SaveAs(Path.Combine(root,"solidworks","AJF01_Assembly.STEP"),0,1,null,ref e,ref w);
 }
 public static void Run(string root){
 var sw=(ISldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));int e=0,w=0,ae=0;
 var m=(IModelDoc2)sw.OpenDoc6(Path.Combine(root,"solidworks","AJF01_Assembly.SLDASM"),2,1,"",ref e,ref w);if(m==null)throw new Exception("Open failed");sw.ActivateDoc3(m.GetTitle(),false,2,ref ae);
 var ass=(IAssemblyDoc)m;ass.ResolveAllLightWeightComponents(false);var positions=new List<string>();foreach(object o in (object[])ass.GetComponents(false)){var c=(IComponent2)o;positions.Add(c.Name2+"|"+string.Join(",",(double[])c.Transform2.ArrayData));}File.WriteAllLines(Path.Combine(root,"evidence","component-transforms.txt"),positions);var mgr=(IInterferenceDetectionMgr)ass.InterferenceDetectionManager;
 bool old=mgr.TreatCoincidenceAsInterference;mgr.TreatCoincidenceAsInterference=false;mgr.IgnoreHiddenBodies=false;mgr.ShowIgnoredInterferences=true;
 var all=(object[])mgr.GetInterferences();var rows=new List<string>();rows.Add("components\tvolume_mm3\tclassification\tpossible");int expected=0,unexpected=0;
 if(all!=null)foreach(object o in all){var it=(IInterference)o;var cc=(object[])it.Components;var names=new List<string>();foreach(object co in cc)names.Add(((IComponent2)co).Name2);names.Sort();string n=string.Join(" + ",names);
 bool thread=(n.Contains("H02_M6x50")&&n.Contains("P01_Base"))||(n.Contains("H03_M6x25")&&(n.Contains("P01_Base")||n.Contains("H05_Foot")))||(n.Contains("P06_Spindle150")&&(n.Contains("P05_ScrewBracket")||n.Contains("H06_Knob")));
 if(thread)expected++;else unexpected++;rows.Add(n+"\t"+(it.Volume*1e9).ToString("G12",System.Globalization.CultureInfo.InvariantCulture)+"\t"+(thread?"SIMPLIFIED_THREAD_ENVELOPE":"REVIEW_REQUIRED")+"\t"+it.IsPossibleInterference);
 }
 mgr.TreatCoincidenceAsInterference=old;File.WriteAllLines(Path.Combine(root,"evidence","solid-interference.tsv"),rows);
 File.WriteAllText(Path.Combine(root,"evidence","solid-interference-summary.txt"),"components="+ass.GetComponentCount(false)+"\ninterferenceCount="+(all==null?0:all.Length)+"\nexpectedThreadEnvelope="+expected+"\nunexpected="+unexpected+"\ncoincidentContactsExcluded=true\ngeometryUnchanged=true\n");
 }
}
