import json,re,sys
from pathlib import Path
import dnfile
raw=Path(sys.argv[1]);loc=Path(sys.argv[2]);diff=loc.with_suffix('.localization.diff.json')
def info(path):
 pe=dnfile.dnPE(str(path));asm=pe.net.mdtables.Assembly.rows[0];version=f'{asm.MajorVersion}.{asm.MinorVersion}.{asm.BuildNumber}.{asm.RevisionNumber}';types=[];methods=[]
 for row in pe.net.mdtables.TypeDef.rows:
  ns=str(row.TypeNamespace or '');name=str(row.TypeName or '');full=f'{ns}.{name}' if ns else name;types.append(full)
  for idx in row.MethodList:methods.append((full,str(idx.row.Name)))
 return version,set(types),methods
rv,rt,rm=info(raw);lv,lt,lm=info(loc)
required={'CompleteCheatMenu.Targeting.ProjectileGuidance','CompleteCheatMenu.Targeting.ProjectileGuidanceMath','CompleteCheatMenu.Patches.ProjectileUpdateScan_Patch','CompleteCheatMenu.Patches.ProjectileRemove_Patch'}
required_methods={('CompleteCheatMenu.Targeting.ProjectileGuidance','Step'),('CompleteCheatMenu.Targeting.ProjectileGuidance','Forget'),('CompleteCheatMenu.Targeting.ProjectileGuidance','Clear'),('CompleteCheatMenu.Targeting.ProjectileGuidanceMath','TrySteer'),('CompleteCheatMenu.Targeting.BallisticPredictor','PredictProjectile'),('CompleteCheatMenu.Patches.ProjectileUpdateScan_Patch','Prefix'),('CompleteCheatMenu.Patches.ProjectileRemove_Patch','Postfix')}
data=json.loads(diff.read_text(encoding='utf-8'));rx=re.compile(r'\{[^{}]+\}');mismatch=[c['original'] for c in data['changed'] if rx.findall(c['original'])!=rx.findall(c['translated'])]
checks={'RAW_VERSION':rv,'LOCALIZED_VERSION':lv,'TYPESET_EQUAL':rt==lt,'METHODSET_EQUAL':rm==lm,'REQUIRED_TYPES_PRESENT':required<=lt,'REQUIRED_METHODS_PRESENT':required_methods<=set(lm),'REQUIRED_TYPES':sorted(required&lt),'LOCALIZATION_CHANGED':len(data['changed']),'PLACEHOLDER_MISMATCHES':len(mismatch)}
for k,v in checks.items():print(f'{k}={v}')
ok=rv==lv=='0.7.0.0' and rt==lt and rm==lm and required<=lt and required_methods<=set(lm) and not mismatch
print('TASK17_4_METADATA='+('PASS' if ok else 'FAIL'));raise SystemExit(0 if ok else 1)
