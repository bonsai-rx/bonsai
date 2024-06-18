#!/usr/bin/env python3
import os
import re
import sys

import gha
import nuget

#==================================================================================================
# Get inputs
#==================================================================================================
def get_environment_variable(name):
    ret = os.getenv(name)

    if ret is None:
        gha.print_error(f"Missing required parameter '{name}'")

    if ret == '':
        return None

    return ret

github_event_name = get_environment_variable('github_event_name')
assert(github_event_name is not None)
github_ref = get_environment_variable('github_ref')
assert(github_ref is not None)
github_run_number = get_environment_variable('github_run_number')
assert(github_run_number is not None)

gha.fail_if_errors()

#==================================================================================================
# Determine build settings
#==================================================================================================

# For GitHub refs besides main, include the branch/tag name in the default version string
ref_part = ''
if github_ref != 'refs/heads/main':
    ref = github_ref

    # Strip the ref prefix
    branch_prefix = 'refs/heads/'
    tag_prefix = 'refs/tags/'
    if ref.startswith(branch_prefix):
        ref = ref[len(branch_prefix):]
    elif ref.startswith(tag_prefix):
        ref = f'tag-{ref[len(tag_prefix):]}'

    # Replace illegal characters with dashes
    ref = re.sub('[^0-9A-Za-z-]', '-', ref)

    # Make the ref part
    ref_part = f'-{ref}'

# Build the default version string
version = ''
version_suffix = f'{ref_part}-ci{github_run_number}'
is_for_release = False

# Handle non-default version strings
# Make sure logic relating to is_for_release matches the publish-packages-nuget-org in the workflow
if github_event_name == 'release':
    is_for_release = True
    version = get_environment_variable('release_version')
    if version is None:
        gha.print_error('Release version was not specified!')
        sys.exit(1)

    # Trim leading v off of version if present
    version = version.strip('v')

    release_is_prerelease = get_environment_variable('release_is_prerelease')
    if release_is_prerelease != 'true' and release_is_prerelease != 'false':
        gha.print_error('Release prerelease status was invalid or unspecified!')

    # There are steps within the workflow which assume that the prerelease state of the release is correct, so we ensure it is
    # We could implicitly detect things for those steps, but this situation probably indicates user error and handling it this way is easier
    if nuget.is_preview_version(version) and release_is_prerelease != 'true':
        gha.print_error(f"The version to be release '{version}' indicates a pre-release version, but the release is not marked as a pre-release!")
        sys.exit(1)
elif github_event_name == 'workflow_dispatch':
    workflow_dispatch_version = get_environment_variable('workflow_dispatch_version')
    workflow_dispatch_will_publish_packages = get_environment_variable('workflow_dispatch_will_publish_packages') or 'false'

    if workflow_dispatch_version is not None:
        version = workflow_dispatch_version

    if workflow_dispatch_will_publish_packages.lower() == 'true':
        is_for_release = True

# Validate the version number
if version != '' and not nuget.is_valid_version(version, forbid_build_metadata=True):
    gha.print_error(f"'{version}' is not a valid semver version!")

# If there are any errors at this point, make sure we exit with an error code
gha.fail_if_errors()

#==================================================================================================
# Emit MSBuild properties
#==================================================================================================
print(f"Configuring build environment to build{' and release' if is_for_release else ''} version {version}")
gha.set_environment_variable('CiBuildVersion', version)
gha.set_environment_variable('CiBuildVersionSuffix', version_suffix)
gha.set_environment_variable('CiRunNumber', github_run_number)
gha.set_environment_variable('CiIsForRelease', str(is_for_release).lower())

gha.fail_if_errors()
