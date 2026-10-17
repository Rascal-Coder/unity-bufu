#!/bin/bash
set -euo pipefail
project_path="$(cd "$(dirname "$0")/.." && pwd)"
if [[ -z "${UNITY_EDITOR:-}" || ! -x "$UNITY_EDITOR" ]]; then
  echo "请先设置 UNITY_EDITOR 为安装了 iOS Build Support 的 Unity 编辑器可执行文件路径。"
  exit 1
fi
mkdir -p "$project_path/Logs"
"$UNITY_EDITOR" -batchmode -quit -projectPath "$project_path" -buildTarget iOS -executeMethod IosBuild.Export -logFile "$project_path/Logs/ios-export.log"
echo "Xcode 工程：$project_path/Build/iOS/Xcode"
