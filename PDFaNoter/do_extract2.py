import sys, re

with open('dump_0.txt', 'r', encoding='utf-8') as f:
    lines = f.readlines()

out = []
started = False
for line in lines:
    if line.startswith('1: '):
        started = True
    if started:
        m = re.match(r'^\d+:\s?(.*)', line)
        if m:
            out.append(m.group(1))

with open('PdfPageView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write('\n'.join(out))
print("Restored from dump_0.txt")
