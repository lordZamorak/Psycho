#!/usr/bin/env python3
"""Export model-shaped JS5 blobs from a classic dat2/idx cache folder.

This is the read-only bridge for external cache releases such as the ATD 921
cache. It scans selected cache indexes, decompresses JS5 containers, keeps only
payloads that look like known RuneScape model layouts, and writes small .dat
candidates into an ignored local export folder for Unity to import.
"""

from __future__ import annotations

import argparse
import hashlib
import importlib.util
import json
import pathlib
import sys
import time
from dataclasses import asdict, dataclass
from typing import Iterable


DEFAULT_SOURCE = pathlib.Path("C:/Users/xzero/Downloads/Atd 921 Cache Release/data")
DEFAULT_OUTPUT = pathlib.Path("necrotic_client-item_attributes/jcache_exports/atd-921/models")
INDEX_ENTRY_SIZE = 6
SECTOR_SIZE = 520
SECTOR_HEADER_SIZE = 8
LARGE_SECTOR_HEADER_SIZE = 10


def load_jcache_helpers():
    helper_path = pathlib.Path(__file__).with_name("export-js5-jcache-models.py")
    spec = importlib.util.spec_from_file_location("psycho_jcache_exporter", helper_path)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Could not load helper script: {helper_path}")

    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


JCACHE_HELPERS = load_jcache_helpers()


@dataclass(frozen=True)
class Candidate:
    cache_index: int
    file_id: int
    codec: str
    format: str
    layout: str
    compressed_bytes: int
    decompressed_bytes: int
    vertices: int
    faces: int
    score: int
    sha1: str


def read_medium(data: bytes, offset: int) -> int:
    return (data[offset] << 16) | (data[offset + 1] << 8) | data[offset + 2]


