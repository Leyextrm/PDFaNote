import json
import os

transcript_path = r'C:\Users\leyex\.gemini\antigravity\brain\fe039cac-1cb9-43f4-8f00-8cc2fc9bb379\.system_generated\logs\transcript_full.jsonl'
files = {}

# Reconstruct files from base by applying all replace_file_content in order
for line in open(transcript_path, 'r', encoding='utf-8'):
    step = json.loads(line)
    if step.get('type') == 'PLANNER_RESPONSE' and 'tool_calls' in step:
        for call in step['tool_calls']:
            if call.get('name') in ['replace_file_content', 'default_api:replace_file_content']:
                args = call.get('args') or call.get('arguments', {})
                tf = args.get('TargetFile', '')
                if tf.endswith('PdfPageView.xaml.cs') or tf.endswith('PdfDocumentView.xaml.cs'):
                    if tf not in files:
                        try:
                            with open(tf, 'r', encoding='utf-8-sig') as f:
                                files[tf] = f.read()
                        except:
                            continue
                    
                    target = args.get('TargetContent', '').replace('\r\n', '\n')
                    replacement = args.get('ReplacementContent', '').replace('\r\n', '\n')
                    content = files[tf].replace('\r\n', '\n')
                    
                    if target in content:
                        files[tf] = content.replace(target, replacement, 1)
                    else:
                        pass

for tf, content in files.items():
    with open(tf, 'w', encoding='utf-8-sig') as f:
        f.write(content.replace('\n', '\r\n'))
    print(f"Wrote {tf}")

