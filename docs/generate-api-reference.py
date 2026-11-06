# Writes docs/api from the public API and the XML docs of a build:
#
#     dotnet build XForma.slnx
#     python docs/generate-api-reference.py
#
# One page per namespace, each type with its summary and members.

import glob
import os
import re
import xml.etree.ElementTree as ET
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PROJECTS = [
    ('framework', 'XForma.Framework'),
    ('xforma', 'XForma'),
    ('toolkit', 'XForma.Toolkit'),
    ('themes/basic', 'XForma.Themes.Basic'),
]


def clean(element):
    # A doc comment's text, with its references as code
    if element is None:
        return ''
    parts = [element.text or '']
    for child in element:
        if child.tag in ('see', 'seealso', 'paramref', 'typeparamref'):
            name = child.get('cref') or child.get('name') or child.get('langword') or ''
            name = re.sub(r'^[A-Z]:', '', name).split('(')[0].split('.')[-1]
            parts.append('`' + name + '`')
        elif child.tag == 'c':
            parts.append('`' + (child.text or '') + '`')
        else:
            parts.append(clean(child))
        parts.append(child.tail or '')
    return re.sub(r'\s+', ' ', ''.join(parts)).strip()


def read_docs():
    docs = {}
    for folder, name in PROJECTS:
        for path in glob.glob(os.path.join(ROOT, folder, 'bin', '**', name + '.xml'), recursive=True):
            for member in ET.parse(path).getroot().iter('member'):
                docs.setdefault(member.get('name'), clean(member.find('summary')))
    return docs


def read_api():
    # Type -> its members, from the PublicAPI files
    types = defaultdict(set)
    known = set()
    entries = []
    for folder, _ in PROJECTS:
        for path in glob.glob(os.path.join(ROOT, folder, 'PublicAPI*.txt')) + glob.glob(os.path.join(ROOT, folder, 'PublicAPI', '*', 'PublicAPI*.txt')):
            with open(path, encoding='utf-8') as f:
                entries += [line.strip() for line in f if line.strip() and not line.startswith('#')]

    for entry in entries:
        entry = entry.replace('~', '')
        signature = re.sub(r'^(static |abstract |virtual |override |sealed |const |readonly )+', '', entry)
        if '->' not in signature and '(' not in signature:
            known.add(signature)

    for entry in entries:
        entry = entry.replace('~', '')
        signature = re.sub(r'^(static |abstract |virtual |override |sealed |const |readonly )+', '', entry)
        if signature in known:
            types[signature]
            continue
        head = signature.split('(')[0].split(' ->')[0].split(' =')[0]
        pieces = head.split('.')
        for count in range(len(pieces) - 1, 0, -1):
            owner = '.'.join(pieces[:count])
            if owner in known:
                member = signature[len(owner) + 1:]
                if not (member.startswith(pieces[count - 1] + '(')):
                    types[owner].add(member)
                else:
                    types[owner].add(member)
                break
    return types


def main():
    docs = read_docs()
    types = read_api()
    namespaces = defaultdict(list)
    for name in types:
        known_namespace = '.'.join(name.split('.')[:-1])
        namespaces[known_namespace].append(name)

    # A nested type stays with the namespace of its outer type
    out = os.path.join(ROOT, 'docs', 'api')
    os.makedirs(out, exist_ok=True)
    for old in glob.glob(os.path.join(out, '*.md')):
        os.remove(old)

    index = ['# API reference', '', 'Generated from the public API and its doc comments by `docs/generate-api-reference.py`.', '']
    for namespace in sorted(namespaces):
        file = (namespace or 'global') + '.md'
        index.append(f'- [{namespace}]({file}): {len(namespaces[namespace])} types')
        lines = [f'# {namespace}', '']
        for name in sorted(namespaces[namespace]):
            short = name[len(namespace) + 1:] if namespace else name
            lines += [f'## {short}', '']
            summary = docs.get('T:' + name, '')
            if summary:
                lines += [summary, '']
            members = sorted(types[name])
            if members:
                lines += ['| Member | |', '|---|---|']
                for member in members:
                    key = member.split('(')[0].split(' ->')[0].split(' =')[0]
                    key = key.replace('.get', '').replace('.set', '').replace('.init', '')
                    text = docs.get(f'P:{name}.{key}') or docs.get(f'F:{name}.{key}') or docs.get(f'E:{name}.{key}') or next((v for k, v in docs.items() if k.startswith(f'M:{name}.{key}(') or k == f'M:{name}.{key}'), '')
                    cell = member.replace('|', '\\|')
                    lines.append(f'| `{cell}` | {text} |')
                lines.append('')
        with open(os.path.join(out, file), 'w', encoding='utf-8', newline='\n') as f:
            f.write('\n'.join(lines))

    with open(os.path.join(out, 'README.md'), 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(index) + '\n')
    print(sum(len(v) for v in namespaces.values()), 'types in', len(namespaces), 'namespaces')


if __name__ == '__main__':
    main()
