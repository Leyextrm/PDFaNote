import json
import os

transcript_path = r'C:\Users\leyex\.gemini\antigravity\brain\b369d447-cd28-4f3c-8cd8-515ccf169610\.system_generated\logs\transcript_full.jsonl'
for line in open(transcript_path, 'r', encoding='utf-8'):
    step = json.loads(line)
    if step.get('type') == 'TOOL_RESPONSE':
        content = step.get('content', '')
        if 'public sealed partial class PdfPageView : UserControl' in content:
            print("Found PdfPageView!")
            print(f"Content length: {len(content)}")
            if 'Content truncated' in content:
                print("Truncated!")
            break
