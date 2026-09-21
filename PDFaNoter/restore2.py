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
                # Add newline because line from readlines() has \n at the end, but the regex (.*) strips the trailing \n if we don't capture it? 
                # Actually (.*) captures everything up to the newline. So we need to add \n back.
                out_lines.append(m.group(1) + '\n')
    
    with open(target_file, 'w', encoding='utf-8-sig') as f:
        f.write(''.join(out_lines))
    print(f"Restored {target_file}")

restore_file('dump_0.txt', 'PdfPageView.xaml.cs')
restore_file('dump_1.txt', 'PdfDocumentView.xaml.cs')
