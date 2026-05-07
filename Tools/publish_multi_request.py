#!/usr/bin/env python3

import argparse
import requests
import os
import subprocess
from typing import Iterable

PUBLISH_TOKEN = os.environ["PUBLISH_TOKEN"]
VERSION = os.environ.get("PUBLISH_VERSION") or os.environ["GITHUB_SHA"]

RELEASE_DIR = "release"

#
# CONFIGURATION PARAMETERS
# Forks should change these to publish to their own infrastructure.
#
ROBUST_CDN_URL = "https://cdn.deadspace14.net/"
FORK_ID = "dsfobos"

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--fork-id", default=FORK_ID)

    args = parser.parse_args()
    fork_id = args.fork_id

    session = requests.Session()
    session.headers = {
        "Authorization": f"Bearer {PUBLISH_TOKEN}",
    }

    engine_version = get_engine_version()
    print(f"Version: {VERSION}")
    print(f"Engine version: {engine_version}")
    print(f"Fork: {fork_id}")
    print(f"CDN: {ROBUST_CDN_URL}")

    def abort_publish():
        try:
            session.post(
                f"{ROBUST_CDN_URL}fork/{fork_id}/publish/abort",
                json={"version": VERSION},
                headers={"Content-Type": "application/json", "Robust-Cdn-Publish-Id": publish_id},
                timeout=30)
        except Exception as e:
            print(f"Abort publish failed: {e}")

    data = {
        "version": VERSION,
        "engineVersion": engine_version,
    }
    headers = {
        "Content-Type": "application/json"
    }

    print(f"Starting publish...")
    resp = session.post(f"{ROBUST_CDN_URL}fork/{fork_id}/publish/start", json=data, headers=headers)
    if not resp.ok:
        print(f"Publish start FAILED: {resp.status_code} {resp.reason}")
        print(f"Response: {resp.text}")
        resp.raise_for_status()
    publish_id = resp.headers.get("Robust-Cdn-Publish-Id")
    if not publish_id:
        raise RuntimeError("CDN did not return Robust-Cdn-Publish-Id")
    print("Publish started OK, uploading files...")

    files = list(get_files_to_publish())
    print(f"Files to upload: {len(files)}")
    if not files:
        abort_publish()
        raise RuntimeError("No files found to publish")

    for file in files:
        try:
            size_mb = os.path.getsize(file) / (1024 * 1024)
            print(f"  Uploading {os.path.basename(file)} ({size_mb:.1f} MB)")
            with open(file, "rb") as f:
                headers = {
                    "Content-Type": "application/octet-stream",
                    "Robust-Cdn-Publish-File": os.path.basename(file),
                    "Robust-Cdn-Publish-Version": VERSION,
                    "Robust-Cdn-Publish-Id": publish_id
                }
                resp = session.post(f"{ROBUST_CDN_URL}fork/{fork_id}/publish/file", data=f, headers=headers)
        except Exception:
            abort_publish()
            raise

        if not resp.ok:
            print(f"  Upload FAILED: {resp.status_code} {resp.reason}")
            print(f"  Response: {resp.text}")
            abort_publish()
            resp.raise_for_status()

    print("All files uploaded, finishing publish...")

    data = {
        "version": VERSION
    }
    headers = {
        "Content-Type": "application/json",
        "Robust-Cdn-Publish-Id": publish_id
    }
    try:
        resp = session.post(f"{ROBUST_CDN_URL}fork/{fork_id}/publish/finish", json=data, headers=headers)
    except Exception:
        abort_publish()
        raise
    if not resp.ok:
        print(f"Publish finish FAILED: {resp.status_code} {resp.reason}")
        print(f"Response: {resp.text}")
        abort_publish()
        resp.raise_for_status()

    print("SUCCESS!")


def get_files_to_publish() -> Iterable[str]:
    for file in os.listdir(RELEASE_DIR):
        yield os.path.join(RELEASE_DIR, file)


def get_engine_version() -> str:
    import xml.etree.ElementTree as ET
    tree = ET.parse(os.path.join("RobustToolbox", "MSBuild", "Robust.Engine.Version.props"))
    version = tree.getroot().find(".//Version").text.strip()
    return version


if __name__ == '__main__':
    main()
