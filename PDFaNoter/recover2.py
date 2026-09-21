import json
import os

transcript_path = r'C:\Users\leyex\.gemini\antigravity\brain\fe039cac-1cb9-43f4-8f00-8cc2fc9bb379\.system_generated\logs\transcript_full.jsonl'

# Read all tool calls and responses
steps = []
with open(transcript_path, 'r', encoding='utf-8') as f:
    for line in f:
        steps.append(json.loads(line))

# Find all replace_file_content calls for our files
# We need to apply them in the exact order they succeeded.
for step in steps:
    if step.get('type') == 'PLANNER_RESPONSE' and 'tool_calls' in step:
        for call in step['tool_calls']:
            if call.get('name') == 'default_api:replace_file_content':
                args = call.get('arguments', {})
                target_file = args.get('TargetFile', '')
                if target_file.endswith('PdfPageView.xaml.cs') or target_file.endswith('PdfDocumentView.xaml.cs') or target_file.endswith('PdfPageView.xaml') or target_file.endswith('PdfDocumentView.xaml'):
                    print(f"Applying change to {target_file}")
                    
                    try:
                        with open(target_file, 'r', encoding='utf-8-sig') as f:
                            content = f.read()
                    except FileNotFoundError:
                        continue
                        
                    target = args.get('TargetContent', '')
                    replacement = args.get('ReplacementContent', '')
                    
                    if target in content:
                        content = content.replace(target, replacement, 1)
                        with open(target_file, 'w', encoding='utf-8-sig') as f:
                            f.write(content)
                        print("Success")
                    else:
                        print("Target not found! Might have been applied already or file changed.")

