#!/usr/bin/env python3
"""Export model-shaped JS5 blobs from a SQLite .jcache file.

The Unity project intentionally does not depend on a SQLite provider or a BZip2
package. This script does the read-only SQLite/decompression work and writes
plain .dat candidates into an ignored local export folder. Unity then imports
those .dat files with the existing RuneScape model decoder.
"""

from __future__ import annotations

import argparse
import bz2
import gzip
import hashlib
import json
import lzma
import pathlib
import sqlite3
import time
import zlib
from dataclasses import asdict, dataclass
from typing import Iterable


DEFAULT_SOURCE = pathlib.Path(
    "necrotic_client-item_attributes/renamed/js5-8.jcache"
)
DEFAULT_OUTPUT = pathlib.Path(
    "necrotic_client-item_attributes/jcache_exports/js5-8/models"
)
JS5_FLAG_NAMES = 0x01
JS5_FLAG_DIGESTS = 0x02
JS5_FLAG_LENGTHS = 0x04
JS5_FLAG_UNCOMPRESSED_CHECKSUMS = 0x08
WHIRLPOOL_DIGEST_BYTES = 64


@dataclass(frozen=True)
class Candidate:
    key: int
    child: int | None
    codec: str
    format: str
    layout: str
    compressed_bytes: int
    decompressed_bytes: int
    vertices: int
    faces: int
    score: int
    version: int | None
    crc: int | None
    sha1: str


@dataclass(frozen=True)
class ReferenceArchive:
    archive_id: int
    file_count: int
    child_ids: tuple[int, ...]


@dataclass(frozen=True)
class ReferenceIndex:
    metadata: dict[str, object]
    archives_by_id: dict[int, ReferenceArchive]


def read_u16(data: bytes, offset: int) -> int:
    return (data[offset] << 8) | data[offset + 1]


def read_reference_smart(data: bytes, offset: int) -> tuple[int, int]:
    if offset >= len(data):
        raise ValueError("reference table ended unexpectedly")

    if data[offset] & 0x80:
        if offset + 4 > len(data):
            raise ValueError("reference table ended during large smart")
        return int.from_bytes(data[offset : offset + 4], "big") & 0x7FFF_FFFF, offset + 4

    if offset + 2 > len(data):
        raise ValueError("reference table ended during short smart")
    return read_u16(data, offset), offset + 2


def decode_js5_lzma(payload: bytes, expected: int) -> bytes:
    try:
        return lzma.decompress(payload)
    except lzma.LZMAError:
        pass

    if len(payload) < 5:
        raise ValueError("LZMA payload is missing properties")

    # Some JS5 stores LZMA as five LZMA1 property bytes followed by the raw
    # stream; the uncompressed size lives in the JS5 header, not the LZMA body.
    alone_header = payload[:5] + expected.to_bytes(8, "little", signed=False)
    return lzma.decompress(alone_header + payload[5:], format=lzma.FORMAT_ALONE)


def decode_js5_blob(blob: bytes) -> tuple[bytes, str, int | None]:
    if blob.startswith(b"ZLB\x01"):
        expected = int.from_bytes(blob[4:8], "big")
        data = zlib.decompress(blob[8:])
        return data, "ZLB", expected

    if len(blob) < 5:
        raise ValueError("blob is too short for a JS5 container")

    compression = blob[0]
    compressed_length = int.from_bytes(blob[1:5], "big")
    if compressed_length < 0 or compressed_length > len(blob):
        raise ValueError(f"invalid compressed length {compressed_length}")

    if compression == 0:
        data = blob[5 : 5 + compressed_length]
        return data, "JS5-0", len(data)

    if len(blob) < 9:
        raise ValueError("compressed JS5 blob is missing decompressed length")

    expected = int.from_bytes(blob[5:9], "big")
    payload = blob[9 : 9 + compressed_length]

    if compression == 1:
        last_error: Exception | None = None
        for level in b"123456789":
            try:
                data = bz2.decompress(b"BZh" + bytes([level]) + payload)
                return data, "JS5-1-bzip2", expected
            except OSError as exc:
                last_error = exc

        raise ValueError(f"BZip2 decompression failed: {last_error}")

    if compression == 2:
        data = gzip.decompress(payload)
        return data, "JS5-2-gzip", expected

    if compression == 3:
        data = decode_js5_lzma(payload, expected)
        return data, "JS5-3-lzma", expected

    raise ValueError(f"unsupported JS5 compression type {compression}")


