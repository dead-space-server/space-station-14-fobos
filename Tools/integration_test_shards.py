#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import re
from dataclasses import dataclass, field
from pathlib import Path

TEST_MARKER_PATTERN = re.compile(r"\[(?:Test|TestCase|TestCaseSource)\b")
NAMESPACE_PATTERN = re.compile(r"(?m)^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)")
INLINE_ATTRIBUTES_PATTERN = re.compile(r"^\s*(?:\[[^\]\n]+\]\s*)+")
TYPE_DECLARATION_PATTERN = re.compile(
    r"^\s*"
    r"(?:(?:public|private|protected|internal|sealed|abstract|static|partial|readonly|ref|unsafe|new|file)\s+)*"
    r"(?:class|struct|record(?:\s+(?:class|struct))?)\s+"
    r"([A-Za-z_][A-Za-z0-9_]*)\b"
)
UNIT_NAME_SPLIT_PATTERN = re.compile(r"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])")

DEFAULT_TESTS_ROOT = Path("Content.IntegrationTests") / "Tests"
DEFAULT_NAMESPACE_PREFIX = "Content.IntegrationTests.Tests"
DEFAULT_SHARD_COUNT = 10
MAX_SHARD_NAME_LENGTH = 96


@dataclass(frozen=True)
class TestUnit:
    name: str
    weight: int
    filter_expression: str


@dataclass
class Shard:
    index: int
    units: list[TestUnit] = field(default_factory=list)
    weight: int = 0

    def add(self, unit: TestUnit) -> None:
        self.units.append(unit)
        self.weight += unit.weight

    @property
    def id(self) -> str:
        return f"shard-{self.index + 1:02d}"

    def to_matrix_entry(self) -> dict[str, object]:
        sorted_units = sorted(self.units, key=lambda unit: unit.name)
        return {
            "id": self.id,
            "name": build_shard_name(self.units, self.weight),
            "tests": self.weight,
            "unit_count": len(sorted_units),
            "units": ", ".join(unit.name for unit in sorted_units),
            "filter": "|".join(unit.filter_expression for unit in sorted_units),
        }


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Generate balanced GitHub Actions shards for Content.IntegrationTests.",
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )
    parser.add_argument("--shards", type=int, default=DEFAULT_SHARD_COUNT)
    parser.add_argument("--tests-root", type=Path, default=DEFAULT_TESTS_ROOT)
    parser.add_argument("--namespace-prefix", default=DEFAULT_NAMESPACE_PREFIX)
    parser.add_argument("--pretty", action="store_true")
    args = parser.parse_args()

    units = discover_units(args.tests_root, args.namespace_prefix)
    if not units:
        raise SystemExit("No integration test units discovered.")

    shards = balance_units(units, args.shards)
    matrix = {"include": [shard.to_matrix_entry() for shard in shards if shard.units]}

    if args.pretty:
        for shard in matrix["include"]:
            print(
                f"{shard['id']}: {shard['name']}, "
                f"{shard['unit_count']} units -> {shard['units']}"
            )
    else:
        print(json.dumps(matrix, separators=(",", ":")))

    return 0


def discover_units(tests_root: Path, namespace_prefix: str) -> list[TestUnit]:
    if not tests_root.exists():
        raise SystemExit(f"Tests root does not exist: {tests_root}")

    units: list[TestUnit] = []

    for file_path in sorted(tests_root.glob("*.cs")):
        unit = build_file_unit(file_path, namespace_prefix)
        if unit is not None:
            units.append(unit)

    for directory in sorted(path for path in tests_root.iterdir() if path.is_dir()):
        unit = build_directory_unit(directory, namespace_prefix)
        if unit is not None:
            units.append(unit)

    return sorted(units, key=lambda unit: (-unit.weight, unit.name))


def build_file_unit(file_path: Path, namespace_prefix: str) -> TestUnit | None:
    test_file = read_test_file(file_path, namespace_prefix)
    if test_file.weight == 0:
        return None

    return TestUnit(
        name=file_path.stem,
        weight=test_file.weight,
        filter_expression=build_filter_expression([test_file]),
    )