def read_cache_file(
    data_file,
    index_file,
    cache_index: int,
    file_id: int,
    max_compressed_bytes: int,
) -> bytes | None:
    index_file.seek(0, 2)
    if file_id < 0 or (file_id + 1) * INDEX_ENTRY_SIZE > index_file.tell():
        return None

    index_file.seek(file_id * INDEX_ENTRY_SIZE)
    entry = index_file.read(INDEX_ENTRY_SIZE)
    if len(entry) != INDEX_ENTRY_SIZE:
        return None

    length = read_medium(entry, 0)
    sector = read_medium(entry, 3)
    if length <= 0 or length > max_compressed_bytes or sector <= 0:
        return None

    payload = bytearray(length)
    bytes_read = 0
    chunk = 0
    large_file_id = file_id > 0xFFFF
    header_size = LARGE_SECTOR_HEADER_SIZE if large_file_id else SECTOR_HEADER_SIZE
    sector_payload_size = SECTOR_SIZE - header_size
    max_chunks = max(1, length // sector_payload_size + 2)
    expected_archives = {cache_index, cache_index + 1}

    while bytes_read < length:
        if chunk > max_chunks or sector <= 0:
            return None

        data_file.seek(sector * SECTOR_SIZE)
        sector_bytes = data_file.read(SECTOR_SIZE)
        if len(sector_bytes) != SECTOR_SIZE:
            return None

        if large_file_id:
            read_file_id = int.from_bytes(sector_bytes[0:4], "big")
            read_chunk = int.from_bytes(sector_bytes[4:6], "big")
            next_sector = read_medium(sector_bytes, 6)
            archive = sector_bytes[9]
        else:
            read_file_id = int.from_bytes(sector_bytes[0:2], "big")
            read_chunk = int.from_bytes(sector_bytes[2:4], "big")
            next_sector = read_medium(sector_bytes, 4)
            archive = sector_bytes[7]

        if read_file_id != file_id or read_chunk != chunk or archive not in expected_archives:
            return None

        copy_length = min(sector_payload_size, length - bytes_read)
        payload[bytes_read : bytes_read + copy_length] = sector_bytes[header_size : header_size + copy_length]
        bytes_read += copy_length
        sector = next_sector
        chunk += 1

    return bytes(payload)


def parse_indexes(value: str) -> list[int]:
    indexes: list[int] = []
    for part in value.split(","):
        part = part.strip()
        if not part:
            continue

        if "-" in part:
            start_text, end_text = part.split("-", 1)
            start = int(start_text)
            end = int(end_text)
            indexes.extend(range(start, end + 1))
        else:
            indexes.append(int(part))

    return indexes


def iter_index_files(args: argparse.Namespace) -> Iterable[tuple[int, int, pathlib.Path]]:
    for cache_index in args.indexes:
        index_path = args.source / f"main_file_cache.idx{cache_index}"
        if not index_path.exists():
            continue

        file_count = index_path.stat().st_size // INDEX_ENTRY_SIZE
        if args.scan_limit:
            file_count = min(file_count, args.scan_limit)

        for file_id in range(args.start_file, file_count):
            yield cache_index, file_id, index_path


def scan_candidates(args: argparse.Namespace) -> tuple[list[Candidate], dict[str, int]]:
    stats = {
        "indexesScanned": 0,
        "filesScanned": 0,
        "containersRead": 0,
        "decodedContainers": 0,
        "candidateFiles": 0,
        "legacyLayoutCandidates": 0,
        "trailerOnlyCandidates": 0,
        "duplicateCandidates": 0,
        "readFailures": 0,
        "decodeFailures": 0,
    }
    candidates: list[Candidate] = []
    seen_hashes: set[str] = set()
    data_path = args.source / "main_file_cache.dat2"
    if not data_path.exists():
        data_path = args.source / "main_file_cache.dat"
    if not data_path.exists():
        raise FileNotFoundError(f"Missing cache data file under {args.source}")

    opened_indexes: dict[int, object] = {}
    seen_indexes: set[int] = set()
    try:
        with data_path.open("rb") as data_file:
            for cache_index, file_id, index_path in iter_index_files(args):
                stats["filesScanned"] += 1
                if cache_index not in seen_indexes:
                    seen_indexes.add(cache_index)
                    stats["indexesScanned"] += 1

                index_file = opened_indexes.get(cache_index)
                if index_file is None:
                    index_file = index_path.open("rb")
                    opened_indexes[cache_index] = index_file

                raw = read_cache_file(data_file, index_file, cache_index, file_id, args.max_compressed_bytes)
                if raw is None:
                    stats["readFailures"] += 1
                    continue

                stats["containersRead"] += 1
                try:
                    data, codec, expected_bytes = JCACHE_HELPERS.decode_js5_blob(raw)
                except Exception:
                    stats["decodeFailures"] += 1
                    continue

                if expected_bytes is not None and expected_bytes != len(data):
                    stats["decodeFailures"] += 1
                    continue

                stats["decodedContainers"] += 1
                classification = JCACHE_HELPERS.classify_model_candidate(
                    data,
                    args.min_vertices,
                    args.max_vertices,
                    args.min_faces,
                    args.max_faces,
                    args.max_decompressed_bytes,
                    args.model_layout,
                )
                if classification is None:
                    continue

                model_format, layout, vertices, faces, score = classification
                sha1 = hashlib.sha1(data).hexdigest()
                if sha1 in seen_hashes:
                    stats["duplicateCandidates"] += 1
                    continue

                seen_hashes.add(sha1)
                stats["candidateFiles"] += 1
                if layout == "legacy":
                    stats["legacyLayoutCandidates"] += 1
                else:
                    stats["trailerOnlyCandidates"] += 1

                candidates.append(
                    Candidate(
                        cache_index=cache_index,
                        file_id=file_id,
                        codec=codec,
                        format=model_format,
                        layout=layout,
                        compressed_bytes=len(raw),
                        decompressed_bytes=len(data),
                        vertices=vertices,
                        faces=faces,
                        score=score,
                        sha1=sha1,
                    )
                )
    finally:
        for index_file in opened_indexes.values():
            index_file.close()

    candidates.sort(key=lambda item: (item.score, item.decompressed_bytes), reverse=True)
    return candidates, stats


def select_candidates(args: argparse.Namespace, candidates: list[Candidate]) -> list[Candidate]:
    if args.selection_strategy == "score" or args.limit <= 0:
        return candidates[: args.limit]

    head_count = min(args.head_count, args.limit, len(candidates))
    selected = list(candidates[:head_count])
    seen = {(candidate.cache_index, candidate.file_id) for candidate in selected}
    remaining_slots = args.limit - len(selected)
    remaining = candidates[head_count:]
    if remaining_slots <= 0 or not remaining:
        return selected

    step = (len(remaining) - 1) / max(remaining_slots - 1, 1)
    for index in range(remaining_slots):
        candidate = remaining[round(index * step)]
        key = (candidate.cache_index, candidate.file_id)
        if key in seen:
            continue

        selected.append(candidate)
        seen.add(key)

    if len(selected) < args.limit:
        for candidate in remaining:
            key = (candidate.cache_index, candidate.file_id)
            if key in seen:
                continue

            selected.append(candidate)
            seen.add(key)
            if len(selected) >= args.limit:
                break

    return selected[: args.limit]


def clear_generated_output(output: pathlib.Path) -> int:
    if not output.exists():
        return 0

    removed = 0
    for path in output.glob("*.dat"):
        path.unlink()
        removed += 1

    manifest = output / "manifest.json"
    if manifest.exists():
        manifest.unlink()

    return removed


def export_candidates(args: argparse.Namespace, candidates: list[Candidate]) -> list[dict[str, object]]:
    selected = select_candidates(args, candidates)
    selected_by_key = {(candidate.cache_index, candidate.file_id): (rank, candidate) for rank, candidate in enumerate(selected)}
    exported: list[dict[str, object]] = []
    args.output.mkdir(parents=True, exist_ok=True)
    if args.clear_output:
        clear_generated_output(args.output)

    data_path = args.source / "main_file_cache.dat2"
    if not data_path.exists():
        data_path = args.source / "main_file_cache.dat"

    opened_indexes: dict[int, object] = {}
    try:
        with data_path.open("rb") as data_file:
            for candidate in selected:
                index_file = opened_indexes.get(candidate.cache_index)
                if index_file is None:
                    index_file = (args.source / f"main_file_cache.idx{candidate.cache_index}").open("rb")
                    opened_indexes[candidate.cache_index] = index_file

                raw = read_cache_file(data_file, index_file, candidate.cache_index, candidate.file_id, args.max_compressed_bytes)
                if raw is None:
                    continue

                data, _codec, _expected = JCACHE_HELPERS.decode_js5_blob(raw)
                rank, _selected_candidate = selected_by_key[(candidate.cache_index, candidate.file_id)]
                file_name = (
                    f"atd_921_rank_{rank:04d}_idx_{candidate.cache_index:03d}_file_{candidate.file_id:06d}_"
                    f"{candidate.format}_{candidate.layout}_v{candidate.vertices}_f{candidate.faces}_s{candidate.score}"
                )
                output_path = args.output / f"{JCACHE_HELPERS.sanitize_filename(file_name)}.dat"
                output_path.write_bytes(data)
                record = asdict(candidate)
                record["rank"] = rank
                record["path"] = str(output_path.relative_to(args.repo_root))
                exported.append(record)
    finally:
        for index_file in opened_indexes.values():
            index_file.close()

    exported.sort(key=lambda item: int(item["rank"]))
    return exported


def write_manifest(
    args: argparse.Namespace,
    stats: dict[str, int],
    candidates: list[Candidate],
    exported: list[dict[str, object]],
) -> pathlib.Path:
    manifest_path = args.output / "manifest.json"
    manifest = {
        "generatedAtUtc": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
        "source": str(args.source),
        "output": str(args.output),
        "stats": stats,
        "candidateCount": len(candidates),
        "exportedCount": len(exported),
        "selection": {
            "indexes": args.indexes,
            "limit": args.limit,
            "scanLimit": args.scan_limit,
            "startFile": args.start_file,
            "selectionStrategy": args.selection_strategy,
            "headCount": args.head_count,
            "clearOutput": args.clear_output,
            "minVertices": args.min_vertices,
            "maxVertices": args.max_vertices,
            "minFaces": args.min_faces,
            "maxFaces": args.max_faces,
            "maxCompressedBytes": args.max_compressed_bytes,
            "maxDecompressedBytes": args.max_decompressed_bytes,
            "modelLayout": args.model_layout,
        },
        "exported": exported,
    }
    manifest_path.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    return manifest_path


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Read a dat2/idx JS5 cache folder and export model-shaped decompressed .dat candidates."
    )
    parser.add_argument("--repo-root", type=pathlib.Path, default=pathlib.Path.cwd())
    parser.add_argument("--source", type=pathlib.Path, default=DEFAULT_SOURCE)
    parser.add_argument("--output", type=pathlib.Path, default=DEFAULT_OUTPUT)
    parser.add_argument("--indexes", type=str, default="7", help="Comma/range list of cache indexes to scan, e.g. 7 or 7,47.")
    parser.add_argument("--limit", type=int, default=900)
    parser.add_argument("--scan-limit", type=int, default=0, help="Max files per index; 0 scans the whole index.")
    parser.add_argument("--start-file", type=int, default=0)
    parser.add_argument("--selection-strategy", choices=("mixed", "score"), default="mixed")
    parser.add_argument("--head-count", type=int, default=120)
    parser.add_argument("--clear-output", action="store_true")
    parser.add_argument("--min-vertices", type=int, default=3)
    parser.add_argument("--max-vertices", type=int, default=35_000)
    parser.add_argument("--min-faces", type=int, default=1)
    parser.add_argument("--max-faces", type=int, default=70_000)
    parser.add_argument("--max-compressed-bytes", type=int, default=12_000_000)
    parser.add_argument("--max-decompressed-bytes", type=int, default=4_500_000)
    parser.add_argument(
        "--model-layout",
        choices=("legacy", "exploratory"),
        default="legacy",
        help="Use legacy for Unity's current model decoder; exploratory is for future decoder research.",
    )
    return parser


