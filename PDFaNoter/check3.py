import json
import os

transcript_path = r'C:\Users\leyex\.gemini\antigravity\brain\b369d447-cd28-4f3c-8cd8-515ccf169610\.system_generated\logs\transcript_full.jsonl'
types = set()
for line in open(transcript_path, 'r', encoding='utf-8'):
    try:
        step = json.loads(line)
        types.add(step.get('type'))
    except:
        pass
print(types)
