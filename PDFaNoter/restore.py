import re

def restore_file(dump_file, target_file):
    with open(dump_file, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    out_lines = []
    started = False
    for line in lines:
        if line.startswith('1: '):
            started = True
        
        if started:
            # Match ^\d+: (.*)
            m = re.match(r'^\d+:\s?(.*)', line)
            if m:
                out_lines.append(m.group(1))
    
    with open(target_file, 'w', encoding='utf-8-sig') as f:
        f.write(''.join(out_lines))
    print(f"Restored {target_file}")

restore_file('dump_0.txt', 'PdfPageView.xaml.cs')
restore_file('dump_1.txt', 'PdfDocumentView.xaml.cs')