def resolve_paths(args: argparse.Namespace) -> argparse.Namespace:
    args.repo_root = args.repo_root.resolve()
    if not args.source.is_absolute():
        args.source = args.repo_root / args.source
    if not args.output.is_absolute():
        args.output = args.repo_root / args.output
    args.source = args.source.resolve()
    args.output = args.output.resolve()
    args.indexes = parse_indexes(args.indexes)
    return args


def main() -> int:
    args = resolve_paths(build_parser().parse_args())
    if not args.source.exists():
        raise FileNotFoundError(args.source)

    candidates, stats = scan_candidates(args)
    exported = export_candidates(args, candidates)
    manifest_path = write_manifest(args, stats, candidates, exported)

    print(f"Source: {args.source}")
    print(f"Output: {args.output}")
    print(f"Indexes: {','.join(str(index) for index in args.indexes)}")
    print(
        "Files scanned: {filesScanned}, containers read: {containersRead}, decoded: {decodedContainers}, "
        "candidates: {candidateFiles}, duplicates: {duplicateCandidates}".format(**stats)
    )
    print(
        "Layout candidates: legacy={legacyLayoutCandidates}, trailer-only={trailerOnlyCandidates}, "
        "mode={mode}".format(mode=args.model_layout, **stats)
    )
    print(f"Exported: {len(exported)}")
    print(f"Manifest: {manifest_path}")
    for record in exported[:12]:
        print(
            "#{rank:04d} idx={cache_index} file={file_id} {format} vertices={vertices} faces={faces} "
            "bytes={decompressed_bytes} score={score} {path}".format(**record)
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
