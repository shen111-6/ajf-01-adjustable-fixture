using System;using System.IO;using System.Collections.Generic;using SolidWorks.Interop.sldworks;
public static class PortableFixture {
 public static void Run(string root,string destination){
 Directory.CreateDirectory(destination);var sw=(ISldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));var logs=new List<string>();
 foreach(string relative in new string[]{"solidworks/AJF01_Assembly.SLDASM","range/AJF01_L25.SLDASM","range/AJF01_L100.SLDASM"}){
 int e=0,w=0;var m=(IModelDoc2)sw.OpenDoc6(Path.Combine(root,relative),2,1,"",ref e,ref w);if(m==null)throw new Exception("Open pack root");var p=(IPackAndGo)m.Extension.GetPackAndGo();p.IncludeDrawings=true;p.FlattenToSingleFolder=true;if(!p.SetSaveToName(false,destination))throw new Exception("Pack folder");
 object names;p.GetDocumentNames(out names);var paths=(string[])names;var targets=new string[paths.Length];for(int i=0;i<paths.Length;i++)targets[i]=Path.Combine(destination,Path.GetFileName(paths[i]));if(!p.SetDocumentSaveToNames(targets))throw new Exception("Pack mappings");var results=(int[])m.Extension.SavePackAndGo((PackAndGo)p);if(results==null)throw new Exception("Pack result");foreach(int code in results)if(code!=0)throw new Exception("Pack error "+code);logs.Add(relative+"|PackAndGo=PASS|documents="+targets.Length);sw.CloseDoc(m.GetTitle());
 }
 File.WriteAllLines(Path.Combine(root,"evidence","portability-pack.txt"),logs);
 }
}
