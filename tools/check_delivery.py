"""Verify release integrity and evidence consistency; no claim of rerunning licensed CAD."""
from pathlib import Path
import csv,hashlib,json,math,re
ROOT=Path(__file__).resolve().parents[1]
checks=[]
def require(ok,label):
    if not ok:raise AssertionError(label)
    checks.append(label)
manifest=json.loads((ROOT/'manifest.json').read_text(encoding='utf-8'))
for item in manifest['files']:
    p=ROOT/item['path'];require(p.is_file(),f"exists:{item['path']}")
    require(hashlib.sha256(p.read_bytes()).hexdigest()==item['sha256'],f"hash:{item['path']}")
require(len(list((ROOT/'solidworks').glob('*.SLDPRT')))==16,'16 part files including 3 workpiece sizes')
require(len(list((ROOT/'solidworks').glob('*.SLDASM')))==3,'three static assemblies')
require(len(list((ROOT/'solidworks').glob('*.SLDDRW')))==6,'six linked drawings')
require(len(list((ROOT/'cad').glob('*.dwg')))==6,'six native DWG sheets')
rows=list(csv.DictReader((ROOT/'evidence'/'solid-interference.tsv').open(encoding='utf-8-sig'),delimiter='\t'))
require(len(rows)==11,'11 classified interference records')
require(all(r['classification']=='SIMPLIFIED_THREAD_ENVELOPE' and r['possible']=='False' for r in rows),'no unclassified or possible interference')
require(any('H06_Knob' in r['components'] and 'P06_Spindle150' in r['components'] for r in rows),'knob thread engagement present')
native=(ROOT/'evidence'/'native-validation.txt').read_text(encoding='utf-8-sig')
require('lockCount=28|featureErrors=0' in native,'28 active main mates and no feature errors')
require('uniquePartTypes=14' in native,'main assembly uses 14 model types')
range_log=(ROOT/'evidence'/'range-validation.txt').read_text(encoding='utf-8-sig')
for L in [25,100]:
    line=next((x for x in range_log.splitlines() if x.startswith(f'L={L}|')),'')
    require('unexpected=0' in line and 'knobThreadEngagement=12' in line and 'softwareGeometry=PASS' in line,f'actual endpoint assembly L{L}')
portable=(ROOT/'evidence'/'portability-reopen.txt').read_text(encoding='utf-8-sig')
require(portable.count('referencesWithinPackage=true')==3,'three assemblies reopen without external part references')
calc=json.loads((ROOT/'docs'/'calculations.json').read_text(encoding='utf-8'))
require(math.isclose(calc['jaw_bending_stress_MPa'],12,rel_tol=1e-9),'hand calculation stress=12MPa under declared assumptions')
require(math.isclose(calc['M6_slip_margin_mu0p1_min'],1,rel_tol=1e-9),'low-friction limitation is retained')
for L in range(25,101):
    x=L+50;require(65<x<165 and L-20>0 and L+60<=185 and L+210>=205,f'geometry range L={L}')
page=(ROOT/'index.html').read_text(encoding='utf-8')
for target in re.findall(r'(?:src|href)="([^"]+)"',page):
    if target.startswith(('https:','http:','#')):continue
    require((ROOT/target).exists(),f'page link:{target}')
print(json.dumps({'status':'PASS','checks':len(checks),'scope':'release integrity, formulas and recorded CAD evidence; not physical certification'},ensure_ascii=False))