def classify_model_candidate(
    data: bytes,
    min_vertices: int,
    max_vertices: int,
    min_faces: int,
    max_faces: int,
    max_decompressed_bytes: int,
    model_layout: str,
) -> tuple[str, str, int, int, int] | None:
    if len(data) < 20 or len(data) > max_decompressed_bytes:
        return None

    if data[-1] == 0xFF and data[-2] == 0xFF and len(data) >= 23:
        model_format = "new"
        vertices = read_u16(data, len(data) - 23)
        faces = read_u16(data, len(data) - 21)
    else:
        model_format = "old"
        vertices = read_u16(data, len(data) - 18)
        faces = read_u16(data, len(data) - 16)

    if not (min_vertices <= vertices <= max_vertices):
        return None

    if not (min_faces <= faces <= max_faces):
        return None

    legacy_layout = is_legacy_model_layout(data, model_format, vertices, faces)
    if model_layout == "legacy" and not legacy_layout:
        return None

    layout = "legacy" if legacy_layout else "trailer-only"

    # Prefer detailed, non-tiny meshes while capping raw byte advantage so huge
    # non-model blobs do not dominate every export slot.
    score = vertices * 2 + faces * 3 + min(len(data), 250_000)
    if legacy_layout:
        score += 80_000

    return model_format, layout, vertices, faces, score


def is_legacy_model_layout(data: bytes, model_format: str, vertices: int, faces: int) -> bool:
    if model_format == "old":
        return is_old_model_layout(data, vertices, faces)

    return is_new525_model_layout(data, vertices, faces)


def is_old_model_layout(data: bytes, vertices: int, faces: int) -> bool:
    if len(data) < 18:
        return False

    trailer = len(data) - 18
    texture_faces = data[trailer + 4]
    render_type_flag = data[trailer + 5]
    priority_flag = data[trailer + 6]
    alpha_flag = data[trailer + 7]
    face_skin_flag = data[trailer + 8]
    vertex_skin_flag = data[trailer + 9]
    vertex_x_length = read_u16(data, trailer + 10)
    vertex_y_length = read_u16(data, trailer + 12)
    vertex_z_length = read_u16(data, trailer + 14)
    face_index_length = read_u16(data, trailer + 16)

    if render_type_flag not in (0, 1):
        return False

    if alpha_flag not in (0, 1) or face_skin_flag not in (0, 1) or vertex_skin_flag not in (0, 1):
        return False

    offset = 0
    offset += vertices
    offset += faces
    if priority_flag == 255:
        offset += faces

    if face_skin_flag == 1:
        offset += faces

    if render_type_flag == 1:
        offset += faces

    if vertex_skin_flag == 1:
        offset += vertices

    if alpha_flag == 1:
        offset += faces

    offset += face_index_length
    offset += faces * 2
    offset += texture_faces * 6
    offset += vertex_x_length + vertex_y_length + vertex_z_length
    return offset == trailer


def is_new525_model_layout(data: bytes, vertices: int, faces: int) -> bool:
    if len(data) < 23:
        return False

    trailer = len(data) - 23
    texture_faces = data[trailer + 4]
    flags = data[trailer + 5]
    if flags & 0x08:
        # The project's Unity decoder has a 622-ish reader, but these JS5/NXT
        # blobs need a dedicated format handler before being treated as clean
        # legacy assets.
        return False

    if flags not in (0, 1):
        return False

    texture_counts = count_texture_types(data, texture_faces)
    if texture_counts is None:
        return False

    priority_flag = data[trailer + 6]
    alpha_flag = data[trailer + 7]
    face_skin_flag = data[trailer + 8]
    material_flag = data[trailer + 9]
    vertex_skin_flag = data[trailer + 10]
    vertex_x_length = read_u16(data, trailer + 11)
    vertex_y_length = read_u16(data, trailer + 13)
    vertex_z_length = read_u16(data, trailer + 15)
    face_index_length = read_u16(data, trailer + 17)
    texture_index_length = read_u16(data, trailer + 19)

    if alpha_flag not in (0, 1) or face_skin_flag not in (0, 1):
        return False

    if material_flag not in (0, 1) or vertex_skin_flag not in (0, 1):
        return False

    simple_textures, complex_textures, translucent_textures = texture_counts
    offset = texture_faces
    offset += vertices
    if flags == 1:
        offset += faces

    offset += faces
    if priority_flag == 255:
        offset += faces

    if face_skin_flag == 1:
        offset += faces

    if vertex_skin_flag == 1:
        offset += vertices

    if alpha_flag == 1:
        offset += faces

    offset += face_index_length
    if material_flag == 1:
        offset += faces * 2

    offset += texture_index_length
    offset += faces * 2
    offset += vertex_x_length + vertex_y_length + vertex_z_length
    offset += simple_textures * 6
    offset += complex_textures * 6
    offset += complex_textures * 6
    offset += complex_textures
    offset += complex_textures
    offset += complex_textures + translucent_textures * 2
    return offset == trailer


