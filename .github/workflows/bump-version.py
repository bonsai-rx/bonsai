#!/usr/bin/env python3
import os

import gha
import nuget

#==================================================================================================
# Get inputs
#==================================================================================================
def get_environment_variable(name):
    ret = os.getenv(name)

    if ret is None or ret == '':
        gha.print_error(f"Missing required parameter '{name}'")
        return ''

    return ret

version_file_path = get_environment_variable('version_file_path')
just_released_version = get_environment_variable('just_released_version').strip('v')

if not nuget.is_valid_version(just_released_version):
    gha.print_error('The specified just-released version is not a valid semver version.')

gha.fail_if_errors()

#==================================================================================================
# Bump verison number
#==================================================================================================

version = nuget.get_version_parts(just_released_version)
version.patch += 1
version.prerelease = None
version.build_metadata = None

print(f"Bumping to version {version}")

with open(version_file_path, 'w') as f:
    f.write("<!-- [auto-generated] This file is automatically re-created when Bonsai releases and generally should not be modified by hand [/auto-generated] -->\n")
    f.write("<Project>\n")
    f.write("  <PropertyGroup>\n")
    f.write(f"    <BonsaiVersion>{version}</BonsaiVersion>\n")
    f.write("  </PropertyGroup>\n")
    f.write("</Project>")

gha.set_environment_variable('NEXT_VERSION', str(version))

gha.fail_if_errors()
