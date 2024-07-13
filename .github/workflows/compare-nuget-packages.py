#!/usr/bin/env python3
import hashlib
import os
import sys

from pathlib import Path
from zipfile import ZipFile, ZipInfo

import gha
import nuget

# Symbol packages will change even for changes that we don't care about because the deterministic hash embedded in the PDB
# is affected by the MVID of a package's dependencies. We don't want to release a new package when the only things that
# changed were external to the package, so we don't check them.
CHECK_SYMBOL_PACKAGES = False

# The following packages will always release no matter what
always_release_packages = set([
    'Bonsai',
    'Bonsai.Core',
    'Bonsai.Design',
    'Bonsai.Editor',
    'Bonsai.Player',
])

if len(sys.argv) != 5:
    gha.print_error('Usage: compare-nuget-packages.py <previous-dummy-packages-path> <next-dummy-packages-path> <release-packages-path> <release-manifest-path>')
    sys.exit(1)
else:
    previous_packages_path = Path(sys.argv[1])
    next_packages_path = Path(sys.argv[2])
    release_packages_path = Path(sys.argv[3])
    release_manifest_path = Path(sys.argv[4])

if not previous_packages_path.exists():
    gha.print_error(f"Previous packages path '{previous_packages_path}' does not exist.")
if not next_packages_path.exists():
    gha.print_error(f"Next packages path '{next_packages_path}' does not exist.")
if not release_packages_path.exists():
    gha.print_error(f"Release packages path '{previous_packages_path}' does not exist.")
if release_manifest_path.exists():
    gha.print_error(f"Release manifest '{release_manifest_path}' already exists.")
gha.fail_if_errors()

def verbose_log(message: str):
    gha.print_debug(message)

def should_ignore(file: ZipInfo) -> bool:
    # Ignore metadata files which change on every pack
    if file.filename == '_rels/.rels':
        return True
    if file.filename.startswith('package/services/metadata/core-properties/') and file.filename.endswith('.psmdcp'):
        return True
    
    # Don't care about explicit directories
    if file.is_dir():
        return True
    
    return False

def nuget_packages_are_equivalent(a_path: Path, b_path: Path, is_snupkg: bool = False) -> bool:
    verbose_log(f"Comparing '{a_path}' and '{b_path}'")

    # One package exists and the other does not
    if a_path.exists() != b_path.exists():
        verbose_log(f"Not equivalent: Only one package actually exists")
        return False
    
    # The package doesn't exist at all, assume mistake unless we're checking the optional symbol packages
    if not a_path.exists():
        if is_snupkg:
            verbose_log("Equivalent: Neither package exists")
            return True
        raise FileNotFoundError(f"Neither package exists: '{a_path}' or '{b_path}'")
    
    # From this point on: Check everything and emit messages for debugging purposes
    is_equivalent = True

    # Check if corresponding symbol packages are equivalent
    if CHECK_SYMBOL_PACKAGES and not is_snupkg:
        if not nuget_packages_are_equivalent(a_path.with_suffix(".snupkg"), b_path.with_suffix(".snupkg"), True):
            verbose_log("Not equivalent: Symbol packages are not equivalent")
            is_equivalent = False
        else:
            verbose_log("Symbol packages are equivalent")

    # Compare the contents of the packages
    # NuGet package packing is unfortunately not fully deterministic so we cannot compare the packages directly
    # https://github.com/NuGet/Home/issues/8601
    with ZipFile(a_path, 'r') as a_zip, ZipFile(b_path, 'r') as b_zip:
        b_infos = { }
        for b_info in b_zip.infolist():
            if should_ignore(b_info):
                continue
            assert b_info.filename not in b_infos
            b_infos[b_info.filename] = b_info

        for a_info in a_zip.infolist():
            if should_ignore(a_info):
                continue

            b_info = b_infos.pop(a_info.filename, None)
            if b_info is None:
                verbose_log(f"Not equivalent: '{a_info.filename}' exists in '{a_path}' but not in '{b_path}'")
                is_equivalent = False
                continue

            if a_info.CRC != b_info.CRC:
                verbose_log(f"Not equivalent: CRCs of '{a_info.filename}' do not match between '{a_path}' and '{b_path}'")
                is_equivalent = False
                continue
            
            if a_info.file_size != b_info.file_size:
                verbose_log(f"Not equivalent: File sizes of '{a_info.filename}' do not match between '{a_path}' and '{b_path}'")
                is_equivalent = False
                continue

            a_hash = hashlib.file_digest(a_zip.open(a_info), 'sha256').hexdigest() # type: ignore
            b_hash = hashlib.file_digest(b_zip.open(b_info), 'sha256').hexdigest() # type: ignore
            if a_hash != b_hash:
                verbose_log(f"Not equivalent: SHA256 hashes of '{a_info.filename}' do not match between '{a_path}' and '{b_path}'")
                is_equivalent = False
                continue

        # Ensure every file in B was processed
        if len(b_infos) > 0:
            is_equivalent = False
            verbose_log(f"Not equivalent: The following file(s) exist in '{a_path}' but not in '{b_path}'")
            for filename in b_infos:
                verbose_log(f"  '{filename}'")

    return is_equivalent

