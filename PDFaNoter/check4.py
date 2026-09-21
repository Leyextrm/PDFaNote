import json
import os

transcript_path = r'C:\Users\leyex\.gemini\antigravity\brain\b369d447-cd28-4f3c-8cd8-515ccf169610\.system_generated\logs\transcript_full.jsonl'
for line in open(transcript_path, 'r', encoding='utf-8'):
    try:
        step = json.loads(line)
        if 'PdfPageView' in str(step):
            if step.get('type') == 'GENERIC':
                # Tool responses are usually in step['content'] or step['tool_responses']
                if 'public sealed partial class' in str(step):
                    print("Found in GENERIC!")
                    print(step.keys())
    except Exception as e:
        pass
