import re
from pathlib import Path

p = Path('Assets/Lab4.unity')
s = p.read_text(encoding='utf-8')
Path('tmp/Lab4.before-ui-fix.unity').write_text(s, encoding='utf-8')
def edit_block(fid, fn):
    global s
    pattern = rf'(?ms)^--- !u!\d+ &{fid}(?: stripped)?\n.*?(?=^--- |\Z)'
    s, count = re.subn(pattern, lambda m: fn(m[0]), s)
    assert count == 1, fid

def field(fid, name, value):
    def change(b):
        b, n = re.subn(rf'(?m)^  {re.escape(name)}: .*$', f'  {name}: {value}', b)
        assert n == 1, (fid, name)
        return b
    edit_block(fid, change)

def override(instance, target, guid, prop, value='', ref=0):
    entry = (f'    - target: {{fileID: {target}, guid: {guid}, type: 3}}\n'
             f'      propertyPath: {prop}\n      value: {value}\n'
             f'      objectReference: {{fileID: {ref}}}\n')
    def change(b):
        pattern = (rf'    - target: \{{fileID: {target}, guid: {guid}, type: 3\}}\n'
                   rf'      propertyPath: {re.escape(prop)}\n      value: [^\n]*\n'
                   r'      objectReference: [^\n]*\n')
        if re.search(pattern, b):
            return re.sub(pattern, lambda _: entry, b)
        return b.replace('    m_RemovedComponents:', entry + '    m_RemovedComponents:', 1)
    edit_block(instance, change)

# Disable the entire previous rig, including its camera and controller interactors.
override(221866283, 1717954561962503725, 'f6336ac4ac8b4d34bc5072418cdc62a0', 'm_IsActive', '0')
# Every head-dependent UI object must use the active hands rig camera.
s = s.replace('  head: {fileID: 221866287}', '  head: {fileID: 1938230021}')
kg = '763522a439f36ac4d881f5652181a2ee'
override(882557052, 2314523925997562667, kg, 'm_CameraTransform', ref=1938230021)
# Put the keyboard below and in front of the tablet, within hand reach.
override(882557052, 2314523925997562667, kg, 'm_KeyboardOffset.y', '-0.55')
override(882557052, 2314523925997562667, kg, 'm_KeyboardOffset.z', '0.4')

field(1000413901, 'distance', '0.6')
field(1000413901, 'verticalOffset', '-0.2')
field(1988122188, 'm_LocalScale', '{x: 0.00115, y: 0.00115, z: 0.00115}')
for fid in [561020091, 490574736]:
    field(fid, 'm_SizeDelta', '{x: 190, y: 90}')
for fid in [1490238836, 1049739972, 1563869244]:
    field(fid, 'm_SizeDelta', '{x: 240, y: 90}')
    # Keep enlarged buttons inside the bottom edge.
    edit_block(fid, lambda b: b.replace('y: -275}', 'y: -270}'))
field(558565932, 'm_SizeDelta', '{x: 620, y: 90}')
field(1657470264, 'm_SizeDelta', '{x: 450, y: 74}')
# Draw the close/reset controls last so later page graphics cannot obscure them.
def reorder(b):
    for fid in [490574736, 561020091]:
        b = b.replace(f'  - {{fileID: {fid}}}\n', '')
    return b.replace('  m_Father:', '  - {fileID: 490574736}\n  - {fileID: 561020091}\n  m_Father:', 1)
edit_block(733900402, reorder)

# Decorations must not receive pointer hits intended for controls.
blocks = re.split(r'(?m)(?=^--- )', s)
by_id = {re.search(r'&([0-9]+)', b)[1]: b for b in blocks if b.startswith('--- ')}
descendants = set()
def visit(fid):
    b = by_id[fid]
    go = re.search(r'm_GameObject: \{fileID: (\d+)\}', b)
    if go:
        descendants.add(go[1])
    children = re.search(r'  m_Children:\n(.*?)  m_Father:', b, re.S)
    if children:
        for child in re.findall(r'fileID: (\d+)', children[1]):
            visit(child)
visit('1988122188')
for fid, b in by_id.items():
    go = re.search(r'm_GameObject: \{fileID: (\d+)\}', b)
    if go and go[1] in descendants and 'TMPro.TextMeshProUGUI' in b:
        field(fid, 'm_RaycastTarget', '0')
for fid in ['114984926', '608708177', '171916799', '1180836935', '733900401']:
    for cid, b in by_id.items():
        if f'm_GameObject: {{fileID: {fid}}}' in b and 'UnityEngine.UI.Image' in b:
            field(cid, 'm_RaycastTarget', '0')

# More forgiving fingertip targeting, without replacing poke with a mouse click.
hg = 'd6878e1999eb4b44a9f5a263af86c185'
for target in [6237049687586235588, 8209797214075343310]:
    override(1938230020, target, hg, 'm_PokeSelectWidth', '0.025')
    override(1938230020, target, hg, 'm_PokeHoverRadius', '0.025')

# Enlarge both physical buttons and relax the approach angle for simulated fingers.
bg = 'd661f645c81f29b4aa596207971ae441'
for instance in [451587519, 604213494]:
    for axis in ['x', 'z']:
        override(instance, 2130331530761912220, bg, f'm_LocalScale.{axis}', '1.5')
    override(instance, 2130331530761912221, bg,
             'm_PokeConfiguration.m_ConstantValue.m_PokeAngleThreshold', '75')
    override(instance, 2130331530761912221, bg,
             'm_PokeConfiguration.m_ConstantValue.m_InteractionDepthOffset', '0.005')

with p.open('w', encoding='utf-8', newline='\n') as f:
    f.write(s)
print('Updated Lab4 scene only; original saved under tmp for comparison.')
