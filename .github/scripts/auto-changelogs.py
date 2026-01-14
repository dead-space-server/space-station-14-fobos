#!/usr/bin/env python3
import argparse
import yaml
import re
from datetime import datetime
import os
import sys

CHANGELOG_FILE = "Resources/Changelog/ChangelogDS14.yml"

def parse_description(description):
    if not description:
        return []

    sections = {
        "Добавлено": "Add",
        "Удалено": "Remove",
        "Изменено": "Tweak",
        "Исправлено": "Fix"
    }
    changes = []

    for section_title, change_type in sections.items():
        pattern = re.escape(section_title) + r":\s*((?:[^\n](?:\n(?!\n|\w+:))*)+)\n*"
        matches = re.findall(pattern, description, re.DOTALL)
        for match in matches:
            lines = [line.strip() for line in match.strip().split('\n') if line.strip()]
            message = '\n'.join(lines)
            if message:  # Только если есть текст
                changes.append({
                    "type": change_type,
                    "message": message
                })
    return changes

def load_changelog():
    if os.path.exists(CHANGELOG_FILE):
        try:
            with open(CHANGELOG_FILE, 'r', encoding='utf-8') as f:
                return yaml.safe_load(f) or []
        except Exception as e:
            print(f"Error reading changelog: {e}")
            return []
    return []

def save_changelog(entries):
    try:
        with open(CHANGELOG_FILE, 'w', encoding='utf-8') as f:
            yaml.dump(entries, f, default_style=None, default_flow_style=False, allow_unicode=True, sort_keys=False)
        print("Changelog updated successfully.")
    except Exception as e:
        print(f"Error writing changelog: {e}")
        sys.exit(1)

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--pr-number", type=int, required=True)
    parser.add_argument("--author", type=str, required=True)
    parser.add_argument("--description", type=str, required=True)
    args = parser.parse_args()

    changes = parse_description(args.description)
    if not changes:
        print("No changelog entries found in PR description.")
        return

    changelog = load_changelog()
    max_id = max((entry["id"] for entry in changelog), default=0)
    new_id = max_id + 1

    new_entry = {
        "id": new_id,
        "author": args.author,
        "time": datetime.utcnow().isoformat().replace('+00:00', 'Z'),
        "changes": changes
    }
    changelog.append(new_entry)

    save_changelog(changelog)
    print(f"Added changelog entry #{new_id}.")

if __name__ == "__main__":
    main()
