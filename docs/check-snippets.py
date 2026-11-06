# Compiles the C# in the docs against the real API, so that they can't drift from it:
#
#     python docs/check-snippets.py
#
# A block with a class in it is compiled as a file. Other blocks go inside a class derived from
# Page: as members if they start like a member, and otherwise as the body of an async method. A
# block fenced as "csharp no-compile" is skipped. A page's blocks can use the classes in each other.

import glob
import os
import re
import shutil
import subprocess
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
WORK = os.path.join(ROOT, 'test', 'XForma.DocSnippets', 'obj', 'snippets')
PROJECT = os.path.join(ROOT, 'test', 'XForma.DocSnippets', 'XForma.DocSnippets.csproj')

USINGS = '''using System;
using System.ComponentModel;
using System.Threading.Tasks;
using XForma;
using XForma.Controls;
using XForma.Input;
using XForma.Layouts;
using XForma.Primitives;
using Button = XForma.Controls.Button;
'''


def blocks(path):
    with open(path, encoding='utf-8') as f:
        text = f.read()
    for match in re.finditer(r'^```csharp[ \t]*\r?\n(.*?)^```', text, re.S | re.M):
        line = text.count('\n', 0, match.start()) + 1
        yield line, match.group(1)


def wrap(code, namespace, name, label):
    header = f'#nullable enable\n// {label}\n{USINGS}\nnamespace Snippets.{namespace}\n{{\n'
    if re.search(r'^(public |internal )?(sealed |static |abstract )*(class|record|interface) ', code, re.M) and not re.match(r'\s*(protected|private|override)', code):
        return header + code + '\n}\n'

    if re.match(r'\s*(\[|protected |public |private |internal |override )', code):
        return header + f'public class {name} : Page\n{{\n' + code + '\n}\n}\n'

    return header + f'public class {name} : Page\n{{\npublic async Task Run()\n{{\n' + code + '\nawait Task.CompletedTask;\n}\n}\n}\n'


def main():
    if os.path.exists(WORK):
        shutil.rmtree(WORK)
    os.makedirs(WORK)

    count = 0
    files = glob.glob(os.path.join(ROOT, 'docs', '*.md')) + glob.glob(os.path.join(ROOT, 'docs', 'guides', '*.md')) + [os.path.join(ROOT, 'README.md')]
    for path in files:
        base = os.path.splitext(os.path.basename(path))[0]
        if base in ('alignment', 'architecture'):
            continue
        for line, code in blocks(path):
            count += 1
            name = re.sub(r'\W', '_', f'{base}_{line}')
            label = f'{os.path.relpath(path, ROOT)}:{line}'
            with open(os.path.join(WORK, name + '.cs'), 'w', encoding='utf-8') as out:
                out.write(wrap(code, 'S' + re.sub(r'\W', '_', base), f'Snippet{line}', label))

    result = subprocess.run(['dotnet', 'build', PROJECT, '--nologo', '-v', 'q', '-p:SnippetsFolder=' + WORK], capture_output=True, text=True)
    errors = sorted({re.sub(r' \[.*$', '', line) for line in result.stdout.splitlines() if ': error ' in line})
    for error in errors:
        print(error)
    print(f'{count} snippets, {len(errors)} errors')
    sys.exit(1 if errors else 0)


if __name__ == '__main__':
    main()