def build_directory_unit(directory: Path, namespace_prefix: str) -> TestUnit | None:
    test_files = [
        test_file
        for test_file in (read_test_file(file_path, namespace_prefix) for file_path in sorted(directory.rglob("*.cs")))
        if test_file.weight > 0
    ]

    if not test_files:
        return None

    return TestUnit(
        name=directory.name,
        weight=sum(test_file.weight for test_file in test_files),
        filter_expression=build_filter_expression(test_files),
    )


@dataclass(frozen=True)
class TestFile:
    weight: int
    filters: tuple[str, ...]


def read_test_file(file_path: Path, namespace_prefix: str) -> TestFile:
    text = file_path.read_text(encoding="utf-8")
    weight = len(TEST_MARKER_PATTERN.findall(text))
    if weight == 0:
        return TestFile(weight=0, filters=())

    namespace = read_namespace(text, namespace_prefix)
    type_names = read_type_names(text, file_path.stem)
    if not type_names:
        type_names = (file_path.stem,)

    filters = tuple(f"FullyQualifiedName~{namespace}.{type_name}" for type_name in type_names)
    return TestFile(weight=weight, filters=filters)


def read_namespace(text: str, namespace_prefix: str) -> str:
    match = NAMESPACE_PATTERN.search(text)
    if match is not None:
        return match.group(1).rstrip(".")

    return namespace_prefix


def read_type_names(text: str, fallback_type_name: str) -> tuple[str, ...]:
    type_names: list[str] = []

    for line in text.splitlines():
        line = INLINE_ATTRIBUTES_PATTERN.sub("", line)
        match = TYPE_DECLARATION_PATTERN.match(line)
        if match is not None:
            type_names.append(match.group(1))

    return tuple(sorted(set(type_names))) or (fallback_type_name,)


def build_filter_expression(test_files: list[TestFile]) -> str:
    filters: list[str] = []
    seen: set[str] = set()

    for test_file in test_files:
        for filter_expression in test_file.filters:
            if filter_expression in seen:
                continue

            filters.append(filter_expression)
            seen.add(filter_expression)

    return "|".join(filters)


def build_shard_name(units: list[TestUnit], weight: int) -> str:
    display_units = [humanize_unit_name(unit.name) for unit in sort_units_for_display(units)]
    suffix = f" ({weight} markers)"
    summary = summarize_display_units(display_units, MAX_SHARD_NAME_LENGTH - len(suffix))
    return f"{summary}{suffix}"


def sort_units_for_display(units: list[TestUnit]) -> list[TestUnit]:
    return sorted(units, key=lambda unit: (-unit.weight, unit.name))


def humanize_unit_name(unit_name: str) -> str:
    for suffix in ("Tests", "Test"):
        if unit_name.endswith(suffix):
            unit_name = unit_name[: -len(suffix)]
            break

    return UNIT_NAME_SPLIT_PATTERN.sub(" ", unit_name).strip()


def summarize_display_units(display_units: list[str], max_length: int) -> str:
    if not display_units:
        return "Unknown"

    selected: list[str] = []
    for name in display_units:
        candidate_units = selected + [name]
        remaining = len(display_units) - len(candidate_units)
        candidate = ", ".join(candidate_units)
        if remaining > 0:
            candidate = f"{candidate} +{remaining} more"

        if selected and len(candidate) > max_length:
            break

        selected.append(name)

    remaining = len(display_units) - len(selected)
    summary = ", ".join(selected)
    if remaining > 0:
        summary = f"{summary} +{remaining} more"

    return summary


def balance_units(units: list[TestUnit], shard_count: int) -> list[Shard]:
    if shard_count < 1:
        raise SystemExit("--shards must be at least 1.")

    shard_count = min(shard_count, len(units))
    shards = [Shard(index=index) for index in range(shard_count)]
    for unit in units:
        shard = min(shards, key=lambda candidate: (candidate.weight, len(candidate.units), candidate.index))
        shard.add(unit)

    return shards


if __name__ == "__main__":
    raise SystemExit(main())