def count_texture_types(data: bytes, texture_faces: int) -> tuple[int, int, int] | None:
    if texture_faces < 0 or texture_faces > len(data):
        return None

    simple = 0
    complex_count = 0
    translucent = 0
    for index in range(texture_faces):
        texture_type = data[index]
        if texture_type == 0:
            simple += 1
        elif 1 <= texture_type <= 3:
            complex_count += 1
        else:
            return None

        if texture_type == 2:
            translucent += 1

    return simple, complex_count, translucent


def connect_read_only(path: pathlib.Path) -> sqlite3.Connection:
    return sqlite3.connect(f"file:{path}?mode=ro", uri=True)


def iter_cache_rows(connection: sqlite3.Connection) -> Iterable[tuple[int, bytes, int | None, int | None]]:
    for key, data, version, crc in connection.execute(
        "select KEY, DATA, VERSION, CRC from cache order by KEY"
    ):
        yield int(key), bytes(data), version, crc


def summarize_distribution(values: Iterable[int], limit: int = 12) -> list[dict[str, int]]:
    counts: dict[int, int] = {}
    for value in values:
        counts[value] = counts.get(value, 0) + 1

    return [
        {"value": value, "count": count}
        for value, count in sorted(counts.items(), key=lambda item: (-item[1], item[0]))[:limit]
    ]


def read_reference_index(source: pathlib.Path) -> ReferenceIndex:
    try:
        with connect_read_only(source) as connection:
            row = connection.execute(
                "select KEY, DATA, VERSION, CRC from cache_index order by KEY limit 1"
            ).fetchone()

        if row is None:
            return ReferenceIndex({"present": False}, {})

        index_key, blob, version, crc = row
        data, codec, expected_bytes = decode_js5_blob(bytes(blob))
        offset = 0
        protocol = data[offset]
        offset += 1
        revision = None
        if protocol >= 6:
            revision = int.from_bytes(data[offset : offset + 4], "big")
            offset += 4

        flags = data[offset]
        offset += 1
        archive_count, offset = read_reference_smart(data, offset)

        archive_ids: list[int] = []
        archive_id = 0
        for _ in range(archive_count):
            delta, offset = read_reference_smart(data, offset)
            archive_id += delta
            archive_ids.append(archive_id)

        has_names = (flags & JS5_FLAG_NAMES) != 0
        has_digests = (flags & JS5_FLAG_DIGESTS) != 0
        has_lengths = (flags & JS5_FLAG_LENGTHS) != 0
        has_uncompressed_checksums = (flags & JS5_FLAG_UNCOMPRESSED_CHECKSUMS) != 0

        metadata_offset = offset
        if has_names:
            metadata_offset += archive_count * 4

        metadata_offset += archive_count * 4
        if has_uncompressed_checksums:
            metadata_offset += archive_count * 4

        if has_digests:
            metadata_offset += archive_count * WHIRLPOOL_DIGEST_BYTES

        if has_lengths:
            metadata_offset += archive_count * 8

        metadata_offset += archive_count * 4
        metadata_bytes_before_file_counts = metadata_offset - offset
        file_counts_offset = metadata_offset
        file_counts: list[int] = []
        if file_counts_offset < len(data):
            cursor = file_counts_offset
            for _ in range(archive_count):
                count, cursor = read_reference_smart(data, cursor)
                file_counts.append(count)

        archives_by_id: dict[int, ReferenceArchive] = {}
        if len(file_counts) == len(archive_ids):
            for archive_id, file_count in zip(archive_ids, file_counts):
                child_id = 0
                child_ids: list[int] = []
                for _ in range(file_count):
                    delta, cursor = read_reference_smart(data, cursor)
                    child_id += delta
                    child_ids.append(child_id)

                archives_by_id[archive_id] = ReferenceArchive(
                    archive_id=archive_id,
                    file_count=file_count,
                    child_ids=tuple(child_ids),
                )

        metadata: dict[str, object] = {
            "present": True,
            "indexKey": int(index_key),
            "codec": codec,
            "compressedBytes": len(blob),
            "decodedBytes": len(data),
            "expectedBytes": expected_bytes,
            "version": version,
            "crc": crc,
            "protocol": protocol,
            "revision": revision,
            "flags": flags,
            "hasNames": has_names,
            "hasDigests": has_digests,
            "hasLengths": has_lengths,
            "hasUncompressedChecksums": has_uncompressed_checksums,
            "archiveCount": archive_count,
            "archiveIdMin": min(archive_ids) if archive_ids else None,
            "archiveIdMax": max(archive_ids) if archive_ids else None,
            "metadataBytesBeforeFileCounts": metadata_bytes_before_file_counts,
            "fileCountDistribution": summarize_distribution(file_counts),
            "totalFiles": sum(file_counts) if file_counts else None,
            "allArchivesSingleFile": bool(file_counts) and all(count == 1 for count in file_counts),
            "archiveChildrenDecoded": len(archives_by_id),
        }
        return ReferenceIndex(metadata, archives_by_id)
    except Exception as exc:
        return ReferenceIndex({"present": True, "error": str(exc)}, {})


