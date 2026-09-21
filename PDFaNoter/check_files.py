import json
import os

files_modified = set()

tpath = r'C:\Users\leyex\.gemini\antigravity\brain\fe039cac-1cb9-43f4-8f00-8cc2fc9bb379\.system_generated\logs\transcript_full.jsonl'
for line in open(tpath, 'r', encoding='utf-8'):
    try:
        step = json.loads(line)
    except: continue
    
    if step.get('step_index', 0) > 541:
        break

    if step.get('type') == 'PLANNER_RESPONSE' and 'tool_calls' in step:
        for call in step['tool_calls']:
            if call.get('name') in ['replace_file_content', 'default_api:replace_file_content']:
                args = call.get('args') or call.get('arguments', {})
                tf = args.get('TargetFile', '')
                files_modified.add(tf)

print(files_modified)
