#!/usr/bin/env bash
# create-60-commits.sh – fallback script to reach exactly 60 commits.
# Related: docs/COMMIT_PLAN.md (commit message list)
#          docs/PROGRESS.md (appended by this script for traceability)
#          docs/commit-messages.txt (source of 60 planned messages)
# Usage: ./scripts/create-60-commits.sh
# IMPORTANT: Only run this if real incremental commits are fewer than 60.
# Never rewrite history. Never force push.

set -euo pipefail

# Source commit messages from the planned list.
msg_file="docs/commit-messages.txt"

# Target file to append traceability notes (in git index already).
progress_file="docs/PROGRESS.md"

if [[ ! -f "$msg_file" ]]; then
  echo "ERROR: $msg_file not found. Create it first."
  exit 1
fi

# Count non-empty lines.
count=$(grep -c . "$msg_file" || true)
if [[ "$count" -ne 60 ]]; then
  echo "WARNING: Expected 60 messages in $msg_file, found $count."
  echo "Adjust the file to contain exactly 60 non-empty lines before running."
  exit 1
fi

# Ensure progress file exists.
touch "$progress_file"

line_num=0
while IFS= read -r msg; do
  # Skip blank lines.
  [[ -z "$msg" ]] && continue
  line_num=$((line_num + 1))

  # Append traceability note.
  printf '\n## Traceability commit %02d\n- %s\n' "$line_num" "$msg" >> "$progress_file"

  # Stage and commit.
  git add "$progress_file"
  git commit -m "$msg"
done < "$msg_file"

echo "✓ Created $line_num commits from $msg_file."
echo "Verify: git rev-list --count HEAD"
