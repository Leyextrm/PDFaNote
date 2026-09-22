import json
import os

transcript_path = r'C:\Users\leyex\.gemini\antigravity\brain\b369d447-cd28-4f3c-8cd8-515ccf169610\.system_generated\logs\transcript_full.jsonl'
contents = []
for line in open(transcript_path, 'r', encoding='utf-8'):
    try:
        step = json.loads(line)
        if step.get('type') == 'GENERIC':
            if 'public sealed partial class' in str(step):
                contents.append(step.get('content', ''))
    except Exception as e:
        pass

for i, c in enumerate(contents):
    with open(f'dump_{i}.txt', 'w', encoding='utf-8') as f:
        f.write(c)
        print(f"Dumped {i} length {len(c)}")