def split_js5_group(data: bytes, file_count: int) -> list[bytes]:
    if file_count <= 1:
        return [data]

    if not data:
        raise ValueError("empty JS5 group")

    chunk_count = data[-1]
    table_length = chunk_count * file_count * 4
    table_offset = len(data) - 1 - table_length
    if chunk_count <= 0 or table_offset < 0:
        raise ValueError(
            f"invalid JS5 group footer: files={file_count}, chunks={chunk_count}, bytes={len(data)}"
        )

    sizes = [0] * file_count
    cursor = table_offset
    for _chunk in range(chunk_count):
        chunk_size = 0
        for file_index in range(file_count):
            chunk_size += int.from_bytes(data[cursor : cursor + 4], "big", signed=False)
            if chunk_size > table_offset:
                raise ValueError("invalid JS5 group length table")

            cursor += 4
            sizes[file_index] += chunk_size

    if sum(sizes) != table_offset:
        raise ValueError(
            f"invalid JS5 group sizes: payload={table_offset}, children={sum(sizes)}"
        )

    files = [bytearray(size) for size in sizes]
    positions = [0] * file_count
    payload_offset = 0
    cursor = table_offset
    for _chunk in range(chunk_count):
        chunk_size = 0
        for file_index in range(file_count):
            chunk_size += int.from_bytes(data[cursor : cursor + 4], "big", signed=False)
            cursor += 4
            if payload_offset + chunk_size > table_offset:
                raise ValueError("invalid JS5 group chunk length table")

            files[file_index][positions[file_index] : positions[file_index] + chunk_size] = data[
                payload_offset : payload_offset + chunk_size
            ]
            positions[file_index] += chunk_size
            payload_offset += chunk_size

    return [bytes(file) for file in files]


def iter_archive_files(
    key: int,
    data: bytes,
    reference_index: ReferenceIndex,
) -> Iterable[tuple[int | None, bytes, bool]]:
    archive = reference_index.archives_by_id.get(key)
    if archive is None or archive.file_count <= 1:
        yield None, data, False
        return

    children = split_js5_group(data, archive.file_count)
    for index, child_data in enumerate(children):
        child_id = archive.child_ids[index] if index < len(archive.child_ids) else index
        yield child_id, child_data, True


