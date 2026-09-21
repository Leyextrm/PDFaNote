import json
import os
import re

folders = [
    r'C:\Users\leyex\.gemini\antigravity\brain\b369d447-cd28-4f3c-8cd8-515ccf169610',
    r'C:\Users\leyex\.gemini\antigravity\brain\fe039cac-1cb9-43f4-8f00-8cc2fc9bb379'
]

files = {}
# First, read original HEAD
for tf in ['d:\\OneDrive\\Storage\\General\\Programming\\PDFaNoter\\PDFaNoter\\PdfPageView.xaml.cs', 'd:\\OneDrive\\Storage\\General\\Programming\\PDFaNoter\\PDFaNoter\\PdfDocumentView.xaml.cs']:
    try:
        with open(tf, 'rb') as f:
            content = f.read().decode('utf-8-sig')
            # normalize to \n
            content = content.replace('\r\n', '\n').replace('\r', '\n')
            files[tf] = content
    except:
        pass

for folder in folders:
    tpath = os.path.join(folder, r'.system_generated\logs\transcript_full.jsonl')
    if not os.path.exists(tpath): continue
    for line in open(tpath, 'r', encoding='utf-8'):
        try:
            step = json.loads(line)
        except: continue
        if step.get('type') == 'PLANNER_RESPONSE' and 'tool_calls' in step:
            for call in step['tool_calls']:
                if call.get('name') in ['replace_file_content', 'default_api:replace_file_content']:
                    args = call.get('args') or call.get('arguments', {})
                    tf = args.get('TargetFile', '')
                    if tf in files:
                        target = args.get('TargetContent', '').replace('\r\n', '\n')
                        replacement = args.get('ReplacementContent', '').replace('\r\n', '\n')
                        content = files[tf]
                        if target in content:
                            files[tf] = content.replace(target, replacement, 1)

for tf, content in files.items():
    with open(tf, 'wb') as f:
        # replace back to \r\n
        content = content.replace('\n', '\r\n')
        f.write(content.encode('utf-8-sig'))
    print(f"Wrote {tf}")
