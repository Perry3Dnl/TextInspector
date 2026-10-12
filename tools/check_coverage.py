#!/usr/bin/env python3
"""Enforce a measured line-coverage threshold from Coverlet's Cobertura report."""
import argparse
from pathlib import Path
import sys
import xml.etree.ElementTree as ET

parser = argparse.ArgumentParser()
parser.add_argument("root", type=Path, help="Directory containing test result coverage.cobertura.xml")
parser.add_argument("--threshold", type=float, default=98.0)
parser.add_argument("--module", default="TextInspector", help="Core assembly name")
args = parser.parse_args()
reports = sorted(args.root.rglob("coverage.cobertura.xml"))
if len(reports) != 1:
    sys.exit(f"Expected exactly one coverage report, found {len(reports)} at {args.root}")
root = ET.parse(reports[0]).getroot()
packages = [p for p in root.findall(".//package") if p.get("name") == args.module]
if not packages:
    sys.exit(f"Core module {args.module!r} missing in {reports[0]}")
covered = valid = 0
for package in packages:
    for line in package.findall(".//class/lines/line"):
        valid += 1
        covered += int(line.get("hits", "0")) > 0
if not valid:
    sys.exit("No instrumented lines found for core module")
percent = covered / valid * 100
print(f"{args.module} line coverage: {covered}/{valid} = {percent:.2f}%; required: {args.threshold:.2f}%")
uncovered_classes = []
for package in packages:
    for cls in package.findall(".//class"):
        cls_lines = cls.findall("./lines/line")
        missed = sum(int(line.get("hits", "0")) == 0 for line in cls_lines)
        if missed:
            uncovered_classes.append((missed, cls.get("filename", cls.get("name", "unknown"))))
print("Most uncovered files/classes:")
for count, name in sorted(uncovered_classes, reverse=True)[:20]:
    print(f"  {count:>4} missed  {name}")
if percent + 1e-9 < args.threshold:
    sys.exit("Coverage quality gate FAILED")
print("Coverage quality gate PASSED")