different_packages = []
force_released_packages = []
next_packages = set()
for file in os.listdir(next_packages_path):
    if not file.endswith(".nupkg"):
        continue

    # We don't tolerate build metadata here because the nuget_packages_are_equivalent call doesn't either
    if not file.endswith(".99.99.99.nupkg"):
        gha.print_error(f"Package '{file}' does not have a dummy version.")

    package_name = nuget.get_package_name(file)
    next_packages.add(package_name)

    if not nuget_packages_are_equivalent(next_packages_path / file, previous_packages_path / file):
        verbose_log(f"'{file}' differs")
        different_packages.append(package_name)
    elif package_name in always_release_packages:
        force_released_packages.append(package_name)

previous_packages = set()
for file in os.listdir(previous_packages_path):
    if file.endswith(".nupkg"):
        previous_packages.add(nuget.get_package_name(file))

release_packages = set()
for file in os.listdir(release_packages_path):
    if file.endswith(".nupkg"):
        release_packages.add(nuget.get_package_name(file))

with gha.JobSummary() as md:
    def write_both(line: str = ''):
        print(line)
        md.write_line(line)
    
    print()
    different_packages.sort()
    md.write_line("# Packages with changes\n")
    if len(different_packages) == 0:
        print("There are no packages with any changes.")
        md.write_line("*There are no packages with any changes.*")
    else:
        print("The following packages have changes:")
        for package in different_packages:
            print(f"  {package}")
            md.write_line(f"* {package}")

    if len(force_released_packages) > 0:
        write_both()
        write_both("The following packages are configured to release anyway despite not being changed:")
        md.write_line()
        force_released_packages.sort()
        for package in force_released_packages:
            print(f"  {package}")
            md.write_line(f"* {package}")

        different_packages += force_released_packages
        different_packages.sort()

    # Ensure the next dummy reference and release package sets contain the same packages
    def list_missing_peers(heading: str, md_heading: str, packages: set[str]) -> bool:
        if len(packages) == 0:
            return False

        sorted_packages = list(packages)
        sorted_packages.sort()

        print()
        print(heading)
        md.write_line(f"# {md_heading}")
        md.write_line()
        md.write_line(heading)
        md.write_line()
        for package in sorted_packages:
            print(f"  {package}")
            md.write_line(f"* {package}")
        return True

    list_missing_peers("The following packages are new for this release:", "New packages", next_packages - previous_packages)
    list_missing_peers("The following packages were removed during this release:", "Removed packages", previous_packages - next_packages)

    if list_missing_peers("The following packages exist in the release package artifact, but not in the next dummy reference artifact:", "⚠ Missing reference packages", release_packages - next_packages):
        gha.print_error("Some packages exist in the release package artifact, but not in the next dummy reference artifact.")
    if list_missing_peers("The following packages exist in the next dummy reference artifact, but not in the release package artifact:", "⚠ Missing release packages", next_packages - release_packages):
        gha.print_error("Some packages exist in the next dummy reference artifact, but not in the release package artifact.")
    if list_missing_peers("The following packages are marked to always release but do not exist:", "⚠ Missing always-release packages", always_release_packages - release_packages):
        gha.print_error("Some packages exist in the always-release list, but not in the release package artifact.")

with open(release_manifest_path, 'x') as manifest:
    for package in different_packages:
        manifest.write(f"{package}\n")

gha.fail_if_errors()
