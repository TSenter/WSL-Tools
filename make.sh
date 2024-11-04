#!/usr/bin/env bash

BUILD_DIR="dist"
PLUGIN="Flow.Launcher.Plugin.WSL-Tools"

build() {
  if [[ "$1" == "--debug" ]]; then
    dotnet publish -c Debug
    return
  fi
  echo -e "==================== Building Project ====================\n"
  dotnet publish -c Release
  echo
}

install() {
  if [[ ! -d "$BUILD_DIR" ]]; then
    echo "Build directory not found. Building project first..."
    build
  fi
  _install
}

uninstall() {
  echo -e "==================== Uninstalling Plugin ====================\n"
  kill_flow
  local plugin_dir="$(get_plugin_dir)"
  rm -rfv "$plugin_dir"
  echo
  echo "Plugin uninstalled successfully."
  start_flow
  echo
}

clean() {
  echo -e "==================== Cleaning Project ====================\n"
  local obj_dir="$(find . -type d -name obj)"
  local bin_dir="$(find . -type d -name bin)"
  rm -rf $obj_dir $bin_dir $BUILD_DIR && echo "Project cleaned successfully."
  echo
}

_install() {
  echo -e "==================== Installing Plugin ====================\n"
  kill_flow
  local plugin_dir="$(get_plugin_dir)"
  mkdir -p "$plugin_dir"
  cp -rv "$BUILD_DIR/"* "$plugin_dir"
  echo
  echo "Plugin installed successfully."
  start_flow
  echo
}

get_plugin_dir() {
  local win_path="$(wslpath -w "$(powershell.exe -Command "echo \$env:AppData" | tr -d '\r'))"
  echo "$win_path\\FlowLauncher\\Plugins\\$PLUGIN"
}

kill_flow() {
  if [[ ! -z "$(tasklist.exe | grep 'Flow.Launcher')" ]]; then
    echo "Stopping Flow Launcher..."
    powershell.exe -Command "Stop-Process -Name 'Flow.Launcher' -Force"
    echo
  fi
}

start_flow() {
  if [[ -z "$(tasklist.exe | grep 'Flow.Launcher')" ]]; then
    echo "Starting Flow Launcher..."
    powershell.exe -Command "Start-Process -FilePath \"\$env:LOCALAPPDATA\\FlowLauncher\\Flow.Launcher.exe\""
    echo
  fi
}

case "$1" in
  build)
    build
    ;;
  install)
    install
    ;;
  uninstall)
    uninstall
    ;;
  clean)
    clean
    ;;
  *)
    echo "Usage: $0 {build|install|uninstall|clean}"
    exit 1
    ;;
esac