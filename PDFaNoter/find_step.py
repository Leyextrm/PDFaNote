import json

with open(r'C:\Users\leyex\.gemini\antigravity\brain\fe039cac-1cb9-43f4-8f00-8cc2fc9bb379\.system_generated\logs\transcript_full.jsonl', 'r', encoding='utf-8') as f:
    for line in f:
        step = json.loads(line)
        content = step.get('content', '')
        if '텍스트 추가와 텍스트 하이라이팅 버튼 우측에' in content or '말씀하신 대로 상단 툴바' in content:
            print(f"Step {step.get('step_index')}: {step.get('type')}")
