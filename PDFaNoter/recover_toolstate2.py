import json
import os

folders = [
    r'C:\Users\leyex\.gemini\antigravity\brain\b369d447-cd28-4f3c-8cd8-515ccf169610',
    r'C:\Users\leyex\.gemini\antigravity\brain\fe039cac-1cb9-43f4-8f00-8cc2fc9bb379',
    r'C:\Users\leyex\.gemini\antigravity\brain\54f87099-42f7-47eb-b3d5-d4d34cbb69d8'
]

with open('ToolState.cs.recovered', 'r', encoding='utf-8') as f:
    content = f.read().replace('\r\n', '\n')

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
                    if 'ToolState.cs' in tf:
                        target = args.get('TargetContent', '').replace('\r\n', '\n')
                        replacement = args.get('ReplacementContent', '').replace('\r\n', '\n')
                        if target in content:
                            content = content.replace(target, replacement, 1)
                            print(f"Applied patch from {folder}")

with open('ToolState.cs.fully_recovered', 'w', encoding='utf-8') as f:
    f.write(content.replace('\n', '\r\n'))
    
print("Done!")
