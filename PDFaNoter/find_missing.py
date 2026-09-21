import json
import re

folders = [
    r'C:\Users\leyex\.gemini\antigravity\brain\b369d447-cd28-4f3c-8cd8-515ccf169610',
    r'C:\Users\leyex\.gemini\antigravity\brain\fe039cac-1cb9-43f4-8f00-8cc2fc9bb379',
    r'C:\Users\leyex\.gemini\antigravity\brain\54f87099-42f7-47eb-b3d5-d4d34cbb69d8'
]

for folder in folders:
    tpath = folder + r'\.system_generated\logs\transcript_full.jsonl'
    try:
        for line in open(tpath, 'r', encoding='utf-8'):
            try: step = json.loads(line)
            except: continue
            
            if step.get('type') == 'PLANNER_RESPONSE' and 'tool_calls' in step:
                for call in step['tool_calls']:
                    args = call.get('args') or call.get('arguments', {})
                    if 'ReplacementContent' in args and 'ExtractedTexts' in args['ReplacementContent']:
                        print(args['ReplacementContent'])
                        print('---')
                    elif 'CodeContent' in args and 'ExtractedTexts' in args['CodeContent']:
                        print(args['CodeContent'])
                        print('---')
    except Exception as e: pass