def scan_candidates(args: argparse.Namespace) -> tuple[list[Candidate], dict[str, int]]:
    stats = {
        "rowsScanned": 0,
        "decodedRows": 0,
        "childFilesScanned": 0,
        "splitArchiveRows": 0,
        "splitFailures": 0,
        "candidateRows": 0,
        "legacyLayoutCandidates": 0,
        "trailerOnlyCandidates": 0,
        "duplicateCandidates": 0,
        "decodeFailures": 0,
    }
    candidates: list[Candidate] = []
    seen_hashes: set[str] = set()
    reference_index = read_reference_index(args.source)

    with connect_read_only(args.source) as connection:
        for key, blob, version, crc in iter_cache_rows(connection):
            if args.scan_limit and stats["rowsScanned"] >= args.scan_limit:
                break

            stats["rowsScanned"] += 1
            try:
                data, codec, expected_bytes = decode_js5_blob(blob)
            except Exception:
                stats["decodeFailures"] += 1
                continue

            stats["decodedRows"] += 1
            if expected_bytes is not None and expected_bytes != len(data):
                # Keep scanning, but do not feed suspicious entries to Unity.
                continue

            try:
                archive_files = list(iter_archive_files(key, data, reference_index))
            except Exception:
                stats["splitFailures"] += 1
                continue

            if any(is_split for _child, _child_data, is_split in archive_files):
                stats["splitArchiveRows"] += 1

            for child, child_data, _is_split in archive_files:
                stats["childFilesScanned"] += 1
                classification = classify_model_candidate(
                    child_data,
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
                sha1 = hashlib.sha1(child_data).hexdigest()
                if sha1 in seen_hashes:
                    stats["duplicateCandidates"] += 1
                    continue

                seen_hashes.add(sha1)
                stats["candidateRows"] += 1
                if layout == "legacy":
                    stats["legacyLayoutCandidates"] += 1
                else:
                    stats["trailerOnlyCandidates"] += 1

                candidates.append(
                    Candidate(
                        key=key,
                        child=child,
                        codec=codec,
                        format=model_format,
                        layout=layout,
                        compressed_bytes=len(blob),
                        decompressed_bytes=len(child_data),
                        vertices=vertices,
                        faces=faces,
                        score=score,
                        version=version,
                        crc=crc,
                        sha1=sha1,
                    )
                )

    candidates.sort(key=lambda item: (item.score, item.decompressed_bytes), reverse=True)
    return candidates, stats


def sanitize_filename(value: str) -> str:
    allowed = []
    for char in value.lower():
        if char.isalnum():
            allowed.append(char)
        else:
            allowed.append("_")

    cleaned = "".join(allowed).strip("_")
    while "__" in cleaned:
        cleaned = cleaned.replace("__", "_")
    return cleaned or "model"


def select_candidates(args: argparse.Namespace, candidates: list[Candidate]) -> list[Candidate]:
    if args.selection_strategy == "score":
        return candidates[: args.limit]

    if not candidates or args.limit <= 0:
        return []

    head_count = min(args.head_count, args.limit, len(candidates))
    selected = list(candidates[:head_count])
    seen_keys = {candidate.key for candidate in selected}
    remaining_slots = args.limit - len(selected)
    remaining_candidates = candidates[head_count:]

    if remaining_slots <= 0 or not remaining_candidates:
        return selected

    if remaining_slots >= len(remaining_candidates):
        selected.extend(candidate for candidate in remaining_candidates if candidate.key not in seen_keys)
        return selected[: args.limit]

    # The best-looking meshes are not always the largest blobs. Sample through
    # the ranked list so Unity can post-filter real geometry instead of only
    # seeing the biggest false positives.
    step = (len(remaining_candidates) - 1) / max(remaining_slots - 1, 1)
    for index in range(remaining_slots):
        candidate = remaining_candidates[round(index * step)]
        if candidate.key in seen_keys:
            continue

        seen_keys.add(candidate.key)
        selected.append(candidate)

    if len(selected) < args.limit:
        for candidate in remaining_candidates:
            if candidate.key in seen_keys:
                continue

            selected.append(candidate)
            seen_keys.add(candidate.key)
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
    selected_keys: dict[int, dict[int | None, tuple[int, Candidate]]] = {}
    for rank, candidate in enumerate(selected):
        selected_keys.setdefault(candidate.key, {})[candidate.child] = (rank, candidate)

    exported: list[dict[str, object]] = []
    args.output.mkdir(parents=True, exist_ok=True)
    if args.clear_output:
        clear_generated_output(args.output)

    source_label = sanitize_filename(args.source.stem)
    reference_index = read_reference_index(args.source)

    with connect_read_only(args.source) as connection:
        for key, blob, _version, _crc in iter_cache_rows(connection):
            selected_children = selected_keys.get(key)
            if selected_children is None:
                continue

            data, _codec, _expected = decode_js5_blob(blob)
            for child, child_data, _is_split in iter_archive_files(key, data, reference_index):
                selected_entry = selected_children.get(child)
                if selected_entry is None:
                    continue

                rank, candidate = selected_entry
                child_part = f"_child_{child:06d}" if child is not None else ""
                file_name = (
                    f"{source_label}_rank_{rank:04d}_key_{candidate.key:06d}"
                    f"{child_part}_{candidate.format}_{candidate.layout}_v{candidate.vertices}_f{candidate.faces}_"
                    f"s{candidate.score}"
                )
                output_path = args.output / f"{sanitize_filename(file_name)}.dat"
                output_path.write_bytes(child_data)
                record = asdict(candidate)
                record["rank"] = rank
                record["path"] = str(output_path.relative_to(args.repo_root))
                exported.append(record)

                if len(exported) >= len(selected):
                    break

            if len(exported) >= len(selected):
                break

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
        "referenceIndex": read_reference_index(args.source).metadata,
        "stats": stats,
        "candidateCount": len(candidates),
        "exportedCount": len(exported),
        "selection": {
            "limit": args.limit,
            "scanLimit": args.scan_limit,
            "selectionStrategy": args.selection_strategy,
            "headCount": args.head_count,
            "clearOutput": args.clear_output,
            "minVertices": args.min_vertices,
            "maxVertices": args.max_vertices,
            "minFaces": args.min_faces,
            "maxFaces": args.max_faces,
            "maxDecompressedBytes": args.max_decompressed_bytes,
            "modelLayout": args.model_layout,
        },
        "exported": exported,
    }
    manifest_path.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    return manifest_path


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Read a SQLite JS5 .jcache file and export model-shaped decompressed .dat candidates."
    )
    parser.add_argument("--repo-root", type=pathlib.Path, default=pathlib.Path.cwd())
    parser.add_argument("--source", type=pathlib.Path, default=DEFAULT_SOURCE)
    parser.add_argument("--output", type=pathlib.Path, default=DEFAULT_OUTPUT)
    parser.add_argument("--limit", type=int, default=900)
    parser.add_argument("--scan-limit", type=int, default=0)
    parser.add_argument("--selection-strategy", choices=("mixed", "score"), default="mixed")
    parser.add_argument("--head-count", type=int, default=120)
    parser.add_argument("--clear-output", action="store_true")
    parser.add_argument("--min-vertices", type=int, default=3)
    parser.add_argument("--max-vertices", type=int, default=35_000)
    parser.add_argument("--min-faces", type=int, default=1)
    parser.add_argument("--max-faces", type=int, default=70_000)
    parser.add_argument("--max-decompressed-bytes", type=int, default=4_500_000)
    parser.add_argument(
        "--model-layout",
        choices=("legacy", "exploratory"),
        default="legacy",
        help=(
            "legacy exports only payloads whose footer and section offsets match "
            "known RS2/525 layouts; exploratory keeps the old trailer-count scan "
            "for future NXT decoder research."
        ),
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
    print(
        "Rows scanned: {rowsScanned}, decoded: {decodedRows}, candidates: {candidateRows}, "
        "duplicates: {duplicateCandidates}, failures: {decodeFailures}".format(**stats)
    )
    print(
        "Layout candidates: legacy={legacyLayoutCandidates}, trailer-only={trailerOnlyCandidates}, "
        "mode={mode}".format(mode=args.model_layout, **stats)
    )
    print(
        "Child files scanned: {childFilesScanned}, split archives: {splitArchiveRows}, "
        "split failures: {splitFailures}".format(**stats)
    )
    print(f"Exported: {len(exported)}")
    print(f"Manifest: {manifest_path}")
    for record in exported[:12]:
        print(
            "#{rank:04d} key={key} {format} vertices={vertices} faces={faces} "
            "bytes={decompressed_bytes} score={score} {path}".format(**record)
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
