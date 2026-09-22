import json

with open(r'C:\Users\leyex\.gemini\antigravity\brain\fe039cac-1cb9-43f4-8f00-8cc2fc9bb379\.system_generated\logs\transcript_full.jsonl', 'r', encoding='utf-8') as f:
    for line in f:
        step = json.loads(line)
        if step.get('step_index', 0) <= 541:
            continue
        if step.get('type') == 'PLANNER_RESPONSE' and 'tool_calls' in step:
            for call in step['tool_calls']:
                if call.get('name') in ['run_command', 'default_api:run_command']:
                    args = call.get('args') or call.get('arguments', {})
                    cmd = args.get('CommandLine', '')
                    if 'python' in cmd and ('.py' in cmd or 'write' in cmd):
                        print(f"Step {step.get('step_index')}: {cmd[:80]}")
                elif call.get('name') in ['replace_file_content', 'default_api:replace_file_content']:
                    print(f"Step {step.get('step_index')}: replace_file_content {call.get('args', {}).get('TargetFile')}")
