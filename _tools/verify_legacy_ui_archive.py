"""구형 UI 원본 보존과 런타임 참조 부재를 검사한다."""
import hashlib
from pathlib import Path

root = Path(__file__).resolve().parent.parent
archive = root / 'Documents/Legacy/UI.prefab'
assert hashlib.sha256(archive.read_bytes()).hexdigest() == 'a55c9d923b36981c4ba71151d6737219297c6d6cdc5500dfbda1dc54926fdc91'
assert hashlib.sha256(archive.with_suffix('.prefab.meta').read_bytes()).hexdigest() == '3d6c81405a882bbd7679a25a627ba6c990f339685491d2cafe42673144a99e44'
assert not (root/'Assets/Test/UI.prefab').exists()
guid = b'b3aaacbb493762e429e89817a55fc3cc'
for path in (root/'Assets').rglob('*'):
    if path.suffix in {'.unity', '.prefab', '.asset', '.cs'}:
        assert guid not in path.read_bytes(), 'Legacy reference: ' + str(path)
print('PASS: legacy UI and meta preserved exactly; no runtime references')
