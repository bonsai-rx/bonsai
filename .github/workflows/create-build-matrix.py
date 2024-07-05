#!/usr/bin/env python3
import json
import os

import gha

matrix = [ ]

def add(name: str, runner_os: str, rid: str, configurations: list[str] = ['Debug', 'Release']):
    platform = {
        'name': name,
        'os': runner_os,
        'rid': rid,
    }

    ret = { }
    for configuration in configurations:
        job = {
            'platform': platform.copy(),
            'configuration': configuration,
            'configuration-lower': configuration.lower(),
            'job-title': f"{name} {configuration}",
            'artifacts-suffix': '',
        }
        matrix.append(job)
        ret[configuration] = job
    return ret

windows = add('Windows x64', 'windows-latest', 'win-x64')
linux = add('Linux x64', 'ubuntu-latest', 'linux-x64')

# Collect packages and create installer from Windows Release x64
windows['Release']['collect-packages'] = True
windows['Release']['create-installer'] = True

# Build dummy packages to determine which ones changed (not relevant for pull requests since we won't publish)
def add_dummy(name: str, artifacts_suffix: str):
    dummy = add(name, 'ubuntu-latest', 'linux-x64', ['Release'])['Release']
    dummy['skip-tests'] = True
    dummy['collect-packages'] = True
    dummy['dummy-build'] = True
    dummy['title'] = name # Don't include configuration in dummy target titles
    dummy['artifacts-suffix'] = artifacts_suffix
    return dummy

enable_package_comparison = os.getenv('enable_package_comparison') == 'true'
github_event_name = os.getenv('GITHUB_EVENT_NAME')

if github_event_name != 'pull_request' and enable_package_comparison:
    add_dummy('Previous Dummy', '-dummy-prev')['checkout-ref'] = 'refs/tags/latest'
    add_dummy('Next Dummy', '-dummy-next')

# Fail early if we won't be able to do package comparison and the run must publish packages to make logical sense
# Package comparison requires the `latest` tag to exist, but it will usually either be missing or invalid for forks so we require it to be opt-in
if not enable_package_comparison:
    if github_event_name == 'release' or (github_event_name == 'workflow_dispatch' and os.getenv('will_publish_packages') == 'true'):
        gha.print_error('Release aborted. We would not be able to determine which packages need to be released as this repository is not configured for package comparison.')

# Output
matrix_json = json.dumps({ "include": matrix }, indent=2)
print(matrix_json)
gha.set_output('matrix', matrix_json)

gha.fail_if_errors()
