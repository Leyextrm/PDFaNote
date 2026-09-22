import json
import sys

tpath = r'C:\Users\leyex\.gemini\antigravity\brain\b369d447-cd28-4f3c-8cd8-515ccf169610\.system_generated\logs\transcript_full.jsonl'
toolstate = ""

for line in open(tpath, 'r', encoding='utf-8'):
    try:
        step = json.loads(line)
    except: continue
    
    if step.get('type') == 'PLANNER_RESPONSE' and 'tool_calls' in step:
        for call in step['tool_calls']:
            args = call.get('args') or call.get('arguments', {})
            tf = args.get('TargetFile', '')
            if 'ToolState.cs' in tf:
                if call.get('name') in ['write_to_file', 'default_api:write_to_file']:
                    toolstate = args.get('CodeContent', '')
                elif call.get('name') in ['replace_file_content', 'default_api:replace_file_content']:
                    target = args.get('TargetContent', '').replace('\r\n', '\n')
                    replacement = args.get('ReplacementContent', '').replace('\r\n', '\n')
                    toolstate = toolstate.replace('\r\n', '\n').replace(target, replacement, 1)

with open('ToolState.cs.recovered', 'w', encoding='utf-8-sig') as f:
    f.write(toolstate.replace('\n', '\r\n'))
print(f"Recovered size: {len(toolstate)}")
