import json
tpath = r'C:\Users\leyex\.gemini\antigravity\brain\b369d447-cd28-4f3c-8cd8-515ccf169610\.system_generated\logs\transcript_full.jsonl'
for line in open(tpath, 'r', encoding='utf-8'):
    try:
        step = json.loads(line)
    except: continue
    
    if step.get('type') == 'PLANNER_RESPONSE' and 'tool_calls' in step:
        for call in step['tool_calls']:
            args = call.get('args') or call.get('arguments', {})
            cmd = args.get('CommandLine', '')
            tf = args.get('TargetFile', '')
            name = call.get('name')
            if 'ToolState.cs' in tf or 'ToolState.cs' in cmd:
                print(name)
                print(tf)
                print(cmd)
                print('---')
