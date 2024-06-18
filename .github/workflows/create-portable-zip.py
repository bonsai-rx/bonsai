#!/usr/bin/env python3
import os
import sys
import zipfile

import gha

if len(sys.argv) != 3:
    gha.print_error('Usage: create-portable-zip.py <output_path> <release|debug>')
    sys.exit(1)
else:
    output_path = sys.argv[1]
    configuration = sys.argv[2].lower()

with zipfile.ZipFile(output_path, 'x', zipfile.ZIP_DEFLATED, compresslevel=9) as output:
    output.mkdir('Extensions')
    output.mkdir('Gallery')

    output.write(f'artifacts/bin/Bonsai/{configuration}-repacked/Bonsai.exe', 'Bonsai.exe')
    output.write(f'artifacts/bin/Bonsai/{configuration}/Bonsai32.exe', 'Bonsai32.exe')

    nuget_config = [
        '<?xml version="1.0" encoding="utf-8"?>',
        '<configuration>',
        '  <packageSources>',
        '    <add key="Gallery" value="Gallery" />',
        '    <add key="Bonsai Packages" value="https://www.myget.org/F/bonsai/api/v3/index.json" />',
        '    <add key="Community Packages" value="https://www.myget.org/F/bonsai-community/api/v3/index.json" />',
    ]

    nuget_api_url = os.getenv('NUGET_API_URL')
    if nuget_api_url is not None:
        nuget_config.append(f'    <add key="NuGet Package Testing Feed" value="{nuget_api_url}" />')

    # Unstable builds of Bonsai will automatically reference the GitHub Packages feed
    if os.getenv('IS_FULL_RELEASE') == 'false':
        repo_owner = os.getenv('GITHUB_REPOSITORY_OWNER') or 'bonsai-rx'
        nuget_config.append(f'    <add key="Bonsai Unstable" value="https://nuget.pkg.github.com/{repo_owner}/index.json" />')
        nuget_config.append('  </packageSources>')
        nuget_config.append('  <packageSourceCredentials>')
        nuget_config.append('    <!--')
        nuget_config.append('      To authenticate with the Bonsai Unstable package feed, you need to manually authenticate with GitHub by filling the YOUR_GITHUB_XYZ fields below.')
        nuget_config.append('      You can create a personal access token by following the instructions at the link below, you only need to grant the read:packages scope.')
        nuget_config.append('      https://docs.github.com/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-personal-access-token-classic')
        nuget_config.append('    -->')
        nuget_config.append('    <Bonsai_x0020_Unstable>')
        nuget_config.append('      <add key="Username" value="YOUR_GITHUB_USERNAME" />')
        nuget_config.append('      <add key="ClearTextPassword" value="YOUR_GITHUB_PERSONAL_ACCESS_TOKEN" />')
        nuget_config.append('    </Bonsai_x0020_Unstable>')
        nuget_config.append('  </packageSourceCredentials>')
    else:    
        nuget_config.append('  </packageSources>')

    nuget_config.append('</configuration>')
    nuget_config.append('')

    output.writestr('NuGet.config', '\r\n'.join(nuget_config))

gha.fail_if_errors()
