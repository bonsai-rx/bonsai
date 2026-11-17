#!/usr/bin/env python3
import hashlib
import os
import re
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path

BOOTSTRAPPER = 'Bonsai'
EXECUTABLE = f'{BOOTSTRAPPER}.exe'
FRAMEWORK = 'net48'
SETUP = f'{BOOTSTRAPPER}.Setup'

if len(sys.argv) != 2:
    sys.exit('Usage: verify-bootstrapper.py <release|debug>')

configuration = sys.argv[1].lower()
check_signature = os.getenv('IS_FULL_RELEASE') == 'true'

repacked_path = Path(f'artifacts/bin/{BOOTSTRAPPER}/{configuration}-repacked/{EXECUTABLE}')
package_path = next(path for path in Path(f'artifacts/package/{configuration}').glob(f'{BOOTSTRAPPER}.*.nupkg') if re.match(rf'{BOOTSTRAPPER}\.\d', path.name))
zip_path = Path(f'artifacts/{BOOTSTRAPPER}.zip')
msi_path = Path(f'artifacts/bin/{SETUP}/{configuration}-x86/{SETUP}.msi')

def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def extract(archive_path, entry, folder):
    folder.mkdir()
    target = folder / EXECUTABLE
    with zipfile.ZipFile(archive_path) as archive:
        target.write_bytes(archive.read(entry))
    return target

def signature_status(path):
    command = f"(Get-AuthenticodeSignature -LiteralPath '{path}').Status"
    return subprocess.run(['pwsh', '-NoProfile', '-Command', command], capture_output=True, text=True).stdout.strip()

failures = []
expected = sha256(repacked_path)
with tempfile.TemporaryDirectory() as temp:
    temp = Path(temp)
    msi_folder = temp / 'msi'
    subprocess.run(['msiexec', '/a', str(msi_path.resolve()), '/qn', f'TARGETDIR={msi_folder}'], check=True)
    copies = {
        'package': extract(package_path, f'lib/{FRAMEWORK}/{EXECUTABLE}', temp / 'package'),
        'portable zip': extract(zip_path, EXECUTABLE, temp / 'zip'),
        'installer': next(msi_folder.rglob(EXECUTABLE)),
    }

    for name, path in copies.items():
        if sha256(path) != expected:
            failures.append(f'The bootstrapper in the {name} does not match the repacked bootstrapper.')
        if check_signature and signature_status(path) != 'Valid':
            failures.append(f'The bootstrapper in the {name} is not signed.')

    run_folder = temp / 'run'
    run_folder.mkdir()
    try:
        result = subprocess.run([str(copies['package']), '--info'], cwd=run_folder, capture_output=True, text=True, timeout=30)
        if result.returncode != 0:
            failures.append(f'The bootstrapper in the package does not start on its own:\n{result.stderr}')
    except subprocess.TimeoutExpired as error:
        failures.append(f'The bootstrapper in the package did not exit within {error.timeout} seconds.')

for failure in failures:
    print(failure, file=sys.stderr)
sys.exit(1 if failures else 0)
