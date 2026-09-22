import json
import re

folders = [
    r'C:\Users\leyex\.gemini\antigravity\brain\b369d447-cd28-4f3c-8cd8-515ccf169610',
    r'C:\Users\leyex\.gemini\antigravity\brain\fe039cac-1cb9-43f4-8f00-8cc2fc9bb379',
    r'C:\Users\leyex\.gemini\antigravity\brain\54f87099-42f7-47eb-b3d5-d4d34cbb69d8'
]

content_to_save = ""
for folder in folders:
    tpath = folder + r'\.system_generated\logs\transcript_full.jsonl'
    try:
        for line in open(tpath, 'r', encoding='utf-8'):
            try: step = json.loads(line)
            except: continue
            
            if step.get('type') == 'GENERIC' and 'ToolState.cs' in step.get('content', '') and 'LoadAvailableFonts' in step.get('content', ''):
                content = step['content']
                if 'Total Lines:' in content and 'Original_line' not in content: # wait, the prompt says '<line_number>: <original_line>'
                    lines = content.split('\n')
                    clean_lines = []
                    start = False
                    for l in lines:
                        if l.startswith('1: '):
                            start = True
                        if start:
                            if l.startswith('The above content shows the entire, complete file'):
                                break
                            # remove line number
                            idx = l.find(': ')
                            if idx != -1 and l[:idx].isdigit():
                                clean_lines.append(l[idx+2:])
                            else:
                                # Sometimes lines wrap? No, they don't in view_file unless they have newlines
                                pass
                    if len(clean_lines) > 50:
                        content_to_save = '\n'.join(clean_lines)
                        break
    except Exception as e: pass

if content_to_save:
    with open('ToolState.cs.recovered', 'w', encoding='utf-8') as f:
        f.write(content_to_save)
    print("Recovered!")
else:
    print("Not found")

